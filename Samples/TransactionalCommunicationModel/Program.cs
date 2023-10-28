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
// File: Program.cs  Last modified: 2020-07-18@11:57 by Tim Long

using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using SimulatorChannel;
using TA.Utils.Core;
using TA.Utils.Logging.NLog;
using Timtek.ReactiveCommunications;
using Timtek.ReactiveCommunications.Diagnostics;

namespace TransactionalCommunicationModel
    {
    /*
     * This simple console application demonstrates the Transactional Model of device communications.
     * We define 2 transaction types:
     * - StatusTransaction, which requests a status report and returns a HardwareStatus object.
     * - SetUserPinsTransaction, which sets the state of the user GPIO pins and returns the new pin states as an Octet.
     *
     * The program first obtains a status report using StatusTransaction
     * and extracts the current value of the user GPIO pins from that.
     * Then it turns on Pin 0 and writes the new state using the SetUserPinsTransaction.
     * The result from that transaction is used to check that the pins were set correctly.
     *
     * No device is needed to run this application, because a hardware simulator is used.
     * The simulator code was abstracted from the Digital DomeWorks ASCOM driver
     * and comes in the form os a custom communications channel.
     */
    internal class Program
        {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("AppSettings.json", optional: false, reloadOnChange: true)
            .Build();

        private static void Main(string[] args)
            {
            #region Setup for Reactive ASCOM
            /*
             * First create our channel factory.
             * This is also the place where we can inject a logging service.
             * If you don't provide a logging service here, there will be no logging!
             * Note that logging is configured in NLog.config.
             * See also: TA.Utils.Core.Diagnostics
             *
             * The channel factory knows about serial ports by default, but we are going to
             * register a custom channel implementation that contains a hardware simulator.
             * (the simulator is abstracted from the Digital Domeworks dome driver).
             * If you make your own custom channels, you can register them the same way.
             */
            var log = new LoggingService(); // TA.Utils.Logging.NLog
            log.Info()
                .Message("Program start. Version {gitversion}", GitVersion.GitInformationalVersion)
                .Property("CommitSha", GitVersion.GitCommitSha)
                .Property("CommitDate", GitVersion.GitCommitDate)
                .Write();

            var channelFactory = new ChannelFactory(log);
            channelFactory.RegisterChannelType(
                SimulatorEndpoint.IsConnectionStringValid,
                SimulatorEndpoint.FromConnectionString,
                endpoint => new SimulatorCommunicationsChannel((SimulatorEndpoint) endpoint, log));

            /*
             * Now get our connection string from the AppSettings.json file.
             */

            var connectionString = Configuration["ConnectionString"];

            /*
             * Create the communications channel and connect it to the transaction observer.
             * The TransactionObserver is the beating heart of Rx Comms and is where everything is synchronized.
             */
            var channel = channelFactory.FromConnectionString(connectionString);
            var transactionObserver = new TransactionObserver(channel);

            /*
             * TransactionObsever is an IObservable<DeviceTransaction>, so we need a sequence for it to observe.
             * The transaction sequence can be created by any convenient means, and Rx Comms provides
             * a handy helper class for doing this, called ReactiveTransactionProcessor.
             *
             * However the sequence is created, it must be fed into the transaction observer
             * by creating a subscription. ReactiveTransactionProcessor has a helper method for that.
             * The helper method also gives us a way to rate-limit commands to the hardware, if needed.
             */
            var transactionProcessor = new ReactiveTransactionProcessor();
            transactionProcessor.SubscribeTransactionObserver(transactionObserver, TimeSpan.FromSeconds(1));
            channel.Open();
            #endregion Setup for Reactive ASCOM

            // All set up and ready to go.
            try
                {
                // First we'll create a StatusTransaction and get the device's status.
                var statusTransaction = new StatusTransaction();
                transactionProcessor.CommitTransaction(statusTransaction);
                statusTransaction.WaitForCompletionOrTimeout(); // There is also an async version

                // If the transaction failed, log and throw TransactionException.
                TransactionExtensions.ThrowIfFailed(statusTransaction, log);

                // Now we can retrieve the results and use them.
                // If the transaction succeeded, we should be assured of a valid result.
                // When you write your own transactions, make sure this is so!
                var status = statusTransaction.HardwareStatus;
                log.Debug().Message("Got status: {deviceStatus}", status).Write();

                // Get the user GPIO pins and flip the value of pin 0.
                var gpioPins = status.UserPins;
                var gpioPinsToWrite = gpioPins.WithBitSetTo(0, !gpioPins[0]);

                // Now we'll use another transaction to set the new GPIO pin state.
                var gpioTransaction = new SetUserPinTransaction(gpioPinsToWrite);
                transactionProcessor.CommitTransaction(gpioTransaction);
                gpioTransaction.WaitForCompletionOrTimeout();

                TransactionExtensions.ThrowIfFailed(gpioTransaction, log);
                var newPinState = gpioTransaction.UserPins;
                if (newPinState != gpioPinsToWrite)
                    throw new ApplicationException("Failed to write GPIO pins");
                log.Info()
                    .Message("Successfully changed GPIO pins {oldState} => {newState}", gpioPins, newPinState)
                    .Write();
                }
            catch (TransactionException exception)
                {
                log.Error().Exception(exception)
                    .Message("Transaction failed {transaction}", exception.Transaction)
                    .Write();
                Console.WriteLine(exception);
                }
            catch (ApplicationException exception)
                {
                log.Error().Message("Failed to set teh GPIO pins.").Exception(exception).Write();
                }
            finally
                {
                /*
                 * We just need to dispose the transaction processor. This terminates the
                 * sequence of transactions, which causes the TransactionObserver's OnCompleted method
                 * to be called, which unsubscribes the receive sequence, which closes the
                 * communications channel.
                 */
                transactionProcessor.Dispose();
                log.Info().Message("Cleanup complete").Write();

                /*
                 * It is best practice to explicitly shut down the logging service,
                 * otherwise any buffered log entries might not be written to the log.
                 * This may take a few seconds if you are using a slow log target.
                 */
                log.Shutdown();
                }
            }
        }
    }