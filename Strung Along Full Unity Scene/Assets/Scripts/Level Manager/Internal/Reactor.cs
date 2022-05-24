using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Reactor : MonoBehaviour
{
	[Tooltip("The object to manipulate.")]
	public GameObject targetObject;
	[Header("Reactor Debug")]
	public bool ready = true;
	public int fireCount = 0;
	
	public abstract void fire(float progress);
	public abstract void reset();
	public abstract void checkErrors();
	
	public virtual void Start() {
		
		checkReactorErrors();
		checkErrors();
		
	}
	
	private void checkReactorErrors() {
		
		if (targetObject == null) {
			Debug.LogError(this + ": TargetObject is not assigned.");
		}
		
	}
	
}
