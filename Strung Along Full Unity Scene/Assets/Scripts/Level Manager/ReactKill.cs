using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

Kills or destroys the target object.
If the object is a puppet, tells the Level Manager the puppet has died.
If the object is a prop, sets the prop to disabled.

Has a special interaction with ActvOnTouch.
If the Target Object is the same as one with ActvOnTouch and ReactKill, this will target any objects that fire ActvOnTouch.
This way you can make an object that kills whatever touches it.

*/

public class ReactKill : Reactor
{
	public List<GameObject> kills;
	
	public override void checkErrors() {
		
	}
	
    void Start()
    {
        kills = new List<GameObject>();
    }
	
	public override void fire(float progress) {
		
		if (progress == 1.0f) {
			if (targetObject == this.gameObject) {
				
				// if target is THIS object, search for ActvOnTouch and the Collider list.
				if (TryGetComponent<ActvOnTouch>(out ActvOnTouch comp)  ) {
					foreach (Collider collider in comp.currentActivators) {
						if (collider.TryGetComponent<PuppetController>(out PuppetController pup)  ) {
							// a puppet is touching. kill it and tell LevelManager!
							comp.manager.killPuppet(pup);
						} else {
							collider.gameObject.SetActive(false);
							kills.Add(collider.gameObject);
						}
					}
				}
				
				
			} else {
				targetObject.SetActive(false);
				kills.Add(targetObject);
			}
		}
		
	}
	
	private void explode() {
		// TODO: make particles explode around the thing that died
	}
	
    // Update is called once per frame
    void Update()
    {
        
    }
	
	public override void reset() {
		
		foreach(GameObject skeleton in kills) {
			skeleton.SetActive(true);
		}
		kills.Clear();
		
		
	}
	
}
