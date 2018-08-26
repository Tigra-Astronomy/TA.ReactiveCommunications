
This rather simplistic console application demonstrates a number of important features of Reactive ASCOM.
- It is really simple to get going. Under typical circumstances you need instances of `TransactionObserver`, `ICommunicationsChannel` and `ITransactionProcessor` - and they are really easy to create and use.
- Communicate with the device by creating an appropriate `DeviceTransaction` derived object, and call `ITransactionProcessor.CommitTransaction()`
- Wait for your result by calling `DeviceTransaction.WaitForCompletionOrTimeout()` or `DeviceTransaction.WaitForCompletionOrTimeoutAsync()`.
- Transactions are sequenced based on the order of submission, no matter what else you do to them.
- ***Reactive ASCOM*** guarantees correct sequencing and thread safety.
- You get logging and diagnostics of your protocol 'for free'.
- The key is to create transaction classes for your protocol, derived from `DeviceTransaction`, that only recognise correct responses.

In the code sample, we have done things in a deliberately awkward way to demonstrate some of these features. For example:
- We use `Task.Run()` to submit each transaction asynchronously. We don't know when these tasks will actually run so we have no idea when or in what order the transactions will actually execute.
- We wait for and use the transactions *in the reverse order that we submit them,* to increase the chance that we will be waiting out of sequence. Nevertheless, everything works and the right response goes to the right transaction. Sequencing and thread safety is guaranteed based on the order of submission.

## Sample Output ##
We captured this output from one run of the program with the logging level set to "Debug". Note that you might see things in a slightly different order depending on your machine environment.
<pre>
2015-05-25 17:19:05.2043 INFO Transaction pipeline connected to channel with endpoint COM1:9600,None,8,One
2015-05-25 17:19:05.2668 INFO Channel opening =COM1:9600,None,8,One
2015-05-25 17:19:05.2824 DEBUG Event: SerialDataReceived
2015-05-25 17:19:05.2824 DEBUG Listening to DataReceived event
Waiting for declination
2015-05-25 17:19:05.2980 INFO Committing transaction TID=1 [:GR#] [{no value}] 00:00:02
2015-05-25 17:19:05.3449 DEBUG Rx=0
2015-05-25 17:19:05.3449 DEBUG Rx=8
2015-05-25 17:19:05.3449 DEBUG Rx=:
2015-05-25 17:19:05.3449 DEBUG Rx=3
2015-05-25 17:19:05.3449 DEBUG Rx=7
2015-05-25 17:19:05.3449 DEBUG Rx=:
2015-05-25 17:19:05.3449 DEBUG Rx=1
2015-05-25 17:19:05.3449 DEBUG Rx=8
2015-05-25 17:19:05.3449 DEBUG Rx=#
2015-05-25 17:19:05.3605 INFO Transaction 1 completed
2015-05-25 17:19:05.3605 INFO Completed transaction TID=1 [:GR#] [08:37:18#] 00:00:02
2015-05-25 17:19:05.3605 INFO Committing transaction TID=2 [:GD#] [{no value}] 00:00:02
2015-05-25 17:19:05.3762 DEBUG Rx=-
2015-05-25 17:19:05.3762 DEBUG Rx=1
2015-05-25 17:19:05.3762 DEBUG Rx=8
2015-05-25 17:19:05.3762 DEBUG Rx=ß
2015-05-25 17:19:05.3762 DEBUG Rx=3
2015-05-25 17:19:05.3762 DEBUG Rx=9
2015-05-25 17:19:05.3762 DEBUG Rx=:
2015-05-25 17:19:05.3762 DEBUG Rx=0
2015-05-25 17:19:05.3762 DEBUG Rx=0
2015-05-25 17:19:05.3762 DEBUG Rx=#
Declination: -18ß39:00#
Waiting for Right Ascensions
Right Ascension: 08:37:18#
2015-05-25 17:19:05.3762 INFO Transaction 2 completed
2015-05-25 17:19:05.3762 INFO Completed transaction TID=2 [:GD#] [-18ß39:00#] 00:00:02
2015-05-25 17:19:05.3762 INFO Channel closing =COM1:9600,None,8,One
2015-05-25 17:19:05.3918 DEBUG Stopped listening to DataReceived event
</pre>

## Things to note about the output ##
- You get pretty good logging and diagnostics 'for free' based on [NLog](http://nlog-project.org/ "NLog Project Web Site"). We just write to the console but NLog is extremely flexible. See `NLog.config` where there is a link to the NLog documentation. If you don't want the logging, just delete the configuration, or create a filter to exclude it.
- Note in line 5 of the output that we are already waiting for the declination transaction before it has even been submitted. This shows that there really is some multi-threaded magic going on and that it is absolutely fine to be waiting for a transaction before it is 'live'.
- Both transactions are processed while we are waiting for the Declination result. When we get to the Right Ascension transaction, we don't wait because it has already been processed and the result is immediately available.
- Even though the two transactions were submitted potentially on different threads, neither the transmit nor receive streams got corrupted. Reactive ASCOM guarantees sequencing and thread safety.
- Although we didn't demonstrate this, you can wait for transactions more than once and possibly on multiple threads. Each transaction internally uses a private wait handle to ensure proper thread locking semantics. The wait handle is encapsulated and hidden so there's no way for client code to abuse it.