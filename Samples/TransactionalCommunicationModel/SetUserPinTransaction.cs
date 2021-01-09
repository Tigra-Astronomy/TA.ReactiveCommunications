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
// File: SetUserPinTransaction.cs  Last modified: 2020-07-20@00:49 by Tim Long

using System.Linq;
using SimulatorChannel;
using TA.Utils.Core;

namespace TransactionalCommunicationModel
    {
    /*
     * This transaction class is used to set user I/O pins in the device.
     * The command to do this is "GPxx"
     * where 'xx' is the new value for all of the output pins, in hexadecimal.
     * The command responds with a full status packet, but we want the result of this transaction
     * to be the new pin values, which we can extract from the status values.
     */
    class SetUserPinTransaction : StatusTransaction
        {
        /// <inheritdoc />
        public SetUserPinTransaction(Octet pins) : base(string.Format(Constants.CmdSetUserPins, (byte) pins)) { }

        /*
         * The UserPins property is the result of our transaction.
         * Note that we'll also have a HardwareStatus property inherited from StatusTransaction
         */
        public Octet UserPins { get; private set; }

        /*
         * We override OnCompleted so that we have a chance to extract the new user pins
         * value from the response.
         */
        protected override void OnCompleted()
            {
            if (Response.Any())
                {
                var responseString = Response.Single();
                var status = factory.FromStatusPacket(responseString);
                UserPins = status.UserPins;
                }

            /*
             * it is super important that DeviceTransaction.OnCompleted gets called!
             * This is what ultimately marks the transaction as completed.
             * Note the call sequence of these virtual methods:
             * SetUserPinsTransaction.OnCompleted()
             * └> StatusTransaction.OnCompleted
             *    └> DeviceTransaction.OnCompleted()
             */
            base.OnCompleted();
            }
        }
    }