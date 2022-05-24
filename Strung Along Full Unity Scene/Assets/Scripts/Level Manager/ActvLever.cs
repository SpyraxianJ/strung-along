using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

Fires after the puppets have "turned" the object by standing inside it, like turning a lever.

*/

public class ActvLever : Activator
{
	[Header("Lever Properties")]
	[Range(0, 360)]
	[Tooltip("Target rotation the lever fires at.\n90 for quarter-turn, 180 for half turn etc.")]
	public float rotationToActivate = 90f;
	[Tooltip("Time it takes to rotate.")]
	public float activationTime = 1f;
	[Tooltip("Time it takes to rotate back after being turned.")]
	public float resetTime = 2f;
	[Space]
	[Header("Debug")]
	public bool isActivating = false;
	public Quaternion originalRotation;
	public Quaternion targetRotation;
	
	
	public override void checkErrors() {
		if ( TryGetComponent<Collider>(out Collider comp) ) {
			if (comp.isTrigger == false) {
				Debug.LogError(this + ": Collider must be a Trigger.");
			}
		}
	}
	
	public override void reset() {
		isActivating = false;
		lerpProgress = 0.0f;
		transform.rotation = originalRotation;
	}
	
    // Start is called before the first frame update
    public override void Start()
    {
		base.Start();
		originalRotation = transform.rotation;
		targetRotation = originalRotation * Quaternion.Euler(0, 0, rotationToActivate);
		
		handleColor();
		
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		handleColor();
		
		float speedFactor;
		// spin toward the target direction if the player is touching it
		if (isActivating) {
			transform.rotation = Quaternion.Lerp(originalRotation, targetRotation, lerpProgress / 1.0f);
			speedFactor = Time.deltaTime / activationTime;
			lerpProgress = lerpProgress < 1.0f ? lerpProgress += speedFactor : lerpProgress = 1.0f;
		} 
		// spin back if the player isn't touching it anymore
		else {
			transform.rotation = Quaternion.Lerp(originalRotation, targetRotation, lerpProgress / 1.0f);
			speedFactor = Time.deltaTime / resetTime;
			lerpProgress = lerpProgress > 0.0f ? lerpProgress -= speedFactor : lerpProgress = 0.0f;
		}
		
		fireAll(lerpProgress);
		
    }
	
	void OnTriggerStay(Collider other) {
		if (other.gameObject == manager.player1 && p1CanActivate) {
			isActivating = true;
		}
		if (other.gameObject == manager.player2 && p2CanActivate) {
			isActivating = true;
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.gameObject == manager.player1 || other.gameObject == manager.player2) {
			isActivating = false;
		}
	}
	
	private void handleColor() {
		Color objColor = Color.magenta;
		
		if (p1CanActivate && !p2CanActivate) {
			objColor = Color.blue;
		}
		if (!p1CanActivate && p2CanActivate) {
			objColor = Color.red;
		}
		
		if (isActivating) {
			objColor = Color.green;
		}
		
		
		gameObject.GetComponent<Renderer>().material.color = objColor;
		
		
	}
	
	
}
