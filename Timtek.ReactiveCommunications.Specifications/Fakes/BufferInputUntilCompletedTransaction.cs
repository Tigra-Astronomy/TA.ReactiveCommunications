// This file is part of the Timtek.ReactiveCommunications project
// 
// Copyright � 2015-2020 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: BufferInputUntilCompletedTransaction.cs  Last modified: 2020-07-20@00:51 by Tim Long

using System;
using System.Linq;
using System.Reactive.Linq;
using Timtek.ReactiveCommunications;

namespace Timtek.ReactiveCommunications.Specifications.Fakes
    {
    /// <summary>
    ///     Buffers the input sequence effectively forever (actually 1 day). When the input sequence
    ///     completes, the buffer is converted to a string and placed in the Response property.
    /// </summary>
    [Obsolete("This class tends to yield brittle tests. Consider building a test using with_rxascom_context instead.")]
    internal class BufferInputUntilCompletedTransaction : DeviceTransaction
        {
        readonly int completeAfter;

        public BufferInputUntilCompletedTransaction(string command, int completeAfter) : base(command)
            {
            this.completeAfter = completeAfter;
            }

        public string Value { get; private set; }

        public override void ObserveResponse(IObservable<char> source)
            {
            source.Buffer(completeAfter)
                .Select(p => new string(p.ToArray()))
                //.ObserveOn(NewThreadScheduler.Default)
                .Take(1)
                .Subscribe(OnNext, OnError, OnCompleted);
            }

        protected override void OnCompleted()
            {
            if (Response.Any())
                Value = Response.Single();
            base.OnCompleted();
            }
        }
    }