using System.Drawing;
using System.Numerics;

namespace ConnectTheDotsSolver
{
    internal class Reader
    {
        public Vector2 topLeftCorner;
        public Vector2 bottomRightCorner;
        public int sizeX;
        public int sizeY;

        public Gameboard ScanGameboard()
        {
            Gameboard gameBoard = new();
            gameBoard.size = new Size(sizeX,sizeY);
            int width = (int)(bottomRightCorner.X - topLeftCorner.X);
            int height = (int)(bottomRightCorner.Y - topLeftCorner.Y);

            using (Bitmap bitmap = new Bitmap(width, height))
            {
                // Use the Graphics object to copy the pixel from the screen
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen((int)topLeftCorner.X, (int)topLeftCorner.Y, 0, 0, new Size(width, height));
                }


                float cellSizeX = width / sizeX;
                float cellSizeY = height / sizeY;
                gameBoard.Lines = new();
                Dictionary<Color, (Pos, Pos)> colors = new();
                Color backgroundColor = Color.White;
                for (short idx = 0; idx < sizeX; idx += 1)
                {
                    for (short idy = 0; idy < sizeY; idy += 1)
                    {
                        int x = (int)(idx * cellSizeX + cellSizeX / 2f);
                        int y = (int)(idy * cellSizeY + cellSizeY / 2f);

                        Color pixelColor = bitmap.GetPixel(x, y);

                        // Output the color of the pixel
                        Console.WriteLine($"The color of the pixel at ({x}, {y}) is: {pixelColor}");

                        if (pixelColor.GetBrightness() > 0.1f)
                        {
                            Console.WriteLine($"Color is light");
                            if (colors.TryGetValue(pixelColor, out var outValue))
                            {
                                if (colors[pixelColor].Item2.Equals(new Pos(-1, -1)))
                                    colors[pixelColor] = (outValue.Item1, new Pos(idx, idy));
                                else
                                    backgroundColor = pixelColor; // Found more than 2 times the same color
                            }
                            else
                                colors.Add(pixelColor, (new Pos(idx, idy), new Pos(-1, -1)));
                        }
                        else
                            Console.WriteLine($"Color is dark");
                    }
                }
                foreach (var keyValuePair in colors)
                {
                    if (keyValuePair.Key != backgroundColor)
                    {
                        gameBoard.Lines.Add(new Line(
                            keyValuePair.Value.Item1,
                            keyValuePair.Value.Item2,
                            keyValuePair.Key
                        ));
                    }
                }


            }

            return gameBoard;
        }
    }
}
