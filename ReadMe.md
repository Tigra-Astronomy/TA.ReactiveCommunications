# Reactive Communications for ASCOM

![Reactive ASCOM logo](images/RxCommsIcon.png)

**Reactive Communications for ASCOM** is a class library and an object-oriented design pattern for handling device communications. We will explain what it is, explain the key concepts and then provide a code example.

## TL;DR - Quick start

In addition to this documentation, we have posted a number of "HowTo" videos on our YouTube channel.

- [Introduction to Reactive Communications for ASCOM][yt-intro]
- [Building Custom Transactions for Reactive ASCOM][yt-trans-intro]
- [Deep Dive Into Creating Transactions][yt-trans-deep]

The source code also contains a pair of sample console applications, which demonstrate the two main
usage patterns that we have identified.

## What Is It?

**Reactive** refers to a programming style that is well-suited to real-time systems that must react to events, such as received data or user interactivity. The style works especially well with asynchronous I/O.

**Reactive Extensions for .NET** (referred to as _Rx_) is a class library making it easy to use a reactive programming style in .NET languages such as C#. It provides some gaurantees about the order in which things will be processed. The key type in Rx is `IObservable<T>` which defines a sequence of data that is yet to arrive. Rx provides a powerful way to describe what we _intend to do_ with the data _when it arrives_. Therefore it allows us to use a declarative style to describe our intent without having to focus too much on the implementation details.

**Reactive Communications for ASCOM** (referred to as _RxComms_) builds upon Rx and provides both a design pattern and a class library for dealing with device communications.

It provides a few key types and a strong object-oriented design pattern for handling inter-device communications.
The key types are `ICommunicationsChannel` (which descibes a connection to a device) and `DeviceTransaction` which is an abstract base class for building transactional communication models.

## Why?

RxComms arose out of the need to solve difficult problems of sequencing and thread-safety that we encountered when developing commercial ASCOM drivers.
Like many ASCOM driver writers, we didn't give much thought to these issues early on when coding our drivers.
Then multi-threaded client applications began to appear and things started to go wrong.
Developers typically try to solve this by using multi-threading and thread synchronization and this can provide a partial solution.
Unfortunately, due to the workings of Single Threaded Apartments (STA threads) this also tends to go wrong for rather obscure and hard-to-debug reasons.
Any in-process ASCOM driver or a driver that has a user interface will likely suffer from this so your multi-threaded code may not work as you expect.

We needed a way to solve these issues in multiple drivers but we were not happy with any of the "brute force" solutions available.
At the same time, we needed a way to make drivers that could work with different types of connection and we needed a general putpose way of doing that.

We decided to take a step back and think about the problem in terms of the SOLID principles of object-oriented design, and come up with a general-purpose object-oriented solution that could be packaged as a reusable library.

RxComms provides a robust object-oriented pattern that doesn't require explicit use of threading, but solves all of the sequencing and thread safety issues.

Another difficult problem provoked by multi-threaded applications is ensuring that commands and responses don't get mixed up.
If two application threads call two different driver methods, each may try to send a command to the device and wait for a response.
There are no gaurantees about the order of the commands and responses and there is a real risk that they will get mixed up.
The ASCOM Serial Helper ensures that commands are serialized, but this only solves half of the problem.
It cannot gaurantee the order that commands and responses will arrive in and it does nothing to ensure that responses go to the correct commands.
RxComms introduces the concept of a _Transaction_ (with support in abstract base class `DeviceTransaction`) that helps to keep commands and responses together.
RxComms can't quite do all the work for you because it doesn't know what protocol your device will speak, so you will implement classes that derive from `DeviceTransaction` for each different data type that you need to receive.
This is easier than it might sound, and the `TransactionalCommunicationModel` sample application shows how to do it.

## Basic approach

To use RxComms, a client must at minimum create an instance of an `ICommunicationsChannel` using the `ChannelFactory` class.
Depending on the communications model adopted, implementations may need to define one or more classes derived from `DeviceTransaction` and will use an `ITransactionProcessor` to process transactions.

## Concepts

How you use RX Comms will most likely depend on the communications protocol that your device uses.
There seems to be two main types.

- _Devices where commands and responses happen in closely coupled pairs_, with the response comming immediately or after a very short delay.
  We call this the **Transactional Model** and each command and response can be theought of as a single *transaction*.
  For this type of protocol, you'll probably want to focus on understanding the `DeviceTransaction` class
  and implementing your own transaction subclasses.
  You'll use the `ReactiveTransactionProcessor` to execute your transactions.

- _Devices where commands may not have a direct response but the device emits notifications
  in real time, not necessarily in response to a direct command_.
  We call this the **Asynchronous Model**.
  This type of protocol can actually be great to work with if done well and
  a reactive programming style is an excellent fit.
  You'll probably want to focus on chopping up your input stream into observable sequences
  and hooking them up to actions, state machines and user interface components.

- _In most cases, there is a continuum between the **Transactional** and **Asynchronous** models_
  and most devices will require some elements of both.
  Rx Comms has you covered, either way.

### Communications Channel

A _communications channel_ is the pipeline through which data is sent to and received from a device. Channels implement the `ICommunicationsChannel` interface. A channel has a method to send a string of data (command) to the device and exposes an `IObservable<char>` representing a sequence of received characters.

RxComms includes a single implementation of `ICommunicationsChannel`: `SerialCommunicationsChannel`.
You can create other types of channel by implementing the `ICommunicationsChannel` interface and registering your class with the channel factory at runtime.

At the base level, received data is represented as an observable sequence of characters, which means that all handling of received data can be done in a reactive style.

Devices using the *Asynchronous Model* are discussed in a separate document, "[Managing Communications for Devices With Asynchronous Protocols][async]"

### Device Transaction

This is perhaps the key point of understanding when using RxComms.

A _transaction_ can be thought of as a single data exchange between an application (ASCOM driver) and a device, over a communications channel.

Each transaction has a well-defined lifecycle. It is _created_, then _committed_. Transactions execute asynchronously and the result can be awaited either synchronously or asynchronously.

Each transaction is responsible for observing the received data sequence and "plucking out" just the exact data that it considers to be a valid response, ignoring all other data. This is achieved by means of LINQ queries and operators.
RxComms provides a number of helpers and extension methods to assist in creating these LINQ queries.

Once valid data has been received, the transaction is complete and is marked as _Successful_. The application can read the response. If a transaction does not complete withing a user specified time, it times out and is marked as _Failed_.

Note that transactions are constructed so that they can only receive valid responses. Invalid data is simply ignored, and absence of a response results in a _Failed_ transaction.

Transactions are most useful if they are constructed to receive a certain type of data. For example, if the application needs to request the date and time from a device, then we might create a `DateTimeTransaction`. The class could then have a property named `Value` of type `DateTime`. When the transaction is completes successfully, the `Value` property would contain the date and time received from the device. Another common example (for ASCOM drivers) is the need to receive coordinates (right ascension, declination, altitude, azimuth, latitude, longitude) as _sexagesimal_ (base 60) strings. We provide an example `SexagesimalTransaction` in the sample project.

### Transaction Observer

The `TransactionObserver` class implements `IObserver<DeviceTransaction>` and therefore takes an observable sequence of transactions as its input.
The `TransactionObserver` also has a reference to an `ICommunicationsChannel` which is passed in the constructor. Once subscribed to a sequence of transactions, the class executes each transaction in the sequence, sending a command to the device and receiving a valid response (or timing out).

The application developer must provide the observable sequence of transactions and create the subscription to the transaction observer. RxComms provides a class (`ReactiveTransactionProcessor`) that provides a ready-made way to do this.

### Reactive Transaction Processor

This is a helper class that provides a way to create an observable sequence of transactions and feed them to a `TransactionObserver`. The class also offers the option to rate-limit ("throttle") the stream of transactions.

Once an instance is created (specifying any rate limit in the constructor), the developer can call `SubscribeTransactionObserver()`, passing in an instance of `TransactionObserver`. This creates the transaction sequence and the link between the `IObserver` and the `IObservable`.

Thereafter, transactions are executed by calling `ReactiveTransactionProcessor.CommitTransaction()`.


  [project]: http://tigra-astronomy.com/reactive-communications-for-ascom "Project Page"
  [rx]: https://rx.codeplex.com/ "Rx Project"
  [ascom]: http://ascom-standards.org "Astronomy Common Object Model"
  [nuget]: http://www.nuget.org "NuGet Package Manager"
  [license]: http://tigra.mit-license.org/ "Tigra Astronomy MIT License"
  [async]: Managing-Communications-for-Devices-With-Asynchronous-Protocols.md "Markdown document"
  [yt-intro]: https://www.youtube.com/watch?v=2rE6ZsNUWCE&t=8s "Quick start introductory video"
  [yt-trans-intro]: https://www.youtube.com/watch?v=QqMK0nu01MI "Basic guide to creating transactions"
  [yt-trans-deep]: https://www.youtube.com/watch?v=hV9BzGyiZwc "Deep dive into creating transactions"