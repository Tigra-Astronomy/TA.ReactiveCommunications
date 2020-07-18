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
// File: StateShutterClosing.cs  Last modified: 2020-07-16@02:26 by Tim Long

namespace SimulatorChannel.StateMachine
    {
    /// <summary>
    ///     Variant of <see cref="StateShutterMoving" /> used when the shutter is closing. The finishing
    ///     state can be either indeterminate or closed.
    /// </summary>
    internal class StateShutterClosing : StateShutterMoving
        {
        public StateShutterClosing(SimulatorStateMachine machine) : base(machine, MotorConfiguration.Reverse) { }

        protected override string Name => "Shutter Closing";

        /// <summary>
        ///     Called (by the state machine) when entering the state. If the shutter is already closed,
        ///     transitions directly to <see cref="StateSendStatus" />. Otherwise, sets the Shutter Sensor to
        ///     indeterminate during movement and configures a timer to simulate shutter motion.
        /// </summary>
        public override void OnEnter()
            {
            if (Machine.SimulatedShutterSensor == SensorState.Closed || Machine.ShutterStuck)
                Transition(new StateSendStatus(Machine));
            else
                base.OnEnter();
            }

        public override void OnExit()
            {
            base.OnExit();
            if (!Machine.ShutterStuck)
                Machine.SimulatedShutterSensor = ShutterTicksRemaining > 0
                    ? SensorState.Indeterminate
                    : SensorState.Closed;
            Machine.HardwareStatus.ShutterSensor = Machine.SimulatedShutterSensor;
            }
        }
    }