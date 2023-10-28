// This file is part of the Timtek.ReactiveCommunications project
// 
// Copyright � 2015-2020 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: with_serial_channel_on_endpoint_com1_9600_n_8_1_and_mock_serial_port.cs  Last modified: 2020-07-20@00:51 by Tim Long

using System;
using System.Diagnostics;
using FakeItEasy;
using Machine.Specifications;
using Timtek.ReactiveCommunications;

// ReSharper disable MissingXmlDoc

namespace Timtek.ReactiveCommunications.Specifications.Contexts
    {
    /// <summary>
    ///     Class with_localhost_endpoint_on_port_1234. Creates a test context for a
    ///     <see cref="SerialCommunicationChannel" /> with an endpoint configured as COM1, 9600, N, 8, 1
    ///     and a mocked serial port.
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
            SerialEndpoint = new SerialDeviceEndpoint("COM1");
            DeviceEndpoint = SerialEndpoint;
            MockPort = A.Fake<ISerialPort>();
            A.CallTo(() => MockPort.IsOpen).Returns(true);
            Channel = new SerialCommunicationChannel(DeviceEndpoint, MockPort);
            Thrown = null;
            };
        }
    }