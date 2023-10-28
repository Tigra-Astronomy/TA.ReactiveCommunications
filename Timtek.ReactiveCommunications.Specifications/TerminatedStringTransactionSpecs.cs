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
// File: TerminatedStringTransactionSpecs.cs  Last modified: 2020-07-20@00:51 by Tim Long

using System.Linq;
using System.Reactive.Linq;
using Machine.Specifications;
using Timtek.ReactiveCommunications.Specifications.Contexts;
using Timtek.ReactiveCommunications.Specifications.Fakes;
using Timtek.ReactiveCommunications.Transactions;

namespace Timtek.ReactiveCommunications.Specifications
    {
    class TerminatedStringTransactionSpecs
        {
        [Subject(typeof(TerminatedStringTransaction))]
        internal class when_spurious_characters_are_received_prior_to_the_initiator : with_fake_transaction_processor
            {
            static TerminatedStringTransaction Transaction;
            private static string fakeResponse = "123:MotherCaughtAFlea#";
            private static string expectedValue = "MotherCaughtAFlea";
            Establish context = () => { Transaction = new TerminatedStringTransaction("command"); };
            Because of = () =>
                {
                    Processor = new FakeTransactionProcessor(fakeResponse.ToObservable());
                    Processor.CommitTransaction(Transaction);
                    Transaction.WaitForCompletionOrTimeout();
                };
            It should_have_a_response = () => Transaction.Response.Any().ShouldBeTrue();
            It should_have_a_value = () => Transaction.Value.ShouldEqual(expectedValue);
            It should_not_have_an_error_message = () => Transaction.ErrorMessage.Any().ShouldBeFalse();
            It should_succeed = () => Transaction.Failed.ShouldBeFalse();
            }
        }
    }