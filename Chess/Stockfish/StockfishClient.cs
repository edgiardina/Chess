using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Chess.Stockfish
{

    public class StockfishClient
    {
        private const string ApiUrl = "https://stockfish.online/api/s/v2.php";
        private readonly HttpClient _httpClient;

        public StockfishClient(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        /// <summary>
        /// Asks the Stockfish Online API (via GET) for the best move given a FEN and a search depth.
        /// </summary>
        /// <param name="fen">The chess position in FEN notation.</param>
        /// <param name="depth">Desired search depth.</param>
        /// <returns>A StockfishResponse with bestmove, evaluation, etc.</returns>
        public async Task<StockfishResponse> GetBestMoveAsync(string fen, int depth)
        {
            // Build the query string
            // e.g. https://stockfish.online/api/s/v2.php?fen=xxx&depth=15
            var url = $"{ApiUrl}?fen={Uri.EscapeDataString(fen)}&depth={depth}";

            // Issue GET
            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode(); // throws if status != 2xx

            // Read the JSON response
            var jsonString = await response.Content.ReadAsStringAsync();

            // Deserialize
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<StockfishResponse>(jsonString, options);

            return result;
        }
    }

}
