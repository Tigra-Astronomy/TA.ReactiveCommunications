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
// File: RegisteredChannelBuilder.cs  Last modified: 2020-07-20@00:51 by Tim Long

using System;

namespace Timtek.ReactiveCommunications;

/// <summary>
///     A set of methods for building an <see cref="ICommunicationChannel" /> from a connection
///     string
/// </summary>
public class RegisteredChannelBuilder
{
    /// <summary>
    ///     A predicate that determines whether the supplied connection string can be used to create a
    ///     channel for this registered device.
    /// </summary>
    public Predicate<string> IsConnectionStringValid { get; internal set; }

    /// <summary>
    ///     Gets an instance of a class derived from <see cref="DeviceEndpoint" /> that is appropriate for
    ///     the registered device.
    /// </summary>
    public Func<string, DeviceEndpoint> EndpointFromConnectionString { get; internal set; }

    /// <summary>
    ///     Creates and returns an implementation of <see cref="ICommunicationChannel" /> that is
    ///     appropriate for the registered device.
    /// </summary>
    public Func<DeviceEndpoint, ICommunicationChannel> ChannelFromEndpoint { get; internal set; }
}