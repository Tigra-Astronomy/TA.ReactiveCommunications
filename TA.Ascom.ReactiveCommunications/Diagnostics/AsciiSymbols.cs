// This file is part of the TA.Ascom.ReactiveCommunications project
// 
// Copyright © 2017 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so,. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: AsciiSymbols.cs  Last modified: 2017-01-21@22:36 by Tim Long

namespace TA.Ascom.ReactiveCommunications.Diagnostics
    {
    /// <summary>
    ///     Enumeration constants for ASCII control codes.
    /// </summary>
    internal enum AsciiSymbols : byte
        {
        // ReSharper disable InconsistentNaming
        NULL = 0x00,
        SOH = 0x01,
        STH = 0x02,
        ETX = 0x03,
        EOT = 0x04,
        ENQ = 0x05,
        ACK = 0x06,
        BELL = 0x07,
        BS = 0x08,
        HT = 0x09,
        LF = 0x0A,
        VT = 0x0B,
        FF = 0x0C,
        CR = 0x0D,
        SO = 0x0E,
        SI = 0x0F,
        DC1 = 0x11,
        DC2 = 0x12,
        DC3 = 0x13,
        DC4 = 0x14,
        NAK = 0x15,
        SYN = 0x16,
        ETB = 0x17,
        CAN = 0x18,
        EM = 0x19,
        SUB = 0x1A,
        ESC = 0x1B,
        FS = 0x1C,
        GS = 0x1D,
        RS = 0x1E,
        US = 0x1F, /*SP = 0x20,*/
        DEL = 0x7F
        }
    }