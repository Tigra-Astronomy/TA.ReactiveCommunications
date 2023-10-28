﻿// This file is part of the Timtek.ReactiveCommunications project
// 
// Copyright © 2015-2020 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: EndpointSpecs.cs  Last modified: 2020-07-20@00:51 by Tim Long

using System.IO.Ports;
using Machine.Specifications;
using Timtek.ReactiveCommunications;

namespace Timtek.ReactiveCommunications.Specifications
    {
    [Subject(typeof(DeviceEndpoint))]
    public class when_creating_a_serial_device_endpoint_from_a_fully_specified_connection_string
        {
        static DeviceEndpoint Endpoint;
        Because of =
            () => Endpoint =
                SerialDeviceEndpoint.FromConnectionString(
                    "COM127:115200,Even,7,OnePointFive,NOdtr,norts,RequestToSendXOnXOff");
        It should_be_a_serial_endpoint = () => Endpoint.ShouldBeOfExactType<SerialDeviceEndpoint>();
        It should_have_baud_rate = () => ((SerialDeviceEndpoint) Endpoint).BaudRate.ShouldEqual(115200);
        It should_have_data_bits = () => ((SerialDeviceEndpoint) Endpoint).DataBits.ShouldEqual(7);
        It should_have_handshake = () =>
            ((SerialDeviceEndpoint) Endpoint).Handshake.ShouldEqual(Handshake.RequestToSendXOnXOff);
        It should_have_parity = () => ((SerialDeviceEndpoint) Endpoint).Parity.ShouldEqual(Parity.Even);
        It should_have_stop_bits = () => ((SerialDeviceEndpoint) Endpoint).StopBits.ShouldEqual(StopBits.OnePointFive);
        It should_not_assert_dtr = () => ((SerialDeviceEndpoint) Endpoint).DtrEnable.ShouldBeFalse();
        It should_not_assert_rts = () => ((SerialDeviceEndpoint) Endpoint).RtsEnable.ShouldBeFalse();
        It should_target_com127 = () => ((SerialDeviceEndpoint) Endpoint).PortName.ShouldEqual("COM127");
        }

    [Subject(typeof(DeviceEndpoint), "network")]
    public class when_creating_an_endpoint_from_a_lan_connection_string
        {
        static DeviceEndpoint Endpoint;
        Because of = () => Endpoint = NetworkDeviceEndpoint.FromConnectionString("localhost:1234");
        It should_be_a_network_endpoint = () => Endpoint.ShouldBeOfExactType<NetworkDeviceEndpoint>();
        It should_be_localhost = () => ((NetworkDeviceEndpoint) Endpoint).Host.ShouldEqual("localhost");
        It should_have_the_expected_port = () => ((NetworkDeviceEndpoint) Endpoint).Port.ShouldEqual(1234);
        }

    [Subject(typeof(DeviceEndpoint), "network")]
    public class when_creating_an_endpoint_from_a_wan_connection_string
        {
        static DeviceEndpoint Endpoint;
        Because of = () => Endpoint = NetworkDeviceEndpoint.FromConnectionString("www.tigranetworks.co.uk:1234");
        It should_be_a_network_endpoint = () => Endpoint.ShouldBeOfExactType<NetworkDeviceEndpoint>();
        It should_be_localhost = () => ((NetworkDeviceEndpoint) Endpoint).Host.ShouldEqual("www.tigranetworks.co.uk");
        It should_have_the_expected_port = () => ((NetworkDeviceEndpoint) Endpoint).Port.ShouldEqual(1234);
        }

    [Subject(typeof(DeviceEndpoint), "network")]
    public class when_creating_an_endpoint_from_an_ipv4_connection_string
        {
        static DeviceEndpoint Endpoint;
        Because of = () => Endpoint = NetworkDeviceEndpoint.FromConnectionString("1.12.123.123:1234");
        It should_be_a_network_endpoint = () => Endpoint.ShouldBeOfExactType<NetworkDeviceEndpoint>();
        It should_be_localhost = () => ((NetworkDeviceEndpoint) Endpoint).Host.ShouldEqual("1.12.123.123");
        It should_have_the_expected_port = () => ((NetworkDeviceEndpoint) Endpoint).Port.ShouldEqual(1234);
        }

    [Subject(typeof(SerialDeviceEndpoint), "connection string generation")]
    internal class when_generating_a_connection_string_from_a_serial_endpoint
        {
        const string expectedConnectionString = "COM127:115200,Even,7,Two,nodtr,norts";
        static DeviceEndpoint endpoint;
        static string connection;
        Establish context = () =>
            endpoint = new SerialDeviceEndpoint("COM127", 115200, Parity.Even, 7, StopBits.Two, false, false);
        Because of = () => connection = endpoint.DeviceAddress;
        It should_produce_a_syntactically_correct_fully_qualified_connection_string =
            () => connection.ShouldEqual(expectedConnectionString);
        }
    }