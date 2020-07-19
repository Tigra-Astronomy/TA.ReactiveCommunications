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
// File: StateShutterMoving.cs  Last modified: 2020-07-20@00:49 by Tim Long

using System;
using System.Timers;

namespace SimulatorChannel.StateMachine
    {
    internal class StateShutterMoving : SimulatorState
        {
        private readonly MotorConfiguration direction;
        private readonly Random motorCurrent = new Random();
        private readonly Timer shutter_tick_timer = new Timer {AutoReset = false};

        protected StateShutterMoving(SimulatorStateMachine machine, MotorConfiguration direction)
            : base(machine)
            {
            this.direction = direction;
            }

        protected int ShutterTicksRemaining { get; private set; }

        protected override string Name => "Shutter Moving";

        /// <summary>
        ///     Called (by the state machine) when entering the state. Sets the Shutter Sensor to indeterminate
        ///     during movement and configures a timer to simulate shutter motion.
        /// </summary>
        public override void OnEnter()
            {
            base.OnEnter();
            ShutterTicksRemaining = Machine.HardwareStatus.ShutterSensor == SensorState.Indeterminate
                ? 20
                : 40;
            Machine.HardwareStatus.ShutterSensor = SensorState.Indeterminate;
            Machine.SimulatedShutterSensor = SensorState.Indeterminate;
            //SimulatorStateMachine.InvokeStatusChanged(new StatusChangedEventArgs(SimulatorStateMachine.HardwareStatus));
            Machine.InvokeMotorConfigurationChanged(new MotorConfigurationEventArgs
                {
                AzimuthMotor = MotorConfiguration.Stopped,
                ShutterMotor = direction
                });
            shutter_tick_timer.Interval = Machine.RealTime ? 250 : 1;
            shutter_tick_timer.Elapsed += ShutterTickTimerElapsed;
            shutter_tick_timer.Start();
            }

        /// <summary>Handles the Elapsed event of the shutter_tick_timer control.</summary>
        /// <param name="sender">The source of the event (not used).</param>
        /// <param name="e">
        ///     The <see cref="System.Timers.ElapsedEventArgs" /> instance containing the event
        ///     data (not used).
        /// </param>
        private void ShutterTickTimerElapsed(object sender, ElapsedEventArgs e)
            {
            shutter_tick_timer.Stop();
            Machine.WriteLine(string.Format("SZ{0:D3}", motorCurrent.Next(6, 25)));
            if (--ShutterTicksRemaining > 0)
                shutter_tick_timer.Start();
            else
                Transition(new StateSendStatus(Machine));
            }

        /// <summary>
        ///     Called (by the state machine) when exiting from the state. Configures the new shutter state
        ///     based on the previous state.
        /// </summary>
        public override void OnExit()
            {
            base.OnExit();
            shutter_tick_timer.Stop();
            }

        /// <summary>
        ///     Provides a stimulus (input) to the state. Any stimulus during movement is interpreted as
        ///     Emergency Stop.
        /// </summary>
        /// <param name="value">The input value or stimulus.</param>
        public override void Stimulus(char value)
            {
            base.Stimulus(value);
            Transition(new StateEmergencyStop(Machine));
            }
        }
    }