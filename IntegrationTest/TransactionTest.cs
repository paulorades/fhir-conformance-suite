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
            };
            
            try
            {
                _fhirClient.Transaction(bundle);
            }
            catch(FhirOperationException)
            {
                AssertHelper.CheckSubsetStatusCode(acceptedValues, _fhirClient.LastResult.Status);
            }

            // Make sure that if the server actually returned something outside of 400 or 500, that we fail the test
            AssertHelper.CheckSubsetStatusCode(acceptedValues, _fhirClient.LastResult.Status);
        }
    }
}
