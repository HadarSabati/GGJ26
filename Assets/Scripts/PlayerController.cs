using UnityEngine;
public class PlayerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))

            //if (Mouse.current.leftButton.isPressed)
        {

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            //Camera.main.ScreenToWorldPoint() function to translate the mouse’s position from screen space to a point in the game world
            //Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.value);
            //Debug.Log("Mouse position: " + mousePos);
        }
    }
}