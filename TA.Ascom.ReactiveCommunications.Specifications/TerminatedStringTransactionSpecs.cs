using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using TA.Ascom.ReactiveCommunications.Specifications.Contexts;
using TA.Ascom.ReactiveCommunications.Specifications.Fakes;
using TA.Ascom.ReactiveCommunications.Transactions;

namespace TA.Ascom.ReactiveCommunications.Specifications
{
    class TerminatedStringTransactionSpecs
    {
    [Subject(typeof(TerminatedStringTransaction))]
    internal class when_spurious_characters_are_received_prior_to_the_initiator : with_fake_transaction_processor
        {
        Establish context = () => { Transaction = new TerminatedStringTransaction("command", '#', ':'); };
        Because of = () =>
            {
            Processor = new FakeTransactionProcessor(fakeResponse.ToObservable());
            Processor.CommitTransaction(Transaction);
            Transaction.WaitForCompletionOrTimeout();
            };
        It should_succeed = () => Transaction.Failed.ShouldBeFalse();
        It should_have_a_response = () => Transaction.Response.Any().ShouldBeTrue();
        It should_have_a_value = () => Transaction.Value.ShouldEqual(expectedValue);
        It should_not_have_an_error_message = () => Transaction.ErrorMessage.Any().ShouldBeFalse();
        static TerminatedStringTransaction Transaction;
        private static string fakeResponse = "123:MotherCaughtAFlea#";
        private static string expectedValue = "MotherCaughtAFlea";
        }

    }
}
