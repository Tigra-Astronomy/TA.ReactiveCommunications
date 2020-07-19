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
// File: StateStartup.cs  Last modified: 2020-07-20@00:49 by Tim Long

using System;

namespace SimulatorChannel.StateMachine
    {
    internal class StateStartup : SimulatorState
        {
        public StateStartup(SimulatorStateMachine machine) : base(machine) { }

        protected override string Name => "Startup";

        /// <summary>
        ///     Called (by the state machine) when entering the state. The Startup state initialises and items
        ///     that must be initialised, then immediately transitions to the
        ///     <see cref="StateReceivingCommand" /> state.
        /// </summary>
        public override void OnEnter()
            {
            //ToDo: Perform any initialization here.
            try
                {
                // If all initialization succeeded, immediately transition to the Ready state.
                Transition(new StateReceivingCommand(Machine));
                }
            catch (Exception ex)
                {
                log.Error()
                    .Message("Failed to start the state machine")
                    .Exception(ex)
                    .Write();
                Transition(new StateStalled(Machine));
                }
            }
        }
    }