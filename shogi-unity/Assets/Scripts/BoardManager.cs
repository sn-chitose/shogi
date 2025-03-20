using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;

    [SerializeField] private GameObject[] piecePrefabs;

    private Dictionary<string, GameObject> pieceTypes;

    private Piece[,] board; // bottom left as (0, 0)
    private Dictionary<string, List<Piece>> capturedPlayer1, capturedPlayer2;

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

        board = new Piece[9, 9];
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
        piece1.Setup(type, player2Only && !bothPlayers);
        board[x, y] = piece1;

        if (bothPlayers)
        {
            byte x2 = (byte)(8 - x);
            byte y2 = (byte)(8 - y);
            var piece2 = Instantiate(prefab).GetComponent<Piece>();
            piece2.gameObject.transform.SetParent(transform);
            piece2.Setup(type, true);
            board[x2, y2] = piece2;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
