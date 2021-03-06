using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

Moves the object to the given position on fire.

*/

public class ReactMove : Reactor
{
	[Header("Movement Properties")]
	[Tooltip("Fire on the first time and disable afterward.")]
	public bool onlyActivateOnce = true;
	[Tooltip("Position to move the object to.\nLeave at 0,0,0 with relative = true to leave as-is.")]
	public Vector3 targetPosition;
	[Tooltip("Move the object by these values rather than move to them in world space.")]
	public bool relativePosition = true;
	// TODO: add rotation support.
	//[Tooltip("Leave at 0,0,0 with relative = true to leave as-is.")]
	//public Quaternion targetRotation;
	//public bool relativeRotation = true;
	[Tooltip("Speed of the object.")]
	public float speed = 0.1f;
	[Tooltip("Maximum speed allowed.")]
	public float maxSpeed = 10f;
	[Tooltip("Move the prop back to it's original spot after a delay.")]
	public bool returnToOriginalPos = false;
	[Tooltip("Time to wait before returning.")]
	public float returnDelay = 3;
	
	[Header("Movement Debug")]
	public Vector3 originalWorldPosition;
	public Vector3 targetWorldPosition;
	
	public override void checkErrors() {
		// nothing to check here!
	}
	
    public override void Start()
    {
        base.Start();
		originalWorldPosition = targetObject.transform.position;
		
		if (relativePosition) {
			targetWorldPosition = originalWorldPosition + targetPosition;
		} else {
			targetWorldPosition = targetPosition;
		}
		
	}
	
    public override void fire(float progress) {
		
		if (progress == 1.0f && ready) {
			fireCount++;
			MoveProp moverComponent;
			moverComponent = targetObject.AddComponent<MoveProp>();
			moverComponent.target = targetWorldPosition;
			moverComponent.moveSpeed = speed;
			moverComponent.maxSpeed = maxSpeed;
			moverComponent.enabled = true;
			
			// while it's moving it can't fire. wait till it's done!
			ready = false;
			if (returnToOriginalPos) {
				StartCoroutine( waitBeforeReturn() );
			}
		}
		
		// disable on first fire if onlyActivateOnce
		if (progress == 1.0f && onlyActivateOnce && ready) {
			ready = false;
		}
		
		
	}
	
	IEnumerator waitBeforeReturn() {
		yield return new WaitForSeconds(returnDelay);
		
		MoveProp moverComponent;
		moverComponent = targetObject.AddComponent<MoveProp>();
		moverComponent.target = originalWorldPosition;
		moverComponent.moveSpeed = speed;
		moverComponent.maxSpeed = maxSpeed;
		moverComponent.enabled = true;
		
		// okay we're finished moving.
		ready = true;
		
	}
	
	public override void reset() {
		
		if (fireCount > 0) {
			// it's been fired. change pos!
			targetObject.transform.position = originalWorldPosition;
		}
		
		ready = true;
		fireCount = 0;
	}
	
}
