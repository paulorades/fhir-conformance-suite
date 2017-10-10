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
