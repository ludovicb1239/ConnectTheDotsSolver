using System.Drawing.Design;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Windows.Forms;

namespace ConnectTheDotsSolver
{
    public partial class Form1 : Form
    {
        private PictureBox pictureBox;
        Solver solver;
        Reader reader;
        Player player;
        private Bitmap gameBoardBitmap;
        public Form1()
        {
            InitializeComponent();
            InitializePictureBox();
            solver = new Solver(pictureBox);
            reader = new Reader();
            player = new Player();
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
            Gameboard board;
            if (false){
                board = new();

                /*
                board.size = new(5, 5);
                List<Line> lines = new List<Line>();
                lines.Add(new Line(new Pos(0, 0), new Pos(4, 3), Color.Yellow));
                lines.Add(new Line(new Pos(0, 3), new Pos(4, 4), Color.Blue));
                lines.Add(new Line(new Pos(1, 3), new Pos(2, 2), Color.Green));
                lines.Add(new Line(new Pos(0, 4), new Pos(2, 3), Color.Red));
                */


                //board.size = new(9, 9);
                //List<Line> lines = new List<Line>();
                //lines.Add(new Line(new Pos(0, 0), new Pos(8, 4), Color.Red));
                //lines.Add(new Line(new Pos(0, 1), new Pos(1, 8), Color.Blue));
                //lines.Add(new Line(new Pos(0, 2), new Pos(3, 3), Color.Yellow));
                //lines.Add(new Line(new Pos(1, 5), new Pos(8, 6), Color.DeepPink));
                //lines.Add(new Line(new Pos(2, 5), new Pos(4, 7), Color.Pink));
                //lines.Add(new Line(new Pos(3, 5), new Pos(7, 7), Color.Gray));
                //lines.Add(new Line(new Pos(5, 2), new Pos(5, 5), Color.Green));
                //lines.Add(new Line(new Pos(2, 1), new Pos(7, 6), Color.DarkOrange));
                //lines.Add(new Line(new Pos(7, 1), new Pos(8, 5), Color.Orange));



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



                //board.size = new(7, 7);
                //List<Line> lines = new List<Line>();
                //lines.Add(new Line(new Pos(3, 3), new Pos(6, 3), Color.Red));
                //lines.Add(new Line(new Pos(3, 4), new Pos(6, 4), Color.Blue));
                //lines.Add(new Line(new Pos(2, 3), new Pos(2, 5), Color.Yellow));
                //lines.Add(new Line(new Pos(6, 1), new Pos(1, 5), Color.Orange));
                //lines.Add(new Line(new Pos(4, 0), new Pos(5, 6), Color.Green));
                //lines.Add(new Line(new Pos(1, 1), new Pos(6, 0), Color.Cyan));
                //lines.Add(new Line(new Pos(3, 5), new Pos(6, 6), Color.Pink));

                board.Lines = lines;
            }
            else
            {
                Vector2 topRight = new(1080, 166);
                Vector2 boardSize = new(715, 715);
                int sides = 10;
                reader.topLeftCorner = topRight;
                reader.bottomRightCorner = topRight + boardSize;
                reader.sizeX = sides;
                reader.sizeY = sides;
                player.topLeftCorner = topRight;
                player.bottomRightCorner = topRight + boardSize;
                player.sizeX = sides;
                player.sizeY = sides;
                board = reader.ScanGameboard();
            }

            gameBoardBitmap = Logger.GenerateGameBoardBitmap(board);
            pictureBox.Image = gameBoardBitmap;
            gameBoardBitmap.Save("board.png");

            bool solved = solver.SolveGameBoard(board);

            if (solved)
                player.Play(board);
        }
    }
}
