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
// File: FakeCommunicationChannel.cs  Last modified: 2020-07-20@00:51 by Tim Long

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Timtek.ReactiveCommunications;

namespace TA.Ascom.ReactiveCommunications.Specifications.Fakes
    {
    /// <summary>
    ///     A fake communication channel that logs any sent data in <see cref="SendLog" /> and receives a
    ///     fake pre-programmed response passed into the constructor. The class also keeps a count of how
    ///     many times each method of <see cref="ICommunicationChannel" /> was called.
    /// </summary>
    public class FakeCommunicationChannel : ICommunicationChannel
        {
        readonly Subject<char> receiveChannelSubject = new Subject<char>();
        readonly IObservable<char> receivedCharacters;
        readonly StringBuilder sendLog;

        /// <summary>
        ///     Dependency injection constructor. Initializes a new instance of the
        ///     <see cref="SafetyMonitorDriver" /> class.
        /// </summary>
        /// <param name="fakeResponse">Implementation of the injected dependency.</param>
        public FakeCommunicationChannel(string fakeResponse)
            {
            Endpoint = new InvalidEndpoint();
            Response = fakeResponse;
            // Concatenating Observable.Never ensures the sequence never terminates
            receivedCharacters = fakeResponse.ToCharArray().ToObservable().Concat(Observable.Never<char>());
            sendLog = new StringBuilder();
            IsOpen = false;
            }

        /// <summary>Gets the send log.</summary>
        /// <value>The send log.</value>
        public string SendLog => sendLog.ToString();

        /// <summary>Gets a copy of the fake pre-programmed response.</summary>
        /// <value>The response.</value>
        public string Response { get; }

        public int TimesDisposed { get; set; }

        public int TimesClosed { get; set; }

        public int TimesOpened { get; set; }

        public void Dispose()
            {
            TimesDisposed++;
            }

        public void Open()
            {
            TimesOpened++;
            IsOpen = true;
            }

        public void Close()
            {
            TimesClosed++;
            IsOpen = false;
            }

        public void Send(string txData)
            {
            sendLog.Append(txData);
            foreach (var c in Response) receiveChannelSubject.OnNext(c);
            }

        public IObservable<char> ObservableReceivedCharacters => receiveChannelSubject.AsObservable();

        public bool IsOpen { get; set; }

        public DeviceEndpoint Endpoint { get; }
        }
    }