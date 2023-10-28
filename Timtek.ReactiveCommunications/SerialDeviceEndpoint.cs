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
// File: SerialDeviceEndpoint.cs  Last modified: 2020-07-20@00:51 by Tim Long

using System;
using System.Diagnostics.Contracts;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;

namespace Timtek.ReactiveCommunications;

/// <summary>
///     Class SerialDeviceEndpoint. Represents a serial port endpoint with a comm port name, data
///     (baud) rate, parity type, number of data bits and number of stop bits.
/// </summary>
public class SerialDeviceEndpoint : DeviceEndpoint
{
    private const string SerialPortPattern =
        @"^(?<PortName>(COM|com)\d{1,3})(:((?<Baud>\d{3,7})(,(?<Parity>None|Even|Odd|Mark|Space))?(,(?<DataBits>7|8))?(,(?<StopBits>Zero|OnePointFive|One|Two))?(,(?<DTR>nodtr|dtr))?(,(?<RTS>norts|rts))?(,(?<Handshake>None|XOnXOff|RequestToSend|RequestToSendXOnXOff))?)?)?$";
    private static readonly Regex SerialRegex = new Regex(SerialPortPattern, Options);

    /// <summary>Initializes a new instance of the <see cref="SerialDeviceEndpoint" /> class.</summary>
    /// <param name="portName">Name of the port, COMx where x is an integer.</param>
    /// <param name="baudRate">The baud rate. Optional; default is 9600.</param>
    /// <param name="parity">
    ///     The parity bit type. Optional; default is
    ///     <see cref="System.IO.Ports.Parity.None" />.
    /// </param>
    /// <param name="dataBits">The number of data bits. Optional; default is 8.</param>
    /// <param name="stopBits">The number stop bits. Optional; default is 1.</param>
    /// <param name="dtrEnable">
    ///     Indicates whether the DTR signal should be asserted or negated. Optional; default is
    ///     <see langword="true" />.
    /// </param>
    /// <param name="rtsEnable">
    ///     Indicates whether the RTS signal should be asserted or negated. Optional; default is
    ///     <see langword="true" />
    /// </param>
    /// <param name="handshake">The serial handshake method (default: None)</param>
    public SerialDeviceEndpoint(string portName,
        int                            baudRate  = 9600,
        Parity                         parity    = Parity.None,
        int                            dataBits  = 8,
        StopBits                       stopBits  = StopBits.One, bool dtrEnable = true, bool rtsEnable = true,
        Handshake                      handshake = Handshake.None)
    {
        Contract.Requires(!string.IsNullOrWhiteSpace(portName));
        PortName = portName;
        BaudRate = baudRate;
        Parity = parity;
        DataBits = dataBits;
        StopBits = stopBits;
        DtrEnable = dtrEnable;
        RtsEnable = rtsEnable;
        Handshake = handshake;
        Encoding = Encoding.UTF8;
    }

    /// <summary>Gets or sets the serial port text encoding (default is UTF-8).</summary>
    public Encoding Encoding { get; set; }

    /// <summary>Gets or sets the serial handshake method (default: None)</summary>
    public Handshake Handshake { get; set; }

    /// <summary>
    ///     Gets the name of the port being used. The port name is typically in the form <c>COMx</c> where
    ///     x is an integer. However, Windows will also accept a UNC path format of <c>\\.\COMx</c>
    /// </summary>
    /// <value>The name of the port.</value>
    public string PortName { get; private set; }

    /// <summary>Gets the configured baud rate. For serial ports, 1 Baud equals 1 bit per second.</summary>
    /// <value>The baud rate.</value>
    public int BaudRate { get; private set; }

    /// <summary>Gets the parity algorithm.</summary>
    /// <value>The parity.</value>
    public Parity Parity { get; private set; }

    /// <summary>Gets the configured number of data bits.</summary>
    /// <value>The data bits.</value>
    public int DataBits { get; private set; }

    /// <summary>Gets the stop bits configuration.</summary>
    /// <value>The stop bits (enumerated value).</value>
    public StopBits StopBits { get; private set; }

    /// <summary>Gets a value indicating whether the RTS (Request To Send) signal is enabled.</summary>
    /// <value><c>true</c> if RTS is enabled; otherwise, <c>false</c>.</value>
    public bool RtsEnable { get; private set; }

    /// <summary>Gets a value indicating whether DTR (Data Terminal Ready) signal is enabled.</summary>
    /// <value><c>true</c> if DTR is enabled; otherwise, <c>false</c>.</value>
    public bool DtrEnable { get; private set; }

    /// <summary>Returns a <see cref="System.String" /> that represents this instance.</summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        Contract.Ensures(Contract.Result<string>() != null);
        var dtr = DtrEnable ? "dtr" : "nodtr";
        var rts = RtsEnable ? "rts" : "norts";
        return $"{PortName}:{BaudRate},{Parity},{DataBits},{StopBits},{dtr},{rts}";
    }

    /// <summary>Creates a SerialDeviceEndpoint from a valid connection string.</summary>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>A populated <see cref="SerialDeviceEndpoint" />.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <remarks>
    ///     <para>
    ///         Valid connection strings must match the following regular expression grammar:
    ///         <code lang="regex">
    ///         ^((COM|com)\d{1,3})(:((\d{3,7})(,(None|Even|Odd|Mark|Space))?(,(7|8))?(,(Zero|OnePointFive|One|Two))?(,(nodtr|dtr))?(,(norts|rts))?(,(None|XOnXOff|RequestToSend|RequestToSendXOnXOff))?)?)?$
    ///         </code>
    ///     </para>
    ///     <para>
    ///         Since that is a rather unfriendly expression to human eyes, one way to determine correct
    ///         connection string syntax is to create a specimen endpoint, passing in the required
    ///         configuration as constructor parameters. Then, write the object to an output stream. The
    ///         ToString method will write out a syntactically correct connection string. There are many
    ///         ways to accomplish this. For example, you can use the C# Interactive window in Visual
    ///         Studio, write a unit test, use Windows PowerShell or an utility such as LINQPad. In
    ///         LINQPad, the following fragment will suffice:
    ///         <code lang="csharp">
    ///             var endpoint = new SerialDeviceEndpoint("COM1"); // Pass in whatever parameters you need
    ///             endpoint.Dump();
    ///         </code>
    ///     </para>
    ///     <para>
    ///         It is probably best to avoid creating connection strings directly as this will tend to be
    ///         error prone, especially if it is from user-supplied input. Instead, create a user interface
    ///         that gathers all of the serial parameters from the user. For an ASCOM driver, this will
    ///         likely be part of the Setup Dialog. Save all the user-supplied data in your settings. Then
    ///         generate a connection string by creating a <see cref="SerialDeviceEndpoint" />, passing the
    ///         user settings in as constructor parameters. Finally, use <see cref="ToString" /> or the
    ///         <see cref="DeviceEndpoint.DeviceAddress" /> property to retrieve the corresponding
    ///         connection string. The connection string can either be saved along with user settings and
    ///         used 'as-is' in the next session, or it can be dynamically regenerated on demand using the
    ///         above technique - the choice is yours. It is also up to you whether or not you decide to
    ///         make the connection string visible to the user.
    ///     </para>
    /// </remarks>
    public static SerialDeviceEndpoint FromConnectionString(string connectionString)
    {
        Contract.Requires(!string.IsNullOrWhiteSpace(connectionString));
        Contract.Ensures(Contract.Result<DeviceEndpoint>() != null);
        var matches = SerialRegex.Match(connectionString);
        if (!matches.Success)
            throw new ArgumentException(
                                        "Not a valid serial connection string; Example: COM127:9600,None,8,One,dtr,norts",
                                        nameof(connectionString));
        var portName = matches.Groups["PortName"].Value;
        var baud = CaptureGroupOrDefault(matches,     "Baud",     9600);
        var parity = CaptureGroupOrDefault(matches,   "Parity",   Parity.None);
        var databits = CaptureGroupOrDefault(matches, "DataBits", 8);
        var stopbits = CaptureGroupOrDefault(matches, "StopBits", StopBits.One);
        var assertDtr = CaptureGroupOrDefault(matches, "DTR", "dtr")
            .Equals("dtr", StringComparison.InvariantCultureIgnoreCase);
        var assertRts = CaptureGroupOrDefault(matches, "RTS", "rts")
            .Equals("rts", StringComparison.InvariantCultureIgnoreCase);
        var handshake = CaptureGroupOrDefault(matches, "Handshake", Handshake.None);
        var endpoint = new SerialDeviceEndpoint(portName, baud, parity, databits, stopbits, assertDtr, assertRts,
                                                handshake);
        return endpoint;
    }

    public static bool IsValidConnectionString(string connectionString)
    {
        return SerialRegex.IsMatch(connectionString);
    }
}