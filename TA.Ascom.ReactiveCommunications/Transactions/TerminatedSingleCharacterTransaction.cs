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
// File: TerminatedSingleCharacterTransaction.cs  Last modified: 2015-05-25@18:22 by Tim Long

using System;
using System.Linq;

namespace TA.Ascom.ReactiveCommunications.Transactions
    {
    /// <summary>
    ///     A transaction type that receives a terminated string response and uses the first character as the response value.
    /// </summary>
    public class TerminatedSingleCharacterTransaction : TerminatedStringTransaction
        {
        public char Value { get; private set; }

        public TerminatedSingleCharacterTransaction(string command) : base(command) {}

        protected override void OnCompleted()
            {
            Value = default(char);
            try
                {
                if (Response.Any())
                    {
                    Value = Response.Single()[0];
                    }
                }
            catch (FormatException)
                {
                Value = default(char);
                Failed = true;
                ErrorMessage = new Maybe<string>("Unable to extract single character from response string");
                }
            base.OnCompleted();
            }
        }
    }