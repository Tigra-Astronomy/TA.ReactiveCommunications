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
// File: ITransactionProcessor.cs  Last modified: 2020-07-20@00:50 by Tim Long

using System;
using System.Diagnostics.Contracts;

namespace Timtek.ReactiveCommunications;

/// <summary>Defines the interface of a transaction processing service.</summary>
[ContractClass(typeof(TransactionProcessorContract))]
public interface ITransactionProcessor
{
    /// <summary>
    ///     Commits a transaction. That is, submits it for execution with no way to cancel. From this
    ///     point, the transaction will either succeed in which case it will contain a valid response, or
    ///     it will fail, in which case the response will be <see cref="Maybe{T}.Empty" />.
    /// </summary>
    /// <param name="transaction">The transaction to be processed.</param>
    void CommitTransaction(DeviceTransaction transaction);
}

[ContractClassFor(typeof(ITransactionProcessor))]
internal abstract class TransactionProcessorContract : ITransactionProcessor
{
    void ITransactionProcessor.CommitTransaction(DeviceTransaction transaction)
    {
        Contract.Requires(transaction != null);
        Contract.Requires(!string.IsNullOrEmpty(transaction.Command));
        throw new NotImplementedException();
    }
}