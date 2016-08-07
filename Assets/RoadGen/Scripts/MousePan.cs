using UnityEngine;

//////////////////////////////////////////////////////////////////////////////////////////
// source: http://answers.unity3d.com/questions/614174/pan-orthographic-camera.html
//////////////////////////////////////////////////////////////////////////////////////////
public class MousePan : MonoBehaviour
{
    public int button = 2;
    public float sensitivityX = 0.5f;
    public float sensitivityY = 0.5f;

    void Update()
    {
        if (!Input.GetMouseButton(button))
            return;
        float y = Input.GetAxis("Mouse Y"), x = Input.GetAxis("Mouse X");
        if (x != 0)
            Camera.main.transform.Translate(Vector3.left * (x * sensitivityX));
        if (y != 0)
            Camera.main.transform.Translate(Vector3.down * (y * sensitivityY));
    }

}
