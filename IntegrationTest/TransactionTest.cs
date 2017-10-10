// <copyright file="TransactionTest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System.Collections.Generic;
using System.Net;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;

namespace FHIRTest
{
    public class TransactionTest : IClassFixture<DataGenerator>
    {
        private readonly FhirClient _fhirClient;
        private readonly Observation _resource;

        public TransactionTest(DataGenerator dataGenerator)
        {
            _fhirClient = new FhirClient(DataGeneratorHelper.GetServerUrl())
            {
                PreferredFormat = ResourceFormat.Json
            };

            _resource = dataGenerator.GetObservation();
        }

        [Fact]
        public void WhenBasicTransactionSubmitted_AllActionsExecuted()
        {
            // Create observation so that we can do the update and create in a transaction
            var updateResource = _fhirClient.Create(_resource);
            updateResource.Comment = "This is an updated resource";

            var transaction = new TransactionBuilder(_fhirClient.Endpoint)
                .Create(_resource)
                .Update(updateResource.Id, updateResource);

            var bundle = transaction.ToBundle();
            bundle.Type = Bundle.BundleType.Transaction;

            BundleHelper.CleanEntryRequestUrls(bundle, _fhirClient.Endpoint);

            var transactionResult = _fhirClient.Transaction(bundle);

            Assert.NotNull(transactionResult);
            Assert.Equal(2, transactionResult.Entry.Count);

            var createResult = transactionResult.Entry[0];
            AssertHelper.CheckStatusCode(HttpStatusCode.Created, createResult.Response.Status);

            var updateResult = transactionResult.Entry[1];
            AssertHelper.CheckStatusCode(HttpStatusCode.OK, updateResult.Response.Status);

            AssertHelper.CheckStatusCode(HttpStatusCode.OK, _fhirClient.LastResult.Status);
        }

        [Fact]
        public void WhenCRUDTransactionSubmitted_AllActionsExecuted()
        {
            // Create observations so that we can do the update, read, and delete in a transaction
            var updateResource = _fhirClient.Create(_resource);
            var readResource = _fhirClient.Create(_resource);
            var deleteResource = _fhirClient.Create(_resource);

            updateResource.Comment = "This is an updated resource";

            var transaction = new TransactionBuilder(_fhirClient.Endpoint)
                .Create(_resource)
                .Read("Observation", readResource.Id)
                .Update(updateResource.Id, updateResource)
                .Delete("Observation", deleteResource.Id);

            var bundle = transaction.ToBundle();
            bundle.Type = Bundle.BundleType.Transaction;

            BundleHelper.CleanEntryRequestUrls(bundle, _fhirClient.Endpoint);

            var transactionResult = _fhirClient.Transaction(bundle);

            Assert.NotNull(transactionResult);
            Assert.Equal(4, transactionResult.Entry.Count);

            var createResult = transactionResult.Entry[0];
            AssertHelper.CheckStatusCode(HttpStatusCode.Created, createResult.Response.Status);

            var readResult = transactionResult.Entry[1];
            AssertHelper.CheckStatusCode(HttpStatusCode.OK, readResult.Response.Status);

            var updateResult = transactionResult.Entry[2];
            AssertHelper.CheckStatusCode(HttpStatusCode.OK, updateResult.Response.Status);

            var deleteResult = transactionResult.Entry[3];
            AssertHelper.CheckDeleteStatusCode(deleteResult.Response.Status);

            AssertHelper.CheckStatusCode(HttpStatusCode.OK, _fhirClient.LastResult.Status);
        }

        [Fact]
        public void WhenTransactionSubmittedWithInvalidPart_AllActionsFail()
        {
            // Create an observation so that we can do the update
            var updateResource1 = _fhirClient.Create(_resource);
            Observation updateResource2 = new Observation();
            updateResource1.CopyTo(updateResource2);
            updateResource1.Comment = "This is an updated resource";
            updateResource2.Comment = "Other update to the same resourceId";

            // Attempting to update the same resource twice in a single transaction is supposed to fail
            // "If any resource identities (including resolved identities from conditional update/delete) overlap in steps 1-3, then the transaction SHALL fail."
            // See https://www.hl7.org/fhir/http.html#transaction for details
            var transaction = new TransactionBuilder(_fhirClient.Endpoint)
                .Update(updateResource1.Id, updateResource1)
                .Update(updateResource1.Id, updateResource2);
            var bundle = transaction.ToBundle();
            bundle.Type = Bundle.BundleType.Transaction;

            BundleHelper.CleanEntryRequestUrls(bundle, _fhirClient.Endpoint);

            var acceptedValues = new HashSet<string>
            {
                ((int)HttpStatusCode.InternalServerError).ToString(),
                ((int)HttpStatusCode.BadRequest).ToString(),
                "500 Internal Server Error",
                "400 Bad Request",
            };

            try
            {
                _fhirClient.Transaction(bundle);
            }
            catch (FhirOperationException)
            {
                AssertHelper.CheckSubsetStatusCode(acceptedValues, _fhirClient.LastResult.Status);
            }

            // Make sure that if the server actually returned something outside of 400 or 500, that we fail the test
            AssertHelper.CheckSubsetStatusCode(acceptedValues, _fhirClient.LastResult.Status);
        }
    }
}
