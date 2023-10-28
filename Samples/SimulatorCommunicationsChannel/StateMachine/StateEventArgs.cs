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
// File: StateEventArgs.cs  Last modified: 2020-07-20@00:49 by Tim Long

using System;

namespace SimulatorChannel.StateMachine
    {
    /// <summary>Event arguments used by state machine events.</summary>
    public class StateEventArgs : EventArgs
        {
        /// <summary>Initializes a new instance of the <see cref="StateEventArgs" /> class.</summary>
        /// <param name="stateName">Name of the new state.</param>
        public StateEventArgs(string stateName)
            {
            StateName = stateName;
            }

        /// <summary>Gets or sets the name of the new state.</summary>
        /// <value>The name of the state.</value>
        public string StateName { get; }
        }
    }