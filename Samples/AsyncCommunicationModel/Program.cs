﻿// This file is part of the TA.Ascom.ReactiveCommunications project
// 
// Copyright © 2015-2020 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: Program.cs  Last modified: 2020-07-20@00:49 by Tim Long

using System;
using System.IO;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using SimulatorChannel;
using TA.Utils.Core;
using TA.Utils.Core.Diagnostics;
using TA.Utils.Logging.NLog;
using Timtek.ReactiveCommunications;
using Timtek.ReactiveCommunications.Diagnostics;

namespace AsyncCommunicationModel
    {
    /// <summary>
    ///     This simple console application demonstrates the bare minimum requirements to set up and use
    ///     the reactive communications library for ASCOM. It creates a connection to a device, which is
    ///     assumed to implement a Meade-style protocol, and queries the Right Ascension and Declination
    ///     coordinates as string values, using one of the basic transaction classes built into the
    ///     library.
    /// </summary>
    /// <remarks>Please refer to the accompanying ReadMe.md markdown file for notes on this implementation.</remarks>
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
                enpoint => new SimulatorCommunicationsChannel((SimulatorEndpoint) enpoint, log));

            /*
             * Now get out connection string from the AppSettings.json file.
             */

            var connectionString = Configuration["ConnectionString"];

            /*
             * Create the communications channel.
             * We will not be using any transactions in this sample, so
             * we can just open the channel and start using it.
             */
            var channel = channelFactory.FromConnectionString(connectionString);
            channel.Open();
            #endregion Setup for Reactive ASCOM

            /*
             * All set up and ready to go.
             */

            // Write position updates out to the console.
            var encoderTickSubscription = MonitorPositionUpdates(channel, log);

            // Now we are going to instruct the simulated observatory dome to rotate to 180 degrees.

            var rotateCommand = string.Format(Constants.CmdGotoAz, 180);
            channel.Send(rotateCommand);

            /*
             * Notice how everything happens asynchronously from this point.
             * The simulated dome starts rotating and emits encoder ticks as it goes.
             * The Rx LINQ query we set up sees those and prints them to the console,
             * while log entries are emitted  from RxComms and the simulator.
             * The "press enter" message from below will most likely scroll right off
             * the screen if console logging is enabled.
             */

            Console.WriteLine("Press ENTER to clean up");
            Console.ReadLine();

            #region Cleanup
            log.Info().Message("Cleaning up for exit").Write();

            // We can first clean up our subscription that is monitoring position updates.
            encoderTickSubscription.Dispose();

            // Then dispose the channel. In general, disposing an object causes it to clean
            // up any references it is holding, so this will close the channel cleanly.
            // Check the log output to verify that the channel is closed.
            channel.Dispose();

            log.Info().Message("Cleanup complete").Write();
            Console.WriteLine("Cleanup complete. Press ENTER to quit.");
            Console.ReadLine();

            /*
             * It is best practice to explicitly shut down the logging service,
             * otherwise any buffered log entries might not be written to the log.
             * This may take a few seconds if you are using a slow log target.
             */
            log.Shutdown();
            #endregion Cleanup
            }

        /*
         * This method composes a query over the received character sequence that looks for
         * azimuth encoder position updates. When it finds one, it is written out to the console.
         * The method returns an IDisposable that allows the subscription to be properly cleaned up later.
         */
        private static IDisposable MonitorPositionUpdates(ICommunicationChannel channel, ILog log)
            {
            /*
             * We can compose a LINQ query to look for azimuth encoder updates coming from the device.
             * These are in the form "Pnnnn" (where nnnn is 1 to 4 decimal digits)
             * and are always followed by a newline character.
             * We can start to narrow the input down by looking for a string
             * beginning with "P" and ending with newline. Rx Comms has a few LINQ query operators
             * to help with this sort of thing, so it is easy to do.
             */
            var observableEncoderTicks = channel.ObservableReceivedCharacters.DelimitedMessageStrings('P', '\n')
                /*
                 * If we want diagnostic logging, we can augment the query with the Trace method.
                 */
                .Trace("Potential Encoder Tick")
                /*
                 * I don't want the newlines on the end of each string, so we can remove those with a LINQ projection.
                 */
                .Select(item => item.Trim());

            /*
             * Once I have my input sequence chopped up into strings, I like to do further analysis
             * to make sure I am only accepting 100% valid data. Typically, I'll test the string
             * against a regular expression. These are cryptic but very powerful. So let's build
             * a regular expression parser.
             *
             * The pattern here looks for:
             *   ^  start of string
             *   P  literal character "P"
             *   (?<Position ... ) a named capture group called "Position"
             *   \d{1,4}  at least 1 and at most 4 decimal digits
             *   $  The end of the string
             */
            var azimuthEncoderTickPattern = @"^P(?<Position>\d{1,4})$";
            var regexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;
            var azimuthEncoderRegex = new Regex(azimuthEncoderTickPattern, regexOptions);

            /*
             * Now we can further compose our query with our regular expression parser to make it
             * "bomb proof" and extract just the position values. Just for variety, I'll use
             * LINQ Comprehension Query syntax.
             */

            var observablePositions = from maybePosition in observableEncoderTicks
                                      let regexMatch = azimuthEncoderRegex.Match(maybePosition)
                                      where regexMatch.Success // This rejects the unsuccessful matches
                                      select regexMatch.Groups["Position"].Value;

            // Then we need to subscribe to the sequence. We'll just write it out to the console,
            var disposable = observablePositions.Subscribe(
                item => log.Info()
                    .LoggerName("Position update")
                    .Message("Got position {position}", item)
                    .Write()
            );
            return disposable;
            }
        }
    }