// This file is part of the TA.Ascom.ReactiveCommunications project
// 
// Copyright © 2018 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: DeviceTransactionSpecs.cs  Last modified: 2018-08-26@14:52 by Tim Long

using System;
using System.Linq;
using System.Threading.Tasks;
using Machine.Specifications;
using TA.Ascom.ReactiveCommunications.Specifications.Behaviours;
using TA.Ascom.ReactiveCommunications.Specifications.Contexts;
using TA.Ascom.ReactiveCommunications.Transactions;

namespace TA.Ascom.ReactiveCommunications.Specifications
    {
    [Subject(typeof(DeviceTransaction))]
    internal class when_a_device_transaction_completes : with_rxascom_context
        {
        Establish context = () => Context = RxAscomContextBuilder
            .WithOpenConnection("Fake")
            .WithFakeResponse("Test")
            .Build();
        Because of = () =>
            {
            Transaction = new NoReplyTransaction("Test");
            Processor.CommitTransaction(Transaction);
            Transaction.WaitForCompletionOrTimeout();
            };
        Behaves_like<successful_transaction> a_successful_transaction;
        }

    [Subject(typeof(DeviceTransaction), "unique identity")]
    internal class when_generating_lots_of_transactions_asynchronously
        {
        Because of = () =>
            {
            var pendingTasks = new Task[NumberOfTransactions];
            for (var i = 0; i < NumberOfTransactions; i++)
                {
                var index = i;
                pendingTasks[index] =
                    Task.Run(() => transactions[index] = new NoReplyTransaction(index.ToString()));
                }

            Task.WaitAll(pendingTasks);
            };
        It should_generate_unique_transaction_ids = () =>
            transactions.Select(t => t.TransactionId).Distinct().Count().ShouldEqual(transactions.Length);
        static DeviceTransaction[] transactions = new DeviceTransaction[NumberOfTransactions];
        const int NumberOfTransactions = 10000;
        }

    [Subject(typeof(DeviceTransaction), "lifecycle")]
    internal class when_creating_a_new_transaction
        {
        Because of = () => transaction = new BooleanTransaction("Dummy");
        It should_have_created_state = () => transaction.State.ShouldEqual(TransactionLifecycle.Created);
        It should_include_created_in_tostring =
            () => transaction.ToString().ShouldContain(nameof(TransactionLifecycle.Created));
        static DeviceTransaction transaction;
        }

    [Subject(typeof(DeviceTransaction), "lifecycle")]
    internal class when_a_transaction_times_out : with_rxascom_context
        {
        Establish context = () => Context = RxAscomContextBuilder
            .WithOpenConnection("Fake")
            .Build();
        Because of = () =>
            {
            Transaction = new BooleanTransaction("Dummy") {Timeout = TimeSpan.FromMilliseconds(1)};
            Processor.CommitTransaction(Transaction);
            Transaction.WaitForCompletionOrTimeout();
            };
        It should_include_failed_in_tostring = () => Transaction.ToString().ShouldContain("Failed");
        It should_give_timeout_as_the_reason_for_failure = () =>
            Transaction.ErrorMessage.Single().ShouldContain("Timed out");
        Behaves_like<failed_transaction> a_failed_transaction;
        }

    [Subject(typeof(DeviceTransaction), "lifecycle")]
    internal class when_a_transaction_is_in_progress : with_rxascom_context
        {
        Establish context = () => Context = RxAscomContextBuilder
            .WithOpenConnection("Fake")
            .Build();
        Because of = () =>
            {
            Transaction = new BooleanTransaction("Dummy");
            Processor.CommitTransaction(Transaction);
            };
        It should_have_lifecycle_state_in_progress =
            () => Transaction.State.ShouldEqual(TransactionLifecycle.InProgress);
        It should_not_be_completed = () => Transaction.Completed.ShouldBeFalse();
        It should_not_be_successful = () => Transaction.Successful.ShouldBeFalse();
        It should_not_be_failed = () => Transaction.Failed.ShouldBeFalse();
        }
    }