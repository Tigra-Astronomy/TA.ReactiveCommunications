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
// File: StateEmergencyStop.cs  Last modified: 2020-07-16@02:26 by Tim Long

using System.Threading;
using TA.Utils.Core;

namespace TA.Ascom.ReactiveCommunications.Sample.ConsoleApp.HardwareSimulator
    {
    /// <summary>
    ///     The emergency stop state doesn't really provide any functionality but it does reflect a genuine
    ///     hardware state and is useful as a place to put logging. The state sleeps for one second before
    ///     transitioning to <see cref="StateSendStatus" />.
    /// </summary>
    internal class StateEmergencyStop : SimulatorState
        {
        internal StateEmergencyStop(SimulatorStateMachine machine) : base(machine) { }

        /// <summary>Gets the descriptive name of the current state.</summary>
        /// <value>The state's descriptive name, as a string.</value>
        protected override string Name => "Emergency Stop";

        /// <summary>
        ///     Called (by the state machine) when entering the state. Sleeps for 1 second and transitions to
        ///     the <see cref="StateSendStatus" /> state.
        /// </summary>
        public override void OnEnter()
            {
            base.OnEnter();
            Machine.InvokeMotorConfigurationChanged(MotorConfigurationEventArgs.AllStopped);
            Thread.Sleep(1000);
            Transition(new StateSendStatus(Machine));
            }

        /// <summary>
        ///     Provides a stimulus (input) to the state. Any stimulus during an emergency stop is
        ///     discarded.
        /// </summary>
        /// <param name="value">The input value or stimulus.</param>
        public override void Stimulus(char value)
            {
            base.Stimulus(value);
            Log.Info($"Discarding input: '{value.ExpandAscii()}'");
            }
        }
    }