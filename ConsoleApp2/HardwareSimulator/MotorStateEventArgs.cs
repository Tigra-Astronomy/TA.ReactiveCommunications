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
// File: MotorStateEventArgs.cs  Last modified: 2020-07-16@02:26 by Tim Long

using System;

namespace TA.Ascom.ReactiveCommunications.Sample.ConsoleApp.HardwareSimulator
    {
    /// <summary>Communicates the state of the various motors.</summary>
    [Obsolete]
    public class MotorStateEventArgs : EventArgs
        {
        /// <summary>Gets or sets a value indicating whether the simulated azimuth motor is running.</summary>
        /// <value><c>true</c> if the azimuth motor is energized; otherwise, <c>false</c>.</value>
        public bool AzimuthMotor { get; internal set; }

        /// <summary>Gets or sets a value indicating whether the simulated shutter motor is energized.</summary>
        /// <value><c>true</c> if the shutter motor is energized; otherwise, <c>false</c>.</value>
        public bool ShutterMotor { get; internal set; }
        }
    }