using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace TA.Ascom.ReactiveCommunications.Specifications.Fakes
    {
    /// <summary>
    ///     Buffers the input sequence effectively forever (actually 1 day).
    ///     When the input sequence completes, the buffer is converted to a string and placed in the Response property.
    /// </summary>
    internal class BufferInputUntilCompletedTransaction : DeviceTransaction
        {
        public BufferInputUntilCompletedTransaction(string command) : base(command) {}

        public string Value { get; private set; }

        public override void ObserveResponse(IObservable<char> source)
            {
            source.Buffer(TimeSpan.FromDays(1))
                .Select(p => new string(p.ToArray()))
                .ObserveOn(NewThreadScheduler.Default)
                .Subscribe(OnNext, OnError, OnCompleted);
            }

        protected override void OnCompleted()
            {
            if (Response.Any())
                Value = Response.Single();
            base.OnCompleted();
            }
        }
    }