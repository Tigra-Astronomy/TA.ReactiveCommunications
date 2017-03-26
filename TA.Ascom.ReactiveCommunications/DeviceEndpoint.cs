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
// File: DeviceEndpoint.cs  Last modified: 2015-05-27@20:12 by Tim Long

using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO.Ports;
using System.Text.RegularExpressions;
using NLog;

namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>
    ///     Represents a device specific address that describes a connection to a device. This class cannot be instantiated and
    ///     must be inherited.
    /// </summary>
    [Serializable]
    public abstract class DeviceEndpoint
        {
        const string SerialPortPattern =
            @"^(?<PortName>(COM|com)\d{1,3})(:((?<Baud>\d{3,7})(,(?<Parity>None|Even|Odd|Mark|Space))?(,(?<DataBits>7|8))?(,(?<StopBits>Zero|OnePointFive|One|Two))?(,(?<DTR>nodtr|dtr))?(,(?<RTS>norts|rts))?(,(?<Handshake>None|XOnXOff|RequestToSend|RequestToSendXOnXOff))?)?)?$";
        const string NetworkIPv4Pattern =
            @"^(?<Host>((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])))(\:(?<Port>\d{1,5}))?";
        const string NetworkHostPattern =
            @"^(?<Host>[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*(?:\.[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*)*)(\:(?<Port>\d{1,5}))?";
        const RegexOptions Options =
            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline |
            RegexOptions.IgnoreCase | RegexOptions.Compiled;
        static readonly Regex SerialRegex = new Regex(SerialPortPattern, Options);
        static readonly Regex NetworkIPv4Regex = new Regex(NetworkIPv4Pattern, Options);
        static readonly Regex NetworkHostRegex = new Regex(NetworkHostPattern, Options);
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Gets the device address. The address format is device specific.
        ///     For example, for an Ethernet connection, it could be a DNS host name or an IP address.
        ///     For a serial port, it would be the name of the port.
        /// </summary>
        /// <value>The device address.</value>
        public string DeviceAddress
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return ToString();
            }
        }

        /// <summary>
        ///     Creates an endpoint from a connection string. Connection strings may be in one of the following formats:
        ///     <list type="number">
        ///         <item>
        ///             <term>
        ///                 <c>COM127:115200,Even,7,One,DTR,NoRTS</c>
        ///             </term>
        ///             <description>
        ///                 A named serial port endpoint. The "COMx" must be present and <c>x</c> must be an integer between 1 and
        ///                 127, inclusive. The colon must be present if any parameters are specified, otherwise it may be omitted.
        ///                 Individual parameters may be omitted completely, but if they are present
        ///                 then they must be in the order shown and separated by a single comma and no spaces. Omitted parameters
        ///                 will have their default values, which are: <c>9600,None,8,One,DTR,RTS</c>. Parameters are
        ///                 not case sensitive. The allowed parameters are as follows:
        ///                 <list type="table">
        ///                     <item>
        ///                         <term>Baud Rate (integer)</term>
        ///                         <definition>
        ///                             The transmission speed (in bits per second) of the serial port.
        ///                             Valid values depend on the hardware configuration but typically include 300, 1200, 9600,
        ///                             19200, 115200 and possibly others.
        ///                         </definition>
        ///                     </item>
        ///                     <item>
        ///                         <term>Parity (enumeration)</term>
        ///                         <description>
        ///                             The type of parity bit; one of the following values: None,
        ///                             Even, Odd, Mark, Space
        ///                         </description>
        ///                     </item>
        ///                     <item>
        ///                         <term>Data Bits (integer)</term>
        ///                         <description>The number of data bits per character; must be 7 or 8.</description>
        ///                     </item>
        ///                     <item>
        ///                         <term>Stop Bits (enumeration)</term>
        ///                         <definition>
        ///                             The number of stop bits per character; one of the follwing values: Zero, One,
        ///                             OnePointFive, Two.
        ///                         </definition>
        ///                     </item>
        ///                     <item>
        ///                         <term>DTR signal (enumeration)</term>
        ///                         <definition>
        ///                             When <c>DTR</c> is specified or the parameter omitted, then the Data Terminal Ready
        ///                             signal is asserted. When specified as <c>NoDTR</c>, then Data Terminal Ready is negated.
        ///                         </definition>
        ///                     </item>
        ///                     <item>
        ///                         <term>RTS signal (enumeration)</term>
        ///                         <definition>
        ///                             When <c>RTS</c> is specified or the parameter omitted, then the Request To Send signal
        ///                             is asserted. When specified as <c>NoRTS</c>, then Request To Send is negated.
        ///                         </definition>
        ///                     </item>
        ///                 </list>
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term><c>hostname[:port]</c> or <c>ip-address[:port]</c></term>
        ///             <description>
        ///                 A host name, or IP address, followed by an option port number. If the port number is
        ///                 specified, then the colon must be present, otherwise it must be omitted.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="connection">The connection string.</param>
        /// <returns>A <see cref="DeviceEndpoint" /> constructed from the connection string.</returns>
        /// <exception cref="NotSupportedException">The connection string is for an unsupported connection type.</exception>
        /// <exception cref="ArgumentException">Invalid connection string syntax</exception>
        public static DeviceEndpoint FromConnectionString(string connection)
            {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(connection),
                "connection string was null or empty");
            Contract.Ensures(Contract.Result<DeviceEndpoint>() != null);
            if (SerialRegex.IsMatch(connection))
                return CreateSerialEndpoint(connection); // It's complicated, so delegate it.
            if (NetworkHostRegex.IsMatch(connection))
                return CreateNetworkEndpoint(connection, NetworkHostRegex);
            if (NetworkIPv4Regex.IsMatch(connection))
                return CreateNetworkEndpoint(connection, NetworkIPv4Regex);
            Log.Warn("Connection string is for an unsupported endpoint, returning InvalidEndpoint");
            return new InvalidEndpoint();
            }

        static DeviceEndpoint CreateNetworkEndpoint(string connection, Regex regex)
            {
            Contract.Requires(!string.IsNullOrWhiteSpace(connection));
            Contract.Requires(regex != null);
            Contract.Ensures(Contract.Result<DeviceEndpoint>() != null);
            var matches = regex.Match(connection);
            var host = matches.Groups["Host"].Value; // IP address or host name
            var port = CaptureOrDefaultValueAs(matches, "Port", 80);
            var endpoint = new NetworkDeviceEndpoint(host, port);
            return endpoint;
            }

        static DeviceEndpoint CreateSerialEndpoint(string connectionString)
            {
            Contract.Requires(!string.IsNullOrWhiteSpace(connectionString));
            Contract.Ensures(Contract.Result<DeviceEndpoint>() != null);
            var matches = SerialRegex.Match(connectionString);
            if (!matches.Success)
                throw new ArgumentException(
                    "Not a valid serial connection string; Example: COM127:9600,None,8,One,dtr,norts",
                    "connectionString");
            var portName = matches.Groups["PortName"].Value;
            var baud = CaptureOrDefaultValueAs(matches, "Baud", 9600);
            var parity = CaptureOrDefaultValueAs(matches, "Parity", Parity.None);
            var databits = CaptureOrDefaultValueAs(matches, "DataBits", 8);
            var stopbits = CaptureOrDefaultValueAs(matches, "StopBits", StopBits.One);
            var assertDtr = CaptureOrDefaultValueAs(matches, "DTR", "dtr")
                .Equals("dtr", StringComparison.InvariantCultureIgnoreCase);
            var assertRts = CaptureOrDefaultValueAs(matches, "RTS", "rts")
                .Equals("rts", StringComparison.InvariantCultureIgnoreCase);
            Handshake handshake = CaptureOrDefaultValueAs<Handshake>(matches, "Handshake", Handshake.None);
            var endpoint = new SerialDeviceEndpoint(portName, baud, parity, databits, stopbits, assertDtr, assertRts, handshake);
            return endpoint;
            }

        static TResult CaptureOrDefaultValueAs<TResult>(Match matches, string groupName, TResult defaultValue)
            {
            Contract.Requires(matches != null);
            Contract.Requires(groupName != null);
            Contract.Requires(defaultValue != null);
            if (matches.Groups[groupName].Success == false) return defaultValue;
            var capture = matches.Groups[groupName].Value;
            try
                {
                TResult result = ConvertFromString<TResult>(capture);
                return result;
                }
            catch (NotSupportedException ex)
                {
                var message =
                    string.Format(
                        "The parameter '{0}' with value '{1}' could not be parsed. See InnerException for details.",
                        groupName,
                        capture);
                throw new ArgumentException(message, ex);
                }
            }

        static TResult ConvertFromString<TResult>(string valueString)
            {
            Contract.Requires(!string.IsNullOrWhiteSpace(valueString));
            var valueType = typeof (string);
            var resultType = typeof (TResult);
            // look for a type converter that can convert from string to the specified type.
            var converter = TypeDescriptor.GetConverter(resultType);
            var canConvertFrom = converter.CanConvertFrom(valueType);
            if (!canConvertFrom)
                converter = TypeDescriptor.GetConverter(valueType);
            var culture = CultureInfo.InvariantCulture;
            var convertedValue = (canConvertFrom)
                ? converter.ConvertFrom(null, culture, valueString)
                : converter.ConvertTo(null /* context */, culture, valueString, resultType);
            return (TResult) convertedValue;
            }
        }
    }