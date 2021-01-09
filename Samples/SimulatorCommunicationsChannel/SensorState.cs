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
// File: SensorState.cs  Last modified: 2020-07-20@00:49 by Tim Long

namespace SimulatorChannel
    {
    /// <summary>Positional sensor states</summary>
    /// <remarks>
    ///     Typically used where mechanical movement needs to be detected and a pair of reed-switch sensors
    ///     is used. One sensor detects the 'Open' position and another detects the 'Closed' position. When
    ///     neither sensor is active, then the moving apparatus is somewhere in between the two positions.
    /// </remarks>
    public enum SensorState
        {
        /// <summary>The position is indeterminate (neither the open nor the closed sensor is active)</summary>
        Indeterminate = 0,

        /// <summary>The item being sensed is in the Closed position (closed sensor active)</summary>
        Closed = 1,

        /// <summary>The item being sensed is in the Open position (open sensor active)</summary>
        Open = 2
        }
    }