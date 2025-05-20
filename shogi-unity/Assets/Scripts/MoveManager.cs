using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveManager
{
    // Checks whether the own king is in check after the specified move
    // Assumes the target position is in the reach of the moving piece
    public static bool IsCheckAfterMove(Piece piece, Vector2Int target)
    {
        var start = new Vector2Int((int)piece.transform.position.x, (int)piece.transform.position.y);
        foreach (var opponent in BoardManager.instance.Board)
            if (opponent != null && opponent.IsPlayer2() != piece.IsPlayer2())
            {
                var opponentPosition = new Vector2Int((int)opponent.transform.position.x, (int)opponent.transform.position.y);

                // Skip this opponent piece because it is captured
                if (opponentPosition == target)
                    continue;

                // Check for opponent step moves only if the moving piece is the king itself
                // Other movements cannot make the king in check by step moves
                if (piece.IsKing())
                    if (opponent.Reach.Contains(target) || StepList(opponent).Select(position => position + opponentPosition).Contains(target))
                        return true;

                var directions = MoveDirections(opponent);
                foreach (var direction in directions)
                    for (var tempPosition = opponentPosition + direction;
                        tempPosition.x >= 0 && tempPosition.x < 9 && tempPosition.y >= 0 && tempPosition.y < 9;
                        tempPosition += direction)
                    {
                        if (tempPosition == start)
                            continue;

                        if (tempPosition == target)
                        {
                            if (piece.IsKing())
                                return true;
                            break;
                        }

                        var reachable = BoardManager.instance.Board[tempPosition.x, tempPosition.y];
                        if (reachable != null)
                        {
                            if (reachable.IsPlayer2() == piece.IsPlayer2() && reachable.IsKing())
                                return true;
                            break;
                        }
                    }
            }
        return false;
    }

    // Positions where a piece can be dropped from the hand
    // Excludes last rows and same column Fuhyou, but does not evaluate checkmate by Fuhyou drop
    public static List<Vector2Int> GetDroppable(Piece piece)
    {
        List<Vector2Int> candidates = new();
        for (byte x = 0; x < 9; x++)
        {
            if (piece.Type == "Fuhyou")
            {
                bool found = false;
                for (byte y = 0; y < 9; y++)
                {
                    var p = BoardManager.instance.Board[x, y];
                    if (p != null && p.Type == "Fuhyou"
                        && p.IsPlayer2() == piece.IsPlayer2())
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                    continue;
            }

            for (byte y = 0; y < 9; y++)
                if (BoardManager.instance.Board[x, y] == null)
                {
                    switch (piece.Type)
                    {
                        case "Fuhyou":
                        case "Kyousha":
                            if (y == (piece.IsPlayer2() ? 0 : 8))
                                continue;
                            break;
                        case "Keima":
                            if (y == (piece.IsPlayer2() ? 0 : 8)
                                || y == (piece.IsPlayer2() ? 1 : 7))
                                continue;
                            break;
                    }
                    candidates.Add(new Vector2Int(x, y));
                }
        }
        return candidates;
    }

    // Positions inside the reach of a piece.
    // Legal moves for standard movement, without checking special case rules.
    public static List<Vector2Int> GetReach(Piece piece)
    {
        Vector2Int startPosition = new((int)piece.transform.position.x, (int)piece.transform.position.y);
        var candidates = StepList(piece)
            .Select(step => startPosition + step)
            .Where(position => position.x >= 0 && position.x < 9 && position.y >= 0 && position.y < 9)
            .Where(position =>
            {
                var check = BoardManager.instance.Board[position.x, position.y];
                return check == null || check.IsPlayer2() != piece.IsPlayer2();
            })
            .ToList();

        MoveDirections(piece).ForEach(direction =>
        {
            for (var tempPosition = startPosition + direction;
                tempPosition.x >= 0 && tempPosition.x < 9 && tempPosition.y >= 0 && tempPosition.y < 9;
                tempPosition += direction)
            {
                var check = BoardManager.instance.Board[tempPosition.x, tempPosition.y];
                if (check != null)
                {
                    if (check.IsPlayer2() != piece.IsPlayer2())
                        candidates.Add(tempPosition);
                    break;
                }
                else
                {
                    candidates.Add(tempPosition);
                }
            }
        });
        return candidates;
    }

    // List of single step (or jump) moves by piece type, 
    private static List<Vector2Int> StepList(Piece piece)
    {
        List<Vector2Int> moves;
        switch (piece.Type)
        {
            case "Hisha" when piece.Promoted:
                return new List<Vector2Int>() { new(-1, 1), new(1, 1), new(-1, -1), new(1, -1) };
            case "Kakugyou" when piece.Promoted:
                return new List<Vector2Int>() { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            case "Fuhyou" when piece.Promoted:
            case "Kyousha" when piece.Promoted:
            case "Keima" when piece.Promoted:
            case "Ginshou" when piece.Promoted:
            case "Kinshou":
                moves = new List<Vector2Int>() { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, new(-1, 1), new(1, 1) };
                break;
            case "Fuhyou":
                moves = new List<Vector2Int>() { Vector2Int.up };
                break;
            case "Keima":
                moves = new List<Vector2Int>() { new(-1, 2), new(1, 2) };
                break;
            case "Ginshou":
                moves = new List<Vector2Int>() { Vector2Int.up, new(-1, 1), new(1, 1), new(-1, -1), new(1, -1) };
                break;
            case "Gyokushou":
            case "Oushou":
                return new List<Vector2Int>() { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, new(-1, 1), new(1, 1), new(-1, -1), new(1, -1) };
            case "Kyousha":
            case "Hisha":
            case "Kakugyou":
            default:
                return new List<Vector2Int>();
        }

        return piece.IsPlayer2() ? moves.Select(move => -move).ToList() : moves;
    }

    // List of long move directions by piece type
    private static List<Vector2Int> MoveDirections(Piece piece) => piece.Type switch
    {
        "Hisha" => new List<Vector2Int>() { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right },
        "Kakugyou" => new List<Vector2Int>() { new(-1, 1), new(1, 1), new(-1, -1), new(1, -1) },
        "Kyousha" when !piece.Promoted => new List<Vector2Int>() { piece.IsPlayer2() ? Vector2Int.down : Vector2Int.up },
        _ => new List<Vector2Int>(),
    };
}
