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
// File: ICommunicationsChannel.cs  Last modified: 2020-07-20@00:50 by Tim Long

using System;

namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>Interface ICommunicationsChannel</summary>
    public interface ICommunicationChannel : IDisposable
        {
        /// <summary>An observable sequence of the characters received from the serial port.</summary>
        /// <value>The receive stream.</value>
        IObservable<char> ObservableReceivedCharacters { get; }

        /// <summary>Gets a value indicating whether this instance is open.</summary>
        /// <value><c>true</c> if this instance is open; otherwise, <c>false</c>.</value>
        bool IsOpen { get; }

        /// <summary>Gets the endpoint that is associated with the channel.</summary>
        /// <value>The endpoint.</value>
        DeviceEndpoint Endpoint { get; }

        /// <summary>Creates any underlying communication objects and opens the channel ready for transmitting.</summary>
        void Open();

        /// <summary>Closes the channel and destroys any underlying communication objects.</summary>
        void Close();

        /// <summary>Sends the specified data and returns immediately without waiting for a reply.</summary>
        /// <param name="txData">The data to be transmitted.</param>
        void Send(string txData);
        }
    }