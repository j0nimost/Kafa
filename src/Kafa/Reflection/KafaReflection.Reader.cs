using System.Diagnostics.Metrics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace nyingi.Kafa.Reflection
{
    internal partial class KafaReflection
    {
        public async Task<TextWriter> GetProperties<T>(List<T> entities, TextWriter textWriter)
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
                        await textWriter.WriteAsync(propertyInfo.Name);

                        if (countHeader < propertyInfos.Length - 1)
                        {
                            await textWriter.WriteAsync((char)TypeInfo.KafaOptions.FileType);

                        }
                        countHeader++;
                    }

                    await textWriter.WriteLineAsync();
                    readHeader= true;
                }

                propertyCount = propertyInfos.Length;
                foreach (var propertyInfo in propertyInfos)
                {
                    await textWriter.WriteAsync($"{propertyInfo.GetValue(entity)}");

                    if (count < propertyCount - 1)
                    {
                        await textWriter.WriteAsync((char)TypeInfo.KafaOptions.FileType);
                    }
                    count++;
                }
                await textWriter.WriteLineAsync();
                count = 0;
            }

            return textWriter;
        }
    }
}
