
using System.Diagnostics.CodeAnalysis;

#nullable disable warnings

namespace nyingi.Kafa
{
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple =false)]
    public class KafaAttribute : Attribute
    {
        public readonly string FieldName;
        public readonly int FieldIndex;
        /// <summary>
        /// Name of the Column to be matched with
        /// </summary>
        /// <param name="fieldName"></param>
        public KafaAttribute(string fieldName)
        {
            FieldName = fieldName;
        }

        /// <summary>
        /// Index of the Column starting from 0 to N-1
        /// </summary>
        /// <param name="index"></param>
        public KafaAttribute(int index)
        {
            FieldIndex = index;
        }
    }
}
