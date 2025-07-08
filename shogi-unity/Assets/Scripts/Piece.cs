using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] private GameObject reachablePrefab;

    public string Type { get; private set; }
    public bool Promoted { get; private set; }

    public List<Vector2Int> Reach { get; private set; }
    public List<Vector2Int> Droppable { get; private set; }
    public List<Vector2Int> LegalMoves { get; private set; }

    public bool IsPlayer2() => transform.rotation.eulerAngles.z == 180f;

    public bool IsHand() => transform.position.x % 1 != 0;

    public bool IsKing() => Type switch
    {
        "Gyokushou" or "Oushou" => true,
        _ => false,
    };

    public void Setup(string type, byte x, byte y, bool isPlayer2)
    {
        if (Type == null)
        {
            Type = type;
            transform.position = new Vector3(x, y, 0f);
            Promoted = false;
            if (isPlayer2)
                transform.Rotate(0f, 0f, 180f);
        }
    }

    public void UpdateReach()
    {
        if (IsHand())
        {
            Reach.Clear();
            Droppable = MoveManager.GetDroppable(this);
        }
        else
            Reach = MoveManager.GetReach(this);

        if (LegalMoves == null)
            LegalMoves = new List<Vector2Int>();
        else
            LegalMoves.Clear();
    }

    public void SelectPiece()
    {
        BoardManager.instance.SelectedPiece = this;
        transform.Find("SelectionCursor").gameObject.SetActive(true);

        var candidates = IsHand() ? Droppable : Reach;
        foreach (var reachable in candidates)
        {
            if (MoveManager.IsCheckAfterMove(this, reachable))
                continue;
            if (IsHand() && MoveManager.IsCheckmateAfterDropFuhyou(this, reachable))
                continue;

            LegalMoves.Add(reachable);
            var marker = Instantiate(reachablePrefab);
            marker.transform.parent = transform;
            marker.transform.position = new Vector3(reachable.x, reachable.y, 0f);
        }
    }

    public void DeselectPiece()
    {
        foreach (Transform child in transform)
            if (child.gameObject.name == "Reachable(Clone)")
                Destroy(child.gameObject);
        BoardManager.instance.SelectedPiece = null;
        transform.Find("SelectionCursor").gameObject.SetActive(false);
    }

    void OnMouseDown()
    {
        if (BoardManager.instance.Busy)
            return;

        if (BoardManager.instance.SelectedPiece == null)
            SelectPiece();
        else if (BoardManager.instance.SelectedPiece == this)
            DeselectPiece();
        else if (IsPlayer2() == BoardManager.instance.SelectedPiece.IsPlayer2())
        {
            BoardManager.instance.SelectedPiece.DeselectPiece();
            SelectPiece();
        }
        else
            BoardManager.instance.TryAndCapture(this);
    }

    // Set the rendering order of the whole piece and disable selection cursor
    public void SetRenderingOrder(int order)
    {
        var selectionCursor = transform.Find("SelectionCursor");
        selectionCursor.gameObject.SetActive(false);
        selectionCursor.GetComponent<SpriteRenderer>().sortingOrder = order;

        GetComponent<SpriteRenderer>().sortingOrder = order + 1;

        transform.Find("Front").GetComponent<SpriteRenderer>().sortingOrder = order + 2;
        var back = transform.Find("Back");
        if (back != null)
            back.GetComponent<SpriteRenderer>().sortingOrder = order + 2;
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
