using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public string Type { get; private set; }
    public bool Promoted { get; private set; }

    public List<Vector2Int> Reach { get; private set; }

    public bool IsPlayer2()
    {
        return transform.rotation.eulerAngles.z == 180f;
    }

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
        Reach = MoveManager.GetReach(this);
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
