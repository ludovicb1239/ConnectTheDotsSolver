using ConnectTheDotsSolver;
using System.Drawing;

namespace Tests
{
    public class SolverTests
    {
        Solver solver;

        [SetUp]
        public void SetUp()
        {
            solver = new Solver(null);
        }

        void SolveGameboard(Gameboard board)
        {
            bool solved = solver.SolveGameBoard(board);
            Assert.IsTrue(solved);
        }
        [Test]
        public void Solve5x5_0()
        {
            Gameboard board = new();
            board.size = new(5, 5);

            List<Line> lines = new List<Line>();
            lines.Add(new Line(new Pos(0, 0), new Pos(4, 3), Color.Yellow));
            lines.Add(new Line(new Pos(0, 3), new Pos(4, 4), Color.Blue));
            lines.Add(new Line(new Pos(1, 3), new Pos(2, 2), Color.Green));
            lines.Add(new Line(new Pos(0, 4), new Pos(2, 3), Color.Red));

            board.Lines = lines;
            SolveGameboard(board);
        }

        [Test]
        public void Solve7x7_0()
        {
            Gameboard board = new();
            board.size = new(7, 7);

            List<Line> lines = new List<Line>();
            lines.Add(new Line(new Pos(3, 3), new Pos(6, 3), Color.Red));
            lines.Add(new Line(new Pos(3, 4), new Pos(6, 4), Color.Blue));
            lines.Add(new Line(new Pos(2, 3), new Pos(2, 5), Color.Yellow));
            lines.Add(new Line(new Pos(6, 1), new Pos(1, 5), Color.Orange));
            lines.Add(new Line(new Pos(4, 0), new Pos(5, 6), Color.Green));
            lines.Add(new Line(new Pos(1, 1), new Pos(6, 0), Color.Cyan));
            lines.Add(new Line(new Pos(3, 5), new Pos(6, 6), Color.Pink));

            board.Lines = lines;
            SolveGameboard(board);
        }

        [Test]
        public void Solve8x8_0()
        {
            Gameboard board = new();
            board.size = new(8, 8);

            List<Line> lines = new List<Line>();
            lines.Add(new Line(new Pos(0, 0), new Pos(3, 0), Color.Cyan));
            lines.Add(new Line(new Pos(2, 1), new Pos(3, 5), Color.Red));
            lines.Add(new Line(new Pos(5, 3), new Pos(7, 7), Color.Yellow));
            lines.Add(new Line(new Pos(4, 3), new Pos(6, 6), Color.Orange));
            lines.Add(new Line(new Pos(2, 3), new Pos(3, 6), Color.Green));
            lines.Add(new Line(new Pos(1, 6), new Pos(2, 2), Color.Blue));
            lines.Add(new Line(new Pos(4, 1), new Pos(6, 1), Color.Pink));
            lines.Add(new Line(new Pos(4, 0), new Pos(7, 6), Color.DeepPink));
            lines.Add(new Line(new Pos(3, 1), new Pos(4, 4), Color.DarkRed));

            board.Lines = lines;
            SolveGameboard(board);
        }

        [Test]
        public void Solve9x9_0()
        {
            Gameboard board = new();
            board.size = new(9, 9);

            List<Line> lines = new List<Line>();
            lines.Add(new Line(new Pos(0, 0), new Pos(8, 4), Color.Red));
            lines.Add(new Line(new Pos(0, 1), new Pos(1, 8), Color.Blue));
            lines.Add(new Line(new Pos(0, 2), new Pos(3, 3), Color.Yellow));
            lines.Add(new Line(new Pos(1, 5), new Pos(8, 6), Color.DeepPink));
            lines.Add(new Line(new Pos(2, 5), new Pos(4, 7), Color.Pink));
            lines.Add(new Line(new Pos(3, 5), new Pos(7, 7), Color.Gray));
            lines.Add(new Line(new Pos(5, 2), new Pos(5, 5), Color.Green));
            lines.Add(new Line(new Pos(2, 1), new Pos(7, 6), Color.DarkOrange));
            lines.Add(new Line(new Pos(7, 1), new Pos(8, 5), Color.Orange));

            board.Lines = lines;
            SolveGameboard(board);
        }
    }
}
