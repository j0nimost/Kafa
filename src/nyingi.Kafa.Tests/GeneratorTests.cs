using nyingi.Kafa;

namespace nyingi.Kafa.Tests;

public class GeneratorTests
{
    [Fact]
    public void TestPerson()
    {
        var person = new Person() { Name = "Strager", Age = 31 };
        Assert.NotNull(person);
    }
}

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

[KafaSerializable(typeof(Person))]
public partial class GeneratorContext
{
    
}