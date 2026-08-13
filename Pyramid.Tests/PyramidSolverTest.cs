namespace Pyramid.Tests;

public class PyramidSolverTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void PyramidLevel4Test()
        => TestPyramidInternal(new int[,]
        {
            {8, 1, 1, 2, 49 },
            {5, 9, 7, 0, 0 },
            {3, 0, 1, 0, 0 },
            {2, 1, 0, 0, 0 },
            {1, 0, 0, 0, 0 }
        }, 52);

    [Test]
    public void PyramidLevel2Test()
        => TestPyramidInternal(new int[,]
        {
            {2, 1 },
            {8, 0 }
        }, 10);

    /// <summary>
    /// Moje implementace funguje zcela obecně, tedy i pro záporná čísla
    /// </summary>
    [Test]
    public void NegativePyramidLevel2Test()
        => TestPyramidInternal(new int[,]
        {
            {-2, -1 },
            {-8, 0 }
        }, -9);

    [Test]
    public void TrivialPyramidTest()
       => TestPyramidInternal(new int[,]
        {
            {41}
        }, 41);

    [Test]
    public void EmptyPyramidTest()
    {
        var pyramidData = new int[0, 0];

        var pyramid = new Pyramid(pyramidData);
        var solver = new PyramidSolver();
        Assert.Throws<InvalidOperationException>(() => solver.PyramidMaximumTotal(pyramid));
    }

    static void TestPyramidInternal(int[,] pyramidData, long expectedMax)
    {
        var pyramid = new Pyramid(pyramidData);
        var solver = new PyramidSolver();
        var max = solver.PyramidMaximumTotal(pyramid);
        Assert.That(max, Is.EqualTo(expectedMax));
    }
}