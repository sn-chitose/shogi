using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KifuManager : MonoBehaviour
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
                parts.Add(((char)(8 - end.x + '０')).ToString());
                parts.Add(((char)(8 - end.y + '０')).ToString());
            }

            // piece type
            parts.Add(type switch
            {
                "Fuhyou" => moving.Promoted ? "と" : "歩",
                "Kyousha" => moving.Promoted ? "成香" : "香",
                "Keima" => moving.Promoted ? "成桂" : "桂",
                "Ginshou" => moving.Promoted ? "成銀" : "銀",
                "Kinshou" => "金",
                "Kakugyou" => moving.Promoted ? "角" : "馬",
                "Hisha" => moving.Promoted ? "飛" : "龍",
                "Gyokushou" => "玉",
                "Oushou" => "王",
                _ => "?"
            });

            // all other pieces that could be the moving piece without extra information
            var candidates = BoardManager.instance.Board
                .Cast<Piece>().Where(piece => piece != null &&
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
            List<string> parts = new() { $"{number / 2}. ", number % 2 == 1 ? "☖" : "☗" };

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
                piece.IsPlayer2() == moving.IsPlayer2() &&
                piece.Promoted == moving.Promoted &&
                MoveManager.GetReach(piece).Contains(end) &&
                !(piece.transform.position.x == start.x &&
                piece.transform.position.y == start.y)))
            {
                parts.Add($"{8 - start.x}{8 - start.y}");
            }

            parts.Add(drop ? "'" :
                capturedType != null ? "x" : "");

            if (MoveManager.IsPromotable(moving, end.x, end.y))
            {
                parts.Add(promoting ? "+" : "=");
            }

            return string.Join("", parts);
        }
    }

    public static KifuManager instance;

    private List<KifuMove> kifu;

    public int MoveNumber { get; private set; }

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(this);
            return;
        }

        kifu = new();
        MoveNumber = 0;
    }

    public void AddMove(Piece moving, Piece toCapture, bool promoting = false)
    {
        AddMove(moving, (int)toCapture.transform.position.x, (int)toCapture.transform.position.y, promoting, toCapture);
    }

    public void AddMove(Piece moving, int endX, int endY, bool drop = false, bool promoting = false, Piece toCapture = null)
    {
        if (MoveNumber != kifu.Count)
            kifu.RemoveRange(MoveNumber, kifu.Count - MoveNumber);

        kifu.Add(new KifuMove(MoveNumber, moving, endX, endY, drop, promoting, toCapture));
        MoveNumber++;
    }

    public bool UndoMove()
    {
        if (MoveNumber > 0)
        {
            var move = kifu[MoveNumber];
            var board = BoardManager.instance;
            var piece = board.Board[move.end.x, move.end.y];

            if (move.IsDrop)
            {
                bool isPlayer2 = MoveNumber % 2 == 1;
                piece = board.Board[move.end.x, move.end.y];
                board.Board[move.end.x, move.end.y] = null;

                var capturedList = (isPlayer2 ? board.CapturedPlayer2 : board.CapturedPlayer1)[move.type];
                capturedList.Add(piece);
                piece.transform.position = BoardGrid.GetPositionWhenCaptured(piece, isPlayer2);

                piece.SetRenderingOrder(10 * capturedList.Count);
            }
            else
            {
                board.Board[move.start.x, move.start.y] = piece;
                piece.transform.position.Set(move.start.x, move.start.y, 0f);
                if (move.promoting)
                    piece.Promoted = false;

                if (move.capturedType != null)
                {
                    bool isPlayer2 = MoveNumber % 2 == 1;
                    var capturedList = (isPlayer2 ? board.CapturedPlayer2 : board.CapturedPlayer1)[move.capturedType];
                    var capturedPiece = capturedList[-1];
                    capturedList.RemoveAt(-1);
                    board.Board[move.end.x, move.end.y] = capturedPiece;

                    capturedPiece.transform.position.Set(move.end.x, move.end.y, 0f);
                    capturedPiece.transform.Rotate(0f, 0f, 180f - capturedPiece.transform.rotation.eulerAngles.z);
                    capturedPiece.SetRenderingOrder(0);

                    if (move.promotedCapture)
                        capturedPiece.Promoted = true;
                }
            }

            board.SelectedPiece.DeselectPiece();
            board.UpdateReachForAll();

            MoveNumber--;
            return true;
        }
        return false;
    }

    public bool RedoMove()
    {
        if (MoveNumber < kifu.Count)
        {
            var move = kifu[MoveNumber];
            var board = BoardManager.instance;
            var piece = board.Board[move.start.x, move.start.y];
            piece.SelectPiece();

            if (move.IsDrop)
                board.DropPiece(move.end.x, move.end.y);
            else if (move.capturedType != null)
                board.CapturePiece(board.Board[move.end.x, move.end.y], move.promoting);
            else
                board.MovePiece(move.end.x, move.end.y, move.promoting);

            MoveNumber++;
            return true;
        }
        return false;
    }
}
