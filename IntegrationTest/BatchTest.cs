// <copyright file="BatchTest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;
using System.Net;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;

namespace FHIRTest
{
    public class BatchTest : IClassFixture<DataGenerator>
    {
        private readonly FhirClient _fhirClient;
        private readonly Observation _resource;

        public BatchTest(DataGenerator dataGenerator)
        {
            _fhirClient = new FhirClient(DataGeneratorHelper.GetServerUrl())
            {
                PreferredFormat = ResourceFormat.Json
            };

            _resource = dataGenerator.GetObservation();
        }

        [Fact]
        public void WhenBatchSubmitted_AllActionsExecuted()
        {
            // Create observations so that we can do the update, read, and delete in the batch
            var updateResource = _fhirClient.Create(_resource);
            var readResource = _fhirClient.Create(_resource);
            var deleteResource = _fhirClient.Create(_resource);

            updateResource.Comment = "This is an updated resource";

            var transaction = new TransactionBuilder(_fhirClient.Endpoint)
                .Create(_resource)
                .Read("Observation", readResource.Id)
                .Delete("Observation", deleteResource.Id)
                .Update(updateResource.Id, updateResource);

            var bundle = transaction.ToBundle();
            bundle.Type = Bundle.BundleType.Batch;

            BundleHelper.CleanEntryRequestUrls(bundle, _fhirClient.Endpoint);

            var batchResult = _fhirClient.Transaction(bundle);

            Assert.NotNull(batchResult);

            Assert.NotNull(batchResult.Entry);
            Assert.Equal(4, batchResult.Entry.Count);

            var createResult = batchResult.Entry[0];
            AssertHelper.CheckStatusCode(HttpStatusCode.Created, createResult.Response.Status);

            var readResult = batchResult.Entry[1];
            AssertHelper.CheckStatusCode(HttpStatusCode.OK, readResult.Response.Status);

            var deleteResult = batchResult.Entry[2];
            AssertHelper.CheckDeleteStatusCode(deleteResult.Response.Status);

            var updateResult = batchResult.Entry[3];
            AssertHelper.CheckStatusCode(HttpStatusCode.OK, updateResult.Response.Status);

            AssertHelper.CheckStatusCode(HttpStatusCode.OK, _fhirClient.LastResult.Status);
        }

        [Fact]
        public void WhenBatchSubmittedWithInvalidItem_AllActionsExecuted()
        {
            var updateResource = (Observation)_resource.DeepCopy();
            updateResource.Id = Guid.NewGuid().ToString();

            // Per the spec, a create is handled via a post and update is handled via a put.
            // In a put "The request body SHALL be a Resource with an id element that has an identical value to the [id] in the URL. If no id element is provided, or the value is wrong, the server SHALL respond with an HTTP 400 error code..."
            var transaction = new TransactionBuilder(_fhirClient.Endpoint)
                .Create(_resource)
                .Update(Guid.NewGuid().ToString(), updateResource); // Pass a different id than the update resource id.

            var bundle = transaction.ToBundle();
            bundle.Type = Bundle.BundleType.Batch;

            BundleHelper.CleanEntryRequestUrls(bundle, _fhirClient.Endpoint);

            var batchResult = _fhirClient.Transaction(bundle);

            Assert.NotNull(batchResult);
            Assert.Equal(2, batchResult.Entry.Count);

            var createResult = batchResult.Entry[0];
            AssertHelper.CheckStatusCode(HttpStatusCode.Created, createResult.Response.Status);

            var updateResult = batchResult.Entry[1];
            AssertHelper.CheckStatusCode(HttpStatusCode.BadRequest, updateResult.Response.Status);

            AssertHelper.CheckStatusCode(HttpStatusCode.OK, _fhirClient.LastResult.Status);
        }
    }
}
