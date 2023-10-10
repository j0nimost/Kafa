using nyingi.Kafa;

namespace nyingi.Kafa
{
    internal class KafaTypeInfo
    {
        public readonly Type Type;
        public readonly KafaOptions KafaOptions;
        public bool HasAttributes { get; private set;}
        public KafaTypeInfo(Type type, KafaOptions kafaOptions)
        {
            Type = type;
            KafaOptions = kafaOptions;
        }

        public object? ReturnValue;
    }
}
