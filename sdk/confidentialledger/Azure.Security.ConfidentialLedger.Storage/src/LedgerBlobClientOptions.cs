// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Security.ConfidentialLedger;
using Azure.Storage.Blobs;

namespace Azure.Security.ConfidentialLedger.Storage
{
    /// <summary>
    /// Provides configuration options for <see cref="LedgerBlobClient"/>.
    /// </summary>
    public class LedgerBlobClientOptions : ClientOptions
    {
        /// <summary>
        /// Represents the service version exposed by this client.
        /// </summary>
        public enum ServiceVersion
        {
            /// <summary>
            /// The initial version of the blob registration workflow.
            /// </summary>
            V1_0 = 1
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LedgerBlobClientOptions"/>.
        /// </summary>
        /// <param name="version">The service version.</param>
        public LedgerBlobClientOptions(ServiceVersion version = ServiceVersion.V1_0)
        {
        }

        /// <summary>
        /// Gets or sets the options used to configure the Confidential Ledger client.
        /// </summary>
        public ConfidentialLedgerClientOptions LedgerClientOptions { get; set; } = new ConfidentialLedgerClientOptions();

        /// <summary>
        /// Gets or sets the options used to configure the Blob Storage client.
        /// </summary>
        public BlobClientOptions BlobClientOptions { get; set; } = new BlobClientOptions();
    }
}
