using System;
using System.Collections.Generic;
using System.Linq;
using DiscordMultiBot.Maths;
using DiscordMultiBot.Games;
using DSharpPlus.CommandsNext;
using DiscordMultiBot.Games.Strategy;
using NUnit.Framework;
using Moq;

namespace DiscordMultiBot.Tests
{
    [TestFixture]
    public class MetamorphicTests
    {
        private Anaylzer analyzer;
        private GameBotLogic gameBotLogic;
        private Mock<CommandContext> mockContext;

        [SetUp]
        public void Setup()
        {
            mockContext = new Mock<CommandContext>();
            analyzer = new Anaylzer(mockContext.Object, "", new List<int>());
            gameBotLogic = new GameBotLogic();
        }

        [Test]
        public void TestCompositeMathOperations()
        {
            List<(List<int>, List<int>)> testGroups = new List<(List<int>, List<int>)>
            {
                (new List<int> { 1, 2, 3, 4 }, new List<int> { 3, 4, 5, 6 }),
                (new List<int> { -5, 0, 5, 10 }, new List<int> { -10, 0, 10, 20 }),
                (new List<int> { 1, 1, 2, 2, 3, 3 }, new List<int> { 2, 2, 3, 3, 4, 4 }),
                (new List<int> { 100, 200, 300 }, new List<int> { 200, 300, 400 }),
                (new List<int> { -1, -2, -3, -4 }, new List<int> { -4, -3, -2, -1 })
            };

            foreach (var (groupA, groupB) in testGroups)
            {
                var sumA = ((Sum)Factory.Instance().Strategy("sum")).Suming(groupA);
                var sumB = ((Sum)Factory.Instance().Strategy("sum")).Suming(groupB);
                var sumAB = ((Sum)Factory.Instance().Strategy("sum")).Suming(groupA.Concat(groupB).ToList());
                var intersection = groupA.Intersect(groupB).ToList();
                var sumIntersection = ((Sum)Factory.Instance().Strategy("sum")).Suming(intersection);

                Assert.That(sumA + sumB, Is.EqualTo(sumAB - sumIntersection),
                    $"Composite math operation failed for A:{string.Join(",", groupA)} and B:{string.Join(",", groupB)}");
            }
        }

        [Test]
        public void TestTicTacToeGameStateInvariant()
        {
            List<List<Square>> testBoards = new List<List<Square>>
            {
                CreateBoard(new int[] { 1, 2, 0, 0, 1, 0, 2, 0, 0 }),
                CreateBoard(new int[] { 1, 2, 1, 2, 1, 0, 2, 0, 0 }),
                CreateBoard(new int[] { 1, 2, 1, 2, 1, 2, 0, 0, 0 }),
                CreateBoard(new int[] { 1, 0, 1, 2, 2, 0, 1, 0, 2 }),
                CreateBoard(new int[] { 1, 2, 1, 2, 1, 2, 2, 1, 0 })
            };

            foreach (var board in testBoards)
            {
                int xCount = board.Count(s => s.SquareNumber == 1);
                int oCount = board.Count(s => s.SquareNumber == 2);
                int diff = Math.Abs(xCount - oCount);

                Assert.That(diff, Is.LessThanOrEqualTo(1),
                    $"TicTacToe game state invariant failed for board: {string.Join(",", board.Select(s => s.SquareNumber))}");
            }
        }

        [Test]
        public void TestTicTacToeAIStrategyConsistency()
        {
            List<(List<Square>, int, int)> testCases = new List<(List<Square>, int, int)>
            {
                (CreateBoard(new int[] { 1, 0, 0, 0, 2, 0, 0, 0, 0 }), 3, 1),
                (CreateBoard(new int[] { 1, 0, 2, 0, 1, 0, 0, 0, 0 }), 3, 2),
                (CreateBoard(new int[] { 1, 2, 1, 0, 2, 0, 0, 0, 0 }), 3, 3),
                (CreateBoard(new int[] { 1, 0, 0, 0, 1, 0, 2, 0, 2 }), 3, 4),
                (CreateBoard(new int[] { 1, 2, 0, 0, 1, 0, 2, 0, 0 }), 3, 5)
            };

            foreach (var (board, difficulty, turn) in testCases)
            {
                var move1 = gameBotLogic.MakeMove(board, difficulty, turn);
                var move2 = gameBotLogic.MakeMove(board, difficulty, turn);
                var move3 = gameBotLogic.MakeMove(board, difficulty, turn);

                Assert.That(move1, Is.EqualTo(move2));
                Assert.That(move2, Is.EqualTo(move3));
                Assert.That(move1, Is.EqualTo(move3));
            }
        }

        [Test]
        public void TestMathOperationWithGameState()
        {
            List<List<Square>> testBoards = new List<List<Square>>
            {
                CreateBoard(new int[] { 1, 2, 0, 0, 1, 0, 2, 0, 0 }),
                CreateBoard(new int[] { 1, 2, 1, 2, 1, 0, 2, 0, 0 }),
                CreateBoard(new int[] { 1, 2, 1, 2, 1, 2, 0, 0, 0 }),
                CreateBoard(new int[] { 1, 0, 1, 2, 2, 0, 1, 0, 2 }),
                CreateBoard(new int[] { 1, 2, 1, 2, 1, 2, 2, 1, 0 })
            };

            foreach (var board in testBoards)
            {
                int boardSum = (int)((Sum)Factory.Instance().Strategy("sum")).Suming(board.Select(s => s.SquareNumber).ToList());
                int xCount = board.Count(s => s.SquareNumber == 1);
                int oCount = board.Count(s => s.SquareNumber == 2);
                int expectedSum = xCount * 1 + oCount * 2;

                Assert.That(boardSum, Is.EqualTo(expectedSum),
                    $"Math operation with game state failed for board: {string.Join(",", board.Select(s => s.SquareNumber))}");
            }
        }

        [Test]
        public void TestCompositeGameLogicWithMathProperty()
        {
            List<(List<Square>, List<Square>)> testTransitions = new List<(List<Square>, List<Square>)>
            {
                (CreateBoard(new int[] { 1, 2, 0, 0, 1, 0, 2, 0, 0 }), CreateBoard(new int[] { 1, 2, 0, 0, 1, 0, 2, 0, 1 })),
                (CreateBoard(new int[] { 1, 0, 1, 2, 0, 0, 2, 0, 0 }), CreateBoard(new int[] { 1, 2, 1, 2, 0, 0, 2, 0, 0 })),
                (CreateBoard(new int[] { 1, 2, 1, 0, 1, 0, 2, 0, 0 }), CreateBoard(new int[] { 1, 2, 1, 0, 1, 2, 2, 0, 0 })),
                (CreateBoard(new int[] { 1, 2, 0, 2, 1, 0, 1, 0, 0 }), CreateBoard(new int[] { 1, 2, 0, 2, 1, 0, 1, 2, 0 })),
                (CreateBoard(new int[] { 1, 2, 1, 2, 0, 0, 2, 1, 0 }), CreateBoard(new int[] { 1, 2, 1, 2, 1, 0, 2, 1, 0 }))
            };

            foreach (var (boardBefore, boardAfter) in testTransitions)
            {
                int productBefore = boardBefore.Where(s => s.SquareNumber != 0).Aggregate(1, (acc, s) => acc * s.SquareNumber);
                int productAfter = boardAfter.Where(s => s.SquareNumber != 0).Aggregate(1, (acc, s) => acc * s.SquareNumber);

                Assert.That(productAfter, Is.EqualTo(productBefore),
                    $"Composite game logic with math property failed for transition: " +
                    $"{string.Join(",", boardBefore.Select(s => s.SquareNumber))} to " +
                    $"{string.Join(",", boardAfter.Select(s => s.SquareNumber))}");
            }
        }

        private List<Square> CreateBoard(int[] values)
        {
            return values.Select(v => new Square(null, v)).ToList();
        }
    }

    // Mock class for CommandContext
    public class MockCommandContext
    {
        // Add necessary properties and methods to mock CommandContext
    }
}