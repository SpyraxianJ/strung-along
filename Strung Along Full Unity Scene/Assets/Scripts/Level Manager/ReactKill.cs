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
	[Header("Kill Properties")]
	[Tooltip("Respawn the object after a delay. Doesn't work with puppets.\n0 for NEVER.")]
	public float respawnAfter = 0f;
	[Tooltip("Make particles when it respawns.")]
	public ParticleSystem respawnParticle;
	[Header("Kill Debug")]
	public List<GameObject> kills;
	
	public override void checkErrors() {
		
	}
	
    public override void Start()
    {
        base.Start();
		kills = new List<GameObject>();
    }
	
	public override void fire(float progress) {
		
		if (progress == 1.0f && ready) {
			ready = false;
			
			if (targetObject == this.gameObject) {
				
				// if target is THIS object, search for ActvOnTouch and the Collider list.
				if (TryGetComponent<ActvOnTouch>(out ActvOnTouch comp)  ) {
					foreach (Collider collider in comp.currentActivators) {
						if (collider.TryGetComponent<PuppetController>(out PuppetController pup)  ) {
							// a puppet is touching. tell LevelManager to kill it!
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
				if (respawnAfter > 0f) {
					StartCoroutine( waitRespawnAfter(targetObject, respawnAfter) );
				}
				
				
			}
		} else if (progress < 1.0f) {
			ready = true;
		}
		
	}
	
	
	IEnumerator waitRespawnAfter(GameObject skeleton, float delay) {
		yield return new WaitForSeconds(delay);
		
		skeleton.SetActive(true);
		skeleton.transform.position = skeleton.GetComponent<StageProp>().originalPosition;
		
		if (respawnParticle != null) {
			Instantiate(respawnParticle, skeleton.transform.position, Quaternion.identity);
		}
		
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
