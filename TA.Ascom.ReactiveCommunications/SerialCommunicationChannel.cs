// This file is part of the TA.Ascom.ReactiveCommunications project
//
// Copyright © 2018 Tigra Astronomy, all rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
//
// File: SerialCommunicationChannel.cs  Last modified: 2018-08-27@22:37 by Tim Long

using System;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using TA.Ascom.ReactiveCommunications.Diagnostics;
using TA.Utils.Core;
using TA.Utils.Core.Diagnostics;

namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>
    ///     Class SerialCommunicationChannel. Implements sending and receiving to a serial port device.
    /// </summary>
    public class SerialCommunicationChannel : ICommunicationChannel
        {
        private const string LineTerminatorValue = "\n";
        internal readonly SerialDeviceEndpoint endpoint;
        private readonly ILog log = ServiceLocator.LogService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SerialCommunicationChannel" /> class.
        /// </summary>
        /// <param name="endpoint">The device endpoint.</param>
        /// <param name="port">The port.</param>
        /// <exception cref="System.ArgumentException">Expected a SerialDeviceEndpoint</exception>
        public SerialCommunicationChannel(DeviceEndpoint endpoint, ISerialPort port = null)
            {
            if (!(endpoint is SerialDeviceEndpoint))
                throw new ArgumentException("Expected a SerialDeviceEndpoint");
            this.endpoint = endpoint as SerialDeviceEndpoint;
            Port = port ?? CreateSerialPort(this.endpoint);
            observableReceiveSequence = Port.ToObservableCharacterSequence()
                .Trace("Serial Receive")
                .Publish();
            }

        internal ISerialPort Port { get; set; }

        /// <summary>
        ///     Configures the serial port and opens the channel ready for transmitting.
        /// </summary>
        public void Open()
            {
            log.Info().Message("Channel opening => {endpoint}", endpoint).Write();
            Port.PortName = endpoint.PortName;
            Port.BaudRate = endpoint.BaudRate;
            Port.Parity = endpoint.Parity;
            Port.DataBits = endpoint.DataBits;
            Port.StopBits = endpoint.StopBits;
            Port.RtsEnable = endpoint.RtsEnable;
            Port.DtrEnable = endpoint.DtrEnable;
            /*
             * We use a code page of 1252 because most devices speak "extended ASCII".
             * Encoding.ASCII is strictly 7-bit ASCII so some devices and in particular
             * Meade devices don't work if that encoding is used. The 'degree symbol'
             * (ASCII 223 decimal) is particularly problematic and is used by Meade
             * devices in the formatting of the Declination coordinate. Codepage 1252
             * is a superset of ISO-8859-1 8-bit ASCII so the degree symbol works
             * correctly in that code page. The degree symbol typically displays
             * as 'ß' in UTF-8.
             */
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Port.Encoding = Encoding.GetEncoding(1252);
            //ToDo: magic number. Allow this to be specified rather than hard coded.
            Port.Open();
            if (IsOpen)
                receiverListening = observableReceiveSequence.Connect();
            }

        /// <summary>
        ///     Disconnects the serial port and closes the channel.
        /// </summary>
        public void Close()
            {
            log.Info().Message("Channel closing => {endpoint}", endpoint);
            receiverListening?.Dispose(); // Disconnects the serial event handlers
            Port.Close();
            }

        /// <summary>
        ///     Sends the specified data and returns immediately without waiting for a reply.
        /// </summary>
        /// <param name="txData">The data to be transmitted.</param>
        public virtual void Send(string txData)
            {
            log.Debug().Message("Sending [{txdata}]", txData.ExpandAscii());
            Port.Write(txData);
            }

        /// <summary>
        ///     An observable sequence of the characters received from the serial port.
        /// </summary>
        /// <value>The receive sequence.</value>
        public IObservable<char> ObservableReceivedCharacters => observableReceiveSequence;

        /// <summary>
        ///     Gets a value indicating whether this instance is open.
        /// </summary>
        /// <value><c>true</c> if this instance is open; otherwise, <c>false</c>.</value>
        public bool IsOpen => Port.IsOpen;

        /// <summary>
        ///     Gets the endpoint that is associated with the channel.
        /// </summary>
        /// <value>The endpoint.</value>
        public DeviceEndpoint Endpoint => endpoint;

        private ISerialPort CreateSerialPort(SerialDeviceEndpoint endpoint)
            {
            Contract.Requires(endpoint != null);
            var port = new SerialPort
                {
                PortName = endpoint.PortName,
                BaudRate = endpoint.BaudRate,
                Parity = endpoint.Parity,
                DataBits = endpoint.DataBits,
                StopBits = endpoint.StopBits,
                RtsEnable = endpoint.RtsEnable,
                DtrEnable = endpoint.DtrEnable,
                Encoding = endpoint.Encoding,
                Handshake = endpoint.Handshake
                };
            return port;
            }

        [ContractInvariantMethod]
        private void ObjectInvariant()
            {
            Contract.Invariant(log != null);
            Contract.Invariant(endpoint != null);
            }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
            {
            Contract.Ensures(Contract.Result<string>() != null);
            return $"IsOpen: {IsOpen}, Endpoint: {endpoint}";
            }

        #region IDisposable pattern for a base class.
        private bool disposed;
        private readonly IConnectableObservable<char> observableReceiveSequence;
        private IDisposable receiverListening;

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///     Implements <see cref="IDisposable" />.
        /// </summary>
        public void Dispose()
            {
            Dispose(true);
            GC.SuppressFinalize(this);
            }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
            {
            if (!disposed)
                {
                if (disposing)
                    if (Port != null)
                        {
                        if (Port.IsOpen)
                            Port.Close(); // Attempt to close gracefully (this should never fail).
                        Port.Dispose();
                        }
                // Free own state (unmanaged objects); Set large fields to null.

                // Prevent multiple disposals
                disposed = true;
                }
            }

        /// <summary>
        ///     Finalizes an instance of the <see cref="SerialCommunicationChannel" /> class.
        /// </summary>
        ~SerialCommunicationChannel()
            {
            Dispose(false);
            }
        #endregion
        }
    }