using System;
using System.Linq;
using System.Reflection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace FHIRTest
{
    public class RestfulApiDataGenerator
    {
        private readonly FhirClient _client;

        public RestfulApiDataGenerator()
        {
            _client = new FhirClient(DataGeneratorHelper.GetServerUrl());
        }

        public (FhirClient client, DomainResource resource) Get()
        {
            DomainResource resource = GetResourceInstance("Observation");
            return (_client, resource);
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
