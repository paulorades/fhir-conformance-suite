using System.Linq;
using System.Net;
using System.Threading;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;

namespace FHIRTest
{
    public class HistoryTest : IClassFixture<DataGenerator>
    {
        private readonly FhirClient _fhirClient;
        private readonly Observation _resource;

        public HistoryTest(DataGenerator dataGenerator)
        {
            _fhirClient = new FhirClient(DataGeneratorHelper.GetServerUrl())
            {
                PreferredFormat = ResourceFormat.Json
            };

            _resource = dataGenerator.GetObservation();
        }
        
        /// <summary>
        /// Tests that when you request a history for the entire server that a bundle is returned - https://www.hl7.org/fhir/http.html#history
        /// </summary>
        [Fact]
        public void WhenHistoryReadForEntireSystem_ThenBundleReturned()
        {
            // Create an observation so we're guaranteed there's at least one item to be returned
            _fhirClient.Create(_resource);

            var historyResults = _fhirClient.WholeSystemHistory();

            Assert.NotNull(historyResults);
            Assert.True(historyResults.Entry.Count > 0);
            Assert.Equal(((int)HttpStatusCode.OK).ToString(), _fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Tests that when you request a history for the entire server with a specified count that a bundle is returned with specified value - https://www.hl7.org/fhir/http.html#history
        /// </summary>
        [Fact]
        public void WhenHistoryReadForEntireSystemWithCount_ThenCorrectNumberReturned()
        {
            // Create two observations so we're guaranteed there's at least two items in the system
            _fhirClient.Create(_resource);
            _fhirClient.Create(_resource);

            var historyResults = _fhirClient.WholeSystemHistory(pageSize: 1);

            Assert.NotNull(historyResults);
            Assert.True(historyResults.Entry.Count == 1);
            Assert.Equal(((int)HttpStatusCode.OK).ToString(), _fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Tests that when you request a history for the entire server with a specified count that a bundle is returned with specified value - https://www.hl7.org/fhir/http.html#history
        /// </summary>
        [Fact]
        public void WhenHistoryReadForEntireSystemWithSince_ThenCorrectNumberReturned()
        {
            // Create two observations so we're guaranteed there's at least two items in the system
            var firstObservation = _fhirClient.Create(_resource);

            // Put a number of seconds between the two creations because servers aren't required to support a precision finer than one second https://www.hl7.org/fhir/http.html#history
            Thread.Sleep(3000);
            var secondObservation = _fhirClient.Create(_resource);
            
            // Get a since value that's between the first observation creation and second creation
            var sinceValue = secondObservation.Meta.LastUpdated.Value.AddSeconds(-1); 

            var historyResults = _fhirClient.WholeSystemHistory(sinceValue);

            Assert.NotNull(historyResults);
            Assert.False(historyResults.Entry.Any(x => x.Resource.Id == firstObservation.Id));
            Assert.Equal(((int)HttpStatusCode.OK).ToString(), _fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Tests that when you request a history for a resource type that a bundle is returned - https://www.hl7.org/fhir/http.html#history
        /// </summary>
        [Fact]
        public void WhenHistoryReadForResourceType_ThenBundleReturned()
        {
            // Create an observation so we're guaranteed there's at least one item to be returned
            _fhirClient.Create(_resource);

            var historyResults = _fhirClient.TypeHistory<Observation>();

            Assert.NotNull(historyResults);
            Assert.True(historyResults.Entry.Count > 0);
            Assert.Equal(((int)HttpStatusCode.OK).ToString(), _fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Tests that when you request a history for a resource type with a count parameter, that a bundle is returned with the appropriate count- https://www.hl7.org/fhir/http.html#history
        /// </summary>
        [Fact]
        public void WhenHistoryReadForResourceTypeWithCount_ThenCorrectNumberReturned()
        {
            // Create observations so we're guaranteed there's at least two item to be returned
            _fhirClient.Create(_resource);
            _fhirClient.Create(_resource);

            var historyResults = _fhirClient.TypeHistory<Observation>(pageSize: 1);

            Assert.NotNull(historyResults);
            Assert.True(historyResults.Entry.Count == 1);
            Assert.Equal(((int)HttpStatusCode.OK).ToString(), _fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Tests that when you request a history for a resource type with a since parameter, that a bundle is returned with the appropriate entries- https://www.hl7.org/fhir/http.html#history
        /// </summary>
        [Fact]
        public void WhenHistoryReadForResourceTypeWithSince_ThenCorrectNumberReturned()
        {
            // Create two observations so we're guaranteed there's at least two items in the system
            var firstObservation = _fhirClient.Create(_resource);

            // Put a number of seconds between the two creations because servers aren't required to support a precision finer than one second https://www.hl7.org/fhir/http.html#history
            Thread.Sleep(3000);
            var secondObservation = _fhirClient.Create(_resource);

            // Get a since value that's between the first creation and second creation
            var sinceValue = secondObservation.Meta.LastUpdated.Value.AddSeconds(-1);

            var historyResults = _fhirClient.TypeHistory<Observation>(sinceValue);

            Assert.NotNull(historyResults);
            Assert.False(historyResults.Entry.Any(x => x.Resource.Id == firstObservation.Id));
            Assert.Equal(((int)HttpStatusCode.OK).ToString(), _fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Tests that when you request a history for a particular resource that it's returned - https://www.hl7.org/fhir/http.html#history
        /// </summary>
        /// <remarks>Vonk returns an extra resource in the entry collection for the operation outcome</remarks>
        [Fact]
        public void WhenHistoryReadForParticularResource_ThenAllVersionsReturned()
        {
            var createdModelOnTheServer = _fhirClient.Create(_resource);
            createdModelOnTheServer.Comment = "This is an update to the observation";
            createdModelOnTheServer = _fhirClient.Update(createdModelOnTheServer);
            var historyResults = _fhirClient.History($"Observation/{createdModelOnTheServer.Id}");

            Assert.NotNull(historyResults);
            Assert.Equal(2, historyResults.Entry.Count);
            Assert.Equal(((int)HttpStatusCode.OK).ToString(), _fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Tests that when you request a history for a particular resource with count, the correct number of entries is returned - https://www.hl7.org/fhir/http.html#history
        /// </summary>
        /// <remarks>Vonk returns an extra resource in the entry collection for the operation outcome</remarks>
        [Fact]
        public void WhenHistoryReadForParticularResourceWithCount_ThenCorrectNumberReturned()
        {
            var createdModelOnTheServer = _fhirClient.Create(_resource);
            createdModelOnTheServer.Comment = "This is an update to the observation";
            createdModelOnTheServer = _fhirClient.Update(createdModelOnTheServer);
            var historyResults = _fhirClient.History($"Observation/{createdModelOnTheServer.Id}", pageSize: 1);

            Assert.NotNull(historyResults);
            Assert.Equal(1, historyResults.Entry.Count);
            Assert.Equal(((int)HttpStatusCode.OK).ToString(), _fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Tests that when you request a history for a particular resource with since, the correct number of entries is returned - https://www.hl7.org/fhir/http.html#history
        /// </summary>
        /// <remarks>Vonk returns an extra resource in the entry collection for the operation outcome</remarks>
        [Fact]
        public void WhenHistoryReadForParticularResourceWithSince_ThenCorrectNumberReturned()
        {
            // Create the original resource
            var createdModelOnTheServer = _fhirClient.Create(_resource);

            // Sleep for 3 seconds so there's time between the versions
            Thread.Sleep(3000);
            createdModelOnTheServer.Comment = "This is an update to the observation";
            createdModelOnTheServer = _fhirClient.Update(createdModelOnTheServer);

            // Get a value before the update and after the original create
            var sinceValue = createdModelOnTheServer.Meta.LastUpdated.Value.AddSeconds(-1);

            var historyResults = _fhirClient.History($"Observation/{createdModelOnTheServer.Id}", sinceValue);

            Assert.NotNull(historyResults);
            Assert.Equal(1, historyResults.Entry.Count);
            Assert.Equal(((int)HttpStatusCode.OK).ToString(), _fhirClient.LastResult.Status);
        }
    }
}
