using System;
using System.Collections.Generic;
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
            builder.Property("transaction", transaction);
            builder.Property("transactionId", transaction.TransactionId);
            builder.Property("transactionCommand", transaction.Command);
            builder.Property("transactionState", transaction.State);
            if (transaction.ErrorMessage.Any())
                builder.Property("transactionError", transaction.ErrorMessage.Single());
            if (transaction.Response.Any())
                builder.Property("transactionResponse", transaction.Response.Single());
            return builder;
            }

        public static IFluentLogBuilder PublicPropertiesOf(this IFluentLogBuilder logBuilder, object item, string baseName = null)
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
