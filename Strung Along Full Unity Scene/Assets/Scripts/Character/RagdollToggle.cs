using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollToggle : MonoBehaviour
{
	
	Collider _gameplayCollider; // the capsule collider used during normal gameplay
	Collider[] _ragdollColliders; // list of colliders used for the ragdoll
	Animator _anim;
	
	Vector3 _startPos;
	
	public Transform _hook;
	
	public bool ragdolled = false;
	
    void Start() {
		_gameplayCollider = GetComponent<Collider>();
		
		_anim = GetComponentInChildren<Animator>();
		_ragdollColliders = _anim.GetComponentsInChildren<Collider>();
		_hook.GetComponent<Rigidbody>().isKinematic = true;
		
		_startPos = _hook.localPosition;
		
		Off();
    }
	
	void Update() {
		
		// temporary for debug purpose, won't be in final implementation
		if (ragdolled) On(true); else Off();
		
	}
	
	// activates ragdoll on this puppet.
	// if "hooked" is true, the puppet is held at the hook transform position by their back.
	// otherwise, goes totally floppy.
	public void On(bool hooked) {
		_gameplayCollider.enabled = false;
		_anim.enabled = false;
		
		foreach (Collider c in _ragdollColliders) {
			c.enabled = true;
		}
		
		if (hooked) {
			_hook.rotation = Quaternion.identity;
		} else {
			_hook.GetComponent<Rigidbody>().isKinematic = false;
		}
		
		
	}
	
	public void Off() {
		foreach (Collider c in _ragdollColliders) {
			c.enabled = false;
		}
		
		_gameplayCollider.enabled = true;
		_anim.enabled = true;
		
		_hook.GetComponent<Rigidbody>().isKinematic = true;
		_hook.localPosition = _startPos;
	}
	
	

}
