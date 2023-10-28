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
// File: SerialPort.cs  Last modified: 2020-07-20@00:51 by Tim Long

using System;

namespace Timtek.ReactiveCommunications;

/// <summary>
///     This class wraps the <see cref="System.IO.Ports.SerialPort" /> class and provides additional
///     behaviours such as diagnostics. It also acts as a vehicle for implementing the
///     <see cref="ISerialPort" /> interface, to assist with dependency injection and unit testing.
/// </summary>
public sealed class SerialPort : System.IO.Ports.SerialPort, ISerialPort
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SerialPort" /> class and sets the
    ///     <see cref="NewLine" /> property to ASCII Carriage Return.
    /// </summary>
    public SerialPort()
    {
        NewLine = Environment.NewLine;
    }

    #region ISerialPort Members
    /// <summary>Gets or sets the line terminator string.</summary>
    public new string NewLine { get; set; }
    #endregion
}