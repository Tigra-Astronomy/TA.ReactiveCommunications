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
// File: MockHelpers.cs  Last modified: 2020-07-20@00:51 by Tim Long

using System.IO.Ports;
using System.Reflection;

namespace TA.Ascom.ReactiveCommunications.Specifications.Helpers
    {
    internal static class MockHelpers
        {
        /// <summary>
        ///     Factory method that creates an instance of <see cref="SerialDataReceivedEventArgs" />. This
        ///     class has no public constructor so must be created using Reflection.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <returns>A constructed instance of <see cref="SerialDataReceivedEventArgs" />.</returns>
        public static SerialDataReceivedEventArgs CreateSerialDataReceivedEventArgs(SerialData eventType)
            {
            ConstructorInfo constructor =
                typeof(SerialDataReceivedEventArgs).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] {typeof(SerialData)},
                    null);
            SerialDataReceivedEventArgs eventArgs =
                (SerialDataReceivedEventArgs) constructor.Invoke(new object[] {eventType});
            return eventArgs;
            }
        }
    }