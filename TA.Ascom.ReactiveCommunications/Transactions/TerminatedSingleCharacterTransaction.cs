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
// File: TerminatedSingleCharacterTransaction.cs  Last modified: 2020-07-20@00:50 by Tim Long

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using TA.Utils.Core;

namespace Timtek.ReactiveCommunications.Transactions;

/// <summary>
///     A transaction type that receives a terminated string of any length and uses the first character
///     as the response value.
/// </summary>
public class TerminatedSingleCharacterTransaction : TerminatedStringTransaction
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TerminatedSingleCharacterTransaction" />
    ///     class.
    /// </summary>
    /// <param name="command">The command to be sent to the communications channel.</param>
    /// <param name="terminator">The response terminator character. Optional; defaults to '#'.</param>
    public TerminatedSingleCharacterTransaction(string command, char terminator = '#') : base(command, terminator)
    {
        Contract.Requires(!string.IsNullOrEmpty(command));
    }

    /// <summary>Gets the response value of the completed transaction.</summary>
    /// <value>The value, a single character.</value>
    public new char Value { get; private set; }

    /// <summary>
    ///     Called when the input sequence completes. Sets the <see cref="Value" /> property to the first
    ///     character of the response string.
    /// </summary>
    protected override void OnCompleted()
    {
        Value = default;
        try
        {
            if (Response.Any())
            {
                Value = Response.Single()[0];
            }
        }
        catch (FormatException)
        {
            Value = default;
            State = TransactionLifecycle.Failed;
            ErrorMessage = Maybe<string>.From("Unable to convert the response to a single character value");
        }
        base.OnCompleted();
    }
}