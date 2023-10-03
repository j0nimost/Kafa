using nyingi.Kafa;

namespace nyingi.Kafa
{
    internal class KafaTypeInfo
    {
        public Type Type { get; }
        public KafaOptions KafaOptions { get; }
        public bool HasAttributes { get; private set;}
        public KafaTypeInfo(Type type, KafaOptions kafaOptions)
        {
            Type = type;
            KafaOptions = kafaOptions;
        }

        public object? ReturnValue;
    }
}
