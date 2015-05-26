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
// File: TransactionObserver.cs  Last modified: 2015-05-25@18:23 by Tim Long

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using NLog;

namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>
    ///     Observes the incoming transaction pipeline, which ultimately derives from the client application
    ///     and commits each transaction in sequence. Transactions are processed
    ///     synchronously and are guaranteed to be atomic, that is,
    ///     zero or one transactions can be 'in progress' at any given time.
    ///     This may involve blocking the thread while the transaction completes.
    /// </summary>
    public class TransactionObserver : IObserver<DeviceTransaction>
        {
        readonly ICommunicationChannel channel;
        readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly IConnectableObservable<char> observableReceiveSequence;
        int activeTransactions;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionObserver" /> class and associates it with a communications
        ///     channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        public TransactionObserver(ICommunicationChannel channel)
            {
            this.channel = channel;
            observableReceiveSequence = channel.ObservableReceivedCharacters.Publish();
            log.Info("Transaction pipeline connected to channel with endpoint {0}", channel.Endpoint);
            }

        public bool ReceiverReady
            {
            get { return channel.IsOpen; }
            }

        public void OnNext(DeviceTransaction value)
            {
            log.Info("Committing transaction {0}", value);
            CommitTransaction(value);
            log.Info("Completed transaction {0}", value);
            }

        public void OnError(Exception error)
            {
            //ToDo - currently we will just go 'belly up'. Is there a better way of handling errors?
            log.FatalException("Error in transaction pipeline", error);
            }

        public void OnCompleted()
            {
            /*
             * Completion of the transaction pipeline means that the client has disconnected and there will be no more transactions.
             * In that case, we can safely close the communications channel.
             */
            channel.Close();
            }

        void CommitTransaction(DeviceTransaction transaction)
            {
            var transactionsInFlight = Interlocked.Increment(ref activeTransactions);
            if (transactionsInFlight > 1)
                {
                // This should never happen and if it does then we have a serious concurrency bug
                log.Error("Detected transaction overlap before committing {0}", transaction);
                throw new InvalidOperationException("Detected transaction overlap");
                }
            transaction.ObserveResponse(observableReceiveSequence);
            using (var responseSequence = observableReceiveSequence.Connect())
                {
                channel.Send(transaction.Command);
                var succeeded = transaction.WaitForCompletionOrTimeout();
                if (!succeeded)
                    {
                    log.Warn("Transaction {0} timed out", transaction.TransactionId);
                    }
                }
            if (transaction.Failed)
                {
                log.Warn("Transaction {0} was marked as FAILED", transaction.TransactionId);
                }
            log.Info("Transaction {0} completed", transaction.TransactionId);
            transactionsInFlight = Interlocked.Decrement(ref activeTransactions);
            if (transactionsInFlight != 0)
                {
                // This should never happen and if it does then we have a serious concurrency bug
                log.Error("Detected transaction overlap after completing {0}", transaction);
                throw new InvalidOperationException("Detected transaction overlap");
                }
            }
        }
    }