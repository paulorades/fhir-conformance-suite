﻿using System.Collections.Generic;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace FHIRTest
{
    public class VonkDataGenerator
    {
        public DomainResource GetAllergyWithCategory(FhirClient fhirClient)
        {
            AllergyIntolerance allergyIntolerance = new AllergyIntolerance
            {
                ClinicalStatus = AllergyIntolerance.AllergyIntoleranceClinicalStatus.Active,
                VerificationStatus = AllergyIntolerance.AllergyIntoleranceVerificationStatus.Confirmed,
                Patient = new ResourceReference("Patient/example"),
                Category = new List<AllergyIntolerance.AllergyIntoleranceCategory?>
                {
                    AllergyIntolerance.AllergyIntoleranceCategory.Food
                }
            };

            return fhirClient.Create(allergyIntolerance);
        }

        public Procedure GetProcedureWithBodySite(FhirClient fhirClient)
        {
            Procedure procedure = new Procedure
            {
                Status = EventStatus.Completed,
                Subject = new ResourceReference("Patient/example"),
                BodySite = new List<CodeableConcept>
                {
                    new CodeableConcept("http://snomed.info/sct", "272676008", "sample")
                }
            };

            return fhirClient.Create(procedure);
        }
    }
}
