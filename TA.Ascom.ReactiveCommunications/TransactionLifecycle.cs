namespace TA.Ascom.ReactiveCommunications {
    /// <summary>
    /// States of the transaction life cycle.
    /// </summary>
    public enum TransactionLifecycle
        {
        /// <summary>
        /// The transaction has been committed and failed to complete.
        /// </summary>
        Failed = 0,
        /// <summary>
        /// The transaction has been created but not yet committed.
        /// </summary>
        Created = 1,
        /// <summary>
        /// The transaction has been committed and is currently in progress.
        /// </summary>
        InProgress = 2,
        /// <summary>
        /// The transaction has been committed and completed successfully.
        /// </summary>
        Completed = 3
        }
    }