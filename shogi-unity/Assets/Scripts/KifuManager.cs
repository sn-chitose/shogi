using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class KifuManager : MonoBehaviour
{
    private class KifuMove
    {
        public string type;
        public Vector2Int start, end;
        public bool promoting, promotedCapture;
        public string capturedType;
        public string captionJP, captionEN;

        public bool IsDrop => start == null;

        public KifuMove()
        {
            // TODO
        }
    }

    public static KifuManager instance;

    private List<KifuMove> kifu;

    public int MoveNumber { get; private set; }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(this);
            return;
        }

        kifu = new();
    }

    public void AddMove()
    {
        if (MoveNumber != kifu.Count)
            kifu.RemoveRange(MoveNumber, kifu.Count - MoveNumber);

        kifu.Add(new KifuMove());
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
