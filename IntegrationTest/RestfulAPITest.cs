using System;
using System.Linq;
using System.Net;
using System.Reflection;
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
        /// <summary>
        /// Testing the Read behaviour of the spec http://hl7.org/fhir/http.html#read
        /// </summary>
        [Theory]
        [MemberData(nameof(DataGenerator.GetUrlResourceType), MemberType = typeof(DataGenerator))]
        public void WhenResourceRead(FhirClient client, DomainResource resourceInstance)
        {
            var createdModelOnTheServer = client.Create(resourceInstance);
            var domainResource = client.Read<DomainResource>($"{resourceInstance.GetType().Name}/{createdModelOnTheServer.Id}");

            Assert.NotNull(domainResource);
            Assert.Equal(((int)HttpStatusCode.OK).ToString(), client.LastResult.Status);
        }

        /// <summary>
        /// Testing Version Read of the spec - http://hl7.org/fhir/http.html#vread
        /// </summary>
        [Theory]
        [MemberData(nameof(DataGenerator.GetUrlResourceType), MemberType = typeof(DataGenerator))]
        public void WhenVersionRead(FhirClient client, DomainResource resourceInstance)
        {
            var createdModelOnTheServer = client.Create(resourceInstance);
            var domainResource = client.Read<DomainResource>($"{resourceInstance.GetType().Name}/{createdModelOnTheServer.Id}/_history/{createdModelOnTheServer.VersionId}");

            Assert.NotNull(domainResource);
            Assert.NotNull(client.LastResult.Etag == createdModelOnTheServer.VersionId);
            Assert.NotNull(client.LastResult.LastModified);
        }

        /// <summary>
        /// Testing the update behaviour of the spec http://hl7.org/fhir/http.html#update
        /// </summary>
        [Theory]
        [MemberData(nameof(DataGenerator.GetUrlResourceType), MemberType = typeof(DataGenerator))]
        public void WhenResourceUpdated(FhirClient client, DomainResource resourceInstance)
        {
            var createdModel = client.Create(resourceInstance);

            createdModel.Text = new Narrative();

            var domainResource = client.Update(createdModel);

            Assert.NotNull(domainResource.Text);
        }

        /// <summary>
        /// Testing the behaviour of the spec (http://hl7.org/fhir/http.html#cond-update)
        /// Multiple matches: The server returns a 412 Precondition Failed error indicating the client's criteria were not selective enough
        /// </summary>
        /// <remarks>Telstra Health passed, but Vonk failed this test</remarks>
        [Theory]
        [MemberData(nameof(DataGenerator.GetUrlResourceType), MemberType = typeof(DataGenerator))]
        public void WhenUpdateForMultipleResourceMatches(FhirClient client, DomainResource resourceInstance)
        {
            resourceInstance.Id = Guid.NewGuid().ToString();

            // WhenResourceUpdated with search params for resources which have been updated since 1999
            SearchParams searchParams = new SearchParams();
            searchParams.Add("_lastUpdated", "gt1999-01-01");

            HttpStatusCode statusCode = HttpStatusCode.HttpVersionNotSupported;

            try
            {
                client.Update(resourceInstance, searchParams);
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
        [Theory]
        [MemberData(nameof(DataGenerator.GetUrlResourceType), MemberType = typeof(DataGenerator))]
        public void Patch(FhirClient client, DomainResource resourceInstance)
        {
        }

        /// <summary>
        /// Testing spec - http://hl7.org/fhir/http.html#delete
        /// </summary>
        [Theory]
        [MemberData(nameof(DataGenerator.GetUrlResourceType), MemberType = typeof(DataGenerator))]
        public void WhenResourceDeleted(FhirClient client, DomainResource resourceInstance)
        {
            var createdModel = client.Create(resourceInstance);

            client.Delete(createdModel);

            Assert.Equal(((int)HttpStatusCode.OK).ToString(), client.LastResult.Status);
        }

        /// <summary>
        /// Testing spec - http://hl7.org/fhir/http.html#delete
        /// </summary>
        [Theory]
        [MemberData(nameof(DataGenerator.GetUrlResourceType), MemberType = typeof(DataGenerator))]
        public void WhenDeletedResourceRead(FhirClient client, DomainResource resourceInstance)
        {
            var createdModel = client.Create(resourceInstance);

            client.Delete(createdModel);

            HttpStatusCode statusCode = HttpStatusCode.HttpVersionNotSupported;

            try
            {
                client.Read<DomainResource>($"{resourceInstance.GetType().Name}/{createdModel.Id}");
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
        [Theory]
        [MemberData(nameof(DataGenerator.GetUrlResourceType), MemberType = typeof(DataGenerator))]
        public void WhenResourceCreated(FhirClient client, DomainResource resourceInstance)
        {
            client.Create(resourceInstance);
            Assert.Equal(((int)HttpStatusCode.Created).ToString(), client.LastResult.Status);
        }

        /// <summary>
        /// Testing the behaviour of the spec http://hl7.org/fhir/http.html#ccreate
        /// </summary>
        [Theory]
        [MemberData(nameof(DataGenerator.GetUrlResourceType), MemberType = typeof(DataGenerator))]
        public void WhenResourceCreatedWithNoMatches(FhirClient client, DomainResource resourceInstance)
        {
            var searchParams = new SearchParams();
            searchParams.Add("_id", Guid.NewGuid().ToString());

            client.Create(resourceInstance, searchParams);

            Assert.Equal(((int)HttpStatusCode.Created).ToString(), client.LastResult.Status);
        }

        /// <summary>
        /// Testing the behaviour of the spec http://hl7.org/fhir/http.html#ccreate
        /// One Match: The server should ignore the post and return 200 OK
        /// </summary>
        /// <remarks>Both Tesltra and Vonk failed this test</remarks>
        [Theory]
        [MemberData(nameof(DataGenerator.GetUrlResourceType), MemberType = typeof(DataGenerator))]
        public void WhenResourceCreatedWithOneMatch(FhirClient client, DomainResource resourceInstance)
        {
            var createdModel = client.Create(resourceInstance);

            var searchParams = new SearchParams();
            searchParams.Add("_id", createdModel.Id);

            client.Create(resourceInstance, searchParams);

            Assert.Equal(((int)HttpStatusCode.OK).ToString(), client.LastResult.Status);
        }

        /// <summary>
        /// Testing the behaviour of the spec http://hl7.org/fhir/http.html#ccreate
        /// One Match: The server ignore the post and returns 200 OK
        /// </summary>
        /// <remarks>Both Tesltra and Vonk failed this test</remarks>
        [Theory]
        [MemberData(nameof(DataGenerator.GetUrlResourceType), MemberType = typeof(DataGenerator))]
        public void WhenResourceCreatedWithMultipleMatches(FhirClient client, DomainResource resourceInstance)
        {
            var searchParams = new SearchParams();
            searchParams.Add("_lastUpdated", "gt2000-01-01");

            client.Create(resourceInstance, searchParams);

            Assert.Equal(((int)HttpStatusCode.PreconditionFailed).ToString(), client.LastResult.Status);
        }
    }
}
