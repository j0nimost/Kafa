namespace nyingi.Kafa.Reflection
{
    internal sealed class KafaListOfT<T> : List<T>
    {
        public void CreateCollection(KafaTypeInfo kafaTypeInfo, int length)
        {
            kafaTypeInfo.ReturnValue = Array.CreateInstance(kafaTypeInfo.Type, length);
        }
    }
}
