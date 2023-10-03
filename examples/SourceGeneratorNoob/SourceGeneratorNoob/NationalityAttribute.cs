namespace SourceGeneratorNoob
{
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class NationalityAttribute: Attribute
    {

        public bool IsKenyan { get; private set; }

        public NationalityAttribute(string countryCode)
        {
            IsKenyan = countryCode == "KE";
        }

    }
}
