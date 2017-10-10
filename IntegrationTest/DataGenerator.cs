using Hl7.Fhir.Model;

namespace FHIRTest
{
    public class DataGenerator
    {
        public Observation GetObservation()
        {
           return new Observation { 
                Status = ObservationStatus.Final,
                Code = new CodeableConcept("http://loinc.org", "29463-7", "Body weight"),
            };
        }
    }
}
