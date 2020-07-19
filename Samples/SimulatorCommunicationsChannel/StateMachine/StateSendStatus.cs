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
// File: StateSendStatus.cs  Last modified: 2020-07-20@00:49 by Tim Long

using System.Timers;

namespace SimulatorChannel.StateMachine
    {
    internal class StateSendStatus : SimulatorState
        {
        /// <summary>Times the pause between command completion and sending the status response</summary>
        private readonly Timer pauseTimer = new Timer {AutoReset = false};

        protected internal StateSendStatus(SimulatorStateMachine machine) : base(machine) { }

        protected override string Name => "Send Status";

        /// <summary>Called (by the state machine) when entering the state.</summary>
        public override void OnEnter()
            {
            base.OnEnter();
            Machine.InvokeMotorConfigurationChanged(MotorConfigurationEventArgs.AllStopped);
            if (Machine.RealTime)
                {
                pauseTimer.Elapsed += PauseTimerElapsed;
                pauseTimer.Interval = Machine.RealTime ? 500 : 1;
                pauseTimer.Start();
                }
            else
                {
                SendStatus();
                }
            }

        /// <summary>Handles the Elapsed event from the timer.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///     The <see cref="System.Timers.ElapsedEventArgs" /> instance containing the event
        ///     data.
        /// </param>
        private void PauseTimerElapsed(object sender, ElapsedEventArgs e)
            {
            pauseTimer.Stop();
            SendStatus();
            }

        private void SendStatus()
            {
            // Update state that could have been affected by the last operation.
            Machine.SetAzimuthDependentSensorsAndStates();
            Machine.WriteLine(Machine.HardwareStatus.ToDdwStatusString());
            Transition(new StateReceivingCommand(Machine));
            }

        public override void OnExit()
            {
            if (Machine.RealTime)
                {
                pauseTimer.Stop();
                pauseTimer.Elapsed -= PauseTimerElapsed;
                }

            base.OnExit();
            }
        }
    }