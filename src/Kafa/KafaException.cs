namespace nyingi.Kafa
{
    [Serializable]
    public class KafaException : Exception
    {

        public KafaException()
        {
            
        }

        public KafaException(string message)
            : base(message)
        {
            
        }

        public KafaException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }
    }
}
