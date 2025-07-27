using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public partial class KifuManager : MonoBehaviour
{
    public static KifuManager instance;

    private List<KifuMove> kifu;

    [SerializeField] private TMP_Dropdown dropdown;

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

    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += locale =>
        {
            dropdown.options.Clear();
            foreach (var move in kifu)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(locale.Identifier.Code switch
                {
                    "ja" => move.captionJP,
                    "en" => move.captionEN,
                    _ => move.captionJP
                }));
                dropdown.RefreshShownValue();
            }
        };
    }

    public void AddMove(Piece moving, Piece toCapture, bool promoting = false)
    {
        AddMove(moving, (int)toCapture.transform.position.x, (int)toCapture.transform.position.y, false, promoting, toCapture);
    }

    public void AddMove(Piece moving, int endX, int endY, bool drop = false, bool promoting = false, Piece toCapture = null)
    {
        if (MoveNumber != kifu.Count)
        {
            kifu.RemoveRange(MoveNumber, kifu.Count - MoveNumber);
            dropdown.options.RemoveRange(MoveNumber, kifu.Count - MoveNumber);
        }

        kifu.Add(new KifuMove(MoveNumber, moving, endX, endY, drop, promoting, toCapture));
        dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationSettings.SelectedLocale.Identifier.Code switch
        {
            "ja" => kifu.Last().captionJP,
            "en" => kifu.Last().captionEN,
            _ => kifu.Last().captionJP
        }));
        dropdown.value = MoveNumber;
        dropdown.RefreshShownValue();
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
