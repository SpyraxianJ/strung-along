using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

NOT IMPLEMENTED YET
Changes the color or material of the object on fire.

*/

public class ReactColor : Reactor
{
	
	[Header("Color Properties")]
	public bool changeColor;
	public Color toColor;
	public bool changeMaterial;
	public Material toMaterial;
	
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	
	public override void fire(float progress) {
		
	}
	
	public override void reset() {
		
	}
	
	public override void checkErrors() {
		
	}
	
	
}
