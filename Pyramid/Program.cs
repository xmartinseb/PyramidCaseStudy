using Pyramid;
using static System.Console;

// You could imagine pyramid like two dimensional array
// 
// [00059] [00207] [00098] [00095]
// [00087] [00001] [00070] [     ]
// [00036] [00041] [     ] [     ]
// [00023] [     ] [     ] [     ]
// 
// Addressing looks like this [row][column]
// 
// [00,00] [00,01] [00,02] [00,03]
//     [01,00] [01,01] [01,02]
//         [02,00] [02,01]
//             [03,00]
//
// The max. sum of this pyramid is 353.
// The max. path is [03,00],[02,00],[01,00],[00,01]


var generator = new RandomPyramidGenerator(rows: 4, range: 10);
var pyramid = generator.GeneratePyramid();
var solver = new PyramidSolver();

WriteLine("Maximum path sum for this pyramid is :");
WriteLine(solver.PyramidMaximumTotal(pyramid));
WriteLine();
WriteLine(pyramid);

ReadKey();