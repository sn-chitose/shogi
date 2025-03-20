using UnityEngine;

public class Piece : MonoBehaviour
{
    private bool promoted;

    public string Type { get; private set; }

    bool IsPlayer2()
    {
        return transform.rotation.eulerAngles.z == 180f;
    }

    public void Setup(string type, byte x, byte y, bool isPlayer2)
    {
        if (Type == null)
        {
            Type = type;
            transform.position = new Vector3(x, y, 0f);
            promoted = false;
            if (isPlayer2)
                transform.Rotate(0f, 0f, 180f);
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
