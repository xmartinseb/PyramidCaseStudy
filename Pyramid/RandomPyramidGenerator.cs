namespace Pyramid;

public class RandomPyramidGenerator(int rows, int range) : IPyramidGenerator
{
    private readonly int _rows = rows;
    private readonly int _range = range;
    private readonly Random _random = new();

    public Pyramid GeneratePyramid()
    {
        var randomData = new int[_rows, _rows];

        for (int row = 0; row < _rows; row++)
            for (int col = 0; col < _rows - row; col++)
            {
                randomData[row, col] = _random.Next(1, _range);
            }
        return new Pyramid(randomData);
    }
}