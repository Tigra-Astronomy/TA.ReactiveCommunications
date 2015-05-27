using System;
using System.Diagnostics;
using System.IO.Ports;
using FakeItEasy;
using Machine.Specifications;

namespace TA.Ascom.ReactiveCommunications.Specifications.Contexts
    {
    /// <summary>
    ///   Class with_localhost_endpoint_on_port_1234.
    ///   Creates a test context for a SerialCommunicationChannel
    ///   with an endpoint configured as COM1, 9600, N, 8, 1
    ///   and a mocked serial port.
    /// </summary>
    public class with_serial_channel_on_endpoint_com1_9600_n_8_1_and_mock_serial_port
        {
        protected static SerialCommunicationChannel Channel;
        protected static DataReceivedEventArgs ChannelReceiveEventArgs;
        protected static bool ChannelReceiveEventFired;
        protected static object ChannelReceiveEventSender;
        protected static int ChannelReceivedEventCounter;
        protected static DeviceEndpoint DeviceEndpoint;
        protected static ISerialPort MockPort;
        protected static SerialDeviceEndpoint SerialEndpoint;
        protected static Exception Thrown;
        Cleanup after = () =>
            {
            Channel = null;
            DeviceEndpoint = null;
            };
        Establish context = () =>
            {
            SerialEndpoint = new SerialDeviceEndpoint("COM1", 9600, Parity.None, 8, StopBits.One);
            DeviceEndpoint = SerialEndpoint;
            MockPort = A.Fake<ISerialPort>();
            A.CallTo(() => MockPort.IsOpen).Returns(true);
            Channel = new SerialCommunicationChannel(DeviceEndpoint, MockPort);
            Thrown = null;
            };
        }
    }