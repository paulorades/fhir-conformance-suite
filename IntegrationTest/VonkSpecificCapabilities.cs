// <copyright file="VonkSpecificCapabilities.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using Hl7.Fhir.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;

namespace FHIRTest
{
    [TestCategory("Vonk")]
    public class VonkSpecificCapabilities : IClassFixture<VonkDataGenerator>
    {
        private readonly FhirClient _fhirClient;
        private readonly VonkDataGenerator _dataGenerator;

        public VonkSpecificCapabilities(VonkDataGenerator dataGenerator)
        {
            _dataGenerator = dataGenerator;
            _fhirClient = new FhirClient(DataGeneratorHelper.GetServerUrl());
        }

        // Passed
        [Fact]
        public void WhenProcedureSearchedWithBodySite()
        {
            var resource = _dataGenerator.GetProcedureWithBodySite(_fhirClient);

            SearchParams searchParams = new SearchParams();
            searchParams.Add("body-site", "http://snomed.info/sct|272676008");

            SearchTestCapabilitiesHelper.SearchThenAssertResult(_fhirClient, resource, searchParams);
        }

        // Passed
        [Fact]
        public void WhenSearchedWithAllergyCategory()
        {
            var resource = _dataGenerator.GetAllergyWithCategory(_fhirClient);

            SearchParams searchParams = new SearchParams();
            searchParams.Add("category", "food");

            SearchTestCapabilitiesHelper.SearchThenAssertResult(_fhirClient, resource, searchParams);
        }
    }
}
