using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactMove : Reactor
{
	[Header("Movement Properties")]
	public Vector3 targetPosition;
	public bool relativePosition = true;
	public float speed = 0.1f;
	public float maxSpeed = 10f;
	
    public override void fire() {
		fireAttempts++;
		
		if (canFire) {
			MoveProp moverComponent;
		
			moverComponent = targetObject.AddComponent<MoveProp>();
		
			if (relativePosition) {
				moverComponent.target = targetObject.transform.position + targetPosition;
			} else {
				moverComponent.target = targetPosition;
			}
		
			moverComponent.moveSpeed = speed;
			moverComponent.maxSpeed = maxSpeed;
			moverComponent.enabled = true;
		
			fireCount++;
			if (onlyActivateOnce) {
				canFire = false;
			}
		}
	}
	
	public override void reset() {
		canFire = true;
		fireCount = 0;
		fireAttempts = 0;
		
		targetObject.transform.position = targetObject.GetComponent<StageProp>().originalPosition;
	}
	
}
