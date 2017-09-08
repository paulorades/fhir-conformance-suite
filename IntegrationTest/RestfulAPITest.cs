using System;
using System.Collections.Generic;
using System.Net;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Assert = Xunit.Assert;

namespace FHIRTest
{
    /// <summary>
    /// Testing Restful API FHIR spec defined here http://hl7.org/fhir/http.html#2.21.0
    /// </summary>
    public class RestfulApiTest : IClassFixture<DataGenerator>
    {
        private readonly FhirClient _fhirClient;
        private readonly DomainResource _observation;

        public RestfulApiTest(DataGenerator dataGenerator)
        {
            _fhirClient = new FhirClient(DataGeneratorHelper.GetServerUrl())
            {
                PreferredFormat = ResourceFormat.Json
            };

            _observation = restfulApiDataGenerator.GetObservation();
        }

        /// <summary>
        /// Testing the Read behaviour of the spec http://hl7.org/fhir/http.html#read
        /// </summary>
        [Fact]
        public void WhenResourceRead()
        {
            var createdModelOnTheServer = _fhirClient.Create(_observation);
            var domainResource = _fhirClient.Read<DomainResource>($"{_observation.GetType().Name}/{createdModelOnTheServer.Id}");

            Assert.NotNull(domainResource);
            Assert.Equal(((int)HttpStatusCode.OK).ToString(), _fhirClient.LastResult.Status);
        }


        /// <summary>
        /// Testing conditional Read behaviour of the spec https://www.hl7.org/fhir/http.html#cread
        /// </summary>
        [Fact]
        public void WhenResourceReadWithIfModifiedSinceHeader()
        {
            var createdModelOnTheServer = _fhirClient.Create(_observation);

            var domainResource = _fhirClient.Read<DomainResource>($"{_observation.GetType().Name}/{createdModelOnTheServer.Id}", ifModifiedSince: DateTimeOffset.Now);

            bool success = false;
            string message = "Test failed";

            if (domainResource != null)
            {
                success = true;
                message = "Domain Resournce not null";
            }

            if (((int)HttpStatusCode.NotModified).ToString() == _fhirClient.LastResult.Status)
            {
                success = true;
                message = "Status code is not modified";
            }

            Assert.True(success, message);
        }

        /// <summary>
        /// Testing Version Read of the spec - http://hl7.org/fhir/http.html#vread
        /// </summary>
        [Fact]
        public void WhenVersionRead()
        {
            var createdModelOnTheServer = _fhirClient.Create(_observation);
            var domainResource = _fhirClient.Read<DomainResource>($"{_observation.GetType().Name}/{createdModelOnTheServer.Id}/_history/{createdModelOnTheServer.VersionId}");

            Assert.NotNull(domainResource);
            Assert.Equal(_fhirClient.LastResult.Etag, createdModelOnTheServer.VersionId);
            Assert.NotNull(_fhirClient.LastResult.LastModified);
        }

        /// <summary>
        /// Testing the update behaviour of the spec http://hl7.org/fhir/http.html#update
        /// </summary>
        [Fact]
        public void WhenResourceUpdated()
        {
            var createdModel = _fhirClient.Create(_observation);

            createdModel.Text = new Narrative();

            var domainResource = _fhirClient.Update(createdModel);

            Assert.NotNull(domainResource.Text);
        }

        /// <summary>
        /// Testing the behaviour of the spec (http://hl7.org/fhir/http.html#cond-update)
        /// Multiple matches: The server returns a 412 Precondition Failed error indicating the client's criteria were not selective enough
        /// </summary>
        /// <remarks>Telstra Health passed, but Vonk failed this test</remarks>
        [Fact]
        public void WhenUpdateForMultipleResourceMatches()
        {
            _observation.Id = Guid.NewGuid().ToString();

            // WhenResourceUpdated with search params for resources which have been updated since 1999
            SearchParams searchParams = new SearchParams();
            searchParams.Add("_lastUpdated", "gt1999-01-01");

            HttpStatusCode statusCode = HttpStatusCode.HttpVersionNotSupported;

            try
            {
                _fhirClient.Update(_observation, searchParams);
            }
            catch (FhirOperationException ex)
            {
                statusCode = ex.Status;
            }

            Assert.Equal(HttpStatusCode.PreconditionFailed, statusCode);
        }

        /// <summary>
        /// Testing spec - http://hl7.org/fhir/http.html#patch
        /// </summary>
        /// <remarks>Empty method as FHIRClient doesn't support "Patch" method</remarks>
        [Fact]
        public void Patch()
        {
        }

        /// <summary>
        /// Testing spec - http://hl7.org/fhir/http.html#delete
        /// </summary>
        [Fact]
        public void WhenResourceDeleted()
        {
            var createdModel = _fhirClient.Create(_observation);

            _fhirClient.Delete(createdModel);

            Assert.Equal(((int)HttpStatusCode.OK).ToString(), _fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Testing spec - http://hl7.org/fhir/http.html#delete
        /// </summary>
        [Fact]
        public void WhenDeletedResourceRead()
        {
            var createdModel = _fhirClient.Create(_observation);

            _fhirClient.Delete(createdModel);

            HttpStatusCode statusCode = HttpStatusCode.HttpVersionNotSupported;

            try
            {
                _fhirClient.Read<DomainResource>($"{_observation.GetType().Name}/{createdModel.Id}");
            }
            catch (FhirOperationException ex)
            {
                statusCode = ex.Status;
            }

            Assert.Equal(HttpStatusCode.Gone, statusCode);
        }

        /// <summary>
        /// Create 2 observations and then try to delete, expected according to spec is either PreCondition failed or No content
        /// </summary>
        [Fact]
        public void WhenDeleteResourceHasMultipleMatches()
        {
            _fhirClient.Create(_observation);
            _fhirClient.Create(_observation);

            _fhirClient.Delete(nameof(Observation), new SearchParams().Add("status", "final"));

            Assert.Equal(((int)HttpStatusCode.NoContent).ToString(), _fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Testing the behaviour of the spec http://hl7.org/fhir/http.html#create
        /// </summary>
        [Fact]
        public void WhenResourceCreated()
        {
            _fhirClient.Create(_observation);
            Assert.Equal(((int)HttpStatusCode.Created).ToString(), _fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Testing the behaviour of the spec http://hl7.org/fhir/http.html#ccreate
        /// </summary>
        [Fact]
        public void WhenResourceCreatedWithNoMatches()
        {
            var searchParams = new SearchParams();
            searchParams.Add("_id", Guid.NewGuid().ToString());

            _fhirClient.Create(_observation, searchParams);

            Assert.Equal(((int)HttpStatusCode.Created).ToString(), _fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Testing the behaviour of the spec http://hl7.org/fhir/http.html#ccreate
        /// One Match: The server should ignore the post and return 200 OK
        /// </summary>
        /// <remarks>Both Tesltra and Vonk failed this test</remarks>
        [Fact]
        public void WhenResourceCreatedWithOneMatch()
        {
            var createdModel = _fhirClient.Create(_observation);

            var searchParams = new SearchParams();
            searchParams.Add("_id", createdModel.Id);

            _fhirClient.Create(_observation, searchParams);

            Assert.Equal(((int)HttpStatusCode.OK).ToString(), _fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Testing the behaviour of the spec http://hl7.org/fhir/http.html#ccreate
        /// One Match: The server ignore the post and returns 200 OK
        /// </summary>
        /// <remarks>Both Tesltra and Vonk failed this test</remarks>
        [Fact]
        public void WhenResourceCreatedWithMultipleMatches()
        {
            var searchParams = new SearchParams();
            searchParams.Add("_lastUpdated", "gt2000-01-01");

            _fhirClient.Create(_observation, searchParams);

            Assert.Equal(((int)HttpStatusCode.PreconditionFailed).ToString(), _fhirClient.LastResult.Status);
        }
    }
}
