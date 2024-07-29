using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectTheDotsSolver
{
    internal class Logger
    {

        public static Bitmap GenerateGameBoardBitmap(Gameboard gameBoard)
        {
            int cellSize = 50; // Adjust as needed
            int width = gameBoard.size.Width * cellSize;
            int height = gameBoard.size.Height * cellSize;
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);

                // Draw grid lines
                Pen pen = new Pen(Color.Black, 1);
                for (int x = 0; x <= gameBoard.size.Width; x++)
                {
                    g.DrawLine(pen, x * cellSize, 0, x * cellSize, height);
                }
                for (int y = 0; y <= gameBoard.size.Height; y++)
                {
                    g.DrawLine(pen, 0, y * cellSize, width, y * cellSize);
                }

                // Draw points for start and end positions
                foreach (var line in gameBoard.Lines)
                {
                    DrawPoint(g, line.StartPos, line.Color, cellSize, 30);
                    DrawPoint(g, line.EndPos, line.Color, cellSize, 30);
                }
            }
            return bitmap;
        }
        public static Bitmap DrawSolution(Gameboard gameBoard)
        {
            Bitmap original = GenerateGameBoardBitmap(gameBoard);

            int cellSize = 50; // Adjust as needed
            using (Graphics g = Graphics.FromImage(original))
            {
                foreach (var line in gameBoard.Lines)
                {
                    foreach (var pos in line.CurrentTrajectory)
                    {
                        DrawPoint(g, pos, line.Color, cellSize, 20);
                    }
                    foreach (var pos in line.MandatoryStart)
                    {
                        DrawPoint(g, pos, line.Color, cellSize, 15);
                    }
                    foreach (var pos in line.MandatoryEnd)
                    {
                        DrawPoint(g, pos, line.Color, cellSize, 15);
                    }
                }
            }
            return original;
        }
        private static void DrawPoint(Graphics g, Pos pos, Color color, int cellSize, int size)
        {
            int x = pos.x * cellSize + cellSize / 2;
            int y = pos.y * cellSize + cellSize / 2;
            int pointSize = size; // Adjust as needed
            g.FillEllipse(new SolidBrush(color), x - pointSize / 2, y - pointSize / 2, pointSize, pointSize);
        }
    }
}
