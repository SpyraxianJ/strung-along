using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoint : MonoBehaviour
{
	
	public Transform anchorPoint;
	public Transform puppet;
	
	public bool isPlayer2 = false;
	[Space]
	[Header("Per-Level String Properties")]
	public bool elasticString = false;
	public float stringLength = 12f;
	
    // Start is called before the first frame update
    void Start()
    {
		// these are intended as a guide during the Editor.
		// you can't see them during gameplay!
		Component[] renderers = gameObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer r in renderers) {
			Destroy(r);
		}
		
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
