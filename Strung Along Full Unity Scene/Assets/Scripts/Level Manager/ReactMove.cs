using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

bit of info here about how to use it

*/

public class ReactMove : Reactor
{
	[Header("Movement Properties")]
	[Tooltip("Leave at 0,0,0 with relative = true to leave as-is.")]
	public Vector3 targetPosition;
	public bool relativePosition = true;
	// TODO: add rotation support.
	//[Tooltip("Leave at 0,0,0 with relative = true to leave as-is.")]
	//public Quaternion targetRotation;
	//public bool relativeRotation = true;
	
	[Header("Non-Dynamic Properties")]
	public float speed = 0.1f;
	public float maxSpeed = 10f;
	[Tooltip("Move the prop back to it's original spot after a delay.")]
	public bool returnToOriginalPos = false;
	public float returnDelay = 3;
	
	[Header("Dynamic Properties")]
	[Tooltip("Syncs movement with the Activator. Try it out it's super cool.\nIgnores all Non-Dynamic Properties.")]
	public bool dynamicReactor = false;
	[Range(0,1)]
	public float progress;
	
	[Header("Movement Debug")]
	public Vector3 originalWorldPosition;
	public Vector3 targetWorldPosition;
	
	public override void checkErrors() {
		// nothing to check here!
	}
	
	void Start() {
		originalWorldPosition = targetObject.transform.position;
		
		if (relativePosition) {
			targetWorldPosition = originalWorldPosition + targetPosition;
		} else {
			targetWorldPosition = targetPosition;
		}
		
	}
	
    public override void fire(float progress) {
		this.progress = progress;
		
		if (dynamicReactor && ready) {
			targetObject.transform.position = Vector3.Lerp(originalWorldPosition, targetWorldPosition, progress / 1.0f);
		} else if (progress == 1.0f && ready) {
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
		ready = true;
		fireCount = 0;
		
		//targetObject.transform.position = targetObject.GetComponent<StageProp>().originalPosition;
	}
	
}
