using Machine.Specifications;
using TA.Ascom.ReactiveCommunications.Specifications.Fakes;

namespace TA.Ascom.ReactiveCommunications.Specifications.Contexts
    {
    class with_rxascom_context
        {
        Establish context = () => RxAscomContextBuilder = new RxAscomContextBuilder();
        Cleanup after = () =>
            {
            RxAscomContextBuilder = null;
            Context = null;
            };

        #region Convenience properties
        public static ICommunicationChannel Channel => Context.Channel;
        public static FakeCommunicationChannel FakeChannel => Context.Channel as FakeCommunicationChannel;
        public static ITransactionProcessor Processor => Context.Processor;
        public static TransactionObserver Observer => Context.Observer;
        public static DeviceTransaction Transaction
            {
            get => Context.Transaction;
            set => Context.Transaction = value;
            }
        #endregion Convenience properties

        protected static RxAscomContextBuilder RxAscomContextBuilder;
        protected static RxAscomContext Context;
        }
    }