using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    [Tooltip("This list of points attached to this manager, will get all points that are children of this object on start")]
    public List<GridPoint> points;

    [Space]

    [Tooltip("Will display the lines on all points in the list")]
    public bool displayLines;

    // Start is called before the first frame update
    void Start()
    {
        GetChildrenPoints();
    }

    // Update is called once per frame
    void Update()
    {
        if (displayLines) {
            for (int i = 0; i < points.Count; i++)
            {
                points[i].DisplayLines(Color.red);
            }
        }
    }

    public void GetChildrenPoints() // Expensive, only use on start where possible.
    {
        GridPoint[] childrenPoints = gameObject.GetComponentsInChildren<GridPoint>();
        for (int i = 0; i < childrenPoints.Length; i++)
        {
            if (points.Contains(childrenPoints[i]) == false)
            {
                points.Add(childrenPoints[i]);
            }
        }
    }
}
