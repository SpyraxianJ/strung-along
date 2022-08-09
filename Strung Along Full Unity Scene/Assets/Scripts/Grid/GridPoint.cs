using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPoint : MonoBehaviour
{

    [Tooltip("All the points that this point is connected to, these connections make likes the player can travel across")]
    public List<GridPoint> connectedPoints;
    [Tooltip("If the lines should be displayed, they may be displayed by the point manager regardless")]
    public bool displayLines;

    // Start is called before the first frame update
    void Start()
    {
        InitializePoints();
    }

    // Update is called once per frame
    void Update()
    {
        if (displayLines) {
            DisplayLines();
        }
    }

    public void InitializePoints()
    {
        for (int i = 0; i < connectedPoints.Count; i++)
        {
            if (connectedPoints[i].connectedPoints.Contains(this) == false)
            {
                connectedPoints[i].connectedPoints.Add(this);
                Debug.Log("Grid Point " + this + " initilization connected itself to " + connectedPoints[i]);
            }
        }
    }

    public void DisplayLines()
    {
        for (int i = 0; i < connectedPoints.Count; i++)
        {
            Debug.DrawLine(transform.position, connectedPoints[i].transform.position);
        }
    }

}
