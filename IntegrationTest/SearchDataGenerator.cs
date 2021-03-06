﻿// <copyright file="SearchDataGenerator.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

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
                Status = ObservationStatus.Final,
                Code = new CodeableConcept("http://loinc.org", "29463-7", "Body weight"),
                Value = new Quantity(new decimal(130.00), "kg") { Code = "kg" },
                DataAbsentReason = new CodeableConcept("http://hl7.org/fhir/data-absent-reason", "unknown"),
                Category = new List<CodeableConcept> { new CodeableConcept("http://hl7.org/fhir/observation-category", "vital-signs", "Vital Signs") },
                Subject = new ResourceReference("Patient/example"),
                Effective = new FhirDateTime(DateTime.Now),
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
                Status = ObservationStatus.Final,
                Code = new CodeableConcept("http://loinc.org", "LA25391-6", "Normal metabolizer"),
                Value = new CodeableConcept("http://loinc.org", "LA25391-6", "Normal metabolizer")
            };

            return fhirClient.Create(observation);
        }

        public DomainResource GetDataWithConditionCode(FhirClient fhirClient)
        {
            var condition = new Observation
            {
                Status = ObservationStatus.Final,
                Code = new CodeableConcept("http://snomed.info/sct", "39065001", "Normal metabolizer")
            };

            return fhirClient.Create(condition);
        }

        public DomainResource GetDataWithObservationQuantity(FhirClient fhirClient)
        {
            var condition = new Observation
            {
                Status = ObservationStatus.Final,
                Code = new CodeableConcept("http://snomed.info/sct", "39065001", "Normal metabolizer"),
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
                    Status = Narrative.NarrativeStatus.Generated,
                    Div = "<div xmlns=\"http://www.w3.org/1999/xhtml\">bone</div>"
                },
                ClinicalStatus = Condition.ConditionClinicalStatusCodes.Active,
                Subject = new ResourceReference("Patient/example")
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

            Patient uploadedPatient = fhirClient.Create(patient);

            var observation = new Observation
            {
                Status = ObservationStatus.Final,
                Code = new CodeableConcept("http://loinc.org", "29463-7", "Body weight"),
                Value = new Quantity(120.00M, "kg"),
                Subject = new ResourceReference($"Patient/{uploadedPatient.Id}")
            };

            return fhirClient.Create(observation);
        }
    }
}
