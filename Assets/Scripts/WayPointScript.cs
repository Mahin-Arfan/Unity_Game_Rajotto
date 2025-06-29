using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WayPointScript : MonoBehaviour
{
    public Image img;
    public Transform waypoint;
    public Text waypointDistance;
    public Camera cam;
    public Vector3 offset;
    public GameManagerScript gameManagerScript;

    void Start()
    {
        gameManagerScript = GetComponent<GameManagerScript>();
    }

    void Update()
    {
        float minX = img.GetPixelAdjustedRect().width / 2;
        float maxX = Screen.width - minX;

        float minY = img.GetPixelAdjustedRect().height / 2;
        float maxY = Screen.height - minY;

        Vector2 pos = cam.WorldToScreenPoint(waypoint.position + offset);

        if(Vector3.Dot((waypoint.position - cam.transform.position), cam.transform.forward) < 0)
        {
            if(pos.x < Screen.width / 2)
            {
                pos.x = maxX;
            }
            else
            {
                pos.x = minX;
            }
        }

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        img.transform.position = pos;
        waypointDistance.text = ((int)gameManagerScript.waypointDistance).ToString() + "m";
    }
}
