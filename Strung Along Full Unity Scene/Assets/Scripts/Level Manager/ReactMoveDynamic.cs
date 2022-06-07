using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

Moves the object between it's start position and target position,
based on the Activator's progress.

*/


public class ReactMoveDynamic : Reactor
{
	[Header("Dynamic Movement Properties")]
	[Tooltip("Position to move the object to.\nLeave at 0,0,0 with relative = true to leave as-is.")]
	public Vector3 targetPosition;
	[Tooltip("Move the object by these values rather than move to them in world space.")]
	public bool relativePosition = true;
	
	[Header("Movement Debug")]
	public Vector3 originalWorldPosition;
	public Vector3 targetWorldPosition;
	
	
	public override void checkErrors() {
		// nothing to check here!
	}
	
    // Start is called before the first frame update
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
		
		if (ready) {
			targetObject.transform.position = Vector3.Lerp(originalWorldPosition, targetWorldPosition, progress / 1.0f);
		}
		
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
