
namespace Chess
{
    public partial class MainPage : ContentPage
    {
        private ChessBoard _game;  // or ChessGame, if the library calls it that

        public MainPage()
        {
            InitializeComponent();

            // 1) Create the game
            _game = new ChessBoard();
            // 2) Load the start position or FEN, etc.


            // 3) Build your 8x8 board UI, using the library’s Board info
            BuildChessBoardUI();
        }

        private void BuildChessBoardUI()
        {
            // We assume you have Grid x:Name="ChessBoard" in XAML
            ChessBoard.RowDefinitions.Clear();
            ChessBoard.ColumnDefinitions.Clear();
            ChessBoard.Children.Clear();

            // Define 8 rows/cols
            for (int i = 0; i < 8; i++)
            {
                ChessBoard.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                ChessBoard.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // For each square in the library’s board, draw the square color
            // Then, if there's a piece on that square, display it.
            // The library might have something like _game.Board[x, y] or getPiece(row,col).
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var lightSquareColor = Color.FromArgb("#F0D9B5");
                    var darkSquareColor = Color.FromArgb("#B58863");

                    var squareColor = ((row + col) % 2 == 0)
                                        ? lightSquareColor
                                        : darkSquareColor;

                    var squareBoxView = new BoxView { Color = squareColor };

                    // Add tap handling for moves
                    var tapGesture = new TapGestureRecognizer();
                    tapGesture.Tapped += OnSquareTapped;
                    squareBoxView.GestureRecognizers.Add(tapGesture);

                    ChessBoard.Add(squareBoxView, col, row);

                    var pieceAtPosition = _game[col, 7 - row];

                    if (pieceAtPosition != null)
                    {

                        // Now see if there's a piece at [row, col]
                        // The library’s code might differ. Often it’s row-first or col-first.
                        // Check the docs or source for how to retrieve a piece. Example:
                        var pieceLabel = new Label
                        {
                            Text = MapPieceToSymbol(pieceAtPosition),
                            FontSize = 32, // or whatever size looks good
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center,
                            TextColor = pieceAtPosition.Color == PieceColor.White ? Colors.White : Colors.Black

                        };

                        //// Optional: Add tap gesture
                        //var tapGesture = new TapGestureRecognizer();
                        //tapGesture.Tapped += OnPieceTapped;
                        //pieceLabel.GestureRecognizers.Add(tapGesture);

                        ChessBoard.Add(pieceLabel, col, row);
                    }
                }
            }

        }

        private void OnSquareTapped(object sender, EventArgs e)
        {
            // This is if you want to handle "square-only" tapping
            var tappedBox = (BoxView)sender;
            int row = Grid.GetRow(tappedBox);
            int col = Grid.GetColumn(tappedBox);
            // Possibly handle selecting a piece or making a move
        }

        private void OnPieceTapped(object sender, EventArgs e)
        {
            // This is if you want direct piece-tapping logic
            var tappedImage = (Image)sender;
            int row = Grid.GetRow(tappedImage);
            int col = Grid.GetColumn(tappedImage);
            // Then pass to your library’s Move or selection logic
        }

        private static readonly Dictionary<object, string> WhiteSymbols = new()
        {
            { PieceType.Pawn,   "♙" },
            { PieceType.Rook,   "♖" },
            { PieceType.Knight, "♘" },
            { PieceType.Bishop, "♗" },
            { PieceType.Queen,  "♕" },
            { PieceType.King,   "♔" },
        };

        private static readonly Dictionary<object, string> BlackSymbols = new()
        {
            { PieceType.Pawn,   "♟" },
            { PieceType.Rook,   "♜" },
            { PieceType.Knight, "♞" },
            { PieceType.Bishop, "♝" },
            { PieceType.Queen,  "♛" },
            { PieceType.King,   "♚" },
        };

        private string MapPieceToSymbol(Piece piece)
        {
            if (piece.Color == PieceColor.White)
                return WhiteSymbols.TryGetValue(piece.Type, out var symW) ? symW : "?";
            else
                return BlackSymbols.TryGetValue(piece.Type, out var symB) ? symB : "?";
        }


        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            // Force the container to be square, based on the smaller dimension
            double size = Math.Min(width, height);
            ChessBoardContainer.WidthRequest = size;
            ChessBoardContainer.HeightRequest = size;
        }



    }

}
