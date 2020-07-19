# Reactive ASCOM #

![Reactive ASCOM Logo](https://github.com/Tigra-Astronomy/TA.ReactiveCommunications/wiki/Artwork/ReactiveASCOM520x520.png)  

## Reactive-style transactional device communications for [ASCOM][ascom] drivers with guaranteed sequencing and thread-safety ##

[Reactive ASCOM][wiki] (namespace TA.Ascom.ReactiveCommunications) is a library based on the [Reactive Extensions for .NET][rx] for simplifying device communications over serial, Ethernet and other types of communications channel to embedded device controllers.

Originally designed as an aid for developers of [ASCOM][ascom] device drivers,
the library may be of use in any situation where a client application
needs to communicate with some connected device.

Rx comms leverages Rx.net to guarantee correct sequencing and thread-safety of transactions. It is also useful for reacting to
the device's received data stream and updating UI controls with
fresh data, without running into cross-thread control update issues.

For usage instructions and background information, [please refer to the wiki pages][wiki], which also serves as the project home page.

Available as a [NuGet][4] package - simply `Install-Package TA.Ascom.ReactiveCommunications`

## License ##

This project, its source code, compiled binaries and wiki content
are all licensed under the [Tigra MIT license][license].
The project is open souece and free.
Not half-heartedly free as with some projects, but [genuinely free][yt-sysf].
In summary:

> You can do whatever you like with this project and there are no strings attached.  
> Whatever you do is your responsibility and we can't be held liable for the outcome.

[license]: https://tigra.mit-license.org "The Tigra Astronomy no-strings free software license"
[yt-sysf]: https://www.youtube.com/watch?v=kloweL2fw7Q "Set Your Software Free: Our philosphy on open source software"

Tim Long - Tigra Astronomy, July 2020.

[rx: https://github.com/dotnet/reactive "Reactive Extensions for .NET"
[ascom]: http://ascom-standards.org "Astronomy Common Object Model"
[nuget]: http://www.nuget.org "NuGet Package Manager"
[yt-intro]: https://www.youtube.com/watch?v=2rE6ZsNUWCE&t=8s "Quick start introductory video"
[yt-trans-intro]: https://www.youtube.com/watch?v=QqMK0nu01MI "Basic guide to creating transactions"
[yt-trans-deep]: https://www.youtube.com/watch?v=hV9BzGyiZwc "Deep dive into creating transactions"
[license]: https://tigra.mit-license.org "The Tigra Astronomy no-strings free software license"
[yt-sysf]: https://www.youtube.com/watch?v=kloweL2fw7Q "Set Your Software Free: Our philosphy on open source software"
