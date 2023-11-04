using nyingi.Kafa.Reader;
using nyingi.Kafa.Writer;

namespace nyingi.Kafa;

public interface IKafaFormatter<T>
{
    public static abstract void Serialize(ref KafaWriter writer, scoped ref T? entity);
    public static abstract void Deserialize(ref KafaReader reader, scoped ref T? entity);
}