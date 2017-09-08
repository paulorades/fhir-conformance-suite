using System;

namespace FHIRTest
{
    public class DataGeneratorHelper
    {
        private const string FhirTestServerUrl = "FHIR_TEST_SERVER_URL";

        /// <summary>
        /// If set, reads environment variable <see cref="FhirTestServerUrl"/>. This will help in
        /// parameterizing the VSTS build definition to run the tests for different FHIR servers when a build is
        /// queued. In case the environment variable is not set then VONK server will be hit
        /// </summary>
        /// <returns>FHIR test server end point</returns>
        public static string GetServerUrl()
        {
            return Environment.GetEnvironmentVariable(FhirTestServerUrl) ?? Constants.SPARK_FHIR_ENDPOINT;
        }
    }
}
