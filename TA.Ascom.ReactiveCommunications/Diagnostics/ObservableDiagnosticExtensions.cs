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
// File: ObservableDiagnosticExtensions.cs  Last modified: 2020-07-20@00:50 by Tim Long

using System;
using System.Reactive.Linq;
using TA.Utils.Core;

namespace TA.Ascom.ReactiveCommunications.Diagnostics
    {
    /// <summary>Provides debug and trace support for observables.</summary>
    public static class ObservableDiagnosticExtensions
        {
        /// <summary>Traces the specified observable.</summary>
        /// <typeparam name="TSource">The type of the observable sequence.</typeparam>
        /// <param name="source">The source sequence to be traced.</param>
        /// <param name="name">The name emitted in trace output for this source.</param>
        public static IObservable<TSource> Trace<TSource>(this IObservable<TSource> source, string name)
            {
            var log = ServiceLocator.LogService;
            var id = 0;
            return Observable.Create<TSource>(observer =>
                {
                var subscriptionId = ++id; // closure
                var sequenceName = name; // closure

                WriteTraceOutput("Subscribe", "");
                var disposable = source.Subscribe(
                    v =>
                    {
                        WriteTraceOutput("OnNext", v.ToString().ExpandAscii());
                        observer.OnNext(v);
                    },
                    e =>
                    {
                        WriteTraceOutput("OnError", "");
                        observer.OnError(e);
                    },
                    () =>
                    {
                        WriteTraceOutput("OnCompleted", "");
                        observer.OnCompleted();
                    });
                return () =>
                    {
                        WriteTraceOutput("Dispose", "");
                        disposable.Dispose();
                    };

                void WriteTraceOutput(string action, object content)
                {
                    log.Trace(sourceNameOverride: "RxComms")
                        .Message("{@source}[{id}]: {@action}({@content})",
                            sequenceName,
                            subscriptionId,
                            action,
                            content)
                        .Write();
                }
                });
            }
        }
    }