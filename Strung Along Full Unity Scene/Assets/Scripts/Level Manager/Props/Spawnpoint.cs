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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
