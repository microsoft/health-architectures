namespace Fhir.Anonymizer.Core.AnonymizerConfigurations
{
    public class AnonymizerRule
    {
        public string Path { get; set; }
        
        public string Method { get; set; }

        public AnonymizerRule(string path, string method)
        {
            Path = path;
            Method = method;
        }
    }
}
