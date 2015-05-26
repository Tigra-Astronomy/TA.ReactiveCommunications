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
// File: DeviceEndpoint.cs  Last modified: 2015-05-25@18:22 by Tim Long

using System;

namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>
    ///     Class DeviceEndpoint - represents a device specific address used to communicate
    ///     with the hub controller.
    /// </summary>
    [Serializable]
    public abstract class DeviceEndpoint
        {
        /// <summary>
        ///     Gets the device address. The address format is device specific.
        ///     For example, for an ethernet connection, it could be a DNS host name or an IP address.
        ///     For a serial port, it would be the name of the commm port.
        /// </summary>
        /// <value>The device address.</value>
        public string DeviceAddress
            {
            get { return ToString(); }
            }

        /// <summary>
        ///     Creates an endpoint from a connection string.
        ///     Connection strings may be in one of the following formats:
        ///     <list type="number">
        ///         <item>
        ///             <term>COM3:[115200,n,8,1]</term>
        ///             <description>
        ///                 Specifies a serial port COM3; If the parameters are specified, then the colon must be present,
        ///                 otherwise it is optional. The parameters are currently ignored since Gemini uses a fixed configuration,
        ///                 this
        ///                 may change in the future.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>hostname[:port] or ip-address[:port]</term>
        ///             <description>
        ///                 A host name, or IP address, followed by an option port number. If the port number is specified, then
        ///                 the colon must be present, otherwise it must be omitted.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="connection">The connection string.</param>
        /// <returns>A <see cref="DeviceEndpoint" /> constructed from the connection string.</returns>
        /// <exception cref="System.ArgumentException">The connection string is invalid - it can only contain a single colon</exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="connection string was null or empty" /> is
        ///     <see langword="null" />.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     The array is multidimensional and contains more than
        ///     <see cref="F:System.Int32.MaxValue" /> elements.
        /// </exception>
        public static DeviceEndpoint FromConnectionString(string connection)
            {
            if (string.IsNullOrEmpty(connection))
                throw new ArgumentNullException("connection", "connection string was null or empty");
            var parts = connection.Split(':');
            var partCount = parts.Length;
            if (partCount > 2)
                throw new ArgumentException("The connection string is invalid - it can only contain a single colon");
            var scheme = parts[0].ToLowerInvariant();
            if (partCount == 2)
                {
                // ToDo - process Comm Port configuration parameters here
                }

            if (scheme.StartsWith("com"))
                return new SerialDeviceEndpoint(parts[0]); // Parameters not currently supported and are ignored.

            throw new ArgumentException("Unable to parse the connection string", "connection");
            }
        }
    }