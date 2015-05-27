using System;
using System.Diagnostics.Contracts;

namespace TA.Ascom.ReactiveCommunications.Specifications.Fakes
    {
    internal class FakeTransactionProcessor : ITransactionProcessor
        {
        readonly IObservable<char> response;

        public FakeTransactionProcessor(IObservable<char> response)
            {
            this.response = response;
            }

        public void CommitTransaction(DeviceTransaction transaction)
            {
            Contract.Requires(transaction != null);
            Transaction = transaction;
            transaction.ObserveResponse(response);
            }

        public DeviceTransaction Transaction { get; set; }
        }
    }