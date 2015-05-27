// This file is part of the TA.Ascom.ReactiveCommunications project
// 
// Copyright © 2015 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so,. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: NoReplyTransaction.cs  Last modified: 2015-05-27@08:37 by Tim Long

using System;
using System.Diagnostics.Contracts;

namespace TA.Ascom.ReactiveCommunications.Transactions
    {
    /// <summary>
    ///     A transaction that doesn't wait for any reply and completes immediately.
    /// </summary>
    public class NoReplyTransaction : DeviceTransaction
        {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceTransaction" /> class.
        /// </summary>
        /// <param name="command">The command to be sent to the communications channel.</param>
        public NoReplyTransaction(string command) : base(command)
            {
            Contract.Requires(command != null);
            }

        /// <summary>
        ///     Ignores the source sequence and does not wait for any received data. Completes the transaction immediately.
        /// </summary>
        /// <param name="source">The source sequence of received characters (ignored).</param>
        public override void ObserveResponse(IObservable<char> source)
            {
            Contract.Ensures(Response != null);
            Response = new Maybe<string>(string.Empty);
            OnCompleted();
            }
        }
    }