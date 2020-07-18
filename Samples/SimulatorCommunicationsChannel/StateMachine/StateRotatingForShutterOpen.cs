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
// File: StateRotatingForShutterOpen.cs  Last modified: 2020-07-16@02:26 by Tim Long

namespace SimulatorChannel.StateMachine
    {
    internal class StateRotatingForShutterOpen : StateRotating
        {
        internal StateRotatingForShutterOpen(SimulatorStateMachine machine) : base(machine) { }

        /// <summary>Gets the descriptive name of the current state.</summary>
        /// <value>The state's descriptive name, as a string.</value>
        protected override string Name => "Moving To Home for Shutter Open";

        /// <summary>
        ///     Transitions to the next logical state. Overrides the default behaviour and begins opening the
        ///     shutter.
        /// </summary>
        protected override void TransitionToNextState()
            {
            Transition(new StateShutterOpening(Machine));
            }

        /// <summary>Upon entering the state, sets the dome's target azimuth to the home position.</summary>
        public override void OnEnter()
            {
            Machine.TargetAzimuthTicks = Machine.HardwareStatus.HomePosition;
            base.OnEnter();
            }
        }
    }