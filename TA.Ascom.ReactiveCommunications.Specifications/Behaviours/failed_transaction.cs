using System.Linq;
using Machine.Specifications;
using TA.Ascom.ReactiveCommunications.Specifications.Contexts;

namespace TA.Ascom.ReactiveCommunications.Specifications.Behaviours {
    [Behaviors]
    internal class failed_transaction : rxascom_behaviour
        {
        It should_indicate_failed = () => Transaction.Failed.ShouldBeTrue();
        It should_not_indicate_success = () => Transaction.Successful.ShouldBeFalse();
        It should_indicate_completed = () => Transaction.Completed.ShouldBeTrue();
        It should_have_correct_lifecycle_state = () => Transaction.State.ShouldEqual(TransactionLifecycle.Failed);
        It should_have_a_reason_for_failure = () => Transaction.ErrorMessage.Any().ShouldBeTrue();
        }
    }