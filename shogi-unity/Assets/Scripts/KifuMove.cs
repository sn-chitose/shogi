using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class KifuManager
{
    private class KifuMove
    {
        public readonly int number;
        public readonly string type;
        public readonly Vector2Int start, end;
        public readonly bool drop, promoting, promotedCapture;
        public readonly string capturedType;
        public readonly string captionJP, captionEN;

        public bool IsDrop => start == null;

        public KifuMove(int moveNumber, Piece moving, int endX, int endY, bool droppingMove, bool promotingMove, Piece toCapture)
        {
            number = moveNumber;
            type = moving.Type;
            start = new Vector2Int((int)moving.transform.position.x, (int)moving.transform.position.y);
            end = new Vector2Int(endX, endY);
            drop = droppingMove;
            promoting = promotingMove;
            promotedCapture = toCapture && toCapture.Promoted;
            capturedType = toCapture ? toCapture.Type : null;
            captionJP = WriteNotationJP(moving);
            captionEN = WriteNotationEN(moving);
        }

        public string WriteNotationJP(Piece moving)
        {
            List<string> parts = new() { number % 2 == 1 ? "☖" : "☗" };

            // target position
            if (number > 0 && end == instance.kifu[number - 1].end)
            {
                parts.Add("同");
            }
            else
            {
                parts.Add(((char)(9 - end.x + '０')).ToString());
                parts.Add(((char)(9 - end.y + '０')).ToString());
            }

            // piece type
            parts.Add(type switch
            {
                "Fuhyou" => moving.Promoted ? "と" : "歩",
                "Kyousha" => moving.Promoted ? "成香" : "香",
                "Keima" => moving.Promoted ? "成桂" : "桂",
                "Ginshou" => moving.Promoted ? "成銀" : "銀",
                "Kinshou" => "金",
                "Kakugyou" => moving.Promoted ? "馬" : "角",
                "Hisha" => moving.Promoted ? "龍" : "飛",
                "Gyokushou" => "玉",
                "Oushou" => "王",
                _ => "?"
            });

            // all other pieces that could be the moving piece without extra information
            var candidates = BoardManager.instance.Board
                .Cast<Piece>().Where(piece => piece != null &&
                piece.Type == type &&
                piece.IsPlayer2() == moving.IsPlayer2() &&
                piece.Promoted == moving.Promoted &&
                MoveManager.GetReach(piece).Contains(end) &&
                !(piece.transform.position.x == start.x &&
                piece.transform.position.y == start.y));

            // if not unique
            if (candidates.Count() > 0)
            {
                if (drop)
                {
                    parts.Add("打");
                }
                else
                {
                    // relative placement
                    var placementMoving = (start.x - end.x)
                        * (moving.IsPlayer2() ? -1 : 1);
                    var movementMoving = start.y.CompareTo(end.y)
                        * (moving.IsPlayer2() ? -1 : 1);
                    var placements = candidates.Select(piece => (piece.transform.position.x - end.x)
                        * (moving.IsPlayer2() ? -1 : 1));
                    var movements = candidates.Select(piece => piece.transform.position.y.CompareTo(end.y)
                        * (moving.IsPlayer2() ? -1 : 1));

                    string movementString = movementMoving switch
                    {
                        >= 1 => "引",
                        0 => "寄",
                        <= -1 => "上",
                    };

                    if (type is "Kakugyou" or "Hisha")
                    {
                        parts.Add(movementMoving != movements.First() ? movementString :
                            placementMoving > placements.First() ? "右" : "左");
                    }
                    else
                    {
                        // stepper
                        string placementString = placementMoving switch
                        {
                            1 => "右",
                            0 => "直",
                            -1 => "左",
                            _ => "？",
                        };
                        if (!movements.Contains(movementMoving))
                        {
                            parts.Add(movementString);
                        }
                        else
                        {
                            parts.Add(placementString);
                            if (placements.Contains(placementMoving))
                                parts.Add(movementString);
                        }
                    }
                }
            }

            if (MoveManager.IsPromotable(moving, end.x, end.y))
            {
                parts.Add(promoting ? "成" : "不成");
            }
            return string.Join("", parts);
        }

        public string WriteNotationEN(Piece moving)
        {
            List<string> parts = new() { $"{number / 2 + 1}. " };

            if (moving.Promoted)
            {
                parts.Add("+");
            }
            parts.Add(type switch
            {
                "Fuhyou" => "P",
                "Kyousha" => "L",
                "Keima" => "N",
                "Ginshou" => "S",
                "Kinshou" => "G",
                "Kakugyou" => "B",
                "Hisha" => "R",
                "Gyokushou" or "Oushou" => "K",
                _ => "?"
            });

            // if not unique - start position
            if (BoardManager.instance.Board
                .Cast<Piece>().Any(piece => piece != null &&
                piece.Type == type &&
                piece.IsPlayer2() == moving.IsPlayer2() &&
                piece.Promoted == moving.Promoted &&
                MoveManager.GetReach(piece).Contains(end) &&
                !(piece.transform.position.x == start.x &&
                piece.transform.position.y == start.y)))
            {
                parts.Add($"{9 - start.x}{9 - start.y}");
            }

            parts.Add(drop ? "'" :
                capturedType != null ? "x" : "");

            parts.Add($"{9 - end.x}{9 - end.y}");

            if (MoveManager.IsPromotable(moving, end.x, end.y))
            {
                parts.Add(promoting ? "+" : "=");
            }

            return string.Join("", parts);
        }
    }
}
