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
// File: SimulatorCommunicationsChannel.cs  Last modified: 2020-07-20@00:50 by Tim Long

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using SimulatorChannel.StateMachine;
using TA.Utils.Core.Diagnostics;
using Timtek.ReactiveCommunications;

namespace SimulatorChannel
    {
    /// <summary>Provides a direct in-memory communications link to the hardware simulator.</summary>
    public class SimulatorCommunicationsChannel : ICommunicationChannel
        {
        private readonly ILog logger;
        private readonly SimulatorStateMachine simulator;

        /// <summary>Creates a simulator communications channel from a valid endpoint.</summary>
        /// <param name="endpoint">A valid simulator endpoint.</param>
        public SimulatorCommunicationsChannel(SimulatorEndpoint endpoint, ILog logger)
            {
            Contract.Requires(endpoint != null);
            this.logger = logger;
            ServiceLocator.LoggingService = logger;
            Endpoint = endpoint;
            var configuration = new SimulatorConfiguration {Realtime = endpoint.Realtime};
            simulator = new SimulatorStateMachine(new SystemDateTimeUtcClock(), configuration, logger);
            }

        /// <summary>Keeps a log of all commands sent to the simulator.</summary>
        public List<string> SendLog { get; } = new List<string>();

        /// <inheritdoc />
        public void Dispose()
            {
            simulator?.InputObserver.OnCompleted();
            }

        /// <inheritdoc />
        public void Open()
            {
            IsOpen = true;
            }

        /// <inheritdoc />
        public void Close()
            {
            IsOpen = false;
            }

        /// <inheritdoc />
        public void Send(string txData)
            {
            SendLog.Add(txData);
            foreach (var c in txData) simulator.InputObserver.OnNext(c);
            }

        /// <inheritdoc />
        public IObservable<char> ObservableReceivedCharacters => simulator.ObservableResponses;

        /// <inheritdoc />
        public bool IsOpen { get; private set; }

        /// <inheritdoc />
        public DeviceEndpoint Endpoint { get; }
        }
    }