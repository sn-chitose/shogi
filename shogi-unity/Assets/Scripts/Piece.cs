using UnityEngine;

public class Piece : MonoBehaviour
{
    private string type;
    private bool isPlayer2;
    private bool promoted;

    public void Setup(string type, bool isPlayer2)
    {
        if (type == null)
        {
            this.type = type;
            this.isPlayer2 = isPlayer2;
            promoted = false;
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
