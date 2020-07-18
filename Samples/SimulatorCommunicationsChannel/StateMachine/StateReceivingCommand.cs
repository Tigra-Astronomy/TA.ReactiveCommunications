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
// File: StateReceivingCommand.cs  Last modified: 2020-07-16@02:26 by Tim Long

using System;
using System.Timers;

namespace SimulatorChannel.StateMachine
    {
    internal class StateReceivingCommand : SimulatorState
        {
        private readonly Timer interCharTimeout = new Timer(1000);

        internal StateReceivingCommand(SimulatorStateMachine machine) : base(machine) { }

        /// <summary>Gets the descriptive name of the current state.</summary>
        /// <value>The state's descriptive name, as a string.</value>
        protected override string Name => "ReceiveCommand";

        /// <summary>Provides a stimulus (input) to the state.</summary>
        /// <param name="value">The input value or stimulus.</param>
        public override void Stimulus(char value)
            {
            base.Stimulus(value);
            Machine.ReceivedChars.Append(value);
            Machine.InvokeReceivedData(EventArgs.Empty);
            if (Machine.ReceivedChars.Length >= 4) // All DDW commands are 4 characters.
                {
                Transition(new StateExecutingCommand(Machine));
                }
            else if (Machine.RealTime)
                {
                // Reset the inter-character timeout.
                interCharTimeout.Stop();
                interCharTimeout.Start();
                }
            }

        /// <summary>
        ///     Called (by the state machine) when exiting from the state. Ensures that the inter-character
        ///     timeout timer is stopped.
        /// </summary>
        public override void OnExit()
            {
            Machine.InReadyState.Reset(); // Signal dependent threads that they have to wait for us.
            if (Machine.RealTime)
                {
                interCharTimeout.Stop(); // Ensure the timeout timer is stopped.
                interCharTimeout.Elapsed -= InterCharTimeoutElapsed; // Withdraw our delegate.
                }

            base.OnExit();
            }

        /// <summary>
        ///     Called (by the state machine) when entering the state. Coinfigures the inter-character timeout
        ///     timer.
        /// </summary>
        public override void OnEnter()
            {
            base.OnEnter();
            Machine.ReceivedChars.Clear();
            if (Machine.RealTime)
                interCharTimeout.Elapsed += InterCharTimeoutElapsed;
            Machine.InReadyState.Set(); // Signal waiting threads that we are ready to rock.
            }

        /// <summary>Handles the Elapsed event of the interCharTimeout control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="System.Timers.ElapsedEventArgs" /> instance containing the event
        ///     data.
        /// </param>
        /// <remarks>
        ///     Causes the state to transition to itself, which invokes both <see cref="OnExit" /> and
        ///     <see cref="OnEnter" /> and essentially resets the state machine.
        /// </remarks>
        private void InterCharTimeoutElapsed(object sender, ElapsedEventArgs e)
            {
            log.Info().Message("Inter-character timeout").Write();
            Transition(this);
            }
        }
    }