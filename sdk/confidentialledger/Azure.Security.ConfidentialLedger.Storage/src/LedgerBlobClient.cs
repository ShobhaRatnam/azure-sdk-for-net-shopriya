using Azure.Security.ConfidentialLedger;
using Azure.Storage.Blobs;
using System;
using System.Security.Cryptography;

namespce Azure.Security.ConfidentialLedger.Storage

public class LedgerBlobClient
{
    public LedgerBlobClient(
        BlobServiceClient blobClient,
        ConfidentialLedgerClient ledgerClient,
        LedgerBlobClient options = null);

    private static string ComputeImmutableBlobHash(byte[] logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        byte[] hashBytes = SHA256.HashData(logEvent);
        return Convert.ToBase64String(hashBytes);
    }
}
