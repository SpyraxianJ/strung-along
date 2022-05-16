using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Reactor : MonoBehaviour
{
	
	public GameObject targetObject;
	public bool onlyActivateOnce = true;
	[Header("Reactor Debug")]
	public bool canFire = true;
	public int fireCount = 0;
	public int fireAttempts = 0;
	
	public abstract void fire(float progress);
	public abstract void reset();
	
}
