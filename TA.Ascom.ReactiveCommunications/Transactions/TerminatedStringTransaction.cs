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
// File: TerminatedStringTransaction.cs  Last modified: 2015-05-27@20:12 by Tim Long

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;

namespace TA.Ascom.ReactiveCommunications.Transactions
    {
    /// <summary>
    ///     A transaction that receives a string response terminated by a '#' (octothorpe) character and strips off any
    ///     initiator (:) and terminator (#).
    /// </summary>
    public class TerminatedStringTransaction : DeviceTransaction
        {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceTransaction" /> class.
        /// </summary>
        /// <param name="command">The command to be sent to the communications channel.</param>
        public TerminatedStringTransaction(string command) : base(command)
            {
            Contract.Requires(!string.IsNullOrEmpty(command));
            Value = string.Empty;
            }

        [ContractInvariantMethod]
        void ObjectInvariant()
            {
            Contract.Invariant(Value != null);
            }

        /// <summary>
        ///     Gets the final response value.
        /// </summary>
        /// <value>The value as a string.</value>
        public string Value { get; private set; }

        /// <summary>
        ///     Observes the character sequence from the communications channel
        ///     until a satisfactory response has been received.
        /// </summary>
        /// <param name="source">The source sequence.</param>
        public override void ObserveResponse(IObservable<char> source)
            {
            source.TerminatedStrings()
                .Take(1)
                .Subscribe(OnNext, OnError, OnCompleted);
            }

        /// <summary>
        ///     Called when the response sequence completes. This indicates a successful transaction. If a valid
        ///     response was received, then delimiters are stripped off and the unterminated string is copied into the
        ///     <see cref="Value" /> property.
        /// </summary>
        protected override void OnCompleted()
            {
            if (Response.Any())
                {
                var responseString = Response.Single();
                Value = responseString.TrimStart(':').TrimEnd('#');
                }
            base.OnCompleted();
            }
        }
    }