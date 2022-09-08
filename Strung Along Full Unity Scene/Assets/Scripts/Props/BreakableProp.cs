using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class is still a work in progress. basic functionality is there though!
public class BreakableProp : StageProp
{
	[Header("Break Properties")]
	public int _hits = 1;
	int _hitsTaken = 0;
	
	void OnCollisionEnter(Collision collision) {
		
		if (_hitsTaken < _hits) {
			bool puppetAmmo = false;
			if (collision.gameObject.TryGetComponent<PuppetController>(out PuppetController pup) ) {
				puppetAmmo = collision.relativeVelocity.magnitude > 2.0f && pup.timeSinceSlingshot < 3.0f;
			}
		
			if (puppetAmmo) {
				OnDamage(collision.relativeVelocity);
			}
		}
		
		
		
	}
	
	void OnDamage(Vector3 velocity) {
		Debug.Log(this + " just took a hit with a force of " + velocity + ". HP LEFT: " + (_hits - _hitsTaken) );
		_hitsTaken++;
		
		if (_hitsTaken == _hits) {
			OnBreak(velocity);
		}
		
	}
	
	void OnBreak(Vector3 velocity) {
		Debug.Log(this + " is broken!!!!!");
		transform.localScale *= 0.1f;
		
	}
	
	public override void Reset() {
		transform.position = originalPosition;
		
		_hitsTaken = 0;
	}
	
}
