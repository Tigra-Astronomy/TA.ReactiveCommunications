// This file is part of the TA.Ascom.ReactiveCommunications project
// 
// Copyright © 2015 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so,. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: NetworkDeviceEndpoint.cs  Last modified: 2015-05-27@20:12 by Tim Long

using System.Diagnostics.Contracts;

namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>
    ///     Represents a network endpoint, with a hostaddress
    ///     and port number.
    /// </summary>
    public class NetworkDeviceEndpoint : DeviceEndpoint
        {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NetworkDeviceEndpoint" /> class.
        /// </summary>
        /// <param name="host">The network host name or IPv4 address.</param>
        /// <param name="port">The TCP or UDP port number.</param>
        public NetworkDeviceEndpoint(string host, int port)
            {
            Contract.Requires(!string.IsNullOrWhiteSpace(host));
            Contract.Requires(port >= 0 && port <= 65535);
            Host = host;
            Port = port;
            }

        /// <summary>
        ///     Gets the TCP/UDP port number of the endpoint.
        /// </summary>
        /// <value>The port number.</value>
        public int Port { get; private set; }

        /// <summary>
        ///     Gets the network host name or IPv4 address of the endpoint.
        /// </summary>
        /// <value>The host.</value>
        public string Host { get; private set; }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
            {
            Contract.Ensures(Contract.Result<string>() != null);
            return string.Format("{0}:{1}", Host, Port);
            }
        }
    }