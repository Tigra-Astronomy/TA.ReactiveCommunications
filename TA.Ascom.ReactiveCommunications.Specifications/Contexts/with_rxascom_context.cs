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
// File: with_rxascom_context.cs  Last modified: 2020-07-20@00:51 by Tim Long

using Machine.Specifications;
using TA.Ascom.ReactiveCommunications.Specifications.Fakes;
using Timtek.ReactiveCommunications;

namespace TA.Ascom.ReactiveCommunications.Specifications.Contexts
    {
    class with_rxascom_context
        {
        protected static RxAscomContextBuilder RxAscomContextBuilder;
        protected static RxAscomContext Context;
        Cleanup after = () =>
            {
            RxAscomContextBuilder = null;
            Context = null;
            };
        Establish context = () => RxAscomContextBuilder = new RxAscomContextBuilder();

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
        }
    }