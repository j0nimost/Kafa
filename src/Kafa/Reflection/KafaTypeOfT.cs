namespace nyingi.Kafa.Reflection
{
    internal sealed class KafaTypeOfT
    {
        public object? CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
