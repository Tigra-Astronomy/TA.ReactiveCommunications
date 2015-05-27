using System.Reactive.Linq;
using Machine.Specifications;
using TA.Ascom.ReactiveCommunications.Specifications.Fakes;

namespace TA.Ascom.ReactiveCommunications.Specifications.Contexts
    {
    internal class with_fake_transaction_processor
        {
        Establish context = () =>
            {
            var observable = new[] {'M', 'S', 'p', 'e', 'c'}.ToObservable();
            Processor = new FakeTransactionProcessor(observable);
            };
        protected static FakeTransactionProcessor Processor;
        }
    }