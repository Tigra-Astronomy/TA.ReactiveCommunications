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
// File: SerialCommunicationChannel.cs  Last modified: 2015-05-25@18:23 by Tim Long

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using NLog;

namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>
    ///     Class SerialCommunicationChannel. Implements sending and receiving to a serial port device.
    /// </summary>
    public class SerialCommunicationChannel : ICommunicationChannel
        {
        const string LineTerminatorValue = "\n";
        internal readonly SerialDeviceEndpoint endpoint;
        readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Initializes a new instance of the <see cref="SerialCommunicationChannel" /> class.
        /// </summary>
        /// <param name="endpoint">The device endpoint.</param>
        /// <param name="port">The port.</param>
        /// <exception cref="System.ArgumentException">Expected a SerialDeviceEndpoint</exception>
        public SerialCommunicationChannel(DeviceEndpoint endpoint, ISerialPort port = null)
            {
            Port = port ?? new SerialPort();
            if (!(endpoint is SerialDeviceEndpoint))
                throw new ArgumentException("Expected a SerialDeviceEndpoint");
            this.endpoint = endpoint as SerialDeviceEndpoint;
            observableReceiveSequence = Port.ReceivedCharacters()
                .Do(
                    c => Log.Debug("Rx={0}", c),
                    ex => Log.Error("Rx exception", ex),
                    () => Log.Warn("Rx OnCompleted"))
                .Publish();
            }

        ISerialPort Port { get; set; }

        /// <summary>
        ///     Configures the serial port and opens the channel ready for transmitting.
        /// </summary>
        public void Open()
            {
            Log.Info("Channel opening => {0}", endpoint);
            Port.PortName = endpoint.PortName;
            Port.BaudRate = endpoint.BaudRate;
            Port.Parity = endpoint.Parity;
            Port.DataBits = endpoint.DataBits;
            Port.StopBits = endpoint.StopBits;
            Port.RtsEnable = endpoint.RtsEnable;
            Port.DtrEnable = endpoint.DtrEnable;
            /*
             * We use a code page of 1252 because most devices speak "extended ASCII". Encoding.ASCII is strictly 7-bit ASCII
             * so some devices and in particular Meade devices don't work if that encoding is used. The 'degree symbol'
             * (ASCII 223 decimal) is particularly problematic and is used by Meade devices in the formatting of the Declination
             * coordinate. Codepage 1252 is a superset of ISO-8859-1 8-bit ASCII so the degree symbol works correctly in that code page.
             * The degree symbol typically displays as 'ß' in UTF-8.
             */
            Port.Encoding = Encoding.GetEncoding(1252);
                //ToDo: magic number. Allow this to be specified rather than hard coded.
            Port.Open();
            if (IsOpen)
                {
                receiverListening = observableReceiveSequence.Connect();
                }
            }

        /// <summary>
        ///     Disconnects the serial port and closes the channel.
        /// </summary>
        public void Close()
            {
            Log.Info("Channel closing => {0}", endpoint);
            if (receiverListening != null)
                receiverListening.Dispose(); // Disconnects the serial event handlers
            Port.Close();
            }

        /// <summary>
        ///     Sends the specified data and returns immediately without waiting for a reply.
        /// </summary>
        /// <param name="txData">The data to be transmitted.</param>
        public virtual void Send(string txData)
            {
            Log.Trace("Sending [{0}]", txData);
            Port.Write(txData);
            }

        /// <summary>
        ///     An observable sequence of the characters received from the serial port.
        /// </summary>
        /// <value>The receive sequence.</value>
        public IObservable<char> ObservableReceivedCharacters
            {
            get { return observableReceiveSequence; }
            }

        /// <summary>
        ///     Gets a value indicating whether this instance is open.
        /// </summary>
        /// <value><c>true</c> if this instance is open; otherwise, <c>false</c>.</value>
        public bool IsOpen
            {
            get { return Port.IsOpen; }
            }

        public DeviceEndpoint Endpoint
            {
            get { return endpoint; }
            }

        public override string ToString()
            {
            return string.Format("IsOpen: {0}, Endpoint: {1}", IsOpen, endpoint);
            }

        #region IDisposable pattern for a base class.
        bool disposed;
        readonly IConnectableObservable<char> observableReceiveSequence;
        IDisposable receiverListening;

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///     Implements <see cref="IDisposable" />.
        /// </summary>
        public void Dispose()
            {
            Dispose(true);
            GC.SuppressFinalize(this);
            }

        protected virtual void Dispose(bool disposing)
            {
            if (!disposed)
                {
                if (disposing)
                    {
                    if (Port != null)
                        {
                        if (Port.IsOpen)
                            Port.Close(); // Attempt to close gracefully (this should never fail).
                        Port.Dispose();
                        }
                    }
                // Free own state (unmanaged objects); Set large fields to null.

                // Prevent multiple disposals
                disposed = true;
                }
            }

        // Use C# destructor syntax for finalization code.
        ~SerialCommunicationChannel()
            {
            Dispose(false);
            }
        #endregion
        }
    }