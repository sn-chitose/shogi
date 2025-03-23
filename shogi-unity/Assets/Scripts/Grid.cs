using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private Material gridMaterial;

    void Awake()
    {
        var gridPoints = Enumerable.Range(0, 10)
            .Select(i => i - 0.5f)
            .ToArray();

        foreach(var point in gridPoints)
        {
            DrawLine(point, gridPoints.First(), point, gridPoints.Last());
            DrawLine(gridPoints.First(), point, gridPoints.Last(), point);
        }
    }

    private void DrawLine(float x1, float y1, float x2, float y2)
    {
        GameObject lineObject = new("Line");
        lineObject.transform.SetParent(transform);
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.material = gridMaterial;
        lineRenderer.startWidth = 0.04f;
        lineRenderer.endWidth = 0.04f;
        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, new Vector3(x1, y1, 0));
        lineRenderer.SetPosition(1, new Vector3(x2, y2, 0));
    }

    void OnMouseDown()
    {
        if (BoardManager.instance.Busy)
            return;

        // If a piece is selected, try to move it to this empty position
        if (BoardManager.instance.SelectedPiece != null)
        {
            var cursor = GameObject.Find("Cursor").transform.position;
            BoardManager.instance.TryAndMove((int)cursor.x, (int)cursor.y);
        }
    }
}
