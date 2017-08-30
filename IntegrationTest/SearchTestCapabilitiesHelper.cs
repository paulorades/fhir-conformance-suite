using System.Linq;
using System.Net;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;

namespace FHIRTest
{
    public class SearchTestCapabilitiesHelper
    {
        public static void SearchThenAssertResult(FhirClient client, DomainResource resource, SearchParams searchParams)
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
