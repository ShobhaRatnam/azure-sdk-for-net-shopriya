// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Azure.Security.ConfidentialLedger.Storage
{
    /// <summary>
    /// Represents the outcome and details of a blob digest registration workflow.
    /// </summary>
    public class BlobDigestRegistrationResult
    {
        /// <summary>
        /// Gets the URI of the uploaded blob, if the upload succeeded.
        /// </summary>
        public Uri BlobUri { get; internal set; }

        /// <summary>
        /// Gets the Confidential Ledger transaction ID, if the digest was registered.
        /// </summary>
        public string TransactionId { get; internal set; }

        /// <summary>
        /// Gets the status of the blob upload and digest registration workflow.
        /// </summary>
        public BlobDigestRegistrationStatus Status { get; internal set; }

        /// <summary>
        /// Gets the HTTP status code if an underlying service request failed.
        /// </summary>
        public int? HttpStatus { get; internal set; }

        /// <summary>
        /// Gets the service-specific error code if an underlying service request failed.
        /// </summary>
        public string ErrorCode { get; internal set; }

        /// <summary>
        /// Gets the error message if an underlying service request failed.
        /// </summary>
        public string ErrorMessage { get; internal set; }
    }
}
