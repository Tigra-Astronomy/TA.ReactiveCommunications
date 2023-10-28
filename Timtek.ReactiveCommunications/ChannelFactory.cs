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
// File: ChannelFactory.cs  Last modified: 2020-07-20@00:50 by Tim Long

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using TA.Utils.Core.Diagnostics;
using Timtek.ReactiveCommunications.Diagnostics;

namespace Timtek.ReactiveCommunications;

/// <summary>
///     A factory for creating an <see cref="ICommunicationChannel" /> from a connection string. By
///     default, the factory knows how to create serial channels, but custom channel implementations
///     can be registered.
/// </summary>
public class ChannelFactory
{
    private readonly List<RegisteredChannelBuilder> deviceTypes = new List<RegisteredChannelBuilder>();
    private readonly ILog log;

    /// <summary>Creates a new instance with the builtin channel types pre-registered.</summary>
    public ChannelFactory([CanBeNull] ILog logger = null)
    {
        log = logger ?? ServiceLocator.LogService;
        ServiceLocator.LogService = log;
        RegisterDefaultChannelBuilders();
    }

    private void RegisterDefaultChannelBuilders()
    {
        var serialChannelFactory = new RegisteredChannelBuilder
        {
            IsConnectionStringValid = SerialDeviceEndpoint.IsValidConnectionString,
            EndpointFromConnectionString = SerialDeviceEndpoint.FromConnectionString,
            ChannelFromEndpoint = endpoint => new SerialCommunicationChannel(endpoint)
        };
        deviceTypes.Add(serialChannelFactory);
    }

    /// <summary>
    ///     Builds an appropriate <see cref="ICommunicationChannel" /> from the supplied connection
    ///     string.
    /// </summary>
    /// <param name="connection"></param>
    /// <returns></returns>
    public ICommunicationChannel FromConnectionString(string connection)
    {
        Contract.Requires(!string.IsNullOrEmpty(connection));
        log.Info().Message("Creating channel from connection string {connection}", connection);
        var deviceBuilder = ChannelBuilderForConnectionString(connection);
        var endpoint = deviceBuilder.EndpointFromConnectionString(connection);
        log.Debug().Message("Using endpoint type {endpoint}", endpoint.GetType());
        var channel = deviceBuilder.ChannelFromEndpoint(endpoint);
        log.Debug().Message("Using channel type {channel}", channel.GetType());
        return channel;
    }

    internal RegisteredChannelBuilder ChannelBuilderForConnectionString(string connection)
    {
        return deviceTypes.Single(p => p.IsConnectionStringValid(connection));
    }

    /// <summary>Removes all registered devices.</summary>
    public void ClearRegisteredDevices() => deviceTypes.Clear();

    /// <summary>Registers a user-supplied channel implementation with the channel factory.</summary>
    /// <param name="isConnectionStringValid">
    ///     A predicate that returns <c>true</c> if the supplied
    ///     connection string is valid for the channel.
    /// </param>
    /// <param name="createEndpoint">
    ///     A function that takes a connection string and returns a <see cref="DeviceEndpoint" />
    ///     appropriate to the channel.
    /// </param>
    /// <param name="createChannel">
    ///     A function that takes a <see cref="DeviceEndpoint" /> and creates and returns an
    ///     <see cref="ICommunicationChannel" /> implementation.
    /// </param>
    public void RegisterChannelType(
        Predicate<string>                           isConnectionStringValid,
        Func<string, DeviceEndpoint>                createEndpoint,
        Func<DeviceEndpoint, ICommunicationChannel> createChannel)
    {
        var registration = new RegisteredChannelBuilder
        {
            IsConnectionStringValid = isConnectionStringValid,
            EndpointFromConnectionString = createEndpoint,
            ChannelFromEndpoint = createChannel
        };
        deviceTypes.Add(registration);
    }
}