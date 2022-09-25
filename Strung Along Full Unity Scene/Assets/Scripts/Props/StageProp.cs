using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

This component is automatically added to every child object of a level. See LevelDatabase.cs for how that works.
It's for tracking the starting position of the object, so we can teleport it off-stage while invisible and animate it moving back to the starting position, achieving the illusion of the prop moving on and off the stage.

You can manually add this component to a level child object to choose which direction a certain prop moves.
Any prop without this component will move in from the Top by default.

Keep in mind it only cares about FIRST-LEVEL children. If you've got a level child and a child of THAT, this component won't do anything. It's this way because many of the model prefabs have a billion children and adding this component to every single one and checking each while loading+unloading is just fucked. Like this:

- Game Manager
	- act1
		- 1
			- P1 Goal	yes
			- P2 Goal	yes
			- Rock		yes
			- Rock2		yes
			- Boxes		yes
				- Box1	NO
				- Box2	NO
				- Box3	NO
			- Fire		yes


stageMoveDirection
	Top: default. enter and exit from the top of the stage.
	Left/Right: enter and exit from the left/right
	Scrolling: enter from the right, exit to the left. like a sidescrolling game.
	Bottom: enter and exit from the bottom of the stage.

*/

public class StageProp : MonoBehaviour, IResettable
{
	[HideInInspector]
	public Vector3 originalPosition;
	[HideInInspector]
	public Quaternion originalRotation;
	public GameStateManager.Direction stageMoveDirection;
	
	public void Init() {
		originalPosition = transform.position; 
		originalRotation = transform.rotation;
		ToggleColliders(false);
		gameObject.SetActive(false);
	}
	
	public void ToggleColliders(bool toggle) {
		Collider[] colliders = GetComponents<Collider>();
		foreach (Collider comp in colliders) {
			comp.enabled = toggle;
		}
		
		if (TryGetComponent<Rigidbody>(out Rigidbody r)  ) {
			r.isKinematic = toggle ? false : true;
			r.useGravity = toggle ? true : false;
		}
	}
	
	public virtual void Reset() {
		transform.position = originalPosition;
		transform.rotation = originalRotation;
	}
	
}
