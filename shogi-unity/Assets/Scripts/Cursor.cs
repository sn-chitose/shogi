using UnityEngine;

public class Cursor : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float x = Mathf.Floor(mousePosition.x + 0.5f);
        float y = Mathf.Floor(mousePosition.y + 0.5f);

        if (x >= 0 && x < 9 && y >= 0 && y < 9)
        {
            transform.position = new Vector3(x, y, 0);
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
        }
        else
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }
}
