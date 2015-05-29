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
// File: SerialObservableExtensions.cs  Last modified: 2015-05-29@19:58 by Tim Long

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO.Ports;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using NLog;

namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>
    ///     Provides extension methods for capturing and transforming observable sequences of characters received from a serial
    ///     port.
    /// </summary>
    public static class SerialObservableExtensions
        {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Captures the <see cref="System.IO.Ports.SerialPort.DataReceived" /> event of a serial port and returns an
        ///     observable sequence of the events. This implicitly opens the serial port whenever there is an active subscription
        ///     and closes it when the subscription is disposed.
        /// </summary>
        /// <param name="port">The serial port that will act as the event source.</param>
        /// <returns><see cref="IObservable{Char}" /> - an observable sequence of events.</returns>
        public static IObservable<EventPattern<SerialDataReceivedEventArgs>> ObservableDataReceivedEvents(
            this ISerialPort port)
            {
            Contract.Requires(port != null);
            Contract.Ensures(Contract.Result<IObservable<EventPattern<SerialDataReceivedEventArgs>>>() != null);
            var portEvents = Observable.FromEventPattern<SerialDataReceivedEventHandler, SerialDataReceivedEventArgs>(
                handler =>
                    {
                    Log.Debug("Event: SerialDataReceived");
                    return handler.Invoke;
                    },
                handler =>
                    {
                    // We must discard stale data when subscribing or it will pollute the first element of the sequence.
                    port.DiscardInBuffer();
                    port.DataReceived += handler;
                    port.Open();
                    Log.Debug("Listening to DataReceived event");
                    },
                handler =>
                    {
                    port.Close();
                    port.DataReceived -= handler;
                    Log.Debug("Stopped listening to DataReceived event");
                    });
            return portEvents;
            }

        /// <summary>
        ///     Gets an observable sequence of all the characters received by a serial port.
        /// </summary>
        /// <param name="port">The port that is to be the data source.</param>
        /// <returns><see cref="IObservable{Char}" /> - an observable sequence of characters.</returns>
        public static IObservable<char> ReceivedCharacters(this ISerialPort port)
            {
            Contract.Requires(port != null);
            Contract.Ensures(Contract.Result<IObservable<char>>() != null);
            var observableEvents = port.ObservableDataReceivedEvents();
            var observableCharacterSequence = from args in observableEvents
                                              where args.EventArgs.EventType == SerialData.Chars
                                              from character in port.ReadExisting()
                                              select character;
            return observableCharacterSequence;
            }

        /// <summary>
        ///     Produces a sequence of strings delimited by the specified <paramref name="terminator" /> character.
        /// </summary>
        /// <param name="source">The source character sequence.</param>
        /// <param name="terminator">The terminator that will delimit the strings.</param>
        /// <returns><see cref="IObservable{String}" /> - a sequence of strings.</returns>
        public static IObservable<string> TerminatedStrings(this IObservable<char> source, char terminator = '#')
            {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<IObservable<string>>() != null);
            var terminatorString = terminator.ToString();
            var sequence = source.Scan("", (agg, c) => agg.EndsWith(terminatorString) ? c.ToString() : agg + c)
                .Where(s => s.EndsWith(terminatorString));
            return sequence;
            }

        /// <summary>
        ///     Converts a sequence of characters into a sequence of time-delimited strings.
        /// </summary>
        /// <param name="source">The source sequence of characters.</param>
        /// <param name="quietTime">
        ///     The period of time that the receive sequence must be quiescent before the next string will be
        ///     emitted.
        /// </param>
        /// <returns><see cref="IObservable{String}" /> - a sequence of time-delimited strings.</returns>
        public static IObservable<string> TimeDelimitedString(this IObservable<char> source, TimeSpan quietTime)
            {
            Contract.Requires(source != null);
            Contract.Requires(quietTime.TotalMilliseconds > 0);
            Contract.Ensures(Contract.Result<IObservable<string>>() != null);
            var shared = source.Publish().RefCount();
            var buffers = shared.Buffer(() => shared.Throttle(quietTime));
            var strings = buffers.Select(buffer => new string(buffer.ToArray()));
            return strings;
            }

        /// <summary>
        ///     Parses a sequence of characters into a sequence of strings, based on delimiters.
        ///     The delimiters are included in the strings. Characters that do not occur between delimiters are discarded from the
        ///     sequence.
        /// </summary>
        /// <param name="source">The source sequence of characters.</param>
        /// <param name="initiator">The initiator character that triggers the start of a new string.</param>
        /// <param name="terminator">The terminator character that marks the end of a string.</param>
        /// <returns>
        ///     <see cref="IObservable{String}" />, an observable sequence of strings each delimited by an initiator character
        ///     and a terminator character.
        /// </returns>
        public static IObservable<string> DelimitedMessageStrings(
            this IObservable<char> source,
            char initiator = ':',
            char terminator = '#')
            {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<IObservable<string>>() != null);
            var buffers = source.Publish(s => BufferByDelimiters(s, initiator, terminator));
            var strings = from buffer in buffers
                          select new string(buffer.ToArray());
            return strings;
            }

        /// <summary>
        ///     Buffers a sequence of characters based on a pair of delimiters.
        /// </summary>
        /// <param name="source">The source sequence.</param>
        /// <param name="initiator">The initiator character. Optional; default is ':'."</param>
        /// <param name="terminator">The terminator character. Optional; default is '#'."</param>
        /// <returns><see cref="IObservable{T}" /> - an observable sequence of buffers.</returns>
        public static IObservable<IList<char>> BufferByDelimiters(IObservable<char> source, char initiator = ':',
            char terminator = '#')
            {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<IObservable<IList<char>>>() != null);
            return source.Buffer(source.Where(c => c == initiator), x => source.Where(c => c == terminator));
            }

        /// <summary>
        ///     Selects sequences of "0#" and "1#" and produces "0" or "1".
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="terminator">The terminator. Optional; default is '#'</param>
        /// <returns><see cref="IObservable{String}" />.</returns>
        public static IObservable<string> TerminatedBoolean(this IObservable<char> source, char terminator = '#')
            {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<IObservable<string>>() != null);
            var results = from s in source.TerminatedStrings(terminator)
                          where "01".Contains(s[0])
                          select s.Substring(0, 1);
            return results;
            }
        }
    }