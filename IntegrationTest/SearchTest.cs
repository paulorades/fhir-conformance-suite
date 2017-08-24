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
    public class SearchTest : IClassFixture<DataGenerator>
    {
        /// <summary>
        /// Search "_id" parameter
        /// </summary>
        [Theory]
        [MemberData(nameof(DataGenerator.GetResourceOnServer), MemberType = typeof(DataGenerator))]
        public void WhenResourceIdSearched(FhirClient client, DomainResource resource)
        {
            SearchParams searchParams = new SearchParams();
            searchParams.Add("_id", resource.Id);

            var bundle = client.Search(searchParams);

            Assert.NotNull(bundle);
            Assert.Equal(resource.Id, ((DomainResource)bundle.GetResources().First()).Id);
            Assert.Equal(((int)HttpStatusCode.OK).ToString(), client.LastResult.Status);
        }
    }
}
