using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public abstract class Activator : MonoBehaviour
{
	
	public LevelManager manager;
	private Reactor[] reactors;
	public bool p1CanActivate = true;
	public bool p2CanActivate = true;
	
	void Awake() {
		reactors = GetComponents<Reactor>();
	}
	
	public void fireAll() {
		foreach (Reactor reactor in reactors) {
			reactor.fire();
		}
	}
	
	
}
