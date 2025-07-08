using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;

    [SerializeField] private GameObject[] piecePrefabs;

    private Dictionary<string, GameObject> pieceTypes;
    private Dictionary<string, List<Piece>> capturedPlayer1, capturedPlayer2;

    public Piece[,] Board { get; private set; }
    public Piece SelectedPiece { get; set; }

    public bool Busy { get; set; }

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(this);
            return;
        }

        pieceTypes = new Dictionary<string, GameObject>();
        foreach (var tilePrefab in piecePrefabs)
            pieceTypes.Add(tilePrefab.name[..tilePrefab.name.IndexOf(' ')], tilePrefab);

        capturedPlayer1 = new Dictionary<string, List<Piece>>();
        capturedPlayer2 = new Dictionary<string, List<Piece>>();
        foreach (var type in pieceTypes.Keys)
        {
            capturedPlayer1.Add(type, new List<Piece>());
            capturedPlayer2.Add(type, new List<Piece>());
        }

        Board = new Piece[9, 9];
        SetupPieces();
    }

    // Set up the pieces on the board
    private void SetupPieces()
    {
        AddPiece("Gyokushou", 4, 0, false, false);
        AddPiece("Oushou", 4, 8, false, true);


        for (byte x = 0; x < 9; x++)
            AddPiece("Fuhyou", x, 2);

        AddPiece("Kyousha", 0, 0);
        AddPiece("Keima", 1, 0);
        AddPiece("Ginshou", 2, 0);
        AddPiece("Kinshou", 3, 0);
        AddPiece("Kinshou", 5, 0);
        AddPiece("Ginshou", 6, 0);
        AddPiece("Keima", 7, 0);
        AddPiece("Kyousha", 8, 0);

        AddPiece("Hisha", 7, 1);
        AddPiece("Kakugyou", 1, 1);
    }

    // Add a piece to the board for both players
    private void AddPiece(string type, byte x, byte y, bool bothPlayers = true, bool player2Only = false)
    {
        pieceTypes.TryGetValue(type, out var prefab);
        if (prefab == null)
            return;

        var piece1 = Instantiate(prefab).GetComponent<Piece>();
        piece1.gameObject.transform.SetParent(transform);
        piece1.Setup(type, x, y, player2Only && !bothPlayers);
        Board[x, y] = piece1;

        if (bothPlayers)
        {
            byte x2 = (byte)(8 - x);
            byte y2 = (byte)(8 - y);
            var piece2 = Instantiate(prefab).GetComponent<Piece>();
            piece2.gameObject.transform.SetParent(transform);
            piece2.Setup(type, x2, y2, true);
            Board[x2, y2] = piece2;
        }
    }

    public void TryAndDrop(int x, int y)
    {
        if (SelectedPiece.LegalMoves.Contains(new Vector2Int(x, y)))
            DropPiece(x, y);
    }

    public void TryAndCapture(Piece toCapture)
    {
        if (SelectedPiece.LegalMoves.Contains(new Vector2Int((int)toCapture.transform.position.x, (int)toCapture.transform.position.y)))
            CapturePiece(toCapture);
    }

    public void TryAndMove(int x, int y)
    {
        if (SelectedPiece.LegalMoves.Contains(new Vector2Int(x, y)))
            MovePiece(x, y);
    }

    // Drops the selected piece to an empty position on the board
    // Assumes the move to be legal
    private void DropPiece(int x, int y)
    {
        Busy = true;
        (SelectedPiece.IsPlayer2() ? capturedPlayer2 : capturedPlayer1)[SelectedPiece.Type]
            .Remove(SelectedPiece);
        Board[x, y] = SelectedPiece;

        SelectedPiece.SetRenderingOrder(1000);
        var moving = SelectedPiece;
        _ = SelectedPiece.transform.DOMove(new Vector3(x, y, 0f), 0.5f)
            .OnComplete(() =>
            {
                moving.SetRenderingOrder(0);
                UpdateReachForAll();
                Busy = false;
            });

        SelectedPiece.DeselectPiece();
    }

    // Moves the selected piece to capture the given piece
    // Assumes the move to be legal
    private void CapturePiece(Piece toCapture)
    {
        Busy = true;
        var hand = SelectedPiece.IsPlayer2() ? capturedPlayer2 : capturedPlayer1;
        hand[toCapture.Type].Add(toCapture);
        
        int x = (int)toCapture.transform.position.x,
            y = (int)toCapture.transform.position.y;
        toCapture.SetRenderingOrder(10 * hand[toCapture.Type].Count);
        var seq = DOTween.Sequence();
        seq.Join(toCapture.transform.DOMove(BoardGrid.GetPositionWhenCaptured(toCapture, SelectedPiece.IsPlayer2()), 0.5f));// TODO find location for captured pieces
        seq.Join(toCapture.transform.DORotate(new Vector3(0, 0, SelectedPiece.IsPlayer2() ? 180 : 0), 0.5f, RotateMode.FastBeyond360));// TODO find location for captured pieces
        seq.OnComplete(() => { Busy = false; });
        MovePiece(x, y);
    }

    // Moves the selected piece to an empty position on the board
    // Assumes the move to be legal
    private void MovePiece(int x, int y)
    {
        Busy = true;
        Board[(int)SelectedPiece.transform.position.x, (int)SelectedPiece.transform.position.y] = null;
        Board[x, y] = SelectedPiece;

        SelectedPiece.SetRenderingOrder(1000);
        var moving = SelectedPiece;
        _ = SelectedPiece.transform.DOMove(new Vector3(x, y, 0f), 0.5f)
            .OnComplete(() =>
            {
                moving.SetRenderingOrder(0);
                UpdateReachForAll();
                Busy = false;
            });

        SelectedPiece.DeselectPiece();
    }

    public void UpdateReachForAll()
    {
        foreach (var piece in Board)
            if (piece != null)
                piece.UpdateReach();
        foreach (var type in capturedPlayer1.Values)
            foreach (var piece in type)
                piece.UpdateReach();
        foreach (var type in capturedPlayer2.Values)
            foreach (var piece in type)
                piece.UpdateReach();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateReachForAll();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
