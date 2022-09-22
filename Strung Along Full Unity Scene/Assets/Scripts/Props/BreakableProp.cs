using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
add this to a prop to make it breakable.
it works by disabling and activating different variants of the object. to use it, put the component on your object, then add damaged and/or broken variants as child objects.
they should be the very first siblings.
make sure the "Hits" variable is the same number of child objects. so:

- IntactWall (BreakableProp component goes here)
	- BrokenWall

in this setup, Hits should be 1. when no hits are taken, the parent object is visible.
when 1 hit is taken, the second object is activated, and the parent object renderer is disabled. the collider is only disabled on the LAST hit. this keeps things consistent even if the object is damaged.

- IntactWall (BreakableProp component goes here)
	- DamagedWall
	- HeavyDamageWall
	- BrokenWall
	- UnrelatedChildObject
	- UnrelatedChildObject
	
and in this one, Hits should be 3. start at the parent object, and each hit iterates through the child objects until the last one. any other children after that are ignored.

*/

public class BreakableProp : StageProp
{
	[Header("Break Properties")]
	public int _hits = 1;
	int _hitsTaken = 0;
	GameObject _activeChild = null;
	Rigidbody[] _rbs;
	Vector3[,] _rbPosRot;
	
	float _requiredAngle = 45.0f;
	float _requiredVelocity = 2.0f;
	float _requiredSlingTime = 3.0f;
	
	
	[Header("Debug")]
	[Tooltip("Make the prop break simply by walking into it.")]
	public bool _easyBreak = false;
	
	void Start() {
		for (int c = 0; c < _hits; c++ ) {
			transform.GetChild(c).gameObject.SetActive(false);
		}
		
		// store two-dimensional array of all child rigidbody initial position+rotation
		_rbs = GetComponentsInChildren<Rigidbody>(true);
		_rbPosRot = new Vector3[_rbs.Length, 2];
		
		for (int r = 0; r < _rbs.Length; r++) {
			_rbPosRot[r, 0] = _rbs[r].transform.position;
			_rbPosRot[r, 1] = _rbs[r].transform.eulerAngles;
		}
		
	}
	
	void OnCollisionEnter(Collision collision) {
		
		// object needs at least 1 HP, and only consider collisions that are within a certain angle. so grazing the object won't break it.
		bool validHit = _hitsTaken < _hits && Vector3.Angle(collision.GetContact(0).normal, collision.relativeVelocity) < _requiredAngle;
		
		if (validHit) {
			bool slingHit = false;
			if (collision.gameObject.TryGetComponent<PuppetController>(out PuppetController pup) ) {
				slingHit = collision.relativeVelocity.magnitude > _requiredVelocity && pup.timeSinceSlingshot < _requiredSlingTime;
			}
		
			if (slingHit || _easyBreak) {
				OnDamage(collision.GetContact(0), collision.relativeVelocity);
            }
		}


    }

    void OnDamage(ContactPoint contact, Vector3 velocity) {
		_hitsTaken++;
		Debug.Log(this + " just took a hit at " + contact.point + ", force " + velocity + ", angle " + Vector3.Angle(contact.normal, velocity) + ". HP: " + (_hits - _hitsTaken) + "/" + _hits);
		
		if (_activeChild) {
			_activeChild.SetActive(false);
		} else {
			GetComponent<Renderer>().enabled = false;

            // temp for playtest sorry just delete this line after 22/09/2022, box collider was staying after destruction
            gameObject.SetActive(false);
        }
		
		_activeChild = transform.GetChild(_hitsTaken - 1).gameObject;
		_activeChild.SetActive(true);
		
		if (_hitsTaken == _hits) {
			OnBreak(contact, velocity);
		}
		
	}
	
	void OnBreak(ContactPoint contact, Vector3 velocity) {
		Debug.Log(this + " is broken!!!!!");
		GetComponent<Collider>().enabled = false;
        // TODO: here we could add a force to _activeChild based on the impact velocity and point.

	}
	
	public override void Reset() {
		transform.position = originalPosition;
		
		_hitsTaken = 0;
		
		if (_activeChild) {
			_activeChild.SetActive(false);
			_activeChild = null;
		}
		
		GetComponent<Renderer>().enabled = true;
		GetComponent<Collider>().enabled = true;

        // set all rigidbody position and rotation to the recorded value at the start of the level
        try  // temp for playtest sorry just remove the try catch thingy and just take the for loop out
        {
            for (int r = 0; r < _rbs.Length; r++)
            {
                _rbs[r].transform.position = _rbPosRot[r, 0];
                _rbs[r].transform.eulerAngles = _rbPosRot[r, 1];
            }
        }
        catch (System.Exception)
        {
        }
		
	}
	
}
