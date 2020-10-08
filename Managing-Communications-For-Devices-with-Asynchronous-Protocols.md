# Managing Communications for Devices With Asynchronous Protocols

In this article, I will discuss ways of handling unusual devices that don't
follow the typical Command/Response paradigm. A prime example of this is the
Digital Domeworks dome/roof control system from Technical Innovations. This
sytem typically does not respond directly to any command. Instead, it emits a
series of notifications, followed eventually by a "status update" which contains
all of the state information for the entire system. As an example, assume the
dome is at azimuth zero and we wish to rotate to 5 degrees. We would send `G005`
and the receive stream would be something like this:

<pre>
R
P001
P002
P003
P004
P005
V4,414,8,1,5,0,0,1,0,1,16,0,128,255,255,255,255,0,255,255,999,3,0
</pre>
One might be tempted to try to enforce the Command/Response model on this
protocol and to use the `R` as the command's response, but if a user presses a
rotation button on the hand paddle, a subtly different sequence is received
where there is no newline after the initial `R` or `L` and in fact in some
variants of the hardware, the azimuth encoder is not able to detect the
direction of rotation, so this is a risky strategy at best.

In this case, it may be better to simply not wait for the response and update
the system's state in real time, as the responses arrive. The controller
hardware always tells us whenever its internal state changes, so a happy
consequence of this "reactive" approach is that there is never any need to
directly query the controller. The amount of data transmitted on the serial port
is therefore minimal (compared to a typical ASCOM implementation which will be
very "chatty" as it polls the value of every property every second or two.

The Digital DomeWorks protocol may at first seem like an odd approach, but in
fact it is rather well designed and enables us to write some well-structured
real time software to drive it.

There are many ways to achieve our aim, but one thing we don't want to do is
block a thread while waiting on a response from the serial port. In fact, it
would be better to avoid multi-threading altogether as threading is amazingly
incredibly hard to get right - far harder than most people think. It is better
to adopt an *event driven* strategy where we react to data as it arrives.
Instead of saying "I'm ready for more data, I'll wait until it is ready" we can
instead say "When the next piece of data arrives, please run this method. I'm
off to do something else while you get it".

One technology that is perfect for this sort of situation is the Reactive
Extensions for .NET, which are in the namespace `System.Reactive` (obtained by
installing a NuGet package called `System.Reactive`). The Reactive Extensions
are the foundation of the [Reactive Communications for ASCOM] library, which is
in NuGet package [`TA.ASCOM.ReactiveCommunications`]. Herafter I will refer to
this library as `RxAscom`.

The RxAscom library was designed to help with transactional Command/Response
protocols and to gaurantee thread safety, while ensuring that responses go to
the correct commands. However, even though we will not be using transactions
here, RxAscom still has something to offer in this scenario. The library has an
abstraction of a connected device called `ICommunicationsChannel`. A channel
models the connected device as streams of characters and the method of
communication is hidden. We provide an implementation for serial ports, but
developers can create other types of channel (for example,the [Digital
DomeWorks driver][DdwDriver] defines a channel that acts as a hardware
simulator). No matter the type of channel, we will always deal with a character
stream, via `ICommunicationsChannel.ObservableReceivedCharacters`, which exposes
an `IObservable<char>` observable sequence of characters.

## Reacting to the Incoming Data Stream

Once we have opened an `ICommunicationsChannel` then we have an
`IObservable<char>` that represents characters coming from the connected device.
Note: the connection may be a serial port or something else entirely, we don't
need to know that information at this level of abstraction. All we care about is
that we have a stream of characters.

We can process the incoming data stream by creating a collection of LINQ queries
to parse the incoming sequence of characters, thus breaking it up into sequences
of different types of input, that we can then "react" to using different pieces
of code. Some examples of this, taken from the [DdwDriver], can
be found in the `DeviceInterface` project, in the file
[`ObservableExtensions.cs`][ObservableExtensions]:

``` csharp
    /// <summary>
    /// Extracts azimuth encoder ticks from a source sequence and emits
    /// the encoder values as an observable sequence of integers.
    /// </summary>
    /// <param name="source"></param>
    public static IObservable<int> AzimuthEncoderTicks(this IObservable<char> source)
        {
        const string azimuthEncoderPattern = @"^P(?<Azimuth>\d{1,4})[^0-9]";
        var azimuthEncoderRegex =
            new Regex(azimuthEncoderPattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        var buffers = source.Publish(s => s.BufferByPredicates(p => p == 'P', q => !char.IsDigit(q)));
        var azimuthValues = from buffer in buffers
                            let message = new string(buffer.ToArray())
                            let patternMatch = azimuthEncoderRegex.Match(message)
                            where patternMatch.Success
                            let azimuth = int.Parse(patternMatch.Groups["Azimuth"].Value)
                            select azimuth;
        return azimuthValues.Trace("EncoderTicks");
        }
```

This extension method can operate on any `IObservable<char>` and looks for a
sequence of characters that matches a regular expression. Regular expressions
are a very powerful pattern matching mechanism and here we use an expression
that matches anything that begins with a 'P' character, followed by 1 to 4
decimal digits, folowed by a character that is not a number. We know that
Digital DomeWorks always sends a `<CR><LF>` sequence after each azimuth encoder
tick, so either of these will serve as our final character that is not a digit.

Note this part of the regular expression: `(?<Azimuth>\d{1,4})`. This is known
as a *named capture group*, whose name is "Azimuth" and which matches the
pattern `\d{1,4}`, in other words, a sequence of 1 to 4 decimal digits. Using a
named capture will allow us to access this data later.

Our query is a little complex so will need some explanation. First, we have the
following line:

``` csharp
var buffers = source.Publish(s => s.BufferByPredicates(p => p == 'P', q => !char.IsDigit(q)));
```

We will not discuss the `source.Publish()` part here as it is an optimization
that would just confuse things. The meat of this line is another extension
method, `BufferByPredicates`, which takes two predicate expressions as its
arguments, and produces a new observable sequence of buffers (a buffer in this
case is an `IList<char>`). A predicate is simply something that tests whether
its input matches some condition and returns `true` for a match. So the first
predicate operates on the incoming character sequence and returns `true` when we
should start building a new buffer. The second predicate returns `true` when
we've reached the end of the buffer. So, as defined above, we will wait until a
'P' character is received, then we will begin adding characters (including the
'P') to a buffer, as long as we are receiving digits. Once we receive a
non-digit (and our second predicate returns `true`) then we will stop buffering
and emit the buffer into a new sequence, which is an `IObservable<IList<char>>`.
Each element in this new sequence contains a whole buffer, which will always be
a 'P' followed by zero or more digits. In other words, it will contain the
azimuth encoder ticks sent by the firmware (and possibly some other junk that
happens to look a bit like an encoder tick, since our predicates are not an
exhaustive test).

Then comes our LINQ query:

``` csharp
var azimuthValues = from buffer in buffers
                    let message = new string(buffer.ToArray())
                    let patternMatch = azimuthEncoderRegex.Match(message)
                    where patternMatch.Success
                    let azimuth = int.Parse(patternMatch.Groups["Azimuth"].Value)
                    select azimuth;
```

We've used LINQ Comprehension Query syntax here to make it more readable. This
query acts on the sequence of buffers produced by the `BufferByPredicates()`
extension method. First, it transforms the sequence from
`IObservable<IList<char>>` to `IObservable<string>`:

``` csharp
        let message = new string(buffer.ToArray())
```

Next, it feeds the strings to the regular expression matcher to produce a
sequence of matches.

``` csharp
        let patternMatch = azimuthEncoderRegex.Match(message)
```

Now, we filter out any responses that are not syntactically correct azimuth
encoder ticks, by testing whether they successfully matched our strict regular
expression:

``` csharp
        where patternMatch.Success
```

Then, we pull the matched set of digits out of the regular expression match,
using the named capture group "Azimuth", and (since we can gaurantee this is a
valid integer) we parse it directly into an `int` value:

``` csharp
        let azimuth = int.Parse(patternMatch.Groups["Azimuth"].Value)
```

`azimuth` is therefore an integer, and we select that as the final result of our
query, to produce an observable sequence of azimuth encoder positions,
`IObservable<int>`:

``` csharp
        select azimuth;
```

Before returning the transformed sequence, the extension method tacks on a bit
of diagnostics so we can see what's going on:

``` csharp
    return azimuthValues.Trace("EncoderTicks");
```

We can now observe (subscribe to) that sequence, and arrange for some code to
run whenever we receive a valid azimuth encoder tick, ignoring all other input,
and the code that runs will receive the encoder position as an integer!

The sequence is used within the `DeviceController.cs` file in a method called
`SubscribeAzimuthEncoderTicks()`. The extension method is applied to the
sequence of characters coming from the receive channel, then it is subscribed to
so that a method called `stateMachine.AzimuthEncoderTickReceived` is called,
passing in the integer encoder value. This serves to trigger the state machine
that is at the heart of the driver:

``` csharp
private void SubscribeAzimuthEncoderTicks()
    {
    var azimuthEncoderTicks = channel.ObservableReceivedCharacters.AzimuthEncoderTicks();
    var azimuthEncoderSubscription = azimuthEncoderTicks
        //.ObserveOn(Scheduler.Default)
        .Subscribe(
            stateMachine.AzimuthEncoderTickReceived,
            ex => throw new InvalidOperationException(
                "Encoder tick sequence produced an unexpected error (see ineer exception)", ex),
            () => throw new InvalidOperationException(
                "Encoder tick sequence completed unexpectedly, this is probably a bug")
        );
    disposableSubscriptions.Add(azimuthEncoderSubscription);
    }
```

The final line there adds the subscription to a list of subscriptions that must
be disposed later, when the connection is closed. Correct disposal is important.

The driver defines [several similar extension
methods][ObservableExtensions] that serve to isolate and process other
types of input:

- Azimuth encoder ticks
- Shutter motor current measurements
- Status updates
- Shutter movement direction updates
- Dome rotation direction updates

These various different types of input stream are all used to trigger the state
machine that controls all of the driver's actions and in some cases they will
indirectly trigger display updates.

Notice that the driver is never blocked waiting for any data. We define a set of rules
ahead of time for what should happen, and as characters arrive from the receive
channel, things just happen exactly when they should. We never block and we
never poll the device for any information. Polling is the result of poor design
choices!

## Using a State Machine to Track the Hardware State

## Updating Your Graphical User Interface in Real Time

  [Reactive Communications for ASCOM]: http://tigra-astronomy.com/reactive-communications-for-ascom
    "Reactive Communications for ASCOM, project home page"
  [RxAscom]: https://www.nuget.org/packages/TA.Ascom.ReactiveCommunications/
    "NuGet package"
  [DdwDriver]: https://bitbucket.org/tigra-astronomy/ta.digitaldomeworks/
    "Digital Domeworks on BitBucket"
  [ObservableExtensions]: https://bitbucket.org/tigra-astronomy/ta.digitaldomeworks/src/master/TA.DigitalDomeworks.DeviceInterface/ObservableExtensions.cs
    "CSharp Source code"
