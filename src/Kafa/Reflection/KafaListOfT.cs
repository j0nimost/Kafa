namespace nyingi.Kafa.Reflection
{
    internal sealed class KafaListOfT<T> : List<T>
    {
        public void CreateCollection(KafaTypeInfo kafaTypeInfo, int length)
        {
            kafaTypeInfo.ReturnValue = Array.CreateInstance(kafaTypeInfo.Type, length);
        }

        public void AddElement(T t, KafaTypeInfo kafaTypeInfo)
        {
            ((ICollection<T>)kafaTypeInfo.ReturnValue!).Add(t);
        }

        public IEnumerable<T> Write<T>(Type t, KafaTypeInfo kafaTypeInfo)
        {

            // Create a list of the required type, passing the values to the constructor
            Type genericListType = typeof(List<>);
            Type concreteListType = genericListType.MakeGenericType(t);

            return (List<T>)Activator.CreateInstance(concreteListType, new object[] { kafaTypeInfo.ReturnValue });
        }
    }
}
