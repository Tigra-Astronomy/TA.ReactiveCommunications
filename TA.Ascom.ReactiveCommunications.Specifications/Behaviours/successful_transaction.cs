using System.Linq;
using Machine.Specifications;
using TA.Ascom.ReactiveCommunications.Specifications.Contexts;

namespace TA.Ascom.ReactiveCommunications.Specifications.Behaviours {
    [Behaviors]
    internal class successful_transaction : rxascom_behaviour
        {
        It should_not_indicate_failed = () => Transaction.Failed.ShouldBeFalse();
        It should_indicate_success = () => Transaction.Successful.ShouldBeTrue();
        It should_indicate_completed = () => Transaction.Completed.ShouldBeTrue();
        It should_have_correct_lifecycle_state = () => Transaction.State.ShouldEqual(TransactionLifecycle.Completed);
        It should_not_have_an_error_message = () => Transaction.ErrorMessage.Any().ShouldBeFalse();
        }
    }