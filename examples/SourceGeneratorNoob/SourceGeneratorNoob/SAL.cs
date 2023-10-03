using System;

namespace SourceGeneratorNoob
{
    public class SAL
    {
        public string FullName { get; set; }
        public string Religion { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
        [Nationality("KE")]
        public string CountryOfOrigin { get; set; }
    }
}
