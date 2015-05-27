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
// File: DeviceTransactionSpecs.cs  Last modified: 2015-05-27@02:48 by Tim Long

using System;
using System.Linq;
using System.Threading.Tasks;
using Machine.Specifications;
using TA.Ascom.ReactiveCommunications.Specifications.Contexts;
using TA.Ascom.ReactiveCommunications.Specifications.Fakes;

namespace TA.Ascom.ReactiveCommunications.Specifications
    {
    [Subject(typeof (DeviceTransaction))]
    internal class when_a_device_transaction_completes : with_fake_transaction_processor
        {
        Establish context = () => { Transaction = new BufferInputUntilCompletedTransaction("command",int.MaxValue); };
        Because of = () =>
            {
            Processor.CommitTransaction(Transaction);
            Transaction.WaitForCompletionOrTimeout();
            };
        It should_succeed = () => Transaction.Failed.ShouldBeFalse();
        It should_have_a_response = () => Transaction.Response.Any().ShouldBeTrue();
        It should_have_a_value = () => Transaction.Value.ShouldEqual("MSpec");
        It should_not_have_an_error_message = () => Transaction.ErrorMessage.Any().ShouldBeFalse();
        static BufferInputUntilCompletedTransaction Transaction;
        }

    [Subject(typeof (DeviceTransaction), "unique identity")]
    internal class when_generating_lots_of_transactions_asynchronously
        {
        Establish context = () => { transactions = new DeviceTransaction[NumberOfTransactions]; };
        Because of = () =>
            {
            var pendingTasks = new Task[NumberOfTransactions];
            for (int i = 0; i < NumberOfTransactions; i++)
                {
                var index = i;
                pendingTasks[index] =
                    Task.Run(() => transactions[index] = new BufferInputUntilCompletedTransaction(index.ToString(),completeAfter:Int32.MaxValue));
                }
            Task.WaitAll(pendingTasks);
            };
        It should_generate_unique_transaction_ids = () =>
            {
            var duplicates = from transaction in transactions
                             group transaction by transaction.TransactionId
                             into byId
                             select byId.Count();
            duplicates.ShouldEachConformTo(p => p == 1);
            };
        static DeviceTransaction[] transactions;
        const int NumberOfTransactions = 10000;
        }
    }