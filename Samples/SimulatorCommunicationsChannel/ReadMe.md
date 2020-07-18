# Dome Simulator Custom Communications Channel Sample #

This sample shows one use for a custom communications channel.

This code was abstracted from the Digital DomeWorks ASCOM driver.
The simulator was used during development of the driver and is useful
for trying things out without having to have real hardware connected.

The main classes of interest are:

- `SimulatorCommunicationsChannel`, implements `ICommunicationsChannel`.
  Monitors the commands being sent and simulates responses being sent back.
  No serial port is involved.
  Responses are injected directly into the observable sequence of characters.
- `SimulatorEndpoint` holds teh channel configuration (which is rather simple in this case).
  Also provides the helper methods needed by the `ChannelFactory` when creating
  the channel from a connection string.

The simulator is packaged inside an ICommunicationsChannel.
The idea here is that the user can change between real hardware
connected to a serial port and the simulator just by changing a setting.
No code has to change in the ASCOM driver for this to happen, it is all driven
purely by a settings change to the ConnectionString setting.
The driver is completely unaware that its talking to a simulator!

The ability to easily switch between different types of communication channels
was one of the key design goals of RxComms.
We currently supply a single channel implementation, `SerialCommunicationsChannel`.
Driver developers can create their own channel types and we may offer more types
of channel in the future.

Custom channels are registered with the `ChannelFactory` class.
This is typically done immediately on application startup.

``` lang=cs
        private static void Main(string[] args)
            {
            var loggingService = new LoggingService(); // TA.Utils.Logging.NLog
            var channelFactory = new ChannelFactory(loggingService);
            channelFactory.RegisterChannelType(
                SimulatorEndpoint.IsConnectionStringValid,
                SimulatorEndpoint.FromConnectionString,
                enpoint => new SimulatorCommunicationsChannel((SimulatorEndpoint) enpoint, loggingService));
            var connectionString = Configuration["ConnectionString"];
            var channel = channelFactory.FromConnectionString(connectionString);
            // ... rest of program ...
            }
```
