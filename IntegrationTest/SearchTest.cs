using System;
using System.Globalization;
using System.Linq;
using System.Net;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;

namespace FHIRTest
{
    /// <summary>
    /// Contains tests realted to search operations http://hl7.org/fhir/search.html
    /// </summary>
    public class SearchTest : IClassFixture<SearchDataGenerator>
    {
        private readonly SearchDataGenerator _searchDataGenerator;

        public SearchTest(SearchDataGenerator searchDataGenerator)
        {
            _searchDataGenerator = searchDataGenerator;
        }

        /// <summary>
        /// Search "_id" parameter
        /// </summary>
        [Fact]
        public void WhenResourceSearchedWithIdParam()
        {
            var data = _searchDataGenerator.GetDataWithResource();
            var fhirClient = data.Item1;
            var resource = data.Item2;

            SearchParams searchParams = new SearchParams();
            searchParams.Add("_id", resource.Id);

            var bundle = fhirClient.Search(searchParams);

            Assert.NotNull(bundle);
            Assert.Equal(resource.Id, ((DomainResource)bundle.GetResources().First()).Id);
            Assert.Equal(((int)HttpStatusCode.OK).ToString(), fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Search "_lastUpdated" parameter
        /// </summary>
        [Fact]
        public void WhenResourceSearchedWithLastUpdatedParam()
        {
            var data = _searchDataGenerator.GetDataWithResource();
            var fhirClient = data.Item1;
            var resource = data.Item2;

            DateTimeOffset lastUpdated = resource.Meta.LastUpdated.GetValueOrDefault(DateTimeOffset.Now);

            string dateTime = string.Format(
                CultureInfo.InvariantCulture,
                FhirDateTime.FMT_YEARMONTHDAY,
                lastUpdated.Year,
                lastUpdated.Month,
                lastUpdated.Day);

            SearchParams searchParams = new SearchParams();
            searchParams.Add("_lastUpdated", dateTime);
            
            AssertSearch(fhirClient, resource, searchParams);
        }

        [Fact]
        public void WhenResourceSearchedWithValue()
        {
            var data = _searchDataGenerator.GetDataWithResourceCodableConcept();
            var fhirClient = data.Item1;
            var resource = data.Item2;

            SearchParams searchParams = new SearchParams();
            searchParams.Add("value-concept", "http://loinc.org|LA25391-6");

            AssertSearch(fhirClient, resource, searchParams);
        }

        [Fact]
        public void WhenResourceSearchedWithModifierIn()
        {
            var data = _searchDataGenerator.GetDataWithConditionCode();
            var fhirClient = data.Item1;
            var resource = data.Item2;

            SearchParams searchParams = new SearchParams();
            searchParams.Add("code:in", "http://snomed.info/sct|39065001");

            AssertSearch(fhirClient, resource, searchParams);
        }

        [Fact]
        public void WhenResourceSearchedWithQuantity()
        {
            var data = _searchDataGenerator.GetDataWithObservationQuantity();
            var fhirClient = data.Item1;
            var resource = data.Item2;

            SearchParams searchParams = new SearchParams();
            searchParams.Add("value-quantity", "120.00");

            AssertSearch(fhirClient, resource, searchParams);
        }

        [Fact]
        public void WhenResourceSearchedViaChaining()
        {
            var data = _searchDataGenerator.GetObservationForPatient();
            var fhirClient = data.Item1;
            var resource = data.Item2;

            SearchParams searchParams = new SearchParams();
            searchParams.Add("subject:Patient.name", "xyz");

            AssertSearch(fhirClient, resource, searchParams);
        }

        private static void AssertSearch(
            FhirClient client, 
            DomainResource resource, 
            SearchParams searchParams)
        {
            var bundle = client.Search(searchParams, resource.GetType().Name);

            Assert.NotNull(bundle);

            Bundle.EntryComponent entry = null;

            while (bundle.NextLink != null)
            {
                // Find the resource from the bundle
                entry = bundle.FindEntry($"{client.Endpoint}{resource.GetType().Name}/{resource.Id}").FirstOrDefault();

                if (entry != null)
                {
                    break;
                }

                bundle = client.Continue(bundle);
            }

            if (entry == null)
            {
                // Find the resource from the bundle
                entry = bundle.FindEntry($"{client.Endpoint}{resource.GetType().Name}/{resource.Id}").FirstOrDefault();
            }

            Assert.NotNull(entry);
            Assert.Equal(((int)HttpStatusCode.OK).ToString(), client.LastResult.Status);
        }
    }
}
