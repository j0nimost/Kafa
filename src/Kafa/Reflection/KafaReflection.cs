using System.Reflection;
using System.Runtime.CompilerServices;
using static nyingi.Kafa.Reader.KafaReader;

namespace nyingi.Kafa.Reflection
{
    internal class KafaReflection
    {
        public Dictionary<int, PropertyInfo> properties= default;

        public readonly KafaTypeInfo TypeInfo;
        public KafaReflection(KafaTypeInfo typeInfo, Dictionary<string, int>? Headers = default) 
        {
            // match propertyName with header
            TypeInfo = typeInfo;
            // store all the properties
            properties = new Dictionary<int, PropertyInfo>(TypeInfo.Type.GetProperties().Length);
            int count = 0;
            foreach (var property in TypeInfo.Type.GetProperties())
            {
                string name = property.Name.ToLower();
                if (Headers != null && Headers.ContainsKey(name))
                {
                    properties.Add(Headers[name], property); 

                }
                else if (Headers == null)
                {
                    properties.Add(count, property); 
                    count++;
                }
            }
        }

        public IEnumerable<T> SetProperties<T>(RowEnumerable rows)
        {
            if(properties.Count == 0)
            {
                throw new Exception("{0} class is empty");
            }
            var colIndex = 0;

            var instance = new KafaListOfT<T>();
            instance.CreateCollection(TypeInfo, rows.Length);

            var elementInstance = new KafaTypeOfT();

            foreach (var row in rows)
            {
                // create an instance of T
                var instanceOft = elementInstance.CreateInstance(TypeInfo.Type);
                foreach (var col in row.Cols)
                {
                    // populate the properties
                    var propertyInfo = properties[colIndex];
                    propertyInfo.SetValue(instanceOft, TypeResolver(propertyInfo.PropertyType, col));
                    colIndex++;
                }

                instance.Add((T)instanceOft!);
                colIndex = 0;

            }

            return (List<T>)instance;
        }

        private object? TypeResolver(Type type, Col col)
        {
            if (type == typeof(string))
            {
                return col.Value.ToString();
            }
            else if (type == typeof(int))
            {
                return col.Parse<int>();
            }
            else if(type == typeof(long))
            {
                return col.Parse<long>();
            }
            else if(type == typeof(float))
            {
                return col.Parse<float>();
            }
            else if(type == typeof(double))
            {
                return col.Parse<double>();
            }
            else if(type == typeof(decimal))
            {
                return col.Parse<decimal>();
            }
            else if(type == typeof(DateTime))
            {
                return col.Parse<DateTime>();
            }
            else if(type == typeof(DateTimeOffset))
            {
                return col.Parse<DateTimeOffset>();
            }
            else
            {
                return default;
            }

        }
    }
}
