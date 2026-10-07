// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Azure.Core;
using Azure.Core.TestFramework;
using Azure.Identity;

namespace Azure.Security.ConfidentialLedger.Storage.Tests
{
    public class LedgerBlobClientTestEnvironment : TestEnvironment
    {
        public Uri BlobContainerUri => new Uri(GetRecordedVariable("LEDGER_BLOB_CONTAINER_URI"));

        public Uri LedgerUri => new Uri(GetRecordedVariable("LEDGER_URI"));

        protected override TokenCredential CreateDeveloperCredential() =>
            new DefaultAzureCredential();
    }
}
