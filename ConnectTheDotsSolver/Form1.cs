using System.Drawing.Design;
using System.Linq;
using System.Security.Policy;
using System.Windows.Forms;

namespace ConnectTheDotsSolver
{
    public struct Pos
    {
        public short x;
        public short y;
        public Pos(short x, short y)
        {
            this.x = x;
            this.y = y;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Pos))
                return false;

            Pos other = (Pos)obj;
            return this.x == other.x && this.y == other.y;
        }
        public bool Equals(Pos pos)
        {
            return this.x == pos.x && this.y == pos.y;
        }
        public override int GetHashCode()
        {
            return (x.GetHashCode() * 397) ^ y.GetHashCode();
        }
    }
    public class Line
    {
        public Pos StartPos;
        public Pos EndPos;
        public Color Color;
        public List<HashSet<Pos>> PossibleTrajectories;
        public HashSet<Pos> CurrentTrajectory;
        public List<Pos> MandatoryStart;
        public List<Pos> MandatoryEnd;
        public Line (Pos start, Pos end, Color color)
        {
            this.StartPos = start;
            this.EndPos = end;
            this.Color = color;
            PossibleTrajectories = new();
            CurrentTrajectory = new();
            MandatoryStart = new();
            MandatoryEnd = new();
        }
    }
    public class GameBoard
    {
        public List<Line> Lines;
        public Size size;
    }
    public partial class Form1 : Form
    {
        private Bitmap gameBoardBitmap;
        private PictureBox pictureBox;
        public Form1()
        {
            InitializeComponent();
            InitializePictureBox();
        }
        private void InitializePictureBox()
        {
            pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            Controls.Add(pictureBox);
        }
        private void SolveButton_Click(object sender, EventArgs e)
        {
            GameBoard board = new();

            /*
            board.size = new(5, 5);
            List<Line> lines = new List<Line>();
            lines.Add(new Line(new Pos(0, 0), new Pos(4, 3), Color.Yellow));
            lines.Add(new Line(new Pos(0, 3), new Pos(4, 4), Color.Blue));
            lines.Add(new Line(new Pos(1, 3), new Pos(2, 2), Color.Green));
            lines.Add(new Line(new Pos(0, 4), new Pos(2, 3), Color.Red));
            */

            /*
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
            */

            
            board.size = new(8, 8);
            List<Line> lines = new List<Line>();
            lines.Add(new Line(new Pos(2, 3), new Pos(4, 3), Color.Red));
            lines.Add(new Line(new Pos(4, 4), new Pos(3, 6), Color.Blue));
            lines.Add(new Line(new Pos(0, 3), new Pos(0, 6), Color.Yellow));
            lines.Add(new Line(new Pos(1, 1), new Pos(4, 6), Color.Orange));
            lines.Add(new Line(new Pos(2, 2), new Pos(1, 6), Color.Green));
            lines.Add(new Line(new Pos(0, 7), new Pos(2, 6), Color.Cyan));
            

            /*
            board.size = new(7, 7);
            List<Line> lines = new List<Line>();
            lines.Add(new Line(new Pos(3, 3), new Pos(6, 3), Color.Red));
            lines.Add(new Line(new Pos(3, 4), new Pos(6, 4), Color.Blue));
            lines.Add(new Line(new Pos(2, 3), new Pos(2, 5), Color.Yellow));
            lines.Add(new Line(new Pos(6, 1), new Pos(1, 5), Color.Orange));
            lines.Add(new Line(new Pos(4, 0), new Pos(5, 6), Color.Green));
            lines.Add(new Line(new Pos(1, 1), new Pos(6, 0), Color.Cyan));
            lines.Add(new Line(new Pos(3, 5), new Pos(6, 6), Color.Pink));
            */

            board.Lines = lines;
            gameBoardBitmap = GenerateGameBoardBitmap(board);
            pictureBox.Image = gameBoardBitmap;
            gameBoardBitmap.Save("board.png");
            solveGameBoard(board);
        }
        void solveGameBoard(GameBoard gameBoard) {

            foreach (var line in gameBoard.Lines)
            {
                line.MandatoryStart = new();
                line.MandatoryEnd = new();
                line.CurrentTrajectory = new();
                line.PossibleTrajectories = new();
            }

            bool foundPath = true;
            int gen = 0;
            while (foundPath)
            {
                foundPath = false;
                foreach (var line in gameBoard.Lines)
                {
                    if (FindMandatoryPath(gameBoard, line, gen))
                    {
                        foundPath = true;
                    }
                }
                gen++;
            }
            foreach (var line in gameBoard.Lines)
            {
                Console.WriteLine($"Found {line.MandatoryStart.Count} mandatory start points for color {line.Color.Name}");
                Console.WriteLine($"Found {line.MandatoryEnd.Count} mandatory end points for color {line.Color.Name}");
            }
            

            // Start DFS from each valid start position
            foreach (var line in gameBoard.Lines)
            {
                Pos start = line.MandatoryStart.Count != 0 ? line.MandatoryStart.Last() : line.StartPos;
                Pos end = line.MandatoryEnd.Count != 0 ? line.MandatoryEnd.Last() : line.EndPos;
                DFS(gameBoard, start, end, line);
                Console.WriteLine($"Found {line.PossibleTrajectories.Count} paths for color {line.Color.Name}");

                if (line.PossibleTrajectories.Count < 1000)
                {
                    if (!Directory.Exists(line.Color.Name))
                    {
                        Directory.CreateDirectory(line.Color.Name);
                    }
                    int i = 0;
                    foreach(var traj in line.PossibleTrajectories)
                    {
                        line.CurrentTrajectory = traj;
                        gameBoardBitmap = DrawSolution(gameBoard);
                        gameBoardBitmap.Save($"{line.Color.Name}/solution{i}.png");
                        i++;
                    }
                }
                line.CurrentTrajectory = new();
            }

            Dictionary<int, int[]>[] values = new Dictionary<int, int[]>[gameBoard.Lines.Count()];
            for(int i = 0;  i < gameBoard.Lines.Count()-1; i++)
            {
                values[i] = CheckLines(gameBoard, gameBoard.Lines[i], gameBoard.Lines[i+1]);
            }
            values[gameBoard.Lines.Count()-1] = CheckLines(gameBoard, gameBoard.Lines.Last(), gameBoard.Lines[0]);

            /*
            for (int i = 0; i < gameBoard.Lines.Count() - 1; i++)
            {
                Console.WriteLine($"Tree branch {gameBoard.Lines[i].Color.Name} and {gameBoard.Lines[i + 1].Color.Name}");
                foreach (var dict in values[i])
                {
                    Console.WriteLine($"Number {dict.Key}");
                    Console.WriteLine($"Ges with {string.Join(",", dict.Value)}");
                }
            }
            */

            if (checkBranch(gameBoard, 0, values, -1))
            {
                Console.WriteLine("Found solution !");

                gameBoardBitmap = DrawSolution(gameBoard);
                pictureBox.Image = gameBoardBitmap;
                gameBoardBitmap.Save("boardSolved.png");
            }
            else
            {
                Console.WriteLine("Cant find a solution");
            }
        }
        static bool checkBranch(GameBoard gameBoard, int branchID, Dictionary<int, int[]>[] values, int check)
        {
            Dictionary<int, int[]> branch = values[branchID];
            foreach (var key in branch.Keys)
            {
                if (branchID == 0)
                    Console.Write(".");

                if (check == -1 || key == check)
                {
                    gameBoard.Lines[branchID].CurrentTrajectory = gameBoard.Lines[branchID].PossibleTrajectories[key];
                    foreach (var sub in branch[key])
                    {
                        if (branchID == values.Length - 1)
                        {
                            HashSet<Pos> total = new();
                            foreach(var line in gameBoard.Lines)
                            {
                                total.UnionWith(line.CurrentTrajectory);
                                total.UnionWith(line.MandatoryStart);
                                total.UnionWith(line.MandatoryEnd);
                                total.UnionWith([line.StartPos, line.EndPos]);
                            }
                            return total.Count == gameBoard.size.Width * gameBoard.size.Height;
                        }
                        else
                        {
                            if (checkBranch(gameBoard, branchID + 1, values, sub))
                                return true;
                        }
                    }
                }
            }
            return false;
        }
        // returns true if it found a path
        static bool FindMandatoryPath(GameBoard gameBoard, Line currentLine, int gen)
        {
            bool foundPath = false;
            if (currentLine.MandatoryStart.Count == 0)
            {
                HashSet<Pos> PossibleWays = new();
                short[] dx1 = { 0, 1, 0, -1 };
                short[] dy1 = { -1, 0, 1, 0 };

                bool backToItself = false;
                for (int n = 0; n < 4; n++)
                {
                    short nextX = (short)(currentLine.StartPos.x + dx1[n]);
                    short nextY = (short)(currentLine.StartPos.y + dy1[n]);
                    Pos nextPos = new(nextX, nextY);

                    // Check if the next position is within the bounds of the game board
                    if (nextX >= 0 && nextX < gameBoard.size.Width && nextY >= 0 && nextY < gameBoard.size.Height)
                    {
                        if (currentLine.EndPos.x == nextPos.x && currentLine.EndPos.y == nextPos.y)
                        {
                            backToItself = true;
                            break;
                        }
                        if (currentLine.MandatoryEnd.Contains(nextPos))
                        {
                            backToItself = true;
                            break;
                        }
                        if (PosOnColorDot(gameBoard, nextPos, currentLine))
                            continue;
                        PossibleWays.Add(nextPos);
                    }
                }
                if (PossibleWays.Count() == 1 && !backToItself)
                {
                    foundPath = true;
                    currentLine.MandatoryStart.Add(PossibleWays.First());
                }

            }
            else
            {
                HashSet<Pos> PossibleWays = new();
                short[] dx1 = { 0, 1, 0, -1 };
                short[] dy1 = { -1, 0, 1, 0 };

                bool backToItself = false;
                for (int n = 0; n < 4; n++)
                {
                    short nextX = (short)(currentLine.MandatoryStart.Last().x + dx1[n]);
                    short nextY = (short)(currentLine.MandatoryStart.Last().y + dy1[n]);
                    Pos nextPos = new(nextX, nextY);

                    // Check if the next position is within the bounds of the game board
                    if (nextX >= 0 && nextX < gameBoard.size.Width && nextY >= 0 && nextY < gameBoard.size.Height)
                    {
                        if (currentLine.EndPos.x == nextPos.x && currentLine.EndPos.y == nextPos.y)
                        {
                            backToItself = true;
                            break;
                        }
                        if (currentLine.MandatoryEnd.Contains(nextPos))
                        {
                            backToItself = true;
                            break;
                        }
                        if (PosOnColorDot(gameBoard, nextPos, null))
                            continue;

                        PossibleWays.Add(nextPos);
                    }
                }
                if (PossibleWays.Count() == 1 && !backToItself)
                {
                    foundPath = true;
                    currentLine.MandatoryStart.Add(PossibleWays.First());
                }
            }
            if (currentLine.MandatoryEnd.Count == 0)
            {
                HashSet<Pos> PossibleWays = new();
                short[] dx1 = { 0, 1, 0, -1 };
                short[] dy1 = { -1, 0, 1, 0 };

                bool backToItself = false;
                for (int n = 0; n < 4; n++)
                {
                    short nextX = (short)(currentLine.EndPos.x + dx1[n]);
                    short nextY = (short)(currentLine.EndPos.y + dy1[n]);
                    Pos nextPos = new(nextX, nextY);

                    // Check if the next position is within the bounds of the game board
                    if (nextX >= 0 && nextX < gameBoard.size.Width && nextY >= 0 && nextY < gameBoard.size.Height)
                    {
                        if (currentLine.StartPos.x == nextPos.x && currentLine.StartPos.y == nextPos.y)
                        {
                            backToItself = true;
                            break;
                        }
                        if (currentLine.MandatoryStart.Contains(nextPos))
                        {
                            backToItself = true;
                            break;
                        }
                        if (PosOnColorDot(gameBoard, nextPos, currentLine))
                            continue;
                        PossibleWays.Add(nextPos);
                    }
                }
                if (PossibleWays.Count() == 1 && !backToItself)
                {
                    foundPath = true;
                    currentLine.MandatoryEnd.Add(PossibleWays.First());
                }
            }
            else
            {
                HashSet<Pos> PossibleWays = new();
                short[] dx1 = { 0, 1, 0, -1 };
                short[] dy1 = { -1, 0, 1, 0 };

                bool backToItself = false;
                for (int n = 0; n < 4; n++)
                {
                    short nextX = (short)(currentLine.MandatoryEnd.Last().x + dx1[n]);
                    short nextY = (short)(currentLine.MandatoryEnd.Last().y + dy1[n]);
                    Pos nextPos = new(nextX, nextY);

                    // Check if the next position is within the bounds of the game board
                    if (nextX >= 0 && nextX < gameBoard.size.Width && nextY >= 0 && nextY < gameBoard.size.Height)
                    {
                        if (currentLine.StartPos.x == nextPos.x && currentLine.StartPos.y == nextPos.y)
                        {
                            backToItself = true;
                            break;
                        }
                        if (currentLine.MandatoryStart.Contains(nextPos))
                        {
                            backToItself = true;
                            break;
                        }
                        if (PosOnColorDot(gameBoard, nextPos, null))
                            continue;

                        PossibleWays.Add(nextPos);
                    }
                }
                if (PossibleWays.Count() == 1 && !backToItself)
                {
                    foundPath = true;
                    currentLine.MandatoryEnd.Add(PossibleWays.First());
                }
            }
            return foundPath;
        }
        static void DFS(GameBoard gameBoard, Pos currentPos, Pos targetPos, Line currentLine)
        {
            // Add current position to the current path
            currentLine.CurrentTrajectory.Add(currentPos);

            // If current position is the target position, add the current path to allPaths
            if (currentPos.x == targetPos.x && currentPos.y == targetPos.y)
            {
                currentLine.PossibleTrajectories.Add(new (currentLine.CurrentTrajectory));
            }
            else
            {
                // Move in all possible directions (up, right, down, left)
                int[] dx = { 0, 1, 0, -1 };
                int[] dy = { -1, 0, 1, 0 };

                for (int i = 0; i < 4; i++)
                {
                    short nextX = (short)(currentPos.x + dx[i]);
                    short nextY = (short)(currentPos.y + dy[i]);
                    Pos nextPos = new(nextX, nextY);

                    // Check if the next position is within the bounds of the game board
                    if (nextX >= 0 && nextX < gameBoard.size.Width && nextY >= 0 && nextY < gameBoard.size.Height)
                    {
                        if (!nextPos.Equals(targetPos))
                        {
                            if (currentLine.CurrentTrajectory.Contains(nextPos))
                                continue;
                            /*
                            if (nextPos.Equals(currentLine.StartPos))
                                continue;                            
                            if (currentLine.MandatoryEnd.Contains(nextPos))
                                continue;
                            if (currentLine.MandatoryStart.Contains(nextPos))
                                continue;*/
                            if (PosOnColorDot(gameBoard, nextPos, null))
                                continue;
                            if (PosObstructs(gameBoard, nextPos, currentLine))
                                continue;
                        }
                        // Recursive call to explore the next position
                        DFS(gameBoard, nextPos, targetPos, currentLine);
                    }
                }
            }

            // Remove current position from the current path to backtrack
            currentLine.CurrentTrajectory.Remove(currentPos);
        }
        static Dictionary<int, int[]> CheckLines(GameBoard gameBoard, Line line1, Line line2)
        {
            //Returns a list of combinaisons for combining two lines
            Dictionary<int, int[]> toReturn = new();


            HashSet<Pos> basicPos1 = [line1.StartPos, line1.EndPos];
            basicPos1.UnionWith(line1.MandatoryStart);
            basicPos1.UnionWith(line1.MandatoryEnd);

            HashSet<Pos> basicPos2 = [line2.StartPos, line2.EndPos];
            basicPos2.UnionWith(line2.MandatoryStart);
            basicPos2.UnionWith(line2.MandatoryEnd);

            int idx1 = 0, idx2 = 0;
            foreach(var path1 in line1.PossibleTrajectories)
            {
                idx2 = 0;
                List<int> possibleCombinaisons = new();
                foreach (var path2 in line2.PossibleTrajectories)
                {
                    if (!path1.Intersect(path2).Any())
                    {
                        //If there is no intersecting between the two
                        possibleCombinaisons.Add(idx2);
                    }
                    idx2++;
                }
                if (possibleCombinaisons.Count != 0)
                {
                    toReturn.Add(idx1, possibleCombinaisons.ToArray());
                    Console.Write(".");
                }
                idx1++;
            }
            return toReturn;
        }
        static bool PosOnColorDot(GameBoard gameBoard, Pos pos, Line currentLine)
        {
            foreach(var line in gameBoard.Lines)
            {
                if (currentLine == line) 
                    continue;
                if (line.StartPos.x == pos.x && line.StartPos.y == pos.y)
                    return true;
                if (line.EndPos.x == pos.x && line.EndPos.y == pos.y)
                    return true;

                if (line.MandatoryStart.Contains(pos))
                    return true;
                if (line.MandatoryEnd.Contains(pos))
                    return true;
            }
            return false;
        }
        static bool PosObstructs(GameBoard gameBoard, Pos pos, Line currentLine)
        {
            int[] dx1 = { 0, 1, 0, -1 };
            int[] dy1 = { -1, 0, 1, 0 };

            for (int i = 0; i < 4; i++)
            {
                HashSet<Pos> neededToObstruct = new();

                short nextX = (short)(pos.x + dx1[i]);
                short nextY = (short)(pos.y + dy1[i]);
                Pos nextPos = new(nextX, nextY);

                // If outside then skip
                if (!(nextX >= 0 && nextX < gameBoard.size.Width && nextY >= 0 && nextY < gameBoard.size.Height))
                    continue;
                if (currentLine.CurrentTrajectory.Contains(nextPos))
                    continue;

                for (int n = 0; n < 4; n++)
                {
                    short nextX1 = (short)(nextX + dx1[n]);
                    short nextY1 = (short)(nextY + dy1[n]);
                    Pos nextPos1 = new(nextX1, nextY1);

                    // Check if the next position is within the bounds of the game board
                    if (nextX1 >= 0 && nextX1 < gameBoard.size.Width && nextY1 >= 0 && nextY1 < gameBoard.size.Height)
                    {
                        if (PosOnColorDot(gameBoard, nextPos1, null))
                            continue;
                        neededToObstruct.Add(nextPos1);
                    }
                }
                if (neededToObstruct.Except(currentLine.CurrentTrajectory).Count() <= 1)
                {
                    return true;
                }
            }

            /*

            foreach (var line in gameBoard.Lines)
            {
                if (currentLine == line)
                    continue;


                // Check all possible dots it could be obstructing
                int[] dx = { 0, 1, 0, -1 };
                int[] dy = { -1, 0, 1, 0 };

                for (int i = 0; i < 4; i++)
                {
                    if (line.MandatoryStart.Count == 0)
                    {
                        if (line.StartPos.x + dx[i] == pos.x && line.StartPos.y + dy[i] == pos.y)
                        {
                            HashSet<Pos> neededToObstruct = new();

                            int[] dx1 = { 0, 1, 0, -1 };
                            int[] dy1 = { -1, 0, 1, 0 };

                            for (int n = 0; n < 4; n++)
                            {
                                short nextX = (short)(line.StartPos.x + dx1[n]);
                                short nextY = (short)(line.StartPos.y + dy1[n]);
                                Pos nextPos = new(nextX, nextY);

                                // Check if the next position is within the bounds of the game board
                                if (nextX >= 0 && nextX < gameBoard.size.Width && nextY >= 0 && nextY < gameBoard.size.Height)
                                {
                                    if (PosOnColorDot(gameBoard, nextPos, line))
                                        continue;
                                    neededToObstruct.Add(nextPos);
                                }
                            }
                            if (neededToObstruct.Except(currentLine.CurrentTrajectory).Count() - 1 <= 0)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (line.MandatoryStart.Last().x + dx[i] == pos.x && line.MandatoryStart.Last().y + dy[i] == pos.y)
                        {
                            HashSet<Pos> neededToObstruct = new();

                            int[] dx1 = { 0, 1, 0, -1 };
                            int[] dy1 = { -1, 0, 1, 0 };

                            for (int n = 0; n < 4; n++)
                            {
                                short nextX = (short)(line.MandatoryStart.Last().x + dx1[n]);
                                short nextY = (short)(line.MandatoryStart.Last().y + dy1[n]);
                                Pos nextPos = new(nextX, nextY);

                                // Check if the next position is within the bounds of the game board
                                if (nextX >= 0 && nextX < gameBoard.size.Width && nextY >= 0 && nextY < gameBoard.size.Height)
                                {
                                    if (PosOnColorDot(gameBoard, nextPos, line))
                                        continue;
                                    neededToObstruct.Add(nextPos);
                                }
                            }
                            if (neededToObstruct.Except(currentLine.CurrentTrajectory).Count() - 1 <= 0)
                            {
                                return true;
                            }
                        }
                    }
                    if (line.MandatoryEnd.Count == 0)
                    {
                        if (line.EndPos.x + dx[i] == pos.x && line.EndPos.y + dy[i] == pos.y)
                        {
                            HashSet<Pos> neededToObstruct = new();

                            int[] dx1 = { 0, 1, 0, -1 };
                            int[] dy1 = { -1, 0, 1, 0 };

                            for (int n = 0; n < 4; n++)
                            {
                                short nextX = (short)(line.EndPos.x + dx1[n]);
                                short nextY = (short)(line.EndPos.y + dy1[n]);
                                Pos nextPos = new(nextX, nextY);

                                // Check if the next position is within the bounds of the game board
                                if (nextX >= 0 && nextX < gameBoard.size.Width && nextY >= 0 && nextY < gameBoard.size.Height)
                                {
                                    if (PosOnColorDot(gameBoard, nextPos, line))
                                        continue;
                                    neededToObstruct.Add(nextPos);
                                }
                            }
                            if (neededToObstruct.Except(currentLine.CurrentTrajectory).Count() - 1 <= 0)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (line.MandatoryEnd.Last().x + dx[i] == pos.x && line.MandatoryEnd.Last().y + dy[i] == pos.y)
                        {
                            HashSet<Pos> neededToObstruct = new();

                            int[] dx1 = { 0, 1, 0, -1 };
                            int[] dy1 = { -1, 0, 1, 0 };

                            for (int n = 0; n < 4; n++)
                            {
                                short nextX = (short)(line.MandatoryEnd.Last().x + dx1[n]);
                                short nextY = (short)(line.MandatoryEnd.Last().y + dy1[n]);
                                Pos nextPos = new(nextX, nextY);

                                // Check if the next position is within the bounds of the game board
                                if (nextX >= 0 && nextX < gameBoard.size.Width && nextY >= 0 && nextY < gameBoard.size.Height)
                                {
                                    if (PosOnColorDot(gameBoard, nextPos, line))
                                        continue;
                                    neededToObstruct.Add(nextPos);
                                }
                            }
                            if (neededToObstruct.Except(currentLine.CurrentTrajectory).Count() - 1 <= 0)
                            {
                                return true;
                            }
                        }
                    }
                }

            }
            */
            return false;
        }
        private Bitmap GenerateGameBoardBitmap(GameBoard gameBoard)
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
        private Bitmap DrawSolution(GameBoard gameBoard)
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
        private void DrawPoint(Graphics g, Pos pos, Color color, int cellSize, int size)
        {
            int x = pos.x * cellSize + cellSize / 2;
            int y = pos.y * cellSize + cellSize / 2;
            int pointSize = size; // Adjust as needed
            g.FillEllipse(new SolidBrush(color), x - pointSize / 2, y - pointSize / 2, pointSize, pointSize);
        }
    }
}
