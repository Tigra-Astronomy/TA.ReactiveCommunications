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
// File: Program.cs  Last modified: 2015-05-25@18:23 by Tim Long

using System;
using System.Threading.Tasks;
using TA.Ascom.ReactiveCommunications.Sample.ConsoleApp.Properties;
using TA.Ascom.ReactiveCommunications.Transactions;

namespace TA.Ascom.ReactiveCommunications.Sample.ConsoleApp
    {
    /// <summary>
    ///     This simple console application demonstrates the bare minimum requirements to set up and use the reactive
    ///     communications library for ASCOM. It creates a connection to a device, which is assumed to implement a
    ///     Meade-style protocol, and queries the Right Ascension and Declination coordinates as string values.
    /// </summary>
    /// <remarks>
    ///     Please refer to the accompanying ReadMe.md markdown file for notes on this implementation.
    /// </remarks>
    internal class Program
        {
        static void Main(string[] args)
            {
            #region Setup for Reactive ASCOM
            var connectionString = Settings.Default.ConnectionString; // Edit in App.config, default is "COM1:"
            var channelFactory = new ChannelFactory();
            var channel = channelFactory.FromConnectionString(connectionString);
            var transactionObserver = new TransactionObserver(channel);
            var transactionProcessor = new ReactiveTransactionProcessor();
            transactionProcessor.SubscribeTransactionObserver(transactionObserver);
            channel.Open();
            #endregion Setup for Reactive ASCOM

            #region Submit some transactions
            // Ready to go. We are going to use tasks to submit the transactions, just to demonstrate thread safety.
            var raTransaction = new TerminatedStringTransaction(":GR#", '#', ':') {Timeout = TimeSpan.FromSeconds(2)};
            // The terminator and initiator are optional parameters and default to values that work for Meade style protocols.
            var decTransaction = new TerminatedStringTransaction(":GD#") {Timeout = TimeSpan.FromSeconds(2)};
            Task.Run(() => transactionProcessor.CommitTransaction(raTransaction));
            Task.Run(() => transactionProcessor.CommitTransaction(decTransaction));
            #endregion Submit some transactions

            #region Wait for the results
            // NOTE we are using the transactions in the reverse order that we committed them, just to prove a point.
            Console.WriteLine("Waiting for declination");
            decTransaction.WaitForCompletionOrTimeout();
            Console.WriteLine("Declination: {0}", decTransaction.Response);
            Console.WriteLine("Waiting for Right Ascensions");
            raTransaction.WaitForCompletionOrTimeout();
            Console.WriteLine("Right Ascension: {0}", raTransaction.Response);
            #endregion Wait for the results

            #region Cleanup
            // To clean up, we just need to dispose the transactionProcessor
            transactionProcessor.Dispose();
            #endregion Cleanup

            Console.WriteLine("Press ENTER:");
            Console.ReadLine();
            }
        }
    }