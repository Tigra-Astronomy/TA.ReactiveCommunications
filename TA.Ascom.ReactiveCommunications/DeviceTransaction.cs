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
// File: DeviceTransaction.cs  Last modified: 2015-05-25@18:22 by Tim Long

using System;
using System.Threading;

namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>
    ///     Defines the common implementation for all commands
    /// </summary>
    public abstract class DeviceTransaction
        {
        /// <summary>
        ///     Used to generate unique transaction IDs for each created transaction.
        /// </summary>
        static int transactionCounter;

        readonly ManualResetEvent completion;

        public DeviceTransaction(string command)
            {
            TransactionId = GenerateTransactionId();
            Response = Maybe<string>.Empty;
            Command = command;
            Timeout = TimeSpan.MaxValue; // Wait effectively forever by default
            completion = new ManualResetEvent(false); // Not signalled by default
            Failed = true; // Transactions are always failed, until they have succeeded.
            }

        /// <summary>
        ///     Gets the transaction identifier of the transaction.
        ///     Used to match responses to commands where the protocol supports
        ///     multiple overlapping commands and for logging and statistics gathering.
        ///     Each command must have a unique transaction Id.
        /// </summary>
        /// <value>The transaction identifier.</value>
        public long TransactionId { get; private set; }

        /// <summary>
        ///     Gets or sets the command string.
        ///     The command must be in a format ready for sending to a transport provider.
        ///     In other words, commands are 'Layer 7', we are talking directly to the command
        ///     interpreter inside the target device.
        /// </summary>
        /// <value>The command string as expected by the command interpreter in the remote device.</value>
        public string Command { get; private set; }

        /// <summary>
        ///     Gets or sets the time that the command can be pending before it is considered to have failed.
        /// </summary>
        /// <value>The timeout value.</value>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        ///     Gets the response string exactly as observed from the response sequence.
        ///     The response is wrapped in a <see cref="Maybe{string}" /> to differentiate an empty
        ///     response from no response or a failed response. An empty Maybe indicates an error condition.
        ///     Commands where no repsponse is expected will return an empty string, not an empty Maybe.
        /// </summary>
        /// <value>The response string.</value>
        public Maybe<string> Response { get; internal set; }

        /// <summary>
        ///     Gets an indication that the transaction has failed.
        /// </summary>
        /// <value><c>true</c> if failed; otherwise, <c>false</c>.</value>
        public bool Failed { get; set; }

        public Maybe<string> ErrorMessage { get; protected set; }

        static long GenerateTransactionId()
            {
            var tid = Interlocked.Increment(ref transactionCounter);
            return tid;
            }

        /// <summary>
        ///     Waits (blocks) until the transaction is completed or a timeout occurs.
        ///     The timeout duration comes from the <see cref="Timeout" /> property.
        /// </summary>
        /// <returns><c>true</c> if the transaction completed successfully, <c>false</c> otherwise.</returns>
        public bool WaitForCompletionOrTimeout()
            {
            var signalled = completion.WaitOne(Timeout);
            if (!signalled)
                {
                Response = Maybe<string>.Empty;
                }
            return signalled;
            }

        void SignalCompletion()
            {
            completion.Set();
            }

        /// <summary>
        ///     Observes the character sequence from the communications channel
        ///     until a satisfactory response has been received. This default implementation
        ///     should be overridden in derived types toi produce the desired results.
        /// </summary>
        /// <param name="source">The source.</param>
        public abstract void ObserveResponse(IObservable<char> source);

        /// <summary>
        ///     Called when the response sequence produces a value.
        ///     This sets the transaction's Response string but the sequence
        ///     must complete before the transaction is considered successful.
        /// </summary>
        /// <param name="value">The value produced.</param>
        protected virtual void OnNext(string value)
            {
            Response = new Maybe<string>(value);
            }

        /// <summary>
        ///     Called when the response sequence produces an error.
        ///     This indicates a failed transaction.
        /// </summary>
        /// <param name="except">The exception.</param>
        protected virtual void OnError(Exception except)
            {
            Response = Maybe<string>.Empty;
            ErrorMessage = new Maybe<string>(except.Message);
            Failed = true;
            SignalCompletion();
            }

        /// <summary>
        ///     Called when the response sequence completes.
        ///     This indicates a successful transaction.
        /// </summary>
        protected virtual void OnCompleted()
            {
            Failed = false;
            SignalCompletion();
            }

        public override string ToString()
            {
            return string.Format("TID={0} [{1}] [{3}] {2}", TransactionId, Command, Timeout,
                Response);
            }
        }
    }