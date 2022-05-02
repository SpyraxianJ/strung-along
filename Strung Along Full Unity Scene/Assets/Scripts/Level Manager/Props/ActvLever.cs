using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActvLever : Activator
{
	
	private Vector3 originalRotation;
	private bool leverTurning = false;
	private bool antiRapidFire = false; // prevent invoking every update
	private float targetRotation;
	
	[Header("Lever Properties")]
	[Range(0, 360)]
	public float rotationToActivate = 90f;
	public float forwardRotationSpeed = 2f;
	public float resetRotationSpeed = 1f;
	
    // Start is called before the first frame update
    void Start()
    {
		originalRotation = gameObject.transform.eulerAngles;
		targetRotation = originalRotation.z + rotationToActivate;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
		// spin toward the target direction if the player is touching it
		if (leverTurning && transform.eulerAngles.z < targetRotation) {
			transform.Rotate(Vector3.forward * forwardRotationSpeed, Space.Self);
		} 
		// spin back if the player isn't touching it anymore
		else if (!leverTurning && transform.eulerAngles.z > originalRotation.z) {
			antiRapidFire = false;
			transform.Rotate(Vector3.back * resetRotationSpeed, Space.Self);
		}
		
		// if the rotation has reached the target, the lever activates!
		if (transform.eulerAngles.z >= targetRotation && !antiRapidFire) {
			transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, targetRotation);
			onActivate?.Invoke();
			antiRapidFire = true;
		}
		
    }
	
	void OnTriggerStay(Collider other) {
		if (other.gameObject == manager.player1 || other.gameObject == manager.player2) {
			leverTurning = true;
		}
	}

	void OnTriggerExit(Collider other) {
		
		if (other.gameObject == manager.player1 || other.gameObject == manager.player2) {
			leverTurning = false;
		}
		
	}
	
}
