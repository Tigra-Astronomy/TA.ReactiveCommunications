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
// File: SimulatorState.cs  Last modified: 2020-07-20@00:49 by Tim Long

using System;
using System.Diagnostics.Contracts;
using TA.Utils.Core.Diagnostics;

namespace SimulatorChannel.StateMachine
    {
    /// <summary>
    ///     This is the base class for all simulator state-machine states. It defines the behaviours common
    ///     to all states, as well as static data (shared state) and methods that perform the logic
    ///     required to make the state machine work.
    /// </summary>
    internal class SimulatorState
        {
        /// <summary>A reference to the state machine that created the state.</summary>
        protected readonly SimulatorStateMachine Machine;

        /// <summary>Initializes the simulator state with a reference to the parent state machine.</summary>
        /// <param name="machine">The associated state machine.</param>
        internal SimulatorState(SimulatorStateMachine machine)
            {
            Machine = machine;
            }

        /// <summary>Provides logging services</summary>
        protected static ILog log => ServiceLocator.LoggingService;

        /// <summary>Gets the descriptive name of the current state.</summary>
        /// <value>The state's descriptive name, as a string.</value>
        protected virtual string Name
            {
            get
                {
                log.Warn().Message("Subclasses must override the Name property.").Write();
                return "Base class";
                }
            }

        /// <summary>Gets or sets the state of the state machine.</summary>
        /// <value>The state of the current.</value>
        public static SimulatorState CurrentState { get; internal set; }

/*
                        protected SimulatorStateMachine Machine;
                */

        #region IDisposable Members
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///     resources.
        /// </summary>
        public void Dispose()
            {
            CurrentState = null;
            }
        #endregion

        /// <summary>Provides a stimulus (input) to the state.</summary>
        /// <param name="value">The input value or stimulus.</param>
        public virtual void Stimulus(char value)
            {
            log.Info()
                .Message("State [{stateName}] received stimulus '{input}'", CurrentState, value)
                .Write();
            }

        /// <summary>Called (by the state machine) when exiting from the state</summary>
        public virtual void OnExit() { }

        /// <summary>Called (by the state machine) when entering the state.</summary>
        public virtual void OnEnter() { }

        /// <summary>Transitions the state machine to the specified new state.</summary>
        /// <param name="newState">The new state.</param>
        public static void Transition(SimulatorState newState)
            {
            if (newState == null)
                throw new ArgumentNullException("newState", "Must reference a valid state instance.");
            if (CurrentState != null)
                try
                    {
                    CurrentState.OnExit();
                    }
                catch (Exception ex)
                    {
                    log.Error().Message("{0} exception in OnExit():\n{1}", CurrentState.Name, ex).Write();
                    }

            log.Debug().Message("Transitioning from {fromState} => {toState}", CurrentState, newState).Write();
            CurrentState = newState;
            RaiseStateChanged(new StateEventArgs(newState.Name));
            try
                {
                newState.OnEnter();
                }
            catch (Exception ex)
                {
                log.Error()
                    .Message("Exception entering {toState}: {message}", newState.Name, ex.Message)
                    .Exception(ex)
                    .Write();
                }
            }

        /// <inheritdoc />
        public override string ToString() => Name;

        #region Events
        #region Delegates
        /// <summary>Custom delegate signature for state machine static events.</summary>
        public delegate void StateEventHandler(StateEventArgs e);
        #endregion

        /// <summary>Occurs when a state transition has occurred.</summary>
        public static event StateEventHandler StateChanged;

        /// <summary>Raises the <see cref="StateChanged" /> event.</summary>
        /// <param name="e">The <see cref="StateEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.Exception">A delegate callback throws an exception.</exception>
        private static void RaiseStateChanged(StateEventArgs e)
            {
            Contract.Requires(e != null);
            StateChanged?.Invoke(e);
            }
        #endregion
        }
    }