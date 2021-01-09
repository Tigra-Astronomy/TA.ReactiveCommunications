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
// File: BooleanTransaction.cs  Last modified: 2020-07-20@00:50 by Tim Long

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;

namespace TA.Ascom.ReactiveCommunications.Transactions
    {
    /// <summary>Receives a response consisting of <c>0#</c> or <c>1#</c> and interprets it as a boolean.</summary>
    public class BooleanTransaction : DeviceTransaction
        {
        /// <summary>Initializes a new instance of the <see cref="DeviceTransaction" /> class.</summary>
        /// <param name="command">The command string to send to the device.</param>
        public BooleanTransaction(string command) : base(command)
            {
            Contract.Requires(!string.IsNullOrEmpty(command));
            }

        /// <summary>Gets the final value of teh transaction's response, as a boolean.</summary>
        /// <value><c>true</c> if value; otherwise, <c>false</c>.</value>
        public bool Value { get; private set; }

        /// <summary>
        ///     Observes the character sequence from the communications channel until a satisfactory response
        ///     has been received.
        /// </summary>
        /// <param name="source">The source sequence.</param>
        public override void ObserveResponse(IObservable<char> source)
            {
            source.TerminatedBoolean().Take(1).Subscribe(OnNext, OnError, OnCompleted);
            }

        /// <summary>Called when the response sequence completes. This indicates a successful transaction.</summary>
        /// <remarks>
        ///     If there has been a valid response (<c>Response.Any() == true</c>) then it is converted to a
        ///     boolean and placed in the <see cref="Value" /> property.
        /// </remarks>
        protected override void OnCompleted()
            {
            try
                {
                if (Response.Any())
                    Value = Response.Single().StartsWith("1");
                }
            catch (FormatException)
                {
                Value = false;
                }
            base.OnCompleted();
            }
        }
    }