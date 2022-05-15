using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActvLever : Activator
{
	[Header("Lever Properties")]
	[Range(0, 360)]
	public float rotationToActivate = 90f;
	public float activationTime = 1f;
	public float resetTime = 2f;
	[Space]
	[Header("Debug")]
	public bool isActivating = false;
	[Range(0,1)]
	public float lerpProgress = 0.0f;
	public Quaternion originalRotation;
	public Quaternion targetRotation;
	
	
	
    // Start is called before the first frame update
    void Start()
    {
		originalRotation = transform.rotation;
		targetRotation = originalRotation * Quaternion.Euler(0, 0, rotationToActivate);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
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
		
		// if the rotation has reached the target, the lever activates!
		if (lerpProgress == 1) {
			fireAll();
		}
		
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
	
	public override void reset() {
		isActivating = false;
		lerpProgress = 0.0f;
		transform.rotation = originalRotation;
	}
	
	
}
