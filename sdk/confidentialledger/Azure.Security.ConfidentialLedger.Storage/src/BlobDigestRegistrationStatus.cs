namespace Azure.Security.ConfidentialLedger.Storage
{
    /// <summary>
    /// The status of the blob digest registration.
    /// </summary>
    public enum BlobDigestRegistrationStatus
    {
        UploadedAndRegistered,     // both steps succeeded, transaction committed
        UploadedRegistrationPending, // uploaded; entry posted but commit not confirmed
        UploadedRegistrationFailed,  // uploaded; ledger registration failed -> recoverable
        NotUploaded                  // upload itself failed -> nothing to recover
    }
}
