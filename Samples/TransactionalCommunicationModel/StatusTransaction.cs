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
// File: StatusTransaction.cs  Last modified: 2020-07-20@00:49 by Tim Long

using System;
using System.Linq;
using System.Reactive.Linq;
using SimulatorChannel;
using Timtek.ReactiveCommunications;
using Timtek.ReactiveCommunications.Diagnostics;

namespace TransactionalCommunicationModel
    {
    internal class StatusTransaction : DeviceTransaction
        {
        // A factory class that will be used to parse the response into something more useful.
        protected readonly ControllerStatusFactory factory = new ControllerStatusFactory(new SystemDateTimeUtcClock());

        /*
         * Constructor takes an optional command string, with a default value.
         * This is passed along to the base DeviceTransaction class.
         */
        public StatusTransaction(string command = Constants.CmdGetInfo) : base(command) { }

        /*
         * This public property will hold the transformed result of a successful transaction.
         * Transactions are most useful when they convert text responses into
         * more meaningful data structures.
         */
        public IHardwareStatus HardwareStatus { get; private set; }

        /*
         * Every transaction class must override this method.
         * This is where you get to observe the response character stream and
         * pluck out only the data that would be a valid response.
         * You can use LINQ operators and some helper methods provided by RxComms.
         */
        public override void ObserveResponse(IObservable<char> source)
            {
            /*
             * Status responses look like this:
             * "V4,407,376,2,376,0,0,1,0,371,382,0,128,255,255,255,255,255,255,255,999,3,0"
             * Always begin with "V4," and end with a newline.
             */
            var statusResponse = source
                /*
                 * First we slice & dice our input stream based on strings the start with 'V' and end with newline.
                 * We use an RxComms extension method for this.
                 * In practice, it would be wise to perform a more rigorous check here  with
                 * a regular expression match, but for this particular protocol
                 * the letter 'V' can only occur in this exact situation, so we are safe.
                 */
                .DelimitedMessageStrings('V', '\n')
                /*
                 * We'll emit all matching responses into the diagnostic logs.
                 */
                .Trace("StatusTransaction")
                /*
                 * And then (this is critical) we end the sequence after one element is produced.
                 * It is important that the sequence ends so that the OnCompleted action fires.
                 * This is what marks the transaction as "complete".
                 * If the sequence doesn't complete, the transaction will eventually time out and fail.
                 */
                .Take(1)
                /*
                 * And then we subscribe to our transformed sequence, using the handlers provided
                 * in the DeviceTransaction base class and our overridden OnCompleted action.
                 */
                .Subscribe(OnNext, OnError, OnCompleted);
            }

        /*
         * Transactions don't _have_ to override OnCompleted, but it can be a useful
         * way of transforming the raw response data into something more useful to
         * application. In this case, we'll parse the status response into a data
         * transfer object and store that in the HardwareStatus property.
         */
        protected override void OnCompleted()
            {
            if (Response.Any())
                {
                var responseString = Response.Single();
                var status = factory.FromStatusPacket(responseString);
                HardwareStatus = status;
                }
            // it is super important that the base class OnCompleted gets called!
            // This is what marks the transaction as completed.
            base.OnCompleted();
            }
        }
    }