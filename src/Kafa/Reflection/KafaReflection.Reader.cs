using System.Diagnostics.Metrics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using nyingi.Kafa.Writer;

namespace nyingi.Kafa.Reflection
{
    internal partial class KafaReflection
    {
        public void GetProperties<T>(in KafaWriter writer, List<T> entities)
        {

            bool readHeader = false;
            int propertyCount = 0;
            int count = 0;
            PropertyInfo[] propertyInfos = default!;
            foreach (var entity in entities)
            {
                propertyInfos = entity.GetType().GetProperties();

                if(TypeInfo.KafaOptions.HasHeader && !readHeader)
                {
                    int countHeader = 0;
                    foreach (var propertyInfo in propertyInfos)
                    {

                        var kafa = propertyInfo.GetCustomAttribute<KafaColumnAttribute>(false);

                        if (kafa != null && !string.IsNullOrEmpty(kafa.FieldName))
                        {
                            writer.Write(kafa.FieldName);

                        }
                        else
                        {
                            writer.Write(propertyInfo.Name);
                        }

                        if (countHeader < propertyInfos.Length - 1)
                        {
                            writer.WriteSeparator();

                        }
                        countHeader++;
                    }

                    writer.WriteLine();
                    readHeader= true;
                }

                propertyCount = propertyInfos.Length;
                foreach (var propertyInfo in propertyInfos)
                {
                    writer.Write($"{propertyInfo.GetValue(entity)}");

                    if (count < propertyCount - 1)
                    {
                        writer.WriteSeparator();
                    }
                    count++;
                }
                writer.WriteLine();
                count = 0;
            }
        }
    }
}
