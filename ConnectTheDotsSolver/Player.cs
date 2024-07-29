using System.Numerics;
using WindowsInput;

namespace ConnectTheDotsSolver
{
    internal class Player
    {
        public Vector2 topLeftCorner;
        public Vector2 bottomRightCorner;
        public int sizeX;
        public int sizeY;
        InputSimulator sim;

        public static double MapToAbsolute(double value, double screenSize)
        {
            return value * 65535.0 / screenSize;
        }
        public void Play(Gameboard board)
        {

            sim = new();

            int delayms = 50;
            foreach (var line in board.Lines)
            {

                MoveCursor(line.StartPos);
                Thread.Sleep(delayms);
                ClickCursor();

                foreach (var pos in line.MandatoryStart)
                {
                    Thread.Sleep(delayms);
                    MoveCursor(pos);
                }
                foreach (var pos in line.CurrentTrajectory)
                {
                    Thread.Sleep(delayms);
                    MoveCursor(pos);
                }
                for (int i = line.MandatoryEnd.Count - 1; i >= 0; i--)
                {
                    var pos = line.MandatoryEnd[i];
                    Thread.Sleep(delayms);
                    MoveCursor(pos);
                }
                Thread.Sleep(delayms);
                MoveCursor(line.EndPos);
                Thread.Sleep(delayms);
                ClickCursor();

            }
        }
        void MoveCursor(Pos pos)
        {
            // Screen resolution
            double screenWidth = 1920;
            double screenHeight = 1080;

            int width = (int)(bottomRightCorner.X - topLeftCorner.X);
            int height = (int)(bottomRightCorner.Y - topLeftCorner.Y);

            float cellSizeX = width / sizeX;
            float cellSizeY = height / sizeY;

            int x = (int)(pos.x * cellSizeX + cellSizeX / 2 + topLeftCorner.X);
            int y = (int)(pos.y * cellSizeY + cellSizeY / 2 + topLeftCorner.Y);

            // Convert to absolute mouse units
            double absoluteX = MapToAbsolute(x, screenWidth);
            double absoluteY = MapToAbsolute(y, screenHeight);

            // Move the mouse to the specified coordinates
            sim.Mouse.MoveMouseTo(absoluteX, absoluteY);
        }
        void ClickCursor()
        {
            sim.Mouse.LeftButtonClick();
        }
    }
}
