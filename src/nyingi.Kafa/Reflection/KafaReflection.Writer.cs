using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using nyingi.Kafa.Reader;
using static nyingi.Kafa.Reader.KafaReader;

namespace nyingi.Kafa.Reflection
{
    internal partial class KafaReflection
    {
        private Dictionary<int, PropertyInfo>? _properties= default;

        public readonly KafaTypeInfo TypeInfo;
        private readonly CultureInfo? _cultureInfo;
        public KafaReflection(KafaTypeInfo typeInfo) 
        {
            // match propertyName with header
            TypeInfo = typeInfo;
            _cultureInfo = TypeInfo.KafaOptions.CultureInfo;
        }

        private void ReadHeader(OrderedDictionary? headers = null)
        {
            _properties = new Dictionary<int, PropertyInfo>(TypeInfo.Type.GetProperties().Length); // ahead

            int count = 0;
            foreach (var property in TypeInfo.Type.GetProperties())
            {
                if (headers != null)
                {
                    var kafa = property.GetCustomAttribute<KafaColumnAttribute>(false);

                    if (kafa != null)
                    {
                        if (!string.IsNullOrEmpty(kafa.FieldName))
                        {
                            _properties.Add((int)headers[kafa.FieldName], property);

                        }
                        else
                        {
                            _properties.Add((int)headers[kafa.FieldIndex], property);
                        }
                    }
                    else if (headers.Contains(property.Name))
                    {
                        _properties.Add((int)headers[property.Name], property);
                    }
                }
                else
                {
                    _properties.Add(count, property);
                    count++;
                }
            }
        }

        
        public IEnumerable<T> SetProperties<T>(RowEnumerable rows)
        {
            // process header first
            ReadHeader(rows.Headers);

            if(_properties.Count == 0)
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
                    var propertyInfo = _properties[colIndex];
                    propertyInfo.SetValue(instanceOft, TypeResolver(propertyInfo.PropertyType, col));
                    colIndex++;
                }

                instance.Add((T)instanceOft!);
                colIndex = 0;

            }

            return instance;
        }
        private object? TypeResolver(Type type, Col col)
        {
            if (type == typeof(string))
            {
                return col.ToString();
            }
            else if (type == typeof(int))
            {
                return col.ParseInt(_cultureInfo);
            }
            else if(type == typeof(long))
            {
                return col.ParseLong(_cultureInfo);
            }
            else if(type == typeof(float))
            {
                return col.ParseFloat(_cultureInfo);
            }
            else if(type == typeof(double))
            {
                return col.ParseDouble(_cultureInfo);
            }
            else if(type == typeof(decimal))
            {
                return col.ParseDecimal(_cultureInfo);
            }
            else if(type == typeof(bool))
            {
                return col.ParseBool();
            }
            else if(type == typeof(DateTime))
            {
                return col.ParseDateTime(_cultureInfo);
            }
            else if(type == typeof(DateTimeOffset))
            {
                return col.ParseDateTimeOffSet(_cultureInfo);
            }
            else if (type == typeof(Guid))
            {
                return col.ParseGuid(_cultureInfo);
            }
            else
            {
                return null;
            }

        }
    }
}
