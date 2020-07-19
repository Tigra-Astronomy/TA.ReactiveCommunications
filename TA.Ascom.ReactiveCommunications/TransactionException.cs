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
// File: TransactionException.cs  Last modified: 2020-07-18@11:05 by Tim Long

using System;
using System.Runtime.Serialization;

namespace TA.Ascom.ReactiveCommunications
    {
    /// <inheritdoc />
    /// <summary>
    /// An exception that indicates a failed <see cref="DeviceTransaction"/>.
    /// The exception carries a copy of the failed transaction, which can be used
    /// for further analysis and logging.
    /// </summary>
    [Serializable]
    public class TransactionException : Exception
        {
        /// <inheritdoc />
        public TransactionException() : base("The transaction failed") { }

        /// <inheritdoc />
        public TransactionException(string message) : base(message) { }

        /// <inheritdoc />
        public TransactionException(string message, Exception inner) : base(message, inner) { }

        /// <inheritdoc />
        protected TransactionException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }

        /// <summary>Gets a reference to the failed transaction that generated the exception</summary>
        public DeviceTransaction Transaction { get; set; }
        }
    }