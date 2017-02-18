// This file is part of the TA.Ascom.ReactiveCommunications project
// 
// Copyright © 2017 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so,. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: FakeTransactionProcessor.cs  Last modified: 2017-02-18@00:32 by Tim Long

using System;
using System.Diagnostics.Contracts;

namespace TA.Ascom.ReactiveCommunications.Specifications.Fakes
    {
    internal class FakeTransactionProcessor : ITransactionProcessor
        {
        private readonly IObservable<char> response;

        public FakeTransactionProcessor(IObservable<char> response)
            {
            this.response = response;
            }

        public DeviceTransaction Transaction { get; set; }

        public void CommitTransaction(DeviceTransaction transaction)
            {
            Contract.Requires(transaction != null);
            Transaction = transaction;
            transaction.ObserveResponse(response);
            transaction.MakeHot();
            }
        }
    }