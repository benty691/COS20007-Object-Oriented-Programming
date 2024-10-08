using System;
using System.Collections.Generic;
using Xunit;
using DiscordMultiBot.Games;
using DiscordMultiBot.Math;
using DiscordMultiBot.Maths;

namespace DiscordMultiBot.Tests
{
    public class MetamorphicTests
    {
        // Existing code...

        [Theory]
        [MemberData(nameof(ScalingCalculatorData))]
        public void ScalingCalculatorOperations(string operation, double input1, double input2, double scaleFactor)
        {
            var calculator = new Calculator();
            double originalResult = 0;
            double scaledResult = 0;

            switch (operation.ToLower())
            {
                case "add":
                    originalResult = calculator.Add(input1, input2);
                    scaledResult = calculator.Add(input1 * scaleFactor, input2 * scaleFactor);
                    break;
                case "subtract":
                    originalResult = calculator.Subtract(input1, input2);
                    scaledResult = calculator.Subtract(input1 * scaleFactor, input2 * scaleFactor);
                    break;
                case "multiply":
                    originalResult = calculator.Multiply(input1, input2);
                    scaledResult = calculator.Multiply(input1 * scaleFactor, input2 * scaleFactor);
                    break;
                case "divide":
                    originalResult = calculator.Divide(input1, input2);
                    scaledResult = calculator.Divide(input1 * scaleFactor, input2 * scaleFactor);
                    break;
            }

            Assert.Equal(originalResult * scaleFactor, scaledResult, 6);
        }

        public static IEnumerable<object[]> ScalingCalculatorData()
        {
            yield return new object[] { "add", 5, 3, 2 };
            yield return new object[] { "subtract", 10, 4, 3 };
            yield return new object[] { "multiply", 6, 7, 0.5 };
            yield return new object[] { "divide", 20, 5, 4 };
            yield return new object[] { "add", -8, 12, -1 };
            yield return new object[] { "subtract", 15, -3, 2 };
            yield return new object[] { "multiply", -4, -9, 0.25 };
            yield return new object[] { "divide", -100, 20, -0.5 };
        }

        [Theory]
        [MemberData(nameof(TicTacToeBoardSymmetryData))]
        public void TicTacToeBoardSymmetry(string[] originalBoard, string[] transformedBoard)
        {
            var game = new TicTacToeGame();
            var originalStatus = game.CheckGameStatus(originalBoard);
            var transformedStatus = game.CheckGameStatus(transformedBoard);

            Assert.Equal(originalStatus, transformedStatus);
        }

        public static IEnumerable<object[]> TicTacToeBoardSymmetryData()
        {
            // Test group 1: Empty board
            yield return new object[] {
                new string[] { "", "", "", "", "", "", "", "", "" },
                new string[] { "", "", "", "", "", "", "", "", "" }
            };

            // Test group 2: Horizontal reflection
            yield return new object[] {
                new string[] { "X", "O", "X", "", "", "", "", "", "" },
                new string[] { "", "", "", "", "", "", "X", "O", "X" }
            };

            // Test group 3: Vertical reflection
            yield return new object[] {
                new string[] { "X", "", "O", "X", "", "O", "X", "", "" },
                new string[] { "O", "", "X", "O", "", "X", "", "", "X" }
            };

            // Test group 4: Diagonal reflection
            yield return new object[] {
                new string[] { "X", "O", "", "", "X", "", "", "", "O" },
                new string[] { "O", "", "", "", "X", "", "", "O", "X" }
            };

            // Test group 5: 90-degree rotation
            yield return new object[] {
                new string[] { "X", "O", "", "X", "O", "", "", "", "" },
                new string[] { "", "X", "X", "", "O", "O", "", "", "" }
            };

            // Test group 6: 180-degree rotation
            yield return new object[] {
                new string[] { "X", "O", "X", "", "O", "", "", "", "" },
                new string[] { "", "", "", "", "O", "", "X", "O", "X" }
            };

            // Test group 7: 270-degree rotation
            yield return new object[] {
                new string[] { "X", "O", "", "", "X", "", "O", "", "" },
                new string[] { "", "", "O", "", "X", "", "", "O", "X" }
            };

            // Test group 8: Winning condition preserved
            yield return new object[] {
                new string[] { "X", "X", "X", "O", "O", "", "", "", "" },
                new string[] { "X", "O", "", "X", "O", "", "X", "", "" }
            };
        }

        [Theory]
        [MemberData(nameof(MathOperationCommutativityData))]
        public void MathOperationCommutativity(string operation, List<int> numbers)
        {
            var strategy = Factory.Instance().Strategy(operation);
            var originalResult = strategy.SendSummary(numbers);
            var reversedNumbers = new List<int>(numbers);
            reversedNumbers.Reverse();
            var reversedResult = strategy.SendSummary(reversedNumbers);

            Assert.Equal(originalResult, reversedResult);
        }

        public static IEnumerable<object[]> MathOperationCommutativityData()
        {
            yield return new object[] { "sum", new List<int> { 1, 2, 3, 4, 5 } };
            yield return new object[] { "sum", new List<int> { -1, 0, 1 } };
            yield return new object[] { "average", new List<int> { 10, 20, 30, 40 } };
            yield return new object[] { "average", new List<int> { -5, 0, 5 } };
            yield return new object[] { "minmax", new List<int> { 1, 5, 3, 2, 4 } };
            yield return new object[] { "minmax", new List<int> { -10, 0, 10, -5, 5 } };
            yield return new object[] { "multiplication", new List<int> { 2, 3, 4 } };
            yield return new object[] { "multiplication", new List<int> { -2, -3, -4 } };
        }

        [Theory]
        [MemberData(nameof(TicTacToeBoardRotationData))]
        public void TicTacToeBoardRotationInvariance(string[] originalBoard, string[] rotatedBoard)
        {
            var game = new TicTacToeGame();
            var originalStatus = game.CheckGameStatus(originalBoard);
            var rotatedStatus = game.CheckGameStatus(rotatedBoard);

            Assert.Equal(originalStatus, rotatedStatus);
        }

        public static IEnumerable<object[]> TicTacToeBoardRotationData()
        {
            // Test group 1: Empty board
            yield return new object[] {
                new string[] { "", "", "", "", "", "", "", "", "" },
                new string[] { "", "", "", "", "", "", "", "", "" }
            };

            // Test group 2: 90-degree rotation
            yield return new object[] {
                new string[] { "X", "O", "", "X", "O", "", "", "", "" },
                new string[] { "", "X", "X", "", "O", "O", "", "", "" }
            };

            // Test group 3: 180-degree rotation
            yield return new object[] {
                new string[] { "X", "O", "X", "", "O", "", "", "", "" },
                new string[] { "", "", "", "", "O", "", "X", "O", "X" }
            };

            // Test group 4: 270-degree rotation
            yield return new object[] {
                new string[] { "X", "O", "", "", "X", "", "O", "", "" },
                new string[] { "", "", "O", "", "X", "", "", "O", "X" }
            };

            // Test group 5: Horizontal win
            yield return new object[] {
                new string[] { "X", "X", "X", "O", "O", "", "", "", "" },
                new string[] { "", "", "", "X", "X", "X", "O", "O", "" }
            };

            // Test group 6: Vertical win
            yield return new object[] {
                new string[] { "O", "X", "", "O", "X", "", "O", "", "" },
                new string[] { "", "", "O", "X", "X", "O", "", "", "O" }
            };

            // Test group 7: Diagonal win
            yield return new object[] {
                new string[] { "X", "", "", "", "X", "", "", "", "X" },
                new string[] { "", "", "X", "", "X", "", "X", "", "" }
            };

            // Test group 8: Tie game
            yield return new object[] {
                new string[] { "X", "O", "X", "X", "O", "O", "O", "X", "X" },
                new string[] { "X", "X", "O", "O", "O", "X", "X", "O", "X" }
            };
        }
    }
}