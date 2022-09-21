using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPoint : MonoBehaviour
{

    [Tooltip("All the points that this point is connected to, these connections make likes the player can travel across")]
    public List<GridPoint> connectedPoints;
    public List<DecalLineRenderer> lineRenderers;
    public GameObject lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        InitializePoints();
    }

    public void InitializePoints()
    {
        for (int i = 0; i < connectedPoints.Count; i++)
        {

            if (connectedPoints[i] == null)
            {
                connectedPoints.RemoveAt(i);
                i = i - 1;
            }
            else
            {
                if (connectedPoints[i].connectedPoints.Contains(this) == false)
                {
                    connectedPoints[i].connectedPoints.Add(this);
                    Debug.Log("Grid Point " + this + " initilization connected itself to " + connectedPoints[i]);

                    // Create line

                    try
                    {
                        DecalLineRenderer line = Instantiate(lineRenderer).GetComponent<DecalLineRenderer>();

                        line.SetPosition(transform, connectedPoints[i].transform);

                        lineRenderers.Add(line);
                    }
                    catch (System.Exception)
                    {
                        Debug.LogWarning("The point" + this + "doesn't have a lineRenderer prefab attached, which likely means it is old and not referncing the grid point prefab, it means we can't generate the grid here, just tell Tim and it shall be fixed, or manually assign it if you know how or someone around you do, ok thank byeeeeeee!!!");
                    }

                }
            }

        }
    }

    public void DisplayLines(Color col)
    {
        for (int i = 0; i < connectedPoints.Count; i++)
        {
            Debug.DrawLine(transform.position, connectedPoints[i].transform.position, col);
        }
    }

}
