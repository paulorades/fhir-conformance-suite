using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace FHIRTest
{
    public class SearchDataGenerator
    {
        public DomainResource GetDataWithResource(FhirClient fhirClient)
        {
            Observation observation = new Observation
            {
                Value = new FhirDecimal(20),
                Meta = new Meta
                {
                    Profile = new[] { "http://hl7.org/fhir/StructureDefinition/vitalsigns" }
                }
            };

            return fhirClient.Create(observation);
        }

        public DomainResource GetDataWithResourceCodableConcept(FhirClient fhirClient)
        {
            Observation observation = new Observation
            {
                Value = new CodeableConcept("http://loinc.org", "LA25391-6", "Normal metabolizer")
            };

            return fhirClient.Create(observation);
        }

        public DomainResource GetDataWithConditionCode(FhirClient fhirClient)
        {
            var condition = new Observation
            {
                Code = new CodeableConcept("http://snomed.info/sct", "39065001", "Normal metabolizer")
            };

            return fhirClient.Create(condition);
        }

        public DomainResource GetDataWithObservationQuantity(FhirClient fhirClient)
        {
            var condition = new Observation
            {
                Value = new Quantity(120.00M, "kg")
            };

            return fhirClient.Create(condition);
        }

        public DomainResource GetConditionToSearchViaContent(FhirClient fhirClient)
        {
            var condition = new Condition
            {
                Text = new Narrative
                {
                    Div = "bone"
                }
            };

            return fhirClient.Create(condition);
        }

        public DomainResource GetObservationForPatient(FhirClient fhirClient)
        {
            Patient patient = new Patient
            {
                Gender = AdministrativeGender.Male,
                Name = new List<HumanName>
                {
                    new HumanName
                    {
                        Text = "xyz"
                    }
                }
            };

            Patient uplaodedPatient = fhirClient.Create(patient);

            var observation = new Observation
            {
                Value = new Quantity(120.00M, "kg"),
                Subject = new ResourceReference($"Patient/{uplaodedPatient.Id}")
            };

            return fhirClient.Create(observation);
        }

        
    }
}
