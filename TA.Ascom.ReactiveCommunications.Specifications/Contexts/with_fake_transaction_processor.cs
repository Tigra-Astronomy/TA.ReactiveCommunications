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
// File: with_fake_transaction_processor.cs  Last modified: 2020-07-20@00:51 by Tim Long

using System.Reactive.Linq;
using Machine.Specifications;
using TA.Ascom.ReactiveCommunications.Specifications.Fakes;

namespace TA.Ascom.ReactiveCommunications.Specifications.Contexts
    {
    #region Context base classes
    internal class with_fake_transaction_processor
        {
        protected static FakeTransactionProcessor Processor;
        Establish context = () =>
            {
            var observable = new[] {'M', 'S', 'p', 'e', 'c'}.ToObservable();
            Processor = new FakeTransactionProcessor(observable);
            };
        }
    #endregion
    }