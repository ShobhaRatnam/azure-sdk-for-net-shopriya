// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Azure.Security.ConfidentialLedger.Storage
{
    /// <summary>
    /// Provides factory methods for creating model instances for mocking.
    /// </summary>
    public static class ConfidentialLedgerStorageModelFactory
    {
        /// <summary>
        /// Creates a new <see cref="BlobDigestRegistrationResult"/> instance for mocking.
        /// </summary>
        /// <param name="blobUri">The URI of the uploaded blob.</param>
        /// <param name="transactionId">The Confidential Ledger transaction ID.</param>
        /// <param name="status">The outcome of the workflow.</param>
        /// <param name="httpStatus">The HTTP status code for a failed service request.</param>
        /// <param name="errorCode">The service-specific error code.</param>
        /// <param name="errorMessage">The service error message.</param>
        /// <returns>A new <see cref="BlobDigestRegistrationResult"/> instance.</returns>
        public static BlobDigestRegistrationResult BlobDigestRegistrationResult(
            Uri blobUri,
            string transactionId,
            BlobDigestRegistrationStatus status,
            int? httpStatus,
            string errorCode,
            string errorMessage)
        {
            return new BlobDigestRegistrationResult
            {
                BlobUri = blobUri,
                TransactionId = transactionId,
                Status = status,
                HttpStatus = httpStatus,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
        }
    }
}
