using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Activator : MonoBehaviour
{
	[Tooltip("Reference to the Level Manager object.\nMake sure to set this!")]
	public LevelManager manager;
	public List<Reactor> reactors;
	[SerializeField]
	[Tooltip("Animation curve to modify the movement of Reactors.\nOnly applies to Dynamic Reactor.")]
	public AnimationCurve curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
	[Range(0,1)]
	public float lerpProgress = 0.0f;
	[Range(0,1)]
	public float curvedProgress = 0.0f;
	[Space]
	[Tooltip("If Player 1 can activate this.")]
	public bool p1CanActivate = true;
	[Tooltip("If Player 2 can activate this.")]
	public bool p2CanActivate = true;
	
	public abstract void reset();
	public abstract void checkErrors();
	
	public virtual void Start() {
		checkActvErrors();
		checkErrors();
		
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
		
		
		if (reactors.Count == 0) {
			Debug.LogWarning(this + ": has no Reactors. He is so lonely.");
		}
		
	}
	
	public void fireAll(float progress) {
		foreach (Reactor reactor in reactors) {
			curvedProgress = curve.Evaluate(lerpProgress);
			curvedProgress = float.IsNaN(curvedProgress) ? curvedProgress = 0.0f : curvedProgress;
			reactor.fire(curvedProgress);
		}
	}
	
	private void checkActvErrors() {
		
		if (manager == null) {
			Debug.LogError(this + ": LevelManager is not assigned.");
		}
		
	}
	
	
	
	
}
