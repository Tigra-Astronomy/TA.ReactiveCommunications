// This file is part of the TA.DigitalDomeworks project
// 
// Copyright © 2016-2018 Tigra Astronomy, all rights reserved.
// 
// File: RxAscomContextBuilder.cs  Last modified: 2018-03-08@19:17 by Tim Long

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TA.Ascom.ReactiveCommunications.Specifications.Fakes;

namespace TA.Ascom.ReactiveCommunications.Specifications.Contexts
    {
    internal class RxAscomContextBuilder
        {
        public RxAscomContextBuilder()
            {
            channelFactory = new ChannelFactory();
            channelFactory.RegisterChannelType(
                p => p.StartsWith("Fake", StringComparison.InvariantCultureIgnoreCase),
                connection => new FakeEndpoint(),
                endpoint => new FakeCommunicationChannel(fakeResponseBuilder.ToString())
            );
            }

        bool channelShouldBeOpen;
        readonly StringBuilder fakeResponseBuilder = new StringBuilder();
        readonly ChannelFactory channelFactory;
        string connectionString = "Fake";
        PropertyChangedEventHandler propertyChangedAction;
        List<Tuple<string,Action>> propertyChangeObservers = new List<Tuple<string, Action>>();
        IObservable<char> observableResponse;

        public RxAscomContext Build()
            {
            // Build the communications channel
            var channel = channelFactory.FromConnectionString(connectionString);
            var processor = new ReactiveTransactionProcessor();
            var observer = new TransactionObserver(channel);
            processor.SubscribeTransactionObserver(observer);
            if (channelShouldBeOpen)
                channel.Open();

            // Assemble the device controller test context
            var context = new RxAscomContext
                {
                Channel = channel,
                Processor= processor,
                Observer=observer
                };
            return context;
            }

        public RxAscomContextBuilder WithOpenConnection(string connection)
            {
            connectionString = connection;
            channelShouldBeOpen = true;
            return this;
            }

        public RxAscomContextBuilder WithFakeResponse(string fakeResponse)
            {
            fakeResponseBuilder.Append(fakeResponse);
            return this;
            }

        public RxAscomContextBuilder WithClosedConnection(string connection)
            {
            connectionString = connection;
            channelShouldBeOpen = false;
            return this;
            }

        public RxAscomContextBuilder OnPropertyChanged(PropertyChangedEventHandler action)
            {
            propertyChangedAction = action;
            return this;
            }

        public RxAscomContextBuilder WithObservableResponse(IObservable<char> observableResponse)
            {
            this.observableResponse = observableResponse;
            return this;
            }
        }
    }