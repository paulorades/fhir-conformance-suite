using System;
using System.Collections.Generic;
using System.Net;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;
using Assert = Xunit.Assert;

namespace FHIRTest
{
    /// <summary>
    /// Testing Restful API FHIR spec defined here http://hl7.org/fhir/http.html#2.21.0
    /// </summary>
    public class RestfulApiTest : IClassFixture<DataGenerator>
    {
        private readonly FhirClient _fhirClient;
        private readonly DomainResource _resource;

        public RestfulApiTest(DataGenerator dataGenerator)
        {
            _fhirClient = new FhirClient(DataGeneratorHelper.GetServerUrl())
            {
                PreferredFormat = ResourceFormat.Json
            };

            _resource = dataGenerator.GetObservation();
        }

        /// <summary>
        /// Testing the Read behaviour of the spec http://hl7.org/fhir/http.html#read
        /// </summary>
        /// <remarks>The test.fhir.org server occasionally deadlocks on this test.</remarks>
        [Fact]
        public void WhenResourceRead()
        {
            var createdModelOnTheServer = _fhirClient.Create(_resource);
            var domainResource = _fhirClient.Read<DomainResource>($"{_resource.GetType().Name}/{createdModelOnTheServer.Id}");

            Assert.NotNull(domainResource);
            AssertHelper.CheckStatusCode(HttpStatusCode.OK, _fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Testing Version Read of the spec - http://hl7.org/fhir/http.html#vread
        /// </summary>
        [Fact]
        public void WhenVersionRead()
        {
            var createdModelOnTheServer = _fhirClient.Create(_resource);
            var domainResource = _fhirClient.Read<DomainResource>($"{_resource.GetType().Name}/{createdModelOnTheServer.Id}/_history/{createdModelOnTheServer.VersionId}");

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
            var createdModel = _fhirClient.Create(_resource);

            // Per the spec, a div and status are required in a narrative https://www.hl7.org/fhir/narrative.html#Narrative
            createdModel.Text = new Narrative
            {
                Div = @"<div xmlns=""http://www.w3.org/1999/xhtml"">This is a simple example with only plain text </div>",
                Status = Narrative.NarrativeStatus.Generated
            };

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
            _resource.Id = Guid.NewGuid().ToString();

            // WhenResourceUpdated with search params for resources which have been updated since 1999
            SearchParams searchParams = new SearchParams();
            searchParams.Add("_lastUpdated", "gt1999-01-01");

            HttpStatusCode statusCode = HttpStatusCode.HttpVersionNotSupported;

            try
            {
                _fhirClient.Update(_resource, searchParams);
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
            var createdModel = _fhirClient.Create(_resource);

            _fhirClient.Delete(createdModel);

            AssertHelper.CheckDeleteStatusCode(_fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Testing spec - http://hl7.org/fhir/http.html#delete
        /// </summary>
        [Fact]
        public void WhenDeletedResourceRead()
        {
            var createdModel = _fhirClient.Create(_resource);

            _fhirClient.Delete(createdModel);

            HttpStatusCode statusCode = HttpStatusCode.HttpVersionNotSupported;

            try
            {
                _fhirClient.Read<DomainResource>($"{_resource.GetType().Name}/{createdModel.Id}");
            }
            catch (FhirOperationException ex)
            {
                statusCode = ex.Status;
            }

            Assert.Equal(HttpStatusCode.Gone, statusCode);
        }

        /// <summary>
        /// Testing the behaviour of the spec http://hl7.org/fhir/http.html#create
        /// </summary>
        [Fact]
        public void WhenResourceCreated()
        {
            _fhirClient.Create(_resource);

            AssertHelper.CheckStatusCode(HttpStatusCode.Created, _fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Testing the behaviour of the spec http://hl7.org/fhir/http.html#ccreate
        /// </summary>
        /// <remarks>The test.fhir.org server occasionally deadlocks on this test.</remarks>
        [Fact]
        public void WhenResourceCreatedWithNoMatches()
        {
            var searchParams = new SearchParams();
            searchParams.Add("_id", Guid.NewGuid().ToString());

            _fhirClient.Create(_resource, searchParams);

            AssertHelper.CheckStatusCode(HttpStatusCode.Created, _fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Testing the behaviour of the spec http://hl7.org/fhir/http.html#ccreate
        /// One Match: The server should ignore the post and return 200 OK
        /// </summary>
        /// <remarks>Both Tesltra and Vonk failed this test</remarks>
        /// <remarks>The test.fhir.org server occasionally deadlocks on this test.</remarks>
        [Fact]
        public void WhenResourceCreatedWithOneMatch()
        {
            var createdModel = _fhirClient.Create(_resource);

            var searchParams = new SearchParams();
            searchParams.Add("_id", createdModel.Id);

            _fhirClient.Create(_resource, searchParams);

            AssertHelper.CheckStatusCode(HttpStatusCode.OK, _fhirClient.LastResult.Status);
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

            try
            {
                _fhirClient.Create(_resource, searchParams);
            }
            catch (FhirOperationException exception)
            {
                // When a 412 status code is returned, the _fhirClient throws an FhirOperationException
                if (exception.Status != HttpStatusCode.PreconditionFailed)
                {
                    throw exception;
                }
            }
            
            AssertHelper.CheckStatusCode(HttpStatusCode.PreconditionFailed, _fhirClient.LastResult.Status);
        }
    }
}
