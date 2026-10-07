// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Security.ConfidentialLedger;
using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading;

namespace Azure.Security.ConfidentialLedger.Storage
{
    /// <summary>
    /// Provides operations that upload blobs and register their digests in Azure Confidential Ledger.
    /// </summary>
    public class LedgerBlobClient
    {
        private readonly ConfidentialLedgerClient _ledgerClient;
        private readonly BlobContainerClient _blobContainerClient;

        /// <summary>
        /// Initializes a new instance for mocking.
        /// </summary>
        protected LedgerBlobClient()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LedgerBlobClient"/>.
        /// </summary>
        /// <param name="ledgerEndpoint">The URI of the Confidential Ledger.</param>
        /// <param name="blobContainerUri">The URI of the Blob Storage container.</param>
        /// <param name="credential">The credential used to authenticate to Azure services.</param>
        public LedgerBlobClient(
            Uri ledgerEndpoint,
            Uri blobContainerUri,
            TokenCredential credential)
            : this(ledgerEndpoint, blobContainerUri, credential, new LedgerBlobClientOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LedgerBlobClient"/>.
        /// </summary>
        /// <param name="ledgerEndpoint">The URI of the Confidential Ledger.</param>
        /// <param name="blobContainerUri">The URI of the Blob Storage container.</param>
        /// <param name="credential">The credential used to authenticate to Azure services.</param>
        /// <param name="options">The options used to configure the underlying clients.</param>
        public LedgerBlobClient(
            Uri ledgerEndpoint,
            Uri blobContainerUri,
            TokenCredential credential,
            LedgerBlobClientOptions options)
        {
            if (ledgerEndpoint == null)
            {
                throw new ArgumentNullException(nameof(ledgerEndpoint));
            }

            if (blobContainerUri == null)
            {
                throw new ArgumentNullException(nameof(blobContainerUri));
            }

            if (credential == null)
            {
                throw new ArgumentNullException(nameof(credential));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.LedgerClientOptions == null)
            {
                throw new ArgumentNullException(nameof(options.LedgerClientOptions));
            }

            if (options.BlobClientOptions == null)
            {
                throw new ArgumentNullException(nameof(options.BlobClientOptions));
            }

            _ledgerClient = new ConfidentialLedgerClient(
                ledgerEndpoint,
                credential,
                options.LedgerClientOptions);

            _blobContainerClient = new BlobContainerClient(
                blobContainerUri,
                credential,
                options.BlobClientOptions);
        }

        /// <summary>
        /// Uploads a blob and registers its digest in Confidential Ledger.
        /// </summary>
        /// <param name="blobName">The name of the blob to upload.</param>
        /// <param name="content">The byte array containing the blob content.</param>
        /// <param name="cancellationToken">The token used to request cancellation of the operation.</param>
        /// <returns>The response containing the blob digest registration result.</returns>
        public virtual Response<BlobDigestRegistrationResult> UploadAndRegisterBlob(
            string blobName,
            byte[] content,
            CancellationToken cancellationToken = default)
        {
            if (blobName == null)
            {
                throw new ArgumentNullException(nameof(blobName));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var upload = UploadContentToBlob(blobName, content, cancellationToken);
            if (upload.Error != null)
            {
                ExceptionDispatchInfo.Capture(upload.Error).Throw();
            }

            throw new NotImplementedException("Digest computation and ledger registration are not implemented yet.");
        }

        /// <summary>
        /// Uploads a blob and registers its digest in Confidential Ledger.
        /// </summary>
        /// <param name="blobName">The name of the blob to upload.</param>
        /// <param name="content">The byte array containing the blob content.</param>
        /// <param name="cancellationToken">The token used to request cancellation of the operation.</param>
        /// <returns>The response containing the blob digest registration result.</returns>
        public virtual async Task<Response<BlobDigestRegistrationResult>> UploadAndRegisterBlobAsync(
            string blobName,
            byte[] content,
            CancellationToken cancellationToken = default)
        {
            if (blobName == null)
            {
                throw new ArgumentNullException(nameof(blobName));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var upload = await UploadContentToBlobAsync(blobName, content, cancellationToken).ConfigureAwait(false);
            if (upload.Error != null)
            {
                ExceptionDispatchInfo.Capture(upload.Error).Throw();
            }

            throw new NotImplementedException("Digest computation and ledger registration are not implemented yet.");
        }

        private (Uri BlobUri, RequestFailedException Error) UploadContentToBlob(
            string blobName,
            byte[] content,
            CancellationToken cancellationToken)
                {
                    BlobClient blob = _blobContainerClient.GetBlobClient(blobName);

                    try
                    {
                        using var stream = new MemoryStream(content, writable: false);
                        blob.Upload(
                            stream,
                            overwrite: false,
                            cancellationToken: cancellationToken);

                        return (blob.Uri, null);
                    }
                    catch (RequestFailedException ex)
                    {
                        return (null, ex);
                    }
                }

        internal async Task<(Uri BlobUri, RequestFailedException Error)> UploadContentToBlobAsync(
            string blobName,
            byte[] content,
            CancellationToken cancellationToken)
                {
                    BlobClient blob = _blobContainerClient.GetBlobClient(blobName);

                    try
                    {
                        using var stream = new MemoryStream(content, writable: false);
                        await blob.UploadAsync(
                            stream,
                            overwrite: false,
                            cancellationToken: cancellationToken).ConfigureAwait(false);

                        return (blob.Uri, null);
                    }
                    catch (RequestFailedException ex)
                    {
                        return (null, ex);
                    }
                }

        private (string TransactionId, RequestFailedException Error) RegisterDigestInLedger(
            string digest,
            CancellationToken cancellationToken)
        {
            using RequestContent content = RequestContent.Create("{\"contents\":\"" + digest + "\"}");
            try
            {
                Azure.Operation operation = _ledgerClient.PostLedgerEntry(
                    Azure.WaitUntil.Completed,
                    content,
                    context: new RequestContext { CancellationToken = cancellationToken });

                return (operation.Id, null);
            }
            catch (RequestFailedException ex)
            {
                return (null, ex);
            }
        }

        internal async Task<(string TransactionId, RequestFailedException Error)> RegisterDigestInLedgerAsync(
            string digest,
            CancellationToken cancellationToken)
        {
            using RequestContent content = RequestContent.Create("{\"contents\":\"" + digest + "\"}");
            try
            {
                Azure.Operation operation = await _ledgerClient.PostLedgerEntryAsync(
                    Azure.WaitUntil.Completed,
                    content,
                    context: new RequestContext { CancellationToken = cancellationToken }).ConfigureAwait(false);

                return (operation.Id, null);
            }
            catch (RequestFailedException ex)
            {
                return (null, ex);
            }
        }

        private static string ComputeImmutableBlobHash(byte[] logEvent)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            byte[] hashBytes;
            using (SHA256 sha256 = SHA256.Create())
            {
                hashBytes = sha256.ComputeHash(logEvent);
            }

            return Convert.ToBase64String(hashBytes);
        }
    }
}
