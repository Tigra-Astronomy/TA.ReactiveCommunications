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
// File: TransactionObserver.cs  Last modified: 2020-07-20@00:51 by Tim Long

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using JetBrains.Annotations;
using TA.Ascom.ReactiveCommunications.Diagnostics;
using TA.Utils.Core.Diagnostics;

namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>
    ///     Observes the incoming transaction pipeline, which ultimately derives from the client
    ///     application and commits each transaction in sequence. Transactions are processed synchronously
    ///     and are guaranteed to be atomic, that is, zero or one transactions can be 'in progress' at any
    ///     given time. This may involve blocking the thread while the transaction completes.
    /// </summary>
    public class TransactionObserver : IObserver<DeviceTransaction>
        {
        private readonly ICommunicationChannel channel;
        private readonly ILog log = ServiceLocator.LogService;
        private readonly IConnectableObservable<char> observableReceiveSequence;
        private volatile int activeTransactions;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionObserver" /> class and associates it
        ///     with a communications channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        public TransactionObserver(ICommunicationChannel channel)
            {
            Contract.Requires(channel != null);
            this.channel = channel;
            observableReceiveSequence = channel.ObservableReceivedCharacters.Publish();
            log.Info().Message($"Transaction pipeline connected to channel with endpoint {channel.Endpoint}").Write();
            }

        /// <summary>Gets a value indicating whether the receiver is ready.</summary>
        /// <value><c>true</c> if the receiver is ready; otherwise, <c>false</c>.</value>
        public bool ReceiverReady => channel.IsOpen;

        /// <summary>Called when the next transaction is available.</summary>
        /// <param name="transaction">The transaction.</param>
        [UsedImplicitly]
        public void OnNext(DeviceTransaction transaction)
            {
            log.Info()
                .Transaction(transaction)
                .Message("Committing transaction {transaction}", transaction)
                .Write();
            CommitTransaction(transaction);
            log.Info()
                .Transaction(transaction)
                .Message("Completed transaction {transaction}", transaction)
                .Write();
            }

        /// <summary>Notifies the observer that the provider has experienced an error condition.</summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        [UsedImplicitly]
        public void OnError(Exception error)
            {
            //ToDo - currently we will just go 'belly up'. Is there a better way of handling errors?
            log.Fatal().Message("Error in transaction pipeline").Exception(error).Write();
            }

        /// <summary>Notifies the observer that the provider has finished sending push-based notifications.</summary>
        public void OnCompleted()
            {
            /*
             * Completion of the transaction pipeline means that the client has disconnected and there will be no more transactions.
             * In that case, we can safely close the communications channel.
             */
            log.Info().Message("End of transaction sequence - closing channel").Write();
            channel.Close();
            }

        [ContractInvariantMethod]
        private void ObjectInvariant()
            {
            Contract.Invariant(log != null);
            Contract.Invariant(channel != null);
            Contract.Invariant(observableReceiveSequence != null);
            }

        private void CommitTransaction(DeviceTransaction transaction)
            {
            Contract.Requires(transaction != null);
            Contract.Requires(!string.IsNullOrEmpty(transaction.Command));
            var transactionsInFlight = Interlocked.Increment(ref activeTransactions);
            if (transactionsInFlight > 1)
                {
                // This should never happen and if it does then we have a serious concurrency bug
                log.Error()
                    .Message("Detected transaction overlap before committing {transaction}", transaction)
                    .Write();
                throw new InvalidOperationException("Detected transaction overlap");
                }
            transaction.MakeHot();
            transaction.ObserveResponse(observableReceiveSequence);
            using (var responseSequence = observableReceiveSequence.Connect())
                {
                channel.Send(transaction.Command);
                var succeeded = transaction.WaitForCompletionOrTimeout();
                if (!succeeded)
                    log.Warn()
                        .Transaction(transaction)
                        .Message("Transaction {id} timed out with reason: {message}",
                            transaction.TransactionId, transaction.ErrorMessage.SingleOrDefault())
                        .Write();
                }
            if (transaction.Failed)
                log.Warn()
                    .Transaction(transaction)
                    .Message("Transaction {id} was marked as FAILED", transaction.TransactionId)
                    .Write();
            log.Info()
                .Transaction(transaction)
                .Message("Transaction {id} completed", transaction.TransactionId);
            transactionsInFlight = Interlocked.Decrement(ref activeTransactions);
            if (transactionsInFlight != 0)
                {
                // This should never happen and if it does then we have a serious concurrency bug
                log.Error()
                    .Message("Detected transaction overlap after completing {transaction}", transaction);
                throw new InvalidOperationException(
                    "Detected transaction overlap, please report this as an issue on GitHub and include your log file");
                }
            }
        }
    }