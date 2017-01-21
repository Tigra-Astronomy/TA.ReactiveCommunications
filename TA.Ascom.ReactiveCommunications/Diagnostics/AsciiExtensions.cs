// This file is part of the TI.DigitalDomeWorks project
// 
// Copyright © 2014 TiGra Astronomy, all rights reserved.
// 
// File: AsciiExtensions.cs  Created: 2014-10-29@20:45
// Last modified: 2014-11-12@05:56 by Tim

using System;
using System.Text;

namespace TA.Ascom.ReactiveCommunications.Diagnostics
    {
    public static class AsciiExtensions
        {
        /// <summary>
        ///   Utility function. Expands non-printable ASCII characters into mnemonic human-readable form.
        /// </summary>
        /// <returns>
        ///   Returns a new string with non-printing characters replaced by human-readable mnemonics.
        /// </returns>
        public static string ExpandAscii(this string text)
            {
            var expanded = new StringBuilder(Math.Max(64, text.Length*3));
            foreach (char c in text)
                {
                var b = (byte)c;
                var strASCII = Enum.GetName(typeof(AsciiSymbols), b);
                if (strASCII != null)
                    expanded.Append("<" + strASCII + ">");
                else
                    expanded.Append(c);
                }
            return expanded.ToString();
            }

        public static string ExpandAscii(this char c)
            {
            return c.ToString().ExpandAscii();
            }
        }
    }
