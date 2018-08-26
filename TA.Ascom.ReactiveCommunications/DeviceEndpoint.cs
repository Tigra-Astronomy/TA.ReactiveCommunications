// This file is part of the TA.Ascom.ReactiveCommunications project
// 
// Copyright © 2018 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so,. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: DeviceEndpoint.cs  Last modified: 2018-08-26@06:02 by Tim Long

using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text.RegularExpressions;
using NLog;

namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>
    ///     Represents a device specific address that describes a connection to a device. This class cannot be instantiated and
    ///     must be inherited.
    /// </summary>
    [Serializable]
    public abstract class DeviceEndpoint
        {
        /// <summary>
        ///     Regex options for builtin endpoints.
        /// </summary>
        protected const RegexOptions Options =
            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline |
            RegexOptions.IgnoreCase | RegexOptions.Compiled;
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Gets the device address. The address format is device specific.
        ///     For example, for an Ethernet connection, it could be a DNS host name or an IP address.
        ///     For a serial port, it would be the name of the port.
        /// </summary>
        /// <value>The device address.</value>
        public string DeviceAddress
            {
            get
                {
                Contract.Ensures(Contract.Result<string>() != null);
                return ToString();
                }
            }

        /// <summary>
        ///     A helper method that returns the value of a named capture group,
        ///     if it exists, or the specified default value if there was no matching
        ///     named capture.
        /// </summary>
        /// <param name="matches">The match results from a regex match.</param>
        /// <param name="groupName">The named capture group whose value we seek.</param>
        /// <param name="defaultValue">The default value to be used in the event that the named group did not capture a value.</param>
        /// <typeparam name="TResult">The type of the returned result.</typeparam>
        /// <returns>The value of the named capture group, or the default value.</returns>
        /// <exception cref="ArgumentException"></exception>
        protected static TResult CaptureGroupOrDefault<TResult>(Match matches, string groupName, TResult defaultValue)
            {
            Contract.Requires(matches != null);
            Contract.Requires(groupName != null);
            Contract.Requires(defaultValue != null);
            if (matches.Groups[groupName].Success == false) return defaultValue;
            var capture = matches.Groups[groupName].Value;
            try
                {
                var result = ConvertFromString<TResult>(capture);
                return result;
                }
            catch (NotSupportedException ex)
                {
                var message =
                    $"The parameter '{groupName}' with value '{capture}' could not be parsed. See InnerException for details.";
                throw new ArgumentException(message, ex);
                }
            }

        private static TResult ConvertFromString<TResult>(string valueString)
            {
            Contract.Requires(!string.IsNullOrWhiteSpace(valueString));
            var valueType = typeof(string);
            var resultType = typeof(TResult);
            // look for a type converter that can convert from string to the specified type.
            var converter = TypeDescriptor.GetConverter(resultType);
            var canConvertFrom = converter.CanConvertFrom(valueType);
            if (!canConvertFrom)
                converter = TypeDescriptor.GetConverter(valueType);
            var culture = CultureInfo.InvariantCulture;
            var convertedValue = canConvertFrom
                ? converter.ConvertFrom(null, culture, valueString)
                : converter.ConvertTo(null /* context */, culture, valueString, resultType);
            return (TResult) convertedValue;
            }
        }
    }