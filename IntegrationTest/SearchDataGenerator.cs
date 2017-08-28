using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace FHIRTest
{
    public class SearchDataGenerator
    {
        private readonly FhirClient _client;

        public SearchDataGenerator()
        {
            _client = new FhirClient(DataGeneratorHelper.GetServerUrl())
            {
                PreferredFormat = ResourceFormat.Json
            };
        }

        public (FhirClient, DomainResource) GetDataWithResource()
        {
            Observation observation = new Observation
            {
                Value = new FhirDecimal(20),
                Meta = new Meta { Profile = new[] { "http://hl7.org/fhir/StructureDefinition/vitalsigns" } }
            };

            return (_client, _client.Create(observation));
        }

        public (FhirClient, DomainResource) GetDataWithResourceCodableConcept()
        {
            Observation observation = new Observation
            {
                Value = new CodeableConcept("http://loinc.org", "LA25391-6", "Normal metabolizer")
            };

            return (_client, _client.Create(observation));
        }

        public (FhirClient, DomainResource) GetDataWithConditionCode()
        {
            var condition = new Observation
            {
                Code = new CodeableConcept("http://snomed.info/sct", "39065001", "Normal metabolizer")
            };

            return (_client, _client.Create(condition));
        }

        public (FhirClient, DomainResource) GetDataWithObservationQuantity()
        {
            var condition = new Observation
            {
                Value = new Quantity(120.00M, "kg")
            };

            return (_client, _client.Create(condition));
        }

        public (FhirClient, DomainResource) GetObservationForPatient()
        {
            Patient patient = new Patient
            {
                Gender = AdministrativeGender.Male,
                Name = new List<HumanName>
                {
                    new HumanName {Text = "xyz"}
                }
            };

            Patient uplaodedPatient = _client.Create(patient);

            var observation = new Observation
            {
                Value = new Quantity(120.00M, "kg"),
                Subject = new ResourceReference($"Patient/{uplaodedPatient.Id}")
            };

            return (_client, _client.Create(observation));
        }
    }
}
