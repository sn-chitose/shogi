using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveManager
{
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

        var directions = MoveDirections(piece);
        foreach (var direction in directions)
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
        }
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
    private static List<Vector2Int> MoveDirections(Piece piece)
    {
        return piece.Type switch
        {
            "Hisha" => new List<Vector2Int>() { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right },
            "Kakugyou" => new List<Vector2Int>() { new(-1, 1), new(1, 1), new(-1, -1), new(1, -1) },
            "Kyousha" when !piece.Promoted => new List<Vector2Int>() { piece.IsPlayer2() ? Vector2Int.up : Vector2Int.down },
            _ => new List<Vector2Int>(),
        };
    }
}
