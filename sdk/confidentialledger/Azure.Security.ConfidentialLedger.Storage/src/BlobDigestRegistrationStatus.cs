// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Azure.Security.ConfidentialLedger.Storage
{
    /// <summary>
    /// Describes the outcome of uploading a blob and registering its digest.
    /// </summary>
    public enum BlobDigestRegistrationStatus
    {
        /// <summary>
        /// The blob was uploaded and its ledger transaction was committed.
        /// </summary>
        UploadedAndRegistered,

        /// <summary>
        /// The blob was uploaded and the ledger entry was posted, but commitment was not confirmed.
        /// </summary>
        UploadedRegistrationPending,

        /// <summary>
        /// The blob was uploaded, but ledger registration failed.
        /// </summary>
        UploadedRegistrationFailed,

        /// <summary>
        /// The blob upload failed.
        /// </summary>
        NotUploaded
    }
}
