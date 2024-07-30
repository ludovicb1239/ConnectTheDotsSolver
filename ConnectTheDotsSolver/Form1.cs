using System.Numerics;

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
            Vector2 topLeft = new(1080, 166);
            Vector2 boardSize = new(715, 715);
            int sides = 10;

            reader.topLeftCorner = topLeft;
            reader.bottomRightCorner = topLeft + boardSize;
            reader.sizeX = sides;
            reader.sizeY = sides;
            player.topLeftCorner = topLeft;
            player.bottomRightCorner = topLeft + boardSize;
            player.sizeX = sides;
            player.sizeY = sides;
            board = reader.ScanGameboard();

            gameBoardBitmap = Logger.GenerateGameBoardBitmap(board);
            pictureBox.Image = gameBoardBitmap;
            gameBoardBitmap.Save("board.png");

            Thread thread = new Thread(new ThreadStart(() => {
                bool solved = solver.SolveGameBoard(board);

                if (solved)
                    player.Play(board);
            }));
            thread.Name = "Solver";
            thread.Start();
        }
    }
}
