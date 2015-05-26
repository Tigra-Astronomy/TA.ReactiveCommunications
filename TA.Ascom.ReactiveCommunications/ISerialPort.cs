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
// File: ISerialPort.cs  Last modified: 2015-05-25@18:23 by Tim Long

using System;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Runtime.Remoting;
using System.Text;

#pragma warning disable 1591    // Prevent warnings for missing XML comments

namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>
    ///     Mirrors the <see cref="System.IO.Ports.SerialPort" /> class and serves as a vehicle for dependency injection,
    ///     allowing the serial port to be faked.
    /// </summary>
    public interface ISerialPort
        {
        Stream BaseStream { get; }

        int BaudRate { get; set; }

        bool BreakState { get; set; }

        int BytesToWrite { get; }

        int BytesToRead { get; }

        bool CDHolding { get; }

        bool CtsHolding { get; }

        int DataBits { get; set; }

        bool DiscardNull { get; set; }

        bool DsrHolding { get; }

        bool DtrEnable { get; set; }

        Encoding Encoding { get; set; }

        Handshake Handshake { get; set; }

        bool IsOpen { get; }

        string NewLine { get; set; }

        Parity Parity { get; set; }

        byte ParityReplace { get; set; }

        string PortName { get; set; }

        int ReadBufferSize { get; set; }

        int ReadTimeout { get; set; }

        int ReceivedBytesThreshold { get; set; }

        bool RtsEnable { get; set; }

        StopBits StopBits { get; set; }

        int WriteBufferSize { get; set; }

        int WriteTimeout { get; set; }

        ISite Site { get; set; }

        IContainer Container { get; }

        void Close();

        void DiscardInBuffer();

        void DiscardOutBuffer();

        void Open();

        int Read(byte[] buffer, int offset, int count);

        int ReadChar();

        int Read(char[] buffer, int offset, int count);

        int ReadByte();

        string ReadExisting();

        string ReadLine();

        string ReadTo(string value);

        void Write(string text);

        void Write(char[] buffer, int offset, int count);

        void Write(byte[] buffer, int offset, int count);

        void WriteLine(string text);

        event SerialErrorReceivedEventHandler ErrorReceived;

        event SerialPinChangedEventHandler PinChanged;

        event SerialDataReceivedEventHandler DataReceived;

        void Dispose();

        string ToString();

        event EventHandler Disposed;

        object GetLifetimeService();

        object InitializeLifetimeService();

        ObjRef CreateObjRef(Type requestedType);
        }
    }