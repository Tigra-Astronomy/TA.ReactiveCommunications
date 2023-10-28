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
// File: rxascom_behaviour.cs  Last modified: 2020-07-20@00:51 by Tim Long

using JetBrains.Annotations;
using Timtek.ReactiveCommunications.Specifications.Contexts;
using Timtek.ReactiveCommunications;

#pragma warning disable 0649 // Context never assigned

namespace Timtek.ReactiveCommunications.Specifications.Behaviours
    {
    [UsedImplicitly]
    internal class rxascom_behaviour
        {
        protected static RxAscomContext Context;

        protected static ITransactionProcessor Processor => Context.Processor;

        protected static ICommunicationChannel Channel => Context.Channel;

        protected static DeviceTransaction Transaction => Context.Transaction;
        }
    }