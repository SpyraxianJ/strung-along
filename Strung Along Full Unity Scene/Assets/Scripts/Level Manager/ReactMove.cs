using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactMove : Reactor
{
	[Header("Movement Properties")]
	public Vector3 targetPosition;
	public bool relativePosition = true;
	public bool dynamicReactor = false;
	[Header("Non-Dynamic Properties")]
	public float speed = 0.1f;
	public float maxSpeed = 10f;
	public bool returnToOriginalPos = false;
	public float returnDelay = 3;
	[Header("Movement Debug")]
	public Vector3 originalWorldPosition;
	public Vector3 targetWorldPosition;
	
	
	void Start() {
		originalWorldPosition = targetObject.transform.position;
		
		if (relativePosition) {
			targetWorldPosition = originalWorldPosition + targetPosition;
		} else {
			targetWorldPosition = targetPosition;
		}
		
	}
	
    public override void fire(float progress) {
		fireAttempts++;
		
		if (dynamicReactor) {
			targetObject.transform.position = Vector3.Lerp(originalWorldPosition, targetWorldPosition, progress / 1.0f);
		} else if (progress == 1.0f && canFire) {
			MoveProp moverComponent;
			moverComponent = targetObject.AddComponent<MoveProp>();
			moverComponent.target = targetWorldPosition;
			moverComponent.moveSpeed = speed;
			moverComponent.maxSpeed = maxSpeed;
			moverComponent.enabled = true;
		
			fireCount++;
			if (onlyActivateOnce) {
				canFire = false;
			}
			
			if (returnToOriginalPos) {
				StartCoroutine( waitBeforeReturn() );
			}
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
		
	}
	
	public override void reset() {
		canFire = true;
		fireCount = 0;
		fireAttempts = 0;
		
		targetObject.transform.position = targetObject.GetComponent<StageProp>().originalPosition;
	}
	
}
