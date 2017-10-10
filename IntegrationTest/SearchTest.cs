// <copyright file="SearchTest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

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
        private readonly FhirClient _fhirClient;

        public SearchTest(SearchDataGenerator searchDataGenerator)
        {
            _fhirClient = new FhirClient(DataGeneratorHelper.GetServerUrl())
            {
                PreferredFormat = ResourceFormat.Json
            };

            _searchDataGenerator = searchDataGenerator;
        }

        /// <summary>
        /// Search "_id" parameter
        /// </summary>
        [Fact]
        public void WhenResourceSearchedWithIdParam()
        {
            var resource = _searchDataGenerator.GetDataWithResource(_fhirClient);

            SearchParams searchParams = new SearchParams();
            searchParams.Add("_id", resource.Id);

            var bundle = _fhirClient.Search(searchParams);

            Assert.NotNull(bundle);
            Assert.Equal(resource.Id, ((DomainResource)bundle.GetResources().First()).Id);
            AssertHelper.CheckStatusCode(HttpStatusCode.OK, _fhirClient.LastResult.Status);
        }

        /// <summary>
        /// Search "_lastUpdated" parameter
        /// </summary>
        [Fact]
        public void WhenResourceSearchedWithLastUpdatedParam()
        {
            var resource = _searchDataGenerator.GetDataWithResource(_fhirClient);

            DateTimeOffset lastUpdated = resource.Meta.LastUpdated.GetValueOrDefault(DateTimeOffset.Now);

            string dateTime = string.Format(
                CultureInfo.InvariantCulture,
                FhirDateTime.FMT_YEARMONTHDAY,
                lastUpdated.Year,
                lastUpdated.Month,
                lastUpdated.Day);

            SearchParams searchParams = new SearchParams();
            searchParams.Add("_lastUpdated", dateTime);

            SearchTestCapabilitiesHelper.SearchThenAssertResult(_fhirClient, resource, searchParams);
        }

        [Fact]
        public void WhenResourceSearchedWithValue()
        {
            var resource = _searchDataGenerator.GetDataWithResourceCodableConcept(_fhirClient);

            SearchParams searchParams = new SearchParams();
            searchParams.Add("value-concept", "http://loinc.org|LA25391-6");

            SearchTestCapabilitiesHelper.SearchThenAssertResult(_fhirClient, resource, searchParams);
        }

        [Fact]
        public void WhenResourceSearchedWithModifierIn()
        {
            var resource = _searchDataGenerator.GetDataWithConditionCode(_fhirClient);

            SearchParams searchParams = new SearchParams();
            searchParams.Add("code:in", "http://snomed.info/sct|39065001");

            SearchTestCapabilitiesHelper.SearchThenAssertResult(_fhirClient, resource, searchParams);
        }

        [Fact]
        public void WhenResourceSearchedWithQuantity()
        {
            var resource = _searchDataGenerator.GetDataWithObservationQuantity(_fhirClient);

            SearchParams searchParams = new SearchParams();
            searchParams.Add("value-quantity", "120.00");

            SearchTestCapabilitiesHelper.SearchThenAssertResult(_fhirClient, resource, searchParams);
        }

        [Fact]
        public void WhenResourceSearchedViaChaining()
        {
            var resource = _searchDataGenerator.GetObservationForPatient(_fhirClient);

            SearchParams searchParams = new SearchParams();
            searchParams.Add("subject:Patient.name", "xyz");

            SearchTestCapabilitiesHelper.SearchThenAssertResult(_fhirClient, resource, searchParams);
        }

        [Fact]
        public void WhenResourceSearchedByFilter()
        {
            var resource = _searchDataGenerator.GetDataWithResourceCodableConcept(_fhirClient);

            SearchParams searchParams = new SearchParams();
            searchParams.Add(SearchParams.SEARCH_PARAM_FILTER, "name eq http://loinc.org|LA25391-6");

            SearchTestCapabilitiesHelper.SearchThenAssertResult(_fhirClient, resource, searchParams);
        }

        [Fact]
        public void WhenResourceSearchedViaText()
        {
            var resource = _searchDataGenerator.GetConditionToSearchViaContent(_fhirClient);

            SearchParams searchParams = new SearchParams();
            searchParams.Add(SearchParams.SEARCH_PARAM_TEXT, "bone");

            SearchTestCapabilitiesHelper.SearchThenAssertResult(_fhirClient, resource, searchParams);
        }

        [Fact]
        public void WhenResourceSearchedViaProfile()
        {
            var resource = _searchDataGenerator.GetDataWithResource(_fhirClient);

            SearchParams searchParams = new SearchParams();
            searchParams.Add("_profile", "http://hl7.org/fhir/StructureDefinition/vitalsigns");

            SearchTestCapabilitiesHelper.SearchThenAssertResult(_fhirClient, resource, searchParams);
        }
    }
}
