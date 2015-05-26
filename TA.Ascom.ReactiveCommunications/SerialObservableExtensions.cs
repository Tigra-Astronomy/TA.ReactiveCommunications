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
// File: SerialObservableExtensions.cs  Last modified: 2015-05-25@18:23 by Tim Long

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using NLog;

namespace TA.Ascom.ReactiveCommunications
    {
    public static class SerialObservableExtensions
        {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Captures the <see cref="System.IO.Ports.SerialPort.DataReceived" /> event of a serial port and returns an
        ///     observable sequence of the events.
        /// </summary>
        /// <param name="port">The serial port that will act as the event source.</param>
        /// <returns><see cref="IObservable{Char}" /> - an observable sequence of events.</returns>
        public static IObservable<EventPattern<SerialDataReceivedEventArgs>> ObservableDataReceivedEvents(
            this ISerialPort port)
            {
            var portEvents = Observable.FromEventPattern<SerialDataReceivedEventHandler, SerialDataReceivedEventArgs>(
                handler =>
                    {
                    log.Debug("Event: SerialDataReceived");
                    return handler.Invoke;
                    },
                handler =>
                    {
                    // We must discard stale data when subscribing or it will pollute the first element of the sequence.
                    port.DiscardInBuffer();
                    port.DataReceived += handler;
                    log.Debug("Listening to DataReceived event");
                    },
                handler =>
                    {
                    port.DataReceived -= handler;
                    log.Debug("Stopped listening to DataReceived event");
                    });
            return portEvents;
            }

        /// <summary>
        ///     Gets an observable sequence of all the characters received by a serial port.
        /// </summary>
        /// <param name="port">The port that is to be the data source.</param>
        /// <returns><see cref="IObservable{char}" /> - an observable sequence of characters.</returns>
        public static IObservable<char> ReceivedCharacters(this ISerialPort port)
            {
            var observableEvents = port.ObservableDataReceivedEvents();
            var observableCharacterSequence = from args in observableEvents
                                              where args.EventArgs.EventType == SerialData.Chars
                                              from character in port.ReadExisting()
                                              select character;
            return observableCharacterSequence;
            }

        /// <summary>
        ///     Produces strings delimited by the specified terminator character.
        /// </summary>
        /// <param name="source">The source character sequence.</param>
        /// <param name="terminator">The terminator.</param>
        /// <returns><see cref="IObservable{string}" /> - a sequence of strings.</returns>
        public static IObservable<string> TerminatedStrings(this IObservable<char> source, char terminator = '#')
            {
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
        /// <returns><see cref="IObservable{string}" /> - a sequence of time-delimited strings.</returns>
        public static IObservable<string> TimeDelimitedString(this IObservable<char> source, TimeSpan quietTime)
            {
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
        ///     <see cref="IObservable{string}" />, an observable sequence of strings each delimited by an initiator character
        ///     and a terminator character.
        /// </returns>
        public static IObservable<string> DelimitedMessageStrings(
            this IObservable<char> source,
            char initiator = ':',
            char terminator = '#')
            {
            var buffers = source.Publish(s => BufferByDelimiters(s, initiator, terminator));
            var strings = from buffer in buffers
                          select new string(buffer.ToArray());
            return strings;
            }

        /// <summary>
        ///     Buffers a sequence of characters based on a pair of delimiters.
        /// </summary>
        /// <param name="source">The source sequence.</param>
        /// <param name="initiator"></param>
        /// <param name="terminator"></param>
        /// <returns><see cref="IObservable{IList{char}}" /> - an observable sequence of buffers.</returns>
        public static IObservable<IList<char>> BufferByDelimiters(IObservable<char> source, char initiator,
            char terminator)
            {
            return source.Buffer(source.Where(c => c == initiator), x => source.Where(c => c == terminator));
            }

        /// <summary>
        ///     Selects sequences of "0#" and "1#" and produces "0" or "1".
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="terminator">The terminator.</param>
        /// <returns>IObservable&lt;System.String&gt;.</returns>
        public static IObservable<string> TerminatedBoolean(this IObservable<char> source, char terminator = '#')
            {
            var results = from s in source.TerminatedStrings()
                          where "01".Contains(s[0])
                          select s.Substring(0, 1);
            return results;
            }
        }
    }