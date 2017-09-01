using System.Linq;
using System.Net;
using System.Threading;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;

namespace FHIRTest
{
    public class SearchTestCapabilitiesHelper
    {
        public static void SearchThenAssertResult(FhirClient client, DomainResource resource, SearchParams searchParams)
        {
            Bundle.EntryComponent entry = null;

            // Retry because depending on how the server is implemented, there could be a slight lag between when an object is created and when it's indexed and available for search.
            int retryCount = 0;
            while (retryCount < 5)
            {
                var bundle = client.Search(searchParams, resource.GetType().Name);

                Assert.NotNull(bundle);

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

                if (entry != null)
                {
                    break;    
                }

                Thread.Sleep(2000);
                retryCount++;
            }

            Assert.NotNull(entry);
            Assert.Equal(((int)HttpStatusCode.OK).ToString(), client.LastResult.Status);
        }
    }
}
