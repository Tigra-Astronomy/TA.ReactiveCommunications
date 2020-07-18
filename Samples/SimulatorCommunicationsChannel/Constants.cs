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
// File: Constants.cs  Last modified: 2020-07-16@02:26 by Tim Long

namespace SimulatorChannel
    {
    /// <summary>
    ///     Constants that apply to Digital DomeWorks hardware, software, firmware and settings. This
    ///     struct includes all of the commands used by the DD protocol plus other data and magic numbers.
    /// </summary>
    public struct Constants
        {
        #region Command Codes
        /// <summary>
        ///     Command to stop all movement. The controller will actually accept pretty much any spurious
        ///     character for this purpose.
        /// </summary>
        public const string CmdEmergencyStop = "STOP\n";

        /// <summary>command to cancel the 4-minute auto-park timeout.</summary>
        public const string CmdCancelTimeout = "GSSK";

        /// <summary>command to close the shutter.</summary>
        public const string CmdClose = "GCLS";

        /// <summary>command to enable 'fast tracking' mode.</summary>
        public const string CmdFastTrack = "GTCK";

        /// <summary>"Get Info" command. Response is a GINF status packet.</summary>
        public const string CmdGetInfo = "GINF"; // Response is GINF status packet

        /// <summary>
        ///     Format string used with <see cref="string.Format(string,object?)" /> for building a "GoTo
        ///     Azimuth" command. Format Gxxx, 3 digits packed with leading zeroes as necessary.
        ///     <seealso cref="string.Format(string,object?)" /> Response is a series of Pxxx position update
        ///     packets, followed by a GINF status packet when movement has ceased.
        /// </summary>
        public const string CmdGotoAz = "G{0:000}"; // GoTo Azimuth. Use with String.Format.

        /// <summary>
        ///     command to move the dome to the home position. Response is a series of Pxxx position update
        ///     packets, followed by a GINF status packet.
        /// </summary>
        public const string CmdGotoHome = "GHOM";

        /// <summary>command to open the shutter.</summary>
        public const string CmdOpen = "GOPN";

        /// <summary>command to park the LX-200 telescope</summary>
        public const string CmdPark = "GSPK";

        /// <summary>
        ///     Format string used with String.Format for building a command to configure the user output pins.
        ///     Format GPxx, where xx is a 2 digit hexadecimal number packed with leading zeroes as necessary.
        ///     <seealso cref="string.Format(string,object?)" /> Response is a GINF status packet.
        /// </summary>
        public const string CmdSetUserPins = "GP{0:X2}";

        /// <summary>command to turn off LX-200 slave mode. Response is a GINF status packet.</summary>
        public const string CmdSlaveOff = "GVLS"; // Response is GINF status packet

        /// <summary>command to turn on LX-200 slave mode. Response is a GINF status packet.</summary>
        public const string CmdSlaveOn = "GSLV"; // Response is GINF status packet

        /// <summary>command to initiate diagnostic tests. Response is undefined.</summary>
        public const string CmdTest = "GTST";

        /// <summary>command to start the dome training sequence.</summary>
        public const string CmdTrain = "GTRN";

        /// <summary>command to unpark the LX-200 telescope.</summary>
        public const string CmdUnpark = "GSRK";
        #endregion

        #region DDW responses
        #endregion

        #region Other Constants and Magic Numbers
        /// <summary>Delay after sending a blind command, in milliseconds.</summary>
        internal const int CSleepNoReply = 200; // ms to sleep after sending a blind command

        /// <summary>
        ///     The maximum amount of time that the Rx thread will wait for a reply to a transacted command
        ///     before raising an exception, in milliseconds.
        /// </summary>
        internal const int CTransactTimeout = 250; // ms transaction timeout

        /// <summary>ASCII carriage return character</summary>
        internal const string CarriageReturn = "\x0D";

        /// <summary>ASCII line feed character</summary>
        internal const string LineFeed = "\x0A";
        #endregion

        #region Custom Actions
        public const string ActionNameDsrSwingoutState = "DomeSupportRingSwingoutState";
        public const string ActionNameControllerStatus = "ControllerStatus";
        #endregion Custom Actions
        }
    }