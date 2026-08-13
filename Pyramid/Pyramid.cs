using System.Text;

namespace Pyramid;

/// <summary>
/// Pyramida orientovaná obráceně (stojí na svém vrcholu)
/// </summary>
public class Pyramid
{
    // Note: původní field s explicitní property jsem nahradil touto auto-impl property
    public int Rows { get; }

    /// <summary>
    /// Uspořádání: vrchol pyramidy má index [rows-1, 0]; Základna pyramidy má tyto indexy: [0, 0...rows-1]
    /// </summary>
    private readonly int[,] _data;

    public Pyramid(int[,] data)
    {
        // Note: přidal jsem validaci
        if (data.GetLength(0) != data.GetLength(1))
            throw new ArgumentException("Data has to be a square matrix", nameof(data));

        _data = data;
        Rows = data.GetLength(0);
    }

    // Note: pyramida si vystačí s readonly indexováním (po vytvoření se už měnit nemá)
    public int this[int row, int col] => _data[row, col];

    public int ColsInRow(int row) => Rows - row;

    /// <summary>
    /// Pretty print me
    /// </summary>
    public override string ToString()
    {
        var pyramidAsString = new StringBuilder();
        foreach (int row in Enumerable.Range(0, Rows))
        {
            pyramidAsString.Append(new string(' ', 4 * row));
            foreach (int col in Enumerable.Range(0, ColsInRow(row)))
                pyramidAsString.AppendFormat("[{0:00000}] ", _data[row, col]);
            pyramidAsString.AppendLine();
        }

        return pyramidAsString.ToString();
    }
}