// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
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
