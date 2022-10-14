using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageTileManager : MonoBehaviour
{
    public List<bool> activeTiles;
    public List<Transform> tiles;
    public List<Vector3> tilesPos, tilesPosDropped;
    public Transform platforms;
    private float moveVelocity = 2f;
    // Start is called before the first frame update
    void Start()
    {
        platforms = FindObjectOfType<StagePlatformFlag>().transform;
        foreach (Transform child in platforms)
        {
            tiles.Add(child);
            tilesPos.Add(child.transform.localPosition);
            tilesPosDropped.Add(new Vector3(child.transform.localPosition.x, child.transform.localPosition.y - 2, child.transform.localPosition.z));
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < activeTiles.Count; i++)
        {
            if (!activeTiles[i])
                tiles[i].transform.localPosition = Vector3.MoveTowards(tiles[i].transform.localPosition, tilesPosDropped[i], Time.deltaTime * moveVelocity);
            else
                tiles[i].transform.localPosition = Vector3.MoveTowards(tiles[i].transform.localPosition, tilesPos[i], Time.deltaTime * moveVelocity);
        }
    }
}
