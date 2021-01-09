// This file is part of the TA.Ascom.ReactiveCommunications project
// 
// Copyright © 2015-2020 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: NetworkDeviceEndpoint.cs  Last modified: 2020-07-20@00:50 by Tim Long

using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using TA.Ascom.ReactiveCommunications.Diagnostics;
using TA.Utils.Core.Diagnostics;

namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>Represents a network endpoint, with a hostaddress and port number.</summary>
    internal class NetworkDeviceEndpoint : DeviceEndpoint
        {
        private const string NetworkIPv4Pattern =
            @"^(?<Host>((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])))(\:(?<Port>\d{1,5}))?";
        private const string NetworkHostPattern =
            @"^(?<Host>[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*(?:\.[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*)*)(\:(?<Port>\d{1,5}))?";
        private static readonly Regex NetworkIPv4Regex = new Regex(NetworkIPv4Pattern, Options);
        private static readonly Regex NetworkHostRegex = new Regex(NetworkHostPattern, Options);
        private static readonly ILog Log = ServiceLocator.LogService;

        /// <summary>Initializes a new instance of the <see cref="NetworkDeviceEndpoint" /> class.</summary>
        /// <param name="host">The network host name or IPv4 address.</param>
        /// <param name="port">The TCP or UDP port number.</param>
        public NetworkDeviceEndpoint(string host, int port)
            {
            Contract.Requires(!string.IsNullOrWhiteSpace(host));
            Contract.Requires(port >= 0 && port <= 65535);
            Host = host;
            Port = port;
            }

        /// <summary>Gets the TCP/UDP port number of the endpoint.</summary>
        /// <value>The port number.</value>
        public int Port { get; }

        /// <summary>Gets the network host name or IPv4 address of the endpoint.</summary>
        /// <value>The host.</value>
        public string Host { get; }

        /// <summary>Returns a <see cref="System.String" /> that represents this instance.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
            {
            Contract.Ensures(Contract.Result<string>() != null);
            return string.Format("{0}:{1}", Host, Port);
            }

        public static DeviceEndpoint FromConnectionString(string connection)
            {
            Contract.Requires(!string.IsNullOrEmpty(connection),
                "connection string was null or empty");
            Contract.Ensures(Contract.Result<DeviceEndpoint>() != null);
            if (NetworkHostRegex.IsMatch(connection))
                return CreateNetworkEndpoint(connection, NetworkHostRegex);
            if (NetworkIPv4Regex.IsMatch(connection))
                return CreateNetworkEndpoint(connection, NetworkIPv4Regex);
            Log.Warn("Connection string is for an unsupported endpoint, returning InvalidEndpoint");
            return new InvalidEndpoint();
            }

        private static DeviceEndpoint CreateNetworkEndpoint(string connection, Regex regex)
            {
            Contract.Requires(!string.IsNullOrWhiteSpace(connection));
            Contract.Requires(regex != null);
            Contract.Ensures(Contract.Result<DeviceEndpoint>() != null);
            var matches = regex.Match(connection);
            var host = matches.Groups["Host"].Value; // IP address or host name
            var port = CaptureGroupOrDefault(matches, "Port", 80);
            var endpoint = new NetworkDeviceEndpoint(host, port);
            return endpoint;
            }
        }
    }