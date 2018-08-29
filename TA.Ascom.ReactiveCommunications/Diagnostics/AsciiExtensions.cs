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
// File: AsciiExtensions.cs  Last modified: 2018-08-27@22:35 by Tim Long

using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace TA.Ascom.ReactiveCommunications.Diagnostics
    {
    internal static class AsciiExtensions
        {
        /// <summary>
        ///     Utility function. Expands non-printable ASCII characters into mnemonic
        ///     human-readable form.
        /// </summary>
        /// <returns>
        ///     Returns a new string with non-printing characters replaced by human-readable
        ///     mnemonics.
        /// </returns>
        internal static string ExpandAscii(this string text)
            {
            Contract.Requires(text != null);
            Contract.Ensures(Contract.Result<string>() != null);
            var expanded = new StringBuilder(Math.Max(64, text.Length * 3));
            foreach (var c in text)
                {
                var b = (byte) c;
                var strAscii = Enum.GetName(typeof(AsciiSymbols), b);
                if (!string.IsNullOrEmpty(strAscii))
                    expanded.Append("<" + strAscii + ">");
                else
                    expanded.Append(c);
                }
            return expanded.ToString();
            }

        internal static string ExpandAscii(this char c)
            {
            Contract.Ensures(Contract.Result<string>() != null);
            return c.ToString().ExpandAscii();
            }
        }
    }