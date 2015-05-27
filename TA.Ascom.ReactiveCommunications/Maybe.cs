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
// File: Maybe.cs  Last modified: 2015-05-27@09:24 by Tim Long

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>
    ///     Represents an object that may or may not have a value (strictly, a collection of zero or one elements). Use
    ///     LINQ expression <c>maybe.Any()</c> to determine if there is a value. Use LINQ expression
    ///     <c>maybe.Single()</c> to retrieve the value.
    /// </summary>
    /// <typeparam name="T">The type of the item in the collection.</typeparam>
    /// <remarks>
    ///     This type almost completely eliminates any need to return <c>null</c> or deal with possibly null references,
    ///     which makes code cleaner and more clearly expresses the intent of 'no value' versus 'error'.  The value of a
    ///     Maybe cannot be <c>null</c>, because <c>null</c> really means 'no value' and that is better expressed by
    ///     using <see cref="Empty" />.
    /// </remarks>
    public class Maybe<T> : IEnumerable<T>
        {
        static readonly Maybe<T> EmptyInstance = new Maybe<T>();
        readonly IEnumerable<T> values;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Maybe{T}" /> with no value.
        /// </summary>
        Maybe()
            {
            values = new T[0];
            }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Maybe{T}" /> with a value.
        /// </summary>
        /// <param name="value">The value.</param>
        public Maybe(T value)
            {
            Contract.Requires(value != null);
            values = new[] {value};
            }

        [ContractInvariantMethod]
        void ObjectInvariant()
            {
            Contract.Invariant(values != null);
            }

        /// <summary>
        ///     Gets an instance that does not contain a value.
        /// </summary>
        /// <value>The empty instance.</value>
        public static Maybe<T> Empty
        {
            get
            {
                Contract.Ensures(Contract.Result<Maybe<T>>() != null);
                return EmptyInstance;
            }
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the
        ///     collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
            {
            Contract.Ensures(Contract.Result<IEnumerator<T>>() != null);
            return values.GetEnumerator();
            }

        IEnumerator IEnumerable.GetEnumerator()
            {
            Contract.Ensures(Contract.Result<IEnumerator>() != null);
            return GetEnumerator();
            }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        [Pure]
        public override string ToString()
            {
            Contract.Ensures(Contract.Result<string>() != null);
            if (Equals(Empty)) return "{no value}";
            return this.Single().ToString();
            }
        }
    }