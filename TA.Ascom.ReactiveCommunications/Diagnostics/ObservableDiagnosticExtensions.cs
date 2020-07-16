// This file is part of the TA.Ascom.ReactiveCommunications project
//
// Copyright © 2016 Tigra Astronomy, all rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so,. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
//
// File: ObservableDiagnosticExtensions.cs  Last modified: 2016-10-23@23:50 by Tim Long

using System;
using System.Reactive.Linq;
using TA.Utils.Core;

namespace TA.Ascom.ReactiveCommunications.Diagnostics
    {
    /// <summary>
    ///     Provides debug and trace support for observables.
    /// </summary>
    public static class ObservableDiagnosticExtensions
        {
        /// <summary>
        ///     Traces the specified observable.
        /// </summary>
        /// <typeparam name="TSource">The type of the observable sequence.</typeparam>
        /// <param name="source">The source sequence to be traced.</param>
        /// <param name="name">The name emitted in trace output for this source.</param>
        public static IObservable<TSource> Trace<TSource>(this IObservable<TSource> source, string name)
            {
            var log = ServiceLocator.LogService;
            var id = 0;
            return Observable.Create<TSource>(observer =>
                {
                var subscriptionId = ++id;  // closure
                var logName = name;       // closure
                Action<string, object> trace = (action, content) => log
                    .Trace()
                    .LoggerName(logName)
                    .Message("{source}[{id}]: {action}({content})", logName, subscriptionId, action, content)
                    .Write();
                trace("Subscribe", "");
                var disposable = source.Subscribe(
                    v =>
                        {
                        trace("OnNext", v.ToString().ExpandAscii());
                        observer.OnNext(v);
                        },
                    e =>
                        {
                        trace("OnError", "");
                        observer.OnError(e);
                        },
                    () =>
                        {
                        trace("OnCompleted", "");
                        observer.OnCompleted();
                        });
                return () =>
                    {
                    trace("Dispose", "");
                    disposable.Dispose();
                    };
                });
            }
        }
    }