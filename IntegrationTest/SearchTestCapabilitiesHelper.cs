// <copyright file="SearchTestCapabilitiesHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

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
            AssertHelper.CheckStatusCode(HttpStatusCode.OK, client.LastResult.Status);
        }
    }
}
