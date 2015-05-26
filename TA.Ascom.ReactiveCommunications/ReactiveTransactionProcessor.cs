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
// File: ReactiveTransactionProcessor.cs  Last modified: 2015-05-25@18:23 by Tim Long

using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace TA.Ascom.ReactiveCommunications
    {
    public class ReactiveTransactionProcessor : ITransactionProcessor
        {
        public void CommitTransaction(DeviceTransaction transaction)
            {
            OnTransactionAvailable(transaction);
            }

        /// <summary>
        ///     Connects this transaction processor to a transaction observer, which will orchestrate the transaction and arrange
        ///     for it to receive appropriate responses froma communications channel.
        /// </summary>
        /// <param name="observer">The transaction observer.</param>
        public void SubscribeTransactionObserver(TransactionObserver observer)
            {
            var observableTransactionSource = Observable
                .FromEventPattern<DeviceTransaction>(
                    handler => TransactionAvailable += handler,
                    handler => TransactionAvailable -= handler);
            observableTransactionSource.Select(e => e.EventArgs)
                .ObserveOn(NewThreadScheduler.Default)
                .Subscribe(observer);
            }

        #region .NET Standard Event Pattern
        public event EventHandler<DeviceTransaction> TransactionAvailable;


        protected virtual void OnTransactionAvailable(DeviceTransaction transaction)
            {
            if (TransactionAvailable != null)
                {
                TransactionAvailable(this, transaction);
                }
            }
        #endregion .NET Standard Event Pattern
        }
    }