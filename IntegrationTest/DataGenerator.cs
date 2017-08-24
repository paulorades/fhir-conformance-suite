using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace FHIRTest
{
    public class DataGenerator
    {
        public static IEnumerable<object[]> GetUrlResourceType
        {
            get
            {
                yield return new object[] { new FhirClient(Constants.VONK_FHIR_ENDPOINT), GetResourceInstance("Observation") };
                yield return new object[] { new FhirClient(Constants.TELSTRA_FHIR_ENDPOINT), GetResourceInstance("Observation") };
            }
        }

        public static IEnumerable<object[]> GetResourceOnServer()
        {
            //TODO: read from json the endpoint and the resource type
            FhirClient client = new FhirClient(Constants.VONK_FHIR_ENDPOINT);

            Observation observation = new Observation
            {
                Value = new FhirDecimal(20),
                Meta = new Meta() { Profile = new[] { "http://hl7.org/fhir/StructureDefinition/vitalsigns" } }
            };

            yield return new object[] { client, client.Create(observation) };
        }

        private static DomainResource GetResourceInstance(string resourceTypeStr)
        {
            var assembly = Assembly.GetAssembly(typeof(Patient));
            var resourceType = assembly.GetTypes().First(t => !t.IsAbstract && t.Name == resourceTypeStr);
            var domainResource = Activator.CreateInstance(resourceType) as DomainResource;
            return domainResource;
        }
    }
}
