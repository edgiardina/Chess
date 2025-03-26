
namespace Chess
{
    public partial class MainPage : ContentPage
    {
        private ChessBoard _game;  // or ChessGame, if the library calls it that
        private Position selectedPosition = new Position();

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

                        pieceLabel.GestureRecognizers.Add(tapGesture);

                        ChessBoard.Add(pieceLabel, col, row);
                    }
                }
            }

        }

        private void OnSquareTapped(object sender, EventArgs e)
        {
            int row, col;
            BoxView tappedBox = null;

            // if we tapped a label, we tapped on a "piece"
            if (sender is Label label)
            {
                row = Grid.GetRow(label);
                col = Grid.GetColumn(label);

                tappedBox = ChessBoard.Children
                                        .OfType<BoxView>() 
                                        .FirstOrDefault(view =>
                                            Grid.GetRow(view) == row &&
                                            Grid.GetColumn(view) == col);
            }
            else // we tapped on an empty square
            {
                // This is if you want to handle "square-only" tapping
                tappedBox = (BoxView)sender;
                row = Grid.GetRow(tappedBox);
                col = Grid.GetColumn(tappedBox);
                // Possibly handle selecting a piece or making a move
            }

            var positionPressed = new Position((short)col, (short)(7 - row));

            if (selectedPosition.HasValue)
            {
                // if you pressed the same piece again, unselect it
                if (selectedPosition == positionPressed)
                {
                    selectedPosition = new Position();
                    BuildChessBoardUI();
                    return;
                }

                // if is valid move
                var isValidMove = _game.IsValidMove(new Move(selectedPosition, positionPressed));

                if (isValidMove)
                {
                    // move the piece
                    _game.Move(new Move(selectedPosition, positionPressed));

                    // update the UI
                    BuildChessBoardUI();

                    // a new position is always HasValue false (-1, -1)
                    selectedPosition = new Position();
                }
                else
                {
                    // show a dialog because it's an invalid move
                    DisplayAlert("Invalid Move", "Invalid move, try again", "OK");
                }
            }
            else
            {
                // if there's a piece at this square
                var pieceAtPosition = _game[positionPressed];

                if (pieceAtPosition != null)
                {
                    // and the piece is the right color (it's your turn)
                    if (pieceAtPosition.Color == _game.Turn)
                    {
                        selectedPosition = positionPressed;

                        // highlight the selected square
                        tappedBox.BackgroundColor = Colors.Yellow;
                    }
                    // else show a dialog because its not your turn
                    else
                    {
                        DisplayAlert("Invalid Move", $"It's not your turn, it's {_game.Turn}'s turn", "OK");
                    }
                }
            }

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
