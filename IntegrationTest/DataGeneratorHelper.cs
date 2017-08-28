using System;

namespace FHIRTest
{
    public class DataGeneratorHelper
    {
        private const string FhirTestServerUrl = "FHIR_TEST_SERVER_URL";

        public static string GetServerUrl()
        {
            return Environment.GetEnvironmentVariable(FhirTestServerUrl) ?? Constants.VONK_FHIR_ENDPOINT;
        }
    }
}