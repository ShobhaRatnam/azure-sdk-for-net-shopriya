// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core.TestFramework;
using Azure.Storage.Blobs;
using NUnit.Framework;

namespace Azure.Security.ConfidentialLedger.Storage.Tests
{
    public class LedgerBlobClientLiveTests : RecordedTestBase<LedgerBlobClientTestEnvironment>
    {
        public LedgerBlobClientLiveTests(bool isAsync) : base(isAsync)
        {
            TestDiagnostics = false;
        }

        [LiveOnly]
        [RecordedTest]
        public async Task BlobContainer_IsReachable()
        {
            BlobContainerClient container = InstrumentClient(new BlobContainerClient(
                TestEnvironment.BlobContainerUri,
                TestEnvironment.Credential,
                InstrumentClientOptions(new BlobClientOptions())));

            var response = await container.GetPropertiesAsync();

            Assert.That(response.GetRawResponse().Status, Is.EqualTo(200));
        }

        [LiveOnly]
        [RecordedTest]
        public async Task Blob_UploadsTextSuccessfully()
        {
            BlobContainerClient container = InstrumentClient(new BlobContainerClient(
                TestEnvironment.BlobContainerUri,
                TestEnvironment.Credential,
                InstrumentClientOptions(new BlobClientOptions())));

            BlobClient blob = InstrumentClient(container.GetBlobClient($"live-test-{Guid.NewGuid():N}.txt"));
            const string text = "Hello from the Confidential Ledger Storage live test.";
            bool uploaded = false;

            try
            {
                byte[] content = Encoding.UTF8.GetBytes(text);
                var response = await blob.UploadAsync(new BinaryData(content), overwrite: false);
                uploaded = true;

                Assert.That(response.GetRawResponse().Status, Is.EqualTo(201));

                var download = await blob.DownloadContentAsync();
                Assert.That(download.Value.Content.ToString(), Is.EqualTo(text));
            }
            finally
            {
                if (uploaded)
                {
                    await blob.DeleteIfExistsAsync();
                }
            }
        }

        [LiveOnly]
        [RecordedTest]
        public async Task UploadContentToBlobAsync_UploadsBytesSuccessfully()
        {
            var client = new LedgerBlobClient(
                new Uri("https://ledger.example.com"),
                TestEnvironment.BlobContainerUri,
                TestEnvironment.Credential,
                new LedgerBlobClientOptions
                {
                    BlobClientOptions = InstrumentClientOptions(new BlobClientOptions())
                });

            BlobContainerClient container = InstrumentClient(new BlobContainerClient(
                TestEnvironment.BlobContainerUri,
                TestEnvironment.Credential,
                InstrumentClientOptions(new BlobClientOptions())));

            string blobName = $"helper-live-test-{Guid.NewGuid():N}.txt";
            BlobClient blob = InstrumentClient(container.GetBlobClient(blobName));
            byte[] content = Encoding.UTF8.GetBytes("Hello from the blob upload helper live test.");
            bool uploaded = false;

            try
            {
                var upload = await client.UploadContentToBlobAsync(
                    blobName, content, CancellationToken.None);
                uploaded = upload.Error == null;

                Assert.That(upload.Error, Is.Null, upload.Error?.ToString());
                Assert.That(upload.BlobUri, Is.EqualTo(blob.Uri));
                TestContext.Progress.WriteLine($"Uploaded blob URI: {upload.BlobUri}");

                var download = await blob.DownloadContentAsync();
                Assert.That(download.Value.Content.ToArray(), Is.EqualTo(content));
            }
            finally
            {
                if (uploaded)
                {
                    await blob.DeleteIfExistsAsync();
                }
            }
        }

        [LiveOnly]
        [RecordedTest]
        public async Task UploadAndRegisterBlobAsync_UploadsAndRegistersSuccessfully()
        {
            var client = new LedgerBlobClient(
                TestEnvironment.LedgerUri,
                TestEnvironment.BlobContainerUri,
                TestEnvironment.Credential);

            string blobName = $"workflow-live-test-{Guid.NewGuid():N}.txt";
            byte[] content = Encoding.UTF8.GetBytes("Hello from the complete upload and register workflow.");

            Response<BlobDigestRegistrationResult> response =
                await client.UploadAndRegisterBlobAsync(blobName, content, CancellationToken.None);
            BlobDigestRegistrationResult result = response.Value;

            TestContext.Out.WriteLine($"[WorkflowResult] Status: {result.Status}");
            TestContext.Out.WriteLine($"[WorkflowResult] Blob URI: {result.BlobUri}");
            TestContext.Out.WriteLine($"[WorkflowResult] Transaction ID: {result.TransactionId}");
            TestContext.Out.WriteLine($"[WorkflowResult] HTTP status: {result.HttpStatus?.ToString() ?? "<null>"}");
            TestContext.Out.WriteLine($"[WorkflowResult] Error code: {result.ErrorCode ?? "<null>"}");
            TestContext.Out.WriteLine($"[WorkflowResult] Error message: {result.ErrorMessage ?? "<null>"}");
            TestContext.Out.WriteLine($"[WorkflowResult] Raw response HTTP status: {response.GetRawResponse().Status}");

            Assert.That(result.Status, Is.EqualTo(BlobDigestRegistrationStatus.UploadedAndRegistered));
            Assert.That(result.TransactionId, Is.Not.Null.And.Not.Empty);
            Assert.That(result.HttpStatus, Is.Null);
            Assert.That(result.ErrorCode, Is.Null);
            Assert.That(result.ErrorMessage, Is.Null);
            Assert.That(response.GetRawResponse().Status, Is.EqualTo(200));

            var container = new BlobContainerClient(TestEnvironment.BlobContainerUri, TestEnvironment.Credential);
            BlobClient blob = container.GetBlobClient(blobName);
            Assert.That(result.BlobUri, Is.EqualTo(blob.Uri));

            var download = await blob.DownloadContentAsync();
            Assert.That(download.Value.Content.ToArray(), Is.EqualTo(content));

            // Retain the blob for inspection in the Azure portal.
        }

        [LiveOnly]
        [RecordedTest]
        public async Task RegisterDigestInLedgerAsync_CompletesSuccessfully()
        {
            var client = new LedgerBlobClient(
                TestEnvironment.LedgerUri,
                new Uri("https://storage.blob.core.windows.net/container"),
                TestEnvironment.Credential);

            byte[] testContent = Encoding.UTF8.GetBytes($"ledger-live-test-{Guid.NewGuid():N}");
            string digest;
            using (SHA256 sha256 = SHA256.Create())
            {
                digest = Convert.ToBase64String(sha256.ComputeHash(testContent));
            }

            var result = await client.RegisterDigestInLedgerAsync(digest, CancellationToken.None);

            Assert.That(result.Error, Is.Null, result.Error?.ToString());
            Assert.That(result.TransactionId, Is.Not.Null.And.Not.Empty);
            TestContext.Progress.WriteLine($"Ledger transaction ID: {result.TransactionId}");
        }
    }
}
