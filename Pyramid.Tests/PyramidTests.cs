namespace Pyramid.Tests;

public class PyramidTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void CreatePyramidTest()
    {
        var validDataMatrix = new int[2, 2];
        var pyramidOk = new Pyramid(validDataMatrix);

        var invalidDataMatrix = new int[2, 3];
        Assert.Throws<ArgumentException>(() => new Pyramid(invalidDataMatrix));
    }
};