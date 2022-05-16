using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Activator : MonoBehaviour
{
	
	public LevelManager manager;
	public List<Reactor> reactors;
	public bool p1CanActivate = true;
	public bool p2CanActivate = true;
	
	void Awake() {
		bool counting = false;
		Component[] every = GetComponents<Component>();
		foreach(Component comp in every) {
			// found this object: start counting the Reactors after it!
			if (comp == this) {
				counting = true;
				continue;
			}
			// add Reactors until the next object isn't a Reactor.
			if (counting && comp is Reactor) {
				reactors.Add((Reactor)comp);
			} else {
				counting = false;
			}
		}
	}
	
	public void fireAll(float progress) {
		foreach (Reactor reactor in reactors) {
			reactor.fire(progress);
		}
	}
	
	public abstract void reset();
	
	
}
