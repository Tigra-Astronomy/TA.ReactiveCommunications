// This file is part of the Timtek.ReactiveCommunications project
// 
// Copyright © 2015-2020 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: SerialChannelSpecs.cs  Last modified: 2020-07-20@00:51 by Tim Long

using System;
using FakeItEasy;
using Machine.Specifications;
using Timtek.ReactiveCommunications.Specifications.Contexts;
using Timtek.ReactiveCommunications;

// ReSharper disable ComplexConditionExpression

namespace Timtek.ReactiveCommunications.Specifications
    {
    [Subject(typeof(SerialCommunicationChannel), "Construction")]
    public class when_constructing_a_serial_communication_channel :
        with_serial_channel_on_endpoint_com1_9600_n_8_1_and_mock_serial_port
        {
        Because of = () => { };
        It should_not_connect_automatically = () => A.CallTo(() => Channel.Port.Open()).MustNotHaveHappened();
        It should_save_the_endpoint =
            () => Channel.endpoint.ShouldBeTheSameAs(DeviceEndpoint);
        }

    [Subject(typeof(SerialCommunicationChannel), "construction")]
    public class when_constructing_a_serial_channel_with_the_wrong_type_of_endpoint
        {
        static SerialCommunicationChannel Channel;
        static DeviceEndpoint DeviceEndpoint;
        static Exception Thrown;
        Establish context = () =>
            DeviceEndpoint = new NetworkDeviceEndpoint("dummy", 8080);
        Because of =
            () => Thrown = Catch.Exception(() => Channel = new SerialCommunicationChannel(DeviceEndpoint));
        It should_throw_argument_exception = () => Thrown.ShouldBeOfExactType<ArgumentException>();
        }

    [Subject(typeof(SerialCommunicationChannel), "Connecting")]
    public class when_the_serial_channel_is_opened :
        with_serial_channel_on_endpoint_com1_9600_n_8_1_and_mock_serial_port
        {
        Because of = () => Channel.Open();
        It should_configure_the_port_with_the_expected_baud_rate =
            () => Channel.Port.BaudRate.ShouldEqual(SerialEndpoint.BaudRate);
        It should_configure_the_port_with_the_expected_data_bits =
            () => Channel.Port.DataBits.ShouldEqual(SerialEndpoint.DataBits);
        It should_configure_the_port_with_the_expected_parity =
            () => Channel.Port.Parity.ShouldEqual(SerialEndpoint.Parity);
        It should_configure_the_port_with_the_expected_port_name =
            () => Channel.Port.PortName.ShouldEqual(SerialEndpoint.PortName);
        It should_configure_the_port_with_the_expected_stop_bits =
            () => Channel.Port.StopBits.ShouldEqual(SerialEndpoint.StopBits);
        It should_open_the_serial_port =
            () => A.CallTo(() => MockPort.Open()).MustHaveHappenedOnceExactly();
        }

    [Subject(typeof(SerialCommunicationChannel), "Disconnecting")]
    public class when_the_serial_channel_is_closed :
        with_serial_channel_on_endpoint_com1_9600_n_8_1_and_mock_serial_port
        {
        Because of = () => Channel.Close();
        It should_close_the_serial_port =
            () => A.CallTo(() => MockPort.Close()).MustHaveHappenedOnceExactly();
        }

    [Subject(typeof(SerialCommunicationChannel), "Disposing")]
    public class when_the_serial_channel_is_disposed :
        with_serial_channel_on_endpoint_com1_9600_n_8_1_and_mock_serial_port
        {
        Establish context = () => A.CallTo(() => MockPort.IsOpen).Returns(true);
        Because of = () =>
            {
            Channel.Open();
            Channel.Dispose();
            Thrown = Catch.Exception(() => Channel.Dispose());
            };
        It should_close_the_serial_port =
            () => A.CallTo(() => MockPort.Close()).MustHaveHappenedOnceExactly();
        It should_dispose_the_serial_port =
            () => A.CallTo(() => MockPort.Dispose()).MustHaveHappenedOnceExactly();
        It should_not_throw_when_disposed_twice = () => Thrown.ShouldBeNull();
        }

    [Subject(typeof(SerialCommunicationChannel), "Sending")]
    public class when_a_string_is_sent_to_the_serial_channel :
        with_serial_channel_on_endpoint_com1_9600_n_8_1_and_mock_serial_port
        {
        const string TestString = "Test String";
        Establish context = () =>
            {
            A.CallTo(() => MockPort.IsOpen).Returns(true);
            Channel.Open();
            };
        Because of = () => Channel.Send(TestString);
        It should_send_the_string_to_the_serial_port = () =>
            A.CallTo(() => MockPort.Write(A<string>.That.Matches(s => s == TestString)))
                .MustHaveHappenedOnceExactly();
        }
    }