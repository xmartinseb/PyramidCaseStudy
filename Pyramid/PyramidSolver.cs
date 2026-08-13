using System;
using System.Numerics.Tensors;

namespace Pyramid;

public sealed class PyramidSolver : IPyramidSolver
{
    public long PyramidMaximumTotal(Pyramid pyramid)
    {
        if (pyramid.Rows == 0)
            throw new InvalidOperationException("Cannot evaluate an empty pyramid");
        if (pyramid.Rows == 1)
            return pyramid[0, 0]; // Trivialni, neni potreba zadny vypocet

        // Note: pro validaci pyramidy stačí prostá alokace na stacku, není potřeba plnohodnotné pole na heap
        Span<long> rowUnderTopSums = stackalloc long[pyramid.Rows]; // počet řádků = max počet sloupců

        rowUnderTopSums[0] = pyramid[pyramid.Rows - 1, 0]; // Vrchol pyramidy zapíšu mimo iteraci

        Span<long> thisRowTopSums = stackalloc long[pyramid.Rows];
        for (int row = pyramid.Rows - 2; row >= 0; --row)
        {
            rowUnderTopSums.CopyTo(thisRowTopSums); // TODO: zrusit zbytecne copy cells

            int colsInThisRow = pyramid.ColsInRow(row);
            for (int col = 0; col < colsInThisRow; ++col)
            {
                var cellValue = pyramid[row, col];
                var colsInRowBelow = pyramid.ColsInRow(row + 1);
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
                    (null, null) => throw new Exception("Unexpected error in pyramid solver") // unreachable
                };
            }

            thisRowTopSums.CopyTo(rowUnderTopSums);
        }

        // Note: rychlé SIMD nalezení maxima bez jakýchkoliv alokací
        return TensorPrimitives.Max(rowUnderTopSums);
    }

    static void ZeroMemory(Span<long> rowTopSums)
    {
        for (int i = 0; i < rowTopSums.Length; ++i)
            rowTopSums[i] = 0;
    }
}