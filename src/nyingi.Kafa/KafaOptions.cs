using System.Globalization;
using System.Text;

namespace nyingi.Kafa
{
    public class SeparatorFileType
    {
        public const char CSV = ',';
        public const char TSV = ';';
    }
    public sealed class KafaOptions
    {
        public CultureInfo? CultureInfo { get; set; }
        public Encoding? Encoding { get; set; }
        public char Separator { get; set; } = '\0';

        public bool HasHeader { get; set; } = true;

        internal static KafaOptions Default
        {
            get
            {
                return new KafaOptions { Separator = SeparatorFileType.CSV, CultureInfo = CultureInfo.InvariantCulture, Encoding = Encoding.UTF8 };
            }
        }

        internal static KafaOptions ResolveKafaOptions(KafaOptions? options)
        {
            return options ?? Default;
        }
    }
}
