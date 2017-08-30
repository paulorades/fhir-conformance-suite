using System;
using System.Linq;
using System.Reflection;
using Hl7.Fhir.Model;

namespace FHIRTest
{
    public class RestfulApiDataGenerator
    {
        public DomainResource GetResource()
        {
            DomainResource resource = GetResourceInstance("Observation");
            return resource;
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
