using System.Buffers;

namespace Pyramid;

public sealed class PyramidSolver : IPyramidSolver
{
    public long PyramidMaximumTotal(Pyramid pyramid)
    {
        if (pyramid.Rows == 0)
            throw new InvalidOperationException("Cannot evaluate an empty pyramid");
        if (pyramid.Rows == 1)
            return pyramid[0, 0]; // Trivialni, neni potreba zadny vypocet

        var thisRowTopSums = new long[pyramid.Rows]; // Slouží k iteraci aktuálního řádku, ukládá nová maxima
        var rowUnderTopSums = new long[pyramid.Rows]; // Slouží k uchování maxim řádku, který je pod aktuálním řádkem

        rowUnderTopSums[0] = pyramid[pyramid.Rows - 1, 0]; // Vrchol pyramidy zapíšu mimo iteraci

        for (int row = pyramid.Rows - 2; row >= 0; --row)
        {
            int colsInThisRow = pyramid.ColsInRow(row);
            var colsInRowBelow = pyramid.ColsInRow(row + 1);

            for (int col = 0; col < colsInThisRow; ++col)
            {
                var cellValue = pyramid[row, col];
                // Zkouší se levá a pravá varianta (nebo jen ta, která je zrovna k dispozici)
                long? sumLeft = null, sumRight = null;
                if (col - 1 >= 0)
                    sumLeft = cellValue + rowUnderTopSums[col - 1];
                if (col < colsInRowBelow)
                    sumRight = cellValue + rowUnderTopSums[col];

                thisRowTopSums[col] = (sumLeft, sumRight) switch
                {
                    (null, not null) => sumRight.Value,
                    (not null, null) => sumLeft.Value,
                    (not null, not null) => Math.Max(sumLeft.Value, sumRight.Value),
                    (null, null) => throw new Exception("Unexpected error in pyramid solver") // prakticky unreachable
                };
            }

            thisRowTopSums[..colsInThisRow].CopyTo(rowUnderTopSums);
        }

        return rowUnderTopSums.Max();
    }
}