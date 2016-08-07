using UnityEngine;
using System.Collections;

//////////////////////////////////////////////////////////////////////////////////////////
// source: http://answers.unity3d.com/questions/20228/mouse-wheel-zoom.html
//////////////////////////////////////////////////////////////////////////////////////////
public class MouseWheelZoom : MonoBehaviour
{
    public float orthographicSizeMin = 1;
    public float orthographicSizeMax = 6;
    public float sensitivity = 1;

    void Update()
    {
        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
        if (mouseWheel < 0) // forward
            Camera.main.orthographicSize -= mouseWheel * sensitivity;
        else if (mouseWheel > 0) // back
            Camera.main.orthographicSize -= mouseWheel * sensitivity;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, orthographicSizeMin, orthographicSizeMax);
    }

}