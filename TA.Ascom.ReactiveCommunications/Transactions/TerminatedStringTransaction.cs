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
// File: TerminatedStringTransaction.cs  Last modified: 2015-05-25@18:22 by Tim Long

using System;
using System.Linq;
using System.Reactive.Linq;

namespace TA.Ascom.ReactiveCommunications.Transactions
    {
    public class TerminatedStringTransaction : DeviceTransaction
        {
        public TerminatedStringTransaction(string command) : base(command) {}

        public string Value { get; private set; }

        public override void ObserveResponse(IObservable<char> source)
            {
            source.TerminatedStrings()
                .Take(1)
                .Subscribe(OnNext, OnError, OnCompleted);
            }

        protected override void OnCompleted()
            {
            var debug = Response;
            var any = Response.Any();
            var responseString = Response.Single();
            Value = responseString.TrimStart(':').TrimEnd('#');
            base.OnCompleted();
            }
        }
    }