using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

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
        public override string ToString()
        {
            return $"x{this.x}y{this.y}";
        }
    }
    public class Fill
    {
        public byte[,] grid;
        public Size size;
        public Fill(Line line, Size size)
        {
            // Fill from line
            this.size = size;
            grid = new byte[size.Width, size.Height];

            grid[line.StartPos.x, line.StartPos.y] = 0x01;
            grid[line.EndPos.x, line.EndPos.y] = 0x01;

            foreach (var pos in line.CurrentTrajectory)
            {
                grid[pos.x, pos.y] = 0x01;
            }
            foreach (var pos in line.MandatoryStart)
            {
                grid[pos.x, pos.y] = 0x01;
            }
            foreach (var pos in line.MandatoryEnd)
            {
                grid[pos.x, pos.y] = 0x01;
            }
        }
        public Fill(Size size)
        {
            this.size = size;
            grid = new byte[size.Width, size.Height];
        }
        public Fill(Gameboard gameboard)
        {
            this.size = gameboard.size;
            grid = new byte[size.Width, size.Height];

            foreach (Line line in gameboard.Lines)
                this.UnionWith(new Fill(line, size));
        }
        public void UnionWith(Fill otherFill)
        {
            for (short iy = 0; iy < size.Width; iy++)
            {
                for (short ix = 0; ix < size.Height; ix++)
                {
                    grid[ix, iy] |= otherFill.grid[ix, iy];
                }
            }
        }
        public void UnionWith(HashSet<Pos> trajectory)
        {
            foreach (var pos in trajectory)
            {
                grid[pos.x, pos.y] = 0x01;
            }
        }
        public bool OverlapsWith(Fill otherFill)
        {
            for (short iy = 0; iy < size.Width; iy++)
            {
                for (short ix = 0; ix < size.Height; ix++)
                {
                    if ((grid[ix, iy] + otherFill.grid[ix, iy]) != 0)
                        return true;
                }
            }
            return false;
        }
        public bool OverlapAndMerge(Fill otherFill)
        {
            byte[,] newGrid = new byte[size.Width, size.Height];
            //Returns true if it overlaps
            for (short iy = 0; iy < size.Width; iy++)
            {
                for (short ix = 0; ix < size.Height; ix++)
                {
                    if ((grid[ix, iy] & otherFill.grid[ix, iy]) != 0)
                        return true;
                    newGrid[ix, iy] = (byte)(grid[ix, iy] | otherFill.grid[ix, iy]);
                }
            }
            grid = newGrid;
            return false;
        }
        public bool isComplete()
        {
            //Returns true if it overlaps
            for (short iy = 0; iy < size.Width; iy++)
            {
                for (short ix = 0; ix < size.Height; ix++)
                {
                    if (grid[ix, iy] == 0)
                        return false;
                }
            }
            return true;

        }
        public string ToString()
        {
            string str = "";
            for (short iy = 0; iy < size.Width; iy++)
            {
                for (short ix = 0; ix < size.Height; ix++)
                {
                    str += grid[ix, iy] == 0 ? "X" : "0";
                }
                str += "\n";
            }
            return str;
        }
    }
    public class Line
    {
        public Pos StartPos;
        public Pos EndPos;
        public Color Color;
        public List<HashSet<Pos>> PossibleTrajectories;
        public HashSet<Pos> CurrentTrajectory;
        public Fill CurrentFill;
        public List<Pos> MandatoryStart;
        public List<Pos> MandatoryEnd;
        public Line(Pos start, Pos end, Color color)
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
    public class Gameboard
    {
        public List<Line> Lines;
        public Size size;
        public Fill MandatoryFill;
    }
    internal class Solver(PictureBox pictureBox)
    {
        private Bitmap gameBoardBitmap;
        private PictureBox pictureBox = pictureBox;

        public bool SolveGameBoard(Gameboard gameBoard)
        {

            foreach (var line in gameBoard.Lines)
            {
                line.MandatoryStart = new();
                line.MandatoryEnd = new();
                line.CurrentTrajectory = new();
                line.PossibleTrajectories = new();
                line.CurrentFill = new(line, gameBoard.size);
            }

            bool foundPath = true;
            int gen = 0;
            gameBoard.MandatoryFill = new Fill(gameBoard);
            while (foundPath)
            {
                foundPath = false;
                foreach (var line in gameBoard.Lines)
                {
                    if (FindMandatoryPath(gameBoard, line, gen))
                    {
                        foundPath = true;
                        gameBoard.MandatoryFill.UnionWith(new Fill(line, gameBoard.size)); //Add the new fill to the gameboard
                    }
                }
                gen++;
            }
            foreach (var line in gameBoard.Lines)
            {
                Console.WriteLine($"Found {line.MandatoryStart.Count} mandatory start points for color {line.Color.Name}");
                Console.WriteLine($"Found {line.MandatoryEnd.Count} mandatory end points for color {line.Color.Name}");
            }

            gameBoardBitmap = Logger.DrawSolution(gameBoard);
            gameBoardBitmap.Save($"firstpass.png");
            //Console.WriteLine(gameBoard.MandatoryFill.ToString());


            // Start DFS from each valid start position
            foreach (var line in gameBoard.Lines)
            {
                Pos start = line.MandatoryStart.Count != 0 ? line.MandatoryStart.Last() : line.StartPos;
                Pos end = line.MandatoryEnd.Count != 0 ? line.MandatoryEnd.Last() : line.EndPos;
                DFS(gameBoard, start, end, line);
                foreach (var line1 in gameBoard.Lines)
                {
                    line1.CurrentTrajectory = new();
                }

                Console.WriteLine($"Found {line.PossibleTrajectories.Count} paths for color {line.Color.Name}");

                if (line.PossibleTrajectories.Count < 100)
                {
                    if (!Directory.Exists(line.Color.Name))
                    {
                        Directory.CreateDirectory(line.Color.Name);
                    }
                    int i = 0;
                    foreach (var traj in line.PossibleTrajectories)
                    {
                        line.CurrentTrajectory = traj;
                        gameBoardBitmap = Logger.DrawSolution(gameBoard);
                        gameBoardBitmap.Save($"{line.Color.Name}/solution{i}.png");
                        i++;
                    }
                }


            }

            {
                /*

                int lines = gameBoard.Lines.Count();
                Dictionary<int, int[]>[] values = new Dictionary<int, int[]>[lines];
                for (int i1 = 0; i1 < lines - 1; i1++)
                {
                    for (int i2 = 0; i2 < lines - 1; i2++)
                    {
                        CheckLines(gameBoard, gameBoard.Lines[i1], gameBoard.Lines[i2]);
                        Console.WriteLine($"{i1} -> {i2}");
                    }
                }
                */





                Console.WriteLine($"Making tree");
                int lines = gameBoard.Lines.Count();
                Dictionary<int, int[]>[] values = new Dictionary<int, int[]>[lines];
                for (int i = 0; i < lines - 1; i++)
                {
                    values[i] = CheckLines(gameBoard, gameBoard.Lines[i], gameBoard.Lines[i + 1]);
                    Console.WriteLine($"{i} -> {i + 1}");
                }
                values[lines - 1] = CheckLines(gameBoard, gameBoard.Lines.Last(), gameBoard.Lines[0]);
                Console.WriteLine($"{lines - 1} -> {0}");
                Console.WriteLine($"Done tree");

                /*
                for (int i = 0; i < gameBoard.Lines.Count() - 1; i++)
                {
                    Console.WriteLine($"Tree branch {gameBoard.Lines[i].Color.Name} and {gameBoard.Lines[i + 1].Color.Name}");
                    foreach (var dict in values[i])
                    {
                        Console.WriteLine($"Number {dict.Key}");
                        Console.WriteLine($"Gos with {string.Join(",", dict.Value)}");
                    }
                }
                */

                if (checkBranch(gameBoard, 0, values, -1))
                {
                    Console.WriteLine("Found solution !");

                    gameBoardBitmap = Logger.DrawSolution(gameBoard);
                    pictureBox.Image = gameBoardBitmap;
                    gameBoardBitmap.Save("boardSolved.png");
                    return true;
                }
                else
                {
                    Console.WriteLine("Cant find a solution");
                    return false;
                }
            }
            
        }
        static bool checkBranch(Gameboard gameboard, int branchID, Dictionary<int, int[]>[] values, int check)
        {
            Dictionary<int, int[]> branch = values[branchID];
            foreach (var key in branch.Keys)
            {
                if (check == -1 || key == check)
                {
                    gameboard.Lines[branchID].CurrentTrajectory = gameboard.Lines[branchID].PossibleTrajectories[key];
                    foreach (var sub in branch[key])
                    {
                        if (branchID == values.Length - 1)
                        {
                            Fill full = new(gameboard.size);
                            foreach (var line in gameboard.Lines)
                            {
                                line.CurrentFill = new Fill(line, gameboard.size);
                                if (full.OverlapAndMerge(line.CurrentFill))
                                {
                                    return false;
                                }
                            }
                            full.UnionWith(gameboard.MandatoryFill);
                            return full.isComplete();
                        }
                        else
                        {
                            if (checkBranch(gameboard, branchID + 1, values, sub))
                                return true;
                        }
                    }
                }
            }
            return false;
        }
        // returns true if it found a path
        static bool FindMandatoryPath(Gameboard gameBoard, Line currentLine, int gen)
        {
            bool foundPath = false;

            HashSet<Pos> PossibleWays = new();
            short[] dx1 = { 0, 1, 0, -1 };
            short[] dy1 = { -1, 0, 1, 0 };
            bool backToItself;

            if (currentLine.MandatoryStart.Count == 0)
            {
                PossibleWays = new();

                backToItself = false;
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
                PossibleWays = new();

                backToItself = false;
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
                PossibleWays = new();

                backToItself = false;
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
                PossibleWays = new();

                backToItself = false;
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
        static void DFS(Gameboard gameBoard, Pos currentPos, Pos targetPos, Line currentLine)
        {
            // Add current position to the current path
            currentLine.CurrentTrajectory.Add(currentPos);

            // If current position is the target position, add the current path to allPaths
            if (!PosObstructs(gameBoard, currentPos, currentLine, out bool hasNext, out Pos mandatoryNext))
            {
                if (hasNext)
                {
                    DFS(gameBoard, mandatoryNext, targetPos, currentLine);
                }
                else if (!PosNextToItself(gameBoard, currentPos, currentLine, targetPos) && !CheckForCutting(gameBoard, currentPos, currentLine))
                {
                    // Move in all possible directions (up, right, down, left)
                    short[] dx = { 0, 1, 0, -1 };
                    short[] dy = { -1, 0, 1, 0 };

                    for (int i = 0; i < 4; i++)
                    {
                        short nextX = (short)(currentPos.x + dx[i]); //TODO try remove cast
                        short nextY = (short)(currentPos.y + dy[i]);

                        // Check if the next position is within the bounds of the game board
                        if (nextX < 0 || nextX >= gameBoard.size.Width || nextY < 0 || nextY >= gameBoard.size.Height)
                            continue;

                        Pos nextPos = new(nextX, nextY);
                        if (!nextPos.Equals(targetPos))
                        {
                            if (currentLine.CurrentTrajectory.Contains(nextPos))
                                continue;
                            if (PosOnColorDot(gameBoard, nextPos, null, true))
                                continue;
                        }
                        else
                        {
                            currentLine.PossibleTrajectories.Add(new(currentLine.CurrentTrajectory));
                            break;
                        }
                        // Recursive call to explore the next position
                        DFS(gameBoard, nextPos, targetPos, currentLine);
                    }
                }
            }

            // Remove current position from the current path to backtrack
            currentLine.CurrentTrajectory.Remove(currentPos);
        }
        static Dictionary<int, int[]> CheckLines(Gameboard gameBoard, Line line1, Line line2)
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
            foreach (var path1 in line1.PossibleTrajectories)
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
        static bool PosOnColorDot(Gameboard gameBoard, Pos pos, Line currentLine, bool ignoreCurrentTraj = false)
        {
            if (gameBoard.MandatoryFill.grid[pos.x, pos.y] != 0)
                return true;
            if (!ignoreCurrentTraj)
            {
                foreach (var line in gameBoard.Lines)
                {
                    if (currentLine == line)
                        continue;
                    if (line.CurrentTrajectory.Contains(pos))
                        return true;
                }
            }
            return false;
        }
        static bool PosObstructs(Gameboard gameBoard, Pos pos, Line currentLine, out bool hasMandatoryNextPos, out Pos mandatoryNextPos)
        {
            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { -1, 0, 1, 0 };

            hasMandatoryNextPos = false;
            mandatoryNextPos = new Pos();
            HashSet<Pos> SingleHoles = new();

            for (int i = 0; i < 4; i++)
            {

                short nextX = (short)(pos.x + dx[i]);
                short nextY = (short)(pos.y + dy[i]);
                Pos nextPos = new(nextX, nextY);

                // If outside then skip
                if (nextX < 0 || nextX >= gameBoard.size.Width || nextY < 0 || nextY >= gameBoard.size.Height)
                    continue;
                if (currentLine.CurrentTrajectory.Contains(nextPos))
                    continue;
                if (currentLine.MandatoryEnd.Contains(nextPos))
                    continue;
                if (currentLine.MandatoryStart.Contains(nextPos))
                    continue;
                if (currentLine.StartPos.Equals(nextPos)) 
                    continue;
                if (currentLine.EndPos.Equals(nextPos)) 
                    continue;
                if (PosOnColorDot(gameBoard, nextPos, null, true))
                    continue;

                //Check if the pos is empty and has at least two exits
                HashSet<Pos> neededToObstruct = new();
                for (int n = 0; n < 4; n++)
                {
                    short nextX1 = (short)(nextX + dx[n]);
                    short nextY1 = (short)(nextY + dy[n]);
                    Pos nextPos1 = new(nextX1, nextY1);

                    if (nextX1 < 0 || nextX1 >= gameBoard.size.Width || nextY1 < 0 || nextY1 >= gameBoard.size.Height)
                        continue;
                    if (currentLine.CurrentTrajectory.Contains(nextPos1))
                        continue;
                    if (currentLine.MandatoryStart.Contains(nextPos))
                        continue;
                    neededToObstruct.Add(nextPos1);
                }
                if (neededToObstruct.Count() < 2)
                {
                    if (neededToObstruct.Count == 1)
                        SingleHoles.Add(nextPos);
                    else
                        return true;
                }
            }
            if (SingleHoles.Count == 0)
                return false;
            else if (SingleHoles.Count == 1)
            {
                hasMandatoryNextPos = true;
                mandatoryNextPos = SingleHoles.First();
                return false;
            }
            else
                return false;
        }
        static bool CheckForCutting(Gameboard gameboard, Pos pos, Line currentLine)
        {
            // Start DFS from each valid start position
            Fill currentFill = new Fill(currentLine, gameboard.size);
            foreach (var line in gameboard.Lines)
            {
                if (line == currentLine) continue;

                if (line.CurrentTrajectory.Count == 0 || line.CurrentTrajectory.Contains(pos))
                {
                    line.CurrentTrajectory = new();
                    Pos start = line.MandatoryStart.Count != 0 ? line.MandatoryStart.Last() : line.StartPos;
                    Pos end = line.MandatoryEnd.Count != 0 ? line.MandatoryEnd.Last() : line.EndPos;
                    if (!DFS_Reach(gameboard, start, end, line, currentFill)) //If it cant reach
                        return true;
                    //line.CurrentTrajectory = new();
                }
            }
            return false;
        }
        static bool DFS_Reach(Gameboard gameBoard, Pos currentPos, Pos targetPos, Line currentLine, Fill currentFill)
        {
            // Add current position to the current path
            currentLine.CurrentTrajectory.Add(currentPos);

            // Move in all possible directions (up, right, down, left)
            short[] dx = { 0, 1, 0, -1 };
            short[] dy = { -1, 0, 1, 0 };

            for (int i = 0; i < 4; i++)
            {
                short nextX = (short)(currentPos.x + dx[i]); //TODO try remove cast
                short nextY = (short)(currentPos.y + dy[i]);

                // Check if the next position is within the bounds of the game board
                if (nextX < 0 || nextX >= gameBoard.size.Width || nextY < 0 || nextY >= gameBoard.size.Height)
                    continue;

                Pos nextPos = new(nextX, nextY);
                if (!nextPos.Equals(targetPos))
                {
                    if (currentLine.CurrentTrajectory.Contains(nextPos))
                        continue;
                    if (PosOnColorDot(gameBoard, nextPos, null, true))
                        continue;
                    if (currentFill.grid[nextPos.x, nextPos.y] != 0)
                        continue;
                    // Recursive call to explore the next position
                    if (DFS_Reach(gameBoard, nextPos, targetPos, currentLine, currentFill))
                        return true;
                }
                else
                    return true;
            }
            // Remove current position from the current path to backtrack
            currentLine.CurrentTrajectory.Remove(currentPos);

            return false;
        }
        static bool PosNextToItself(Gameboard gameBoard, Pos pos, Line currentLine, Pos targetPos)
        {
            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { -1, 0, 1, 0 };

            bool isNextTo = false;

            if (currentLine.CurrentTrajectory.Count < 3)
                return false;

            for (int i = 0; i < 4; i++)
            {
                short nextX = (short)(pos.x + dx[i]);
                short nextY = (short)(pos.y + dy[i]);
                Pos nextPos = new(nextX, nextY);

                // If outside then skip
                if (nextX < 0 || nextX >= gameBoard.size.Width || nextY < 0 || nextY >= gameBoard.size.Height)
                    continue;
                if (currentLine.CurrentTrajectory.TakeLast(2).Contains(nextPos))
                    continue;
                if (nextPos.Equals(targetPos))
                    continue;
                if (currentLine.CurrentTrajectory.SkipLast(2).Contains(nextPos))
                {
                    isNextTo = true;
                    break;
                }
                if (currentLine.MandatoryEnd.Contains(nextPos))
                {
                    isNextTo = true;
                    break;
                }
                if (currentLine.MandatoryStart.Contains(nextPos))
                {
                    isNextTo = true;
                    break;
                }
            }
            return isNextTo;
        }
    }
}
