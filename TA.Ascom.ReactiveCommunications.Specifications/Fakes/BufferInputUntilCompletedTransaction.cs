// This file is part of the TA.Ascom.ReactiveCommunications project
// 
// Copyright © 2015 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so,. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: BufferInputUntilCompletedTransaction.cs  Last modified: 2015-05-27@02:48 by Tim Long

using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace TA.Ascom.ReactiveCommunications.Specifications.Fakes
    {
    /// <summary>
    ///     Buffers the input sequence effectively forever (actually 1 day).
    ///     When the input sequence completes, the buffer is converted to a string and placed in the Response property.
    /// </summary>
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
            source.Take(completeAfter)
                .Buffer(TimeSpan.FromDays(1))
                .Select(p => new string(p.ToArray()))
                .ObserveOn(NewThreadScheduler.Default)
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