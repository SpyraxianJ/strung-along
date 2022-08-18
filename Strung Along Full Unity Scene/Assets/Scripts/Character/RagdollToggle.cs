using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollToggle : MonoBehaviour
{
	
	Collider _gameplayCollider; // the capsule collider used during normal gameplay
	Collider[] _ragdollColliders; // list of colliders used for the ragdoll
	Animator _anim;
	
	public bool ragdolled = false;
	
    void Start() {
		_gameplayCollider = GetComponent<Collider>();
		
		_anim = GetComponentInChildren<Animator>();
		_ragdollColliders = _anim.GetComponentsInChildren<Collider>();
		
		Off();
    }
	
	void Update() {
		
		if (ragdolled) On(); else Off();
		
	}
	
	public void On() {
		_gameplayCollider.enabled = false;
		_anim.enabled = false;
		
		foreach (Collider c in _ragdollColliders) {
			c.enabled = true;
		}
	}
	
	public void Off() {
		foreach (Collider c in _ragdollColliders) {
			c.enabled = false;
		}
		
		_gameplayCollider.enabled = true;
		_anim.enabled = true;
	}

}
