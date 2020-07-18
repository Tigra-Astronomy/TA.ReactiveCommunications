# Reactive ASCOM #

![Reactive ASCOM Logo](http://tigra-astronomy.com/Media/TigraAstronomy/ProjectIcons/ReactiveASCOM520x520.png)  

## Reactive-style transactional device communications for [ASCOM][3] drivers with guaranteed sequencing and thread-safety ##

[Reactive ASCOM][1] (namespace TA.Ascom.ReactiveCommunications) is a library based on the [Reactive Extensions for .NET][2] for simplifying device communications over serial, Ethernet and other types of communications channel to embedded controllers, while guaranteeing correct sequencing and thread-safety of transactions.

Originally developed for use with [ASCOM][3] drivers, but may be useful for any software that needs to communicate with an embedded controller using a predominantly command-response protocol.

This [blog article][5] describes the approach and gives a bried overview of how to use the library. The source code contains a sample console application demonstrating how to create and use the various classes.

Available as a [NuGet][4] package - simply `Install-Package TA.Ascom.ReactiveCommunications`

The [main project page][1] for [Reactive ASCOM][1] is at: http://tigra-astronomy.com/reactive-communications-for-ascom

## License ##

Reactive ASCOM uses the [MIT License][6].

Tim Long - Tigra Astronomy, May 2015.

## Getting Started ##

We have posted a number of "HowTo" videos on our YouTube channel.

- [Introduction to Reactive Communications for ASCOM][yt-intro]
- [Building Custom Transactions for Reactive ASCOM][yt-trans-intro]
- [Deep Dive Into Creating Transactions][yt-trans-deep]

The source code also contains a pair of sample console applications, which demonstrate the two main
usage patterns that we have identified.
How you use RX Comms will most likely depend on the communications protocol that your device uses.
There seems to be two main types.

- Devices where commands and responses happen in closely coupled pairs, with the response comming immediately or after a very short delay.
  We call this the *Transactional Model* and each command and response can be theought of as a single *transaction*.
  For this type of protocol, you'll probably want to focus on understanding the `DeviceTransaction` class
  and implementing your own transaction subclasses.
  You'll use the `ReactiveTransactionProcessor` to execute your transactions.

- Devices where commands may not have a direct response but the device emits notifications
  in real time, not necessarily in response to a direct command.
  We call this the *Asynchronous Model*.
  This type of protocol can actually be great to work with if done well and
  a reactive programming style is an excellent fit.
  You'll probably want to focus on chopping up your input stream into observable sequences
  and hooking them up to actions, state machines and user interface components.

- In most cases, there is a continuum between the *Transactional* and *Asynchronous* models
  and most devices will require some elements of both.
  Rx Comms has you covered, either way.

- Whatever approach you adopt, you will also need to understand:
  - Communications channels (`ICommunicationsChannel` and `SerialCommunicationsChannel`)
  - `DeviceEndpoint` used for storing channel configuration data.
  - `ChannelFactory` for creating channels from connection strings.


  [1]: http://tigra-astronomy.com/reactive-communications-for-ascom "Project Page"
  [2]: https://rx.codeplex.com/ "Rx Project"
  [3]: http://ascom-standards.org "Astronomy Common Object Model"
  [4]: http://www.nuget.org "NuGet Package Manager"
  [5]: http://tigra-astronomy.com/blog/the-well-travelled-photon/reactive-ascom "Introducing Reactive ASCOM"
  [6]: http://tigra.mit-license.org/ "Tigra Astronomy MIT License"
  [yt-intro]: https://www.youtube.com/watch?v=2rE6ZsNUWCE&t=8s "Quick start introductory video"
  [yt-trans-intro]: https://www.youtube.com/watch?v=QqMK0nu01MI "Basic guide to creating transactions"
  [yt-trans-deep]: https://www.youtube.com/watch?v=hV9BzGyiZwc "Deep dive into creating transactions"