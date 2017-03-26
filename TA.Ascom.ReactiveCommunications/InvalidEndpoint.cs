namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>
    /// A degenerate endpoint (possibly created from an invalud connection string)
    /// that can never transfer any data.
    /// </summary>
    public sealed class InvalidEndpoint : DeviceEndpoint {}
    }