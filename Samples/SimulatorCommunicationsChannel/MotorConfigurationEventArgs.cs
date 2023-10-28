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
// File: MotorConfigurationEventArgs.cs  Last modified: 2020-07-20@00:49 by Tim Long

using System;

namespace SimulatorChannel
    {
    /// <summary>Class MotorConfigurationEventArgs. Communicates motor state and direction.</summary>
    public class MotorConfigurationEventArgs : EventArgs
        {
        internal static readonly MotorConfigurationEventArgs AllStopped = new MotorConfigurationEventArgs
            {
            AzimuthMotor = MotorConfiguration.Stopped,
            ShutterMotor = MotorConfiguration.Stopped
            };

        /// <summary>Gets or sets a value indicating whether the simulated azimuth motor is running.</summary>
        /// <value><c>true</c> if the azimuth motor is energized; otherwise, <c>false</c>.</value>
        public MotorConfiguration AzimuthMotor { get; internal set; }

        /// <summary>Gets or sets a value indicating whether the simulated shutter motor is energized.</summary>
        /// <value><c>true</c> if the shutter motor is energized; otherwise, <c>false</c>.</value>
        public MotorConfiguration ShutterMotor { get; internal set; }
        }
    }