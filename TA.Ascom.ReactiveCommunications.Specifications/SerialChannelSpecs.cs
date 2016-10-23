// This file is part of the TA.Ascom.ReactiveCommunications project
// 
// Copyright © 2016 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so,. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: SerialChannelSpecs.cs  Last modified: 2016-10-23@23:36 by Tim Long

using System;
using System.IO.Ports;
using FakeItEasy;
using Machine.Specifications;
using TA.Ascom.ReactiveCommunications.Specifications.Contexts;
using TA.Ascom.ReactiveCommunications.Specifications.Fakes;
using TA.Ascom.ReactiveCommunications.Specifications.Helpers;

// ReSharper disable ComplexConditionExpression

namespace TA.Ascom.ReactiveCommunications.Specifications
    {
    [Subject(typeof(SerialCommunicationChannel), "Construction")]
    public class when_constructing_a_serial_communication_channel :
        with_serial_channel_on_endpoint_com1_9600_n_8_1_and_mock_serial_port
        {
        private Because of = () => { };
        private It should_not_connect_automatically = () => A.CallTo(() => Channel.Port.Open()).MustNotHaveHappened();
        private It should_save_the_endpoint =
            () => Channel.endpoint.ShouldBeTheSameAs(DeviceEndpoint);
        }

    [Subject(typeof(SerialCommunicationChannel), "construction")]
    public class when_constructing_a_serial_channel_with_the_wrong_type_of_endpoint
        {
        private static SerialCommunicationChannel Channel;
        private static DeviceEndpoint DeviceEndpoint;
        private static Exception Thrown;
        private Establish context = () =>
                DeviceEndpoint = new NetworkDeviceEndpoint("dummy", 8080);
        private Because of =
            () => Thrown = Catch.Exception(() => Channel = new SerialCommunicationChannel(DeviceEndpoint));
        private It should_throw_argument_exception = () => Thrown.ShouldBeOfExactType<ArgumentException>();
        }

    [Subject(typeof(SerialCommunicationChannel), "Connecting")]
    public class when_the_serial_channel_is_opened :
        with_serial_channel_on_endpoint_com1_9600_n_8_1_and_mock_serial_port
        {
        private Because of = () => Channel.Open();
        private It should_configure_the_port_with_the_expected_baud_rate =
            () => Channel.Port.BaudRate.ShouldEqual(SerialEndpoint.BaudRate);
        private It should_configure_the_port_with_the_expected_data_bits =
            () => Channel.Port.DataBits.ShouldEqual(SerialEndpoint.DataBits);
        private It should_configure_the_port_with_the_expected_parity =
            () => Channel.Port.Parity.ShouldEqual(SerialEndpoint.Parity);
        private It should_configure_the_port_with_the_expected_port_name =
            () => Channel.Port.PortName.ShouldEqual(SerialEndpoint.PortName);
        private It should_configure_the_port_with_the_expected_stop_bits =
            () => Channel.Port.StopBits.ShouldEqual(SerialEndpoint.StopBits);
        private It should_open_the_serial_port =
            () => A.CallTo(() => MockPort.Open()).MustHaveHappened(Repeated.Exactly.Once);
        }

    [Subject(typeof(SerialCommunicationChannel), "Disconnecting")]
    public class when_the_serial_channel_is_closed :
        with_serial_channel_on_endpoint_com1_9600_n_8_1_and_mock_serial_port
        {
        private Because of = () => Channel.Close();
        private It should_close_the_serial_port =
            () => A.CallTo(() => MockPort.Close()).MustHaveHappened(Repeated.Exactly.Once);
        }

    [Subject(typeof(SerialCommunicationChannel), "Disposing")]
    public class when_the_serial_channel_is_disposed :
        with_serial_channel_on_endpoint_com1_9600_n_8_1_and_mock_serial_port
        {
        private Establish context = () => A.CallTo(() => MockPort.IsOpen).Returns(true);
        private Because of = () =>
            {
            Channel.Open();
            Channel.Dispose();
            Thrown = Catch.Exception(() => Channel.Dispose());
            };
        private It should_close_the_serial_port =
            () => A.CallTo(() => MockPort.Close()).MustHaveHappened(Repeated.Exactly.Once);
        private It should_dispose_the_serial_port =
            () => A.CallTo(() => MockPort.Dispose()).MustHaveHappened(Repeated.Exactly.Once);
        private It should_not_throw_when_disposed_twice = () => Thrown.ShouldBeNull();
        }

    [Subject(typeof(SerialCommunicationChannel), "Sending")]
    public class when_a_string_is_sent_to_the_serial_channel :
        with_serial_channel_on_endpoint_com1_9600_n_8_1_and_mock_serial_port
        {
        private const string TestString = "Test String";
        private Establish context = () =>
            {
            A.CallTo(() => MockPort.IsOpen).Returns(true);
            Channel.Open();
            };
        private Because of = () => Channel.Send(TestString);
        private It should_send_the_string_to_the_serial_port = () =>
            A.CallTo(() => MockPort.Write(A<string>.That.Matches(s => s == TestString)))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

    [Ignore("Upgrading FakeItEasy broke the event handling")]
    [Subject(typeof(SerialCommunicationChannel), "Loopback test")]
    internal class when_sending_and_receiving_over_a_serial_channel :
        with_serial_channel_on_endpoint_com1_9600_n_8_1_and_mock_serial_port
        {
        private const string ExpectedResponse = "Pong";
        private const string Command = "Ping";
        private static ReactiveTransactionProcessor Processor;
        private static BufferInputUntilCompletedTransaction Transaction;
        private static TransactionObserver TransactionObserver;
        private Establish context = () =>
            {
            A.CallTo(() => MockPort.ReadExisting()).Returns(ExpectedResponse);
            A.CallTo(() => MockPort.Write(A<string>.Ignored)).Invokes(() =>
                {
                var eventArgs = MockHelpers.CreateSerialDataReceivedEventArgs(SerialData.Chars);
                MockPort.DataReceived += Raise.With<SerialDataReceivedEventHandler>(MockPort, eventArgs);
                });
            TransactionObserver = new TransactionObserver(Channel);
            Processor = new ReactiveTransactionProcessor();
            Processor.SubscribeTransactionObserver(TransactionObserver);
            Channel.Open();
            };
        private Because of = () =>
            {
            Transaction = new BufferInputUntilCompletedTransaction(Command, completeAfter: 4);
            Processor.CommitTransaction(Transaction);
            Transaction.WaitForCompletionOrTimeout();
            };
        private It should_receive_pong = () => Transaction.Value.ShouldEqual(ExpectedResponse);
        }
    }