using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nyingi.Kafa
{
    public enum FileType
    {
        CSV = (byte)',',
        TSV = (byte)';'
    }
    public sealed partial class KafaOptions
    {
        public CultureInfo? CultureInfo { get; set; }
        public Encoding? Encoding { get; set; }
        public FileType FileType { get; set; }

        public bool HasHeader { get; set; } = true;

        internal static KafaOptions Default
        {
            get
            {
                return new KafaOptions { FileType = FileType.CSV, CultureInfo = CultureInfo.InvariantCulture, Encoding = Encoding.UTF8 };
            }
        }

        internal static KafaOptions ResolveKafaOptions(KafaOptions? options)
        {
            return options ?? Default;
        }
    }
}
