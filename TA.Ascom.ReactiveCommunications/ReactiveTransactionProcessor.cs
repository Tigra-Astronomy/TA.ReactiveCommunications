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
// File: ReactiveTransactionProcessor.cs  Last modified: 2015-05-26@18:14 by Tim Long

using System;
using System.Diagnostics.Contracts;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>
    ///     A transaction processor that raises a .NET event as each transaction becomes available.
    ///     The transaction is provided as part of the event arguments.
    /// </summary>
    public class ReactiveTransactionProcessor : ITransactionProcessor
        {
        /// <summary>
        ///     Commits a transaction. That is, submits it for execution with no way to cancel. From this point, the
        ///     transaction will either succeed in which case it will contain a valid response, (
        ///     <c>Response.Any() == true</c>) or it will fail, in which case the response will not have a value (
        ///     <see cref="Maybe{T}.Empty" /> or <c>Response.Any() == false</c>).
        /// </summary>
        /// <param name="transaction">The transaction to be processed.</param>
        public void CommitTransaction(DeviceTransaction transaction)
            {
            OnTransactionAvailable(transaction);
            }

        /// <summary>
        ///     Connects this transaction processor to a transaction observer, which will orchestrate the transaction and
        ///     arrange for it to receive appropriate responses from a communications channel.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The transaction sequence is observed on a free-threaded worker thread determined by Rx's default scheduler.
        ///         This is done to avoid any potential threading and reentrancy issues when running in a single threaded
        ///         apartment (STA thread) such as a Windows Forms GUI thread. This has been observed to cause sequencing issues
        ///         and in an STA thread, constructs such as <c>lock ()</c> and <c>WaitHandle.WaitOne()</c> are useless.
        ///     </para>
        ///     <para>
        ///         If different threading semantics are required, then you can inherit from this class and
        ///         <see langword="override" /> the <see cref="ReactiveTransactionProcessor.SubscribeTransactionObserver" />
        ///         method.
        ///     </para>
        /// </remarks>
        /// <param name="observer">The transaction observer.</param>
        public void SubscribeTransactionObserver(TransactionObserver observer)
            {
            Contract.Requires(observer != null);
            var observableTransactionSource = Observable
                .FromEventPattern<DeviceTransaction>(
                    handler => TransactionAvailable += handler,
                    handler => TransactionAvailable -= handler);
            observableTransactionSource.Select(e => e.EventArgs)
                .ObserveOn(NewThreadScheduler.Default)
                .Subscribe(observer);
            }

        #region .NET Standard Event Pattern
        /// <summary>
        ///     Occurs when a new transaction becomes available. The transaction is contained in the event arguments.
        /// </summary>
        public event EventHandler<DeviceTransaction> TransactionAvailable;


        /// <summary>
        ///     Raises the <see cref="TransactionAvailable" /> event. The new transaction must be supplied as an argument.
        /// </summary>
        /// <param name="transaction">The transaction that has become available.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        protected virtual void OnTransactionAvailable(DeviceTransaction transaction)
            {
            Contract.Requires(transaction != null);
            Contract.Requires(!string.IsNullOrEmpty(transaction.Command));
            if (TransactionAvailable != null)
                {
                TransactionAvailable(this, transaction);
                }
            }
        #endregion .NET Standard Event Pattern
        }
    }