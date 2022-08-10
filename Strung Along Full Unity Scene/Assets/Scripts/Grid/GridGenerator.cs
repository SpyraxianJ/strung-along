using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{

    public GridManager manager;
    public GameObject pointPrefab;

    [Space]

    [Header("Grid Values")]

    [Space]

    [Tooltip("This is the dimentions of the grid in like, actual world size, not the resolution of it")]
    public Vector2 gridSize;

    [Space]

    [Tooltip("The number of points along the X axis")]
    [Range(1, 20)]
    public int gridWidth;
    [Tooltip("The number of points along the X axis")]
    [Range(1, 20)]
    public int gridDepth;

    [Space]

    [Tooltip("This is the position in world space the grid will be generated to")]
    public Vector3 root;

    // Start is called before the first frame update
    void Awake()
    {
        GenerateGrid(true); // temp
    }

    // Update is called once per frame
    void Update()
    {
        //GenerateGrid(true); // For testing only
    }

    public void GenerateGrid(bool destoryPoints)
    {

        if (destoryPoints) {
            for (int i = 0; i < manager.points.Count; i++)
            {
                if (manager.points[i] != null)
                {
                    Destroy(manager.points[i].gameObject);
                }
            }
            manager.points.RemoveAll(s => s == null);
        }

        List<GridPoint> newPoints = new List<GridPoint>();

        // This means we will write along the X axis each time
        for (int z = 0; z < gridDepth; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                // The (x * 1f) and (z * 1f) are there so C# turns them into a float, there is like an actual command for it, but I forgot it, oops
                newPoints.Add(Instantiate(pointPrefab, new Vector3(((x * 1f) / Mathf.Max(gridWidth - 1, 1) * gridSize.x) - gridSize.x / 2, 0, ((z * 1f) / Mathf.Max(gridDepth - 1, 1) * gridSize.y) - gridSize.y / 2) + root, Quaternion.identity).GetComponent<GridPoint>());

                // Vertical connections
                if (newPoints.Count - gridWidth > 0) {
                    newPoints[newPoints.Count - 1].connectedPoints.Add(newPoints[newPoints.Count - gridWidth - 1]);
                }

                // Horizontal connections
                if (x != 0) {
                    newPoints[newPoints.Count - 1].connectedPoints.Add(newPoints[newPoints.Count - 2]);
                }

            }
        }

        // Set as children of manager
        for (int i = 0; i < newPoints.Count; i++)
        {
            newPoints[i].gameObject.transform.SetParent(manager.gameObject.transform);
        }

        manager.GetChildrenPoints();

    }
}
