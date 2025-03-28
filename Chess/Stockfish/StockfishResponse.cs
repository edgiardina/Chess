using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Chess.Stockfish
{
    public class StockfishResponse
    {
        // 1) These properties match the JSON fields exactly:
        //    "success", "evaluation", "mate", "bestmove", "continuation".
        //    The serializer can bind them to the constructor parameters below.

        public bool Success { get; set; }
        public double? Evaluation { get; set; }
        public int? Mate { get; set; }

        // We'll store the raw "bestmove" and "continuation" in these auto-properties
        // so they bind automatically. 
        public string Bestmove { get; set; }
        public string Continuation { get; set; }

        // 2) After deserialization, we want these "parsed" properties:
        [JsonIgnore]
        public BestMove ParsedBestMove { get; private set; }

        [JsonIgnore]
        public List<string> ContinuationMoves { get; private set; } = new List<string>();

        // 3) This constructor matches the EXACT property names in case-insensitive manner:
        [JsonConstructor]
        public StockfishResponse(
            bool success,
            double? evaluation,
            int? mate,
            string bestmove,
            string continuation)
        {
            // Assign them so System.Text.Json sees them as "bound".
            // This ensures we don't get the mismatch error.
            Success = success;
            Evaluation = evaluation;
            Mate = mate;
            Bestmove = bestmove;
            Continuation = continuation;

            // Now parse them into nicer forms:
            ParsedBestMove = ParseBestmove(Bestmove);
            ContinuationMoves = ParseContinuation(Continuation);
        }

        private BestMove ParseBestmove(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            // e.g. "bestmove g8f6 ponder g1f3"
            var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            // -> ["bestmove", "g8f6", "ponder", "g1f3"]

            var result = new BestMove();

            if (parts.Length >= 2 && parts[0].Equals("bestmove", StringComparison.OrdinalIgnoreCase))
            {
                result.Move = parts[1];  // "g8f6"
            }
            if (parts.Length >= 4 && parts[2].Equals("ponder", StringComparison.OrdinalIgnoreCase))
            {
                result.Ponder = parts[3]; // "g1f3"
            }
            return result;
        }

        private List<string> ParseContinuation(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new List<string>();

            // e.g. "g8f6 g1f3 e7e6 e2e3 ..."
            return raw.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }

}
