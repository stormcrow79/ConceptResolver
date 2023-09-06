using System.Collections.Generic;

namespace ConceptResolver.Model
{
    public class PatientProvider : IProvider<Patient>
    {
        public Patient Get()
        {
            return new Patient
            {
                FamilyName = "BOO",
                GivenNames = "Boris"
            };
        }
    }

    [Concept("Patient_LaboratoryReport")]
    public class LabResultProvider : ICollectionProvider<LabResult>
    {
        public IEnumerable<LabResult> Get(Filter filter)
        {
            yield return new LabResult { SendingFacility = "PathWest" };
            yield return new LabResult { SendingFacility = "Western Diagnostic" };
        }
    }
}
