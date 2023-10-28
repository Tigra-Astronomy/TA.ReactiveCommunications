// This file is part of the Timtek.ReactiveCommunications project
//
// Copyright © 2015-2020 Tigra Astronomy, all rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
//
// File: TransactionExtensions.cs  Last modified: 2020-07-18@11:03 by Tim Long

using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using TA.Utils.Core.Diagnostics;

namespace Timtek.ReactiveCommunications.Diagnostics;

public static class TransactionExtensions
{
    /// <summary>Raises a TransactionException for the given transaction.</summary>
    /// <param name="transaction">The transaction causing the exception.</param>
    /// <param name="log">
    ///     An <see cref="ILog" /> instance to which the exception message will be logged at Error
    ///     severity. The exception and transaction details will also be added to the log as properties.
    /// </param>
    /// <exception cref="TransactionException">Always thrown.</exception>
    public static void RaiseException([NotNull] this DeviceTransaction transaction, [CanBeNull] ILog log = null)
    {
        Contract.Requires(transaction != null);
        var message = $"Transaction {transaction} failed: {transaction.ErrorMessage}";
        var transactionException = new TransactionException(message) { Transaction = transaction };
        log?.Error()
            .Message(message)
            .Exception(transactionException)
            .Transaction(transaction)
            .Write();
        throw transactionException;
    }

    /// <summary>
    /// Throws a <see cref="TransactionException"/> if the transaction failed.
    /// If a logging service is supplied in the <paramref name="log"/> parameter, then the failure is logged as an error.
    /// </summary>
    /// <param name="transaction">The transaction that may have failed.</param>
    /// <param name="log">Optional. A logging service that can be used to log the error.</param>
    public static void ThrowIfFailed(this DeviceTransaction transaction, ILog log =null)
    {
        if (transaction.Failed)
            transaction.RaiseException(log);
    }
}