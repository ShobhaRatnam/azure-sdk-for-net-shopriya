// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.TestFramework;
using Azure.Storage.Blobs;
using NUnit.Framework;

namespace Azure.Security.ConfidentialLedger.Storage.Tests
{
    public class LedgerBlobClientTests
    {
        [Test]
        public void Constructor_WithValidArguments_CreatesClient()
        {
            var client = new LedgerBlobClient(
                new Uri("https://ledger.example.com"),
                new Uri("https://storage.blob.core.windows.net/container"),
                new TestTokenCredential());

            Assert.That(client, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNullLedgerEndpoint_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new LedgerBlobClient(
                    null,
                    new Uri("https://storage.blob.core.windows.net/container"),
                    new TestTokenCredential()));
        }

        [Test]
        public void Constructor_WithNullBlobContainerUri_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new LedgerBlobClient(
                    new Uri("https://ledger.example.com"),
                    null,
                    new TestTokenCredential()));
        }

        [Test]
        public void Constructor_WithNullCredential_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new LedgerBlobClient(
                    new Uri("https://ledger.example.com"),
                    new Uri("https://storage.blob.core.windows.net/container"),
                    null));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void UploadAndRegisterBlob_WithNullContent_ThrowsArgumentNullException(bool isAsync)
        {
            var client = new LedgerBlobClient(
                new Uri("https://ledger.example.com"),
                new Uri("https://storage.blob.core.windows.net/container"),
                new TestTokenCredential());

            ArgumentNullException exception;
            if (isAsync)
            {
                exception = Assert.ThrowsAsync<ArgumentNullException>(async () =>
                    await client.UploadAndRegisterBlobAsync("test.txt", (byte[])null));
            }
            else
            {
                exception = Assert.Throws<ArgumentNullException>(() =>
                    client.UploadAndRegisterBlob("test.txt", (byte[])null));
            }

            Assert.That(exception.ParamName, Is.EqualTo("content"));
        }

        [Test]
        public async Task UploadContentToBlobAsync_WithSuccessfulResponse_ReturnsBlobUri()
        {
            var response = new MockResponse(201);
            var client = new LedgerBlobClient(
                new Uri("https://ledger.example.com"),
                new Uri("https://storage.blob.core.windows.net/container"),
                new TestTokenCredential(),
                new LedgerBlobClientOptions
                {
                    BlobClientOptions = new BlobClientOptions { Transport = new MockTransport(response) }
                });

            var upload = await client.UploadContentToBlobAsync(
                "test.txt", new byte[] { 1, 2, 3 }, CancellationToken.None);

            Assert.That(upload.Error, Is.Null);
            Assert.That(upload.BlobUri, Is.EqualTo(new Uri("https://storage.blob.core.windows.net/container/test.txt")));
        }

        [TestCase(403, "AuthorizationPermissionMismatch")]
        [TestCase(409, "BlobAlreadyExists")]
        public async Task UploadContentToBlobAsync_WithFailedResponse_ReturnsError(int status, string errorCode)
        {
            var response = new MockResponse(status);
            response.AddHeader("x-ms-error-code", errorCode);
            response.AddHeader("Content-Type", "application/xml");
            response.SetContent($"<Error><Code>{errorCode}</Code><Message>Upload failed.</Message></Error>");
            var options = new BlobClientOptions { Transport = new MockTransport(response) };
            options.Retry.MaxRetries = 0;

            var client = new LedgerBlobClient(
                new Uri("https://ledger.example.com"),
                new Uri("https://storage.blob.core.windows.net/container"),
                new TestTokenCredential(),
                new LedgerBlobClientOptions { BlobClientOptions = options });

            var upload = await client.UploadContentToBlobAsync(
                "test.txt", new byte[] { 1, 2, 3 }, CancellationToken.None);

            Assert.That(upload.BlobUri, Is.Null);
            Assert.That(upload.Error, Is.Not.Null);
            Assert.That(upload.Error.Status, Is.EqualTo(status));
            Assert.That(upload.Error.ErrorCode, Is.EqualTo(errorCode));
            Assert.That(upload.Error.Message, Does.Contain("Upload failed."));
        }

        private sealed class TestTokenCredential : TokenCredential
        {
            public override AccessToken GetToken(
                TokenRequestContext requestContext,
                CancellationToken cancellationToken)
            {
                return new AccessToken(
                    "test-token",
                    DateTimeOffset.UtcNow.AddHours(1));
            }

            public override ValueTask<AccessToken> GetTokenAsync(
                TokenRequestContext requestContext,
                CancellationToken cancellationToken)
            {
                return new ValueTask<AccessToken>(
                    GetToken(requestContext, cancellationToken));
            }
        }
    }
}
