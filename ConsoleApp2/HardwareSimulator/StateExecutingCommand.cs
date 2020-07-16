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
// File: StateExecutingCommand.cs  Last modified: 2020-07-16@02:26 by Tim Long

using System.Globalization;
using TA.Utils.Core;

namespace TA.Ascom.ReactiveCommunications.Sample.ConsoleApp.HardwareSimulator
    {
    internal class StateExecutingCommand : SimulatorState
        {
        protected internal StateExecutingCommand(SimulatorStateMachine machine) : base(machine) { }

        protected override string Name => "Executing Command";

        /// <summary>
        ///     Called (by the state machine) when entering the state. At this point, there should be a
        ///     received command packet in the receive buffer. We need to parse it and act appropriately.
        /// </summary>
        public override void OnEnter()
            {
            base.OnEnter();
            Log.Debug($"Processing command [{Machine.ReceivedChars}]");
            if (Machine.ReceivedChars.Length == 0)
                {
                Log.Warn("Nothing received");
                Transition(new StateReceivingCommand(Machine));
                }

            if (Machine.ReceivedChars.Length != 4)
                {
                Log.Warn("Received string not 4 characters");
                Transition(new StateReceivingCommand(Machine));
                }

            ExecuteCommand();
            }

        /// <summary>
        ///     Any received character at this point should be interpreted as an 'All Stop' instruction, unless
        ///     it is a carriage return, in which case it should be ignored.
        /// </summary>
        /// <param name="value">The input value or stimulus.</param>
        public override void Stimulus(char value)
            {
            base.Stimulus(value);
            if (value == (char) AsciiSymbols.CR)
                return; // Ignore carriage returns

            Transition(new StateEmergencyStop(Machine));
            }

        /// <summary>Parses and executes the received command.</summary>
        /// <remarks>
        ///     The possible valid commands are:
        ///     <list>
        ///         <listheader>
        ///             <item>Command</item> <description>Description</description>
        ///         </listheader>
        ///         <item>GXXX</item>
        ///         <description>
        ///             XXX is a 3-digit decimal representing the target dome azimuth in degrees
        ///             (range 0 to 359).
        ///         </description>
        ///         <item>GHOM</item>
        ///         <description>Move to the HOME position.</description>
        ///     </list>
        /// </remarks>
        private void ExecuteCommand()
            {
            var command = Machine.ReceivedChars.ToString();
            switch (command)
                {
                    case Constants.CmdCancelTimeout:
                        return;
                    case Constants.CmdClose:
                        Transition(new StateRotatingForShutterClose(Machine));
                        return;
                    case Constants.CmdFastTrack:
                        return;
                    case Constants.CmdGetInfo:
                        Transition(new StateSendStatus(Machine));
                        return;
                    case Constants.CmdGotoHome:
                        Transition(new StateRotatingToHome(Machine));
                        return;
                    case Constants.CmdOpen:
                        Transition(new StateRotatingForShutterOpen(Machine));
                        return;
                    case Constants.CmdPark:
                    case Constants.CmdSetUserPins: // ToDo - requires special handling [this can never match]
                    case Constants.CmdSlaveOff:
                    case Constants.CmdSlaveOn:
                    case Constants.CmdTest:
                    case Constants.CmdTrain:
                    case Constants.CmdUnpark:
                        return;
                }

            // Other possibilities are: Gxxx GPnn and Csdr
            if (command.StartsWith("GP"))
                {
                // User pin manipulation
                byte pins;
                if (byte.TryParse(command.Substring(2, 2),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out pins))
                    {
                    // Successfully parsed a user I/O pin packet.
                    Machine.HardwareStatus.UserPins = pins;
                    Transition(new StateSendStatus(Machine));
                    }
                }
            else if (Machine.ReceivedChars[0] == 'G')
                {
                int targetDegrees;
                if (int.TryParse(command.Substring(1, 3),
                    NumberStyles.Integer,
                    CultureInfo.CurrentCulture,
                    out targetDegrees))
                    {
                    // Successfully recognised a rotation command.
                    if (Machine.InDeadZone(targetDegrees))
                        {
                        Transition(new StateSendStatus(Machine));
                        return;
                        }

                    Machine.TargetAzimuthDegrees = targetDegrees;
                    Transition(new StateRotating(Machine));
                    }
                else
                    {
                    Log.Warn("Gxxx command had invalid numeric part (ignored)");
                    Transition(new StateReceivingCommand(Machine));
                    }
                }
            //ToDo: Smart Tracking will likely be implemented in its own state.
            //else if (false) // if it's a smart-tracking command
            //{
            //    //ToDo: Implement smart tracking command (maybe not)
            //    Diagnostics.TraceWarning("Command may be 'smart tracking' but SmartTracking is not implemented.");
            //}
            else
                {
                // Anything else must be an invalid or corrupted command, so discard it.
                Transition(new StateReceivingCommand(Machine));
                }
            }
        }
    }