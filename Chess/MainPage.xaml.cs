
using Chess.Stockfish;

namespace Chess
{
    public partial class MainPage : ContentPage
    {
        private ChessBoard _game;
        private Position selectedPosition = new Position();
        private readonly StockfishClient stockfishClient = new StockfishClient();

        private bool boardDrawn = false;

        public MainPage()
        {
            InitializeComponent();

            // 1) Create the game state object 
            _game = new ChessBoard();            

            BuildChessBoardUI();
        }

        private void BuildChessBoardUI()
        {
            Console.WriteLine("Building board UI");

            // We assume you have Grid x:Name="ChessBoard" in XAML
            // remove all pieces from the chess grid
            ChessBoard.Children
                .OfType<Label>()
                .ToList()
                .ForEach(label => ChessBoard.Children.Remove(label));

            if (!boardDrawn)
            {
                // Define 8 rows/cols
                for (int i = 0; i < 8; i++)
                {
                    ChessBoard.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    ChessBoard.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }
            }

            // For each square in the library’s board, draw the square color
            // Then, if there's a piece on that square, display it.
            // The library might have something like _game.Board[x, y] or getPiece(row,col).
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var tapGesture = new TapGestureRecognizer();

                    if (!boardDrawn)
                    {
                        Color squareColor = DetermineSquareColor(row, col);

                        var squareBoxView = new BoxView { Color = squareColor };

                        // Add tap handling for moves

                        tapGesture.Tapped += OnSquareTapped;
                        squareBoxView.GestureRecognizers.Add(tapGesture);

                        ChessBoard.Add(squareBoxView, col, row);
                    }

                    var pieceAtPosition = _game[col, 7 - row];

                    if (pieceAtPosition != null)
                    {

                        // Now see if there's a piece at [row, col]
                        // The library’s code might differ. Often it’s row-first or col-first.
                        // Check the docs or source for how to retrieve a piece. Example:
                        var pieceLabel = new Label
                        {
                            Text = MapPieceToSymbol(pieceAtPosition),
                            FontSize = 48, // or whatever size looks good
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center,
                            TextColor = pieceAtPosition.Color == PieceColor.White ? Colors.White : Colors.Black
                        };

                        ChessBoard.Add(pieceLabel, col, row);
                    }
                }
            }

            TurnLabel.Text = $"Turn: {_game.Turn}";
            // split executed moves by newline
            MovesLabel.Text = $"Moves: \n{string.Join(Environment.NewLine, _game.ExecutedMoves)}";

            boardDrawn = true;
        }

        private static Color DetermineSquareColor(int row, int col)
        {
            var lightSquareColor = Color.FromArgb("#F0D9B5");
            var darkSquareColor = Color.FromArgb("#B58863");

            var squareColor = ((row + col) % 2 == 0)
                                ? lightSquareColor
                                : darkSquareColor;
            return squareColor;
        }

        private async void OnSquareTapped(object sender, EventArgs e)
        {
            Console.WriteLine("Tapped!");

            int row, col;
            BoxView tappedBox = null;

            tappedBox = (BoxView)sender;
            row = Grid.GetRow(tappedBox);
            col = Grid.GetColumn(tappedBox);

            var positionPressed = new Position((short)col, (short)(7 - row));

            if (selectedPosition.HasValue)
            {
                // if you pressed the same piece again, unselect it
                if (selectedPosition == positionPressed)
                {
                    selectedPosition = new Position();
                    BuildChessBoardUI();

                    // remove tappedBox highlight
                    tappedBox.Color = DetermineSquareColor(row, col);
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

                    // remove tappedBox highlight
                    ResetSquareColor();

                    // a new position is always HasValue false (-1, -1)
                    selectedPosition = new Position();

                    // check if Checkmate happened
                    if (_game.IsEndGame)
                    {
                        // show a dialog because it's an invalid move
                        await DisplayAlert("Game Over", $"{_game.EndGame.WonSide} wins!", "OK");
                        _game.Clear();
                        BuildChessBoardUI();
                    } 
                    else
                    {
                        await ComputerPlayerTurn();
                    }
                }
                else
                {
                    // show a dialog because it's an invalid move
                    await DisplayAlert("Invalid Move", "Invalid move, try again", "OK");
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
                        tappedBox.Color = Colors.Yellow;
                    }
                    // else show a dialog because its not your turn
                    else
                    {
                        await DisplayAlert("Invalid Move", $"It's not your turn, it's {_game.Turn}'s turn", "OK");
                    }
                }
            }
        }

        private async Task ComputerPlayerTurn()
        {
            var bestMoveGivenGameState = await stockfishClient.GetBestMoveAsync(_game.ToFen(), 12);

            var bestMove = bestMoveGivenGameState.ParsedBestMove.Move;

            var from = bestMove.Substring(0, 2); // "g8"
            var to = bestMove.Substring(2, 2); // "f6"

            var move = new Move(from, to);

            _game.Move(move);

            // update the UI
            BuildChessBoardUI();
        }

        private void ResetSquareColor()
        {
            // Let's call it "gamePos" for the library's position
            // gamePos.Column = col
            // gamePos.Row    = 7 - row

            var uiCol = selectedPosition.X;
            // We invert the row back:
            var uiRow = 7 - selectedPosition.Y;

            // Among all children in the grid, pick out the BoxView in row/col
            var boxToUnhighlight = ChessBoard.Children
                .OfType<BoxView>()
                .FirstOrDefault(b =>
                    Grid.GetRow(b) == uiRow &&
                    Grid.GetColumn(b) == uiCol);

            if (boxToUnhighlight != null)
            {
                boxToUnhighlight.Color = DetermineSquareColor(uiRow, uiCol);
            }
        }

        private static readonly Dictionary<object, string> Symbols = new()
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
            return Symbols.TryGetValue(piece.Type, out var symB) ? symB : "?";
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
