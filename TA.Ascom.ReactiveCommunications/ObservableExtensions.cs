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
// File: ObservableExtensions.cs  Last modified: 2015-05-30@16:13 by Tim Long

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO.Ports;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using NLog;

namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>
    ///     Provides extension methods for capturing and transforming observable sequences of characters received from a serial
    ///     port.
    /// </summary>
    public static class ObservableExtensions
        {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Captures the <see cref="System.IO.Ports.SerialPort.DataReceived" /> event of a serial port and returns an
        ///     observable sequence of the events.
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
                    Log.Debug("Listening to DataReceived event");
                    },
                handler =>
                    {
                    port.DataReceived -= handler;
                    Log.Debug("Stopped listening to DataReceived event");
                    });

            return portEvents;
            }


        /// <summary>
        /// Creates an observable sequence of characters from the specified serial port.
        /// </summary>
        /// <param name="port">The port that will be the data source.</param>
        /// <returns><see cref="IObservable{Char}" /> - an observable sequence of characters.</returns>
        /// <remarks>
        ///     This code suggested by Bart De Smet [MVP] at
        ///     https://social.msdn.microsoft.com/Forums/en-US/5a12822d-92a6-4ff3-9a37-9bcce83dae0c/how-to-implement-serialport-parser-in-rx?forum=rx
        ///     Bart's code correctly handles OnCompleted and OnError cases.
        /// </remarks>
        public static IObservable<char> ToObservableCharacterSequence(this ISerialPort port)
            {
            return Observable.Create<char>(observer =>
                {
                port.DataReceived += ReactiveDataReceivedEventHandler(port, observer);
                port.ErrorReceived += ReactiveErrorReceivedEventHandler(observer);
                return UnsubscribeAction(port, observer);
                });
            }

        /// <summary>
        /// Returns an Action to be called when the observer unsubscribes.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="observer">The observer.</param>
        /// <returns>Action.</returns>
        static Action UnsubscribeAction(ISerialPort port, IObserver<char> observer)
            {
            return () =>
                {
                port.DataReceived -= ReactiveDataReceivedEventHandler(port, observer);
                port.ErrorReceived -= ReactiveErrorReceivedEventHandler(observer);
                // depending on ownership of port, we could Dispose it here too
                };
            }

        /// <summary>
        /// Gets an event handler (delegate) that handles the SerialErrorReceived event from a serial port
        /// and passes the error on to an observer by calling the OnError method on the observer.
        /// </summary>
        /// <param name="observer">The observer.</param>
        /// <returns>delegate of type <see cref="SerialErrorReceivedEventHandler"/>.</returns>
        static SerialErrorReceivedEventHandler ReactiveErrorReceivedEventHandler(IObserver<char> observer)
            {
            var errorEventHandler = new SerialErrorReceivedEventHandler(
                    (sender, e) => observer.OnError(new Exception(e.EventType.ToString())));
            return errorEventHandler;
            }

        /// <summary>
        ///     Gets an event handler (delegate) that handles the SerialDataReceived event from a serial port. if the
        ///     event type is <see cref="SerialData.Chars" /> then the received characters are read from the serial port
        ///     buffer and passed on to a subscribed observer by calling the OnNext method. OnNext is called once for
        ///     each received character. If the event type is <see cref="SerialData.Eof" /> then the observer's
        ///     OnCompleted method is called.
        /// </summary>
        /// <param name="port">The data source.</param>
        /// <param name="observer">The subscribed observer.</param>
        /// <returns>SerialDataReceivedEventHandler.</returns>
        /// <remarks>
        /// The documentation for <see cref="System.IO.Ports.SerialPort"/> states that: "Note that this method can leave
        /// trailing lead bytes in the internal buffer, which makes the BytesToRead value greater than zero". In that
        /// situation, we would enter an infinite loop trying to read from en empty stream, which would only terminate
        /// when more data arrives at the serial port and eventually gets flushed into the input stream. Therefore, we
        /// use <c>Thread.Yield()</c> within the receive loop to give other threads (including the serial port) a chance
        /// to run.
        /// </remarks>
        static SerialDataReceivedEventHandler ReactiveDataReceivedEventHandler(ISerialPort port, IObserver<char> observer)
            {
            var receiveEventHandler = new SerialDataReceivedEventHandler((sender, e) =>
                {
                switch (e.EventType)
                    {
                        case SerialData.Eof:
                            observer.OnCompleted();
                            break;
                        case SerialData.Chars:
                            while (port.BytesToRead > 0)
                                {
                                var inputBuffer = port.ReadExisting();
                                foreach (var character in inputBuffer)
                                    {
                                    observer.OnNext(character);
                                    }
                                Thread.Yield(); // There's no point in spinning on an empty stream.
                                }
                            break;
                        default:
                            Log.Warn("Ignoring unexpected serial data received event: {0}", e.EventType);
                            break;
                    }
                });
            return receiveEventHandler;
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

        /// <summary>
        /// Rate-limits a sequence so that it cannot produce elements faster that the specified interval.
        /// No data is discarded. When elements arrive faster than the specified rate, they are buffered
        /// and emitted one at a time in accordance with the configured rate. This may be useful where
        /// devices have a limitation on the number of transactions they can process per second.
        /// </summary>
        /// <typeparam name="T">The type of the observable sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="interval">The minimum interval between emitted elements.</param>
        /// <returns>A rate-limited IObservable{T}.</returns>
        /// <remarks>Credit to <c>yamen</c> via http://stackoverflow.com/a/11285920/98516 for this solution.</remarks>
        public static IObservable<T> RateLimited<T>(this IObservable<T> source, TimeSpan interval)
            {
            return source.Select(x =>
                Observable.Empty<T>()
                    .Delay(interval)
                    .StartWith(x)
                ).Concat();
            }

        }
    }