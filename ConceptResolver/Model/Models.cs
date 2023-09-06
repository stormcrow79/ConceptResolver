namespace ConceptResolver.Model
{
    public class Patient
    {
        [Concept("Patient_FamilyName")]
        public string FamilyName { get; set; }

        [Concept("Patient_GivenNames")]
        public string GivenNames { get; set; }
    }

    public class LabResult
    {
        [Concept("Patient_LaboratoryReport_SendingFacility")]
        public string SendingFacility { get; set; }
    }
}
