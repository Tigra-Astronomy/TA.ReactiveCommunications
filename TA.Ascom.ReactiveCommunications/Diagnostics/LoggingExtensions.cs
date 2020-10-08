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
// File: LoggingExtensions.cs  Last modified: 2020-07-20@00:50 by Tim Long

using System.Linq;
using System.Reflection;
using System.Text;
using TA.Utils.Core.Diagnostics;

namespace TA.Ascom.ReactiveCommunications.Diagnostics
    {
    public static class LoggingExtensions
        {
        private const string NullValue = "null";

        public static IFluentLogBuilder Transaction(this IFluentLogBuilder builder, DeviceTransaction transaction)
            {
            builder.Property("transactionState", transaction);
            builder.Property("transactionId", transaction.TransactionId);
            builder.Property("transactionCommand", transaction.Command);
            builder.Property("transactionState", transaction.State);
            if (transaction.ErrorMessage.Any())
                builder.Property("transactionError", transaction.ErrorMessage.Single());
            if (transaction.Response.Any())
                builder.Property("transactionResponse", transaction.Response.Single());
            return builder;
            }

        public static IFluentLogBuilder PublicPropertiesOf(this IFluentLogBuilder logBuilder, object item,
            string baseName = null)
            {
            var itemType = item.GetType();
            var publicProperties = itemType.GetProperties(BindingFlags.Public);
            if (!publicProperties.Any())
                return logBuilder;
            var logBaseName = baseName ?? itemType.Name;
            var nameBuilder = new StringBuilder();
            foreach (var property in publicProperties)
                {
                var value = property.GetValue(item);
                nameBuilder.Clear().Append(logBaseName).Append('-').Append(property.Name);
                logBuilder.Property(nameBuilder.ToString(), value ?? NullValue);
                }
            return logBuilder;
            }
        }
    }