using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
The lever fires a reaction when it's triggered.
*/
public class LeverProp : MonoBehaviour
{
	public List<Reaction> _reactions;

	
	[Header("Exclusivity")]
	public bool _player1 = true;
	public bool _player2 = true;
	//[Tooltip("Can the lever be activated by a prop pushing it? Ignores any player exclusivity.")]
	//public bool _physics = false;
	[Header("Lever Properties")]
	[Tooltip("Should the lever bounce back to it's neutral position?")]
	public bool _springy = true;
	[Tooltip("Pulling right causes reaction as normal, pulling left goes in reverse direction.")]
	public bool _twoWay = true;
	
	GameStateManager _ctx;
	public Transform _leverBase;
	public Transform _leverHandle;
	
	float _currentAngle = 0.0f;
	float _deadzone = 5.0f; // euler: leeway before activating
	float _turnAngle = 50.0f; // euler: maximum turn amount
	bool _grabbed = false; // true if a valid puppet is grabbing it.
	float _activationFactor = 0.0f; // 0 for no turn, 1 for full turn (max power)
	
    // Start is called before the first frame update
    void Start()
    {
        _ctx = GetComponentInParent<GameStateManager>();
		
		// TODO: color lever ball based on who can activate.
		
		// set lever centre of mass to the pivot point (which is just zero)
		_leverHandle.GetComponent<Rigidbody>().centerOfMass = Vector3.zero;
		
		if (!_springy) {
			// damn bitch you don't want the cool bounce physics what's your problem.
			_leverHandle.GetComponent<HingeJoint>().useSpring = false;
		}
		
    }

    void Update()
    {
		
		
        
		if (_grabbed && _activationFactor != 0.0f) {
			foreach (Reaction react in _reactions) {
				react.Fire(Time.deltaTime * _activationFactor);
			}
		}
		
		
    }
	
	// handle physics based calculations
	void FixedUpdate() {
		CheckTurning();
	}
	
	
	void CheckTurning() {
		
		Debug.DrawRay(_leverHandle.position, _leverBase.up, Color.red);
		Debug.DrawRay(_leverHandle.position, _leverHandle.up, Color.red);
		
		// TODO: check if the right puppet is turning it. gonna have to talk to the guys about the grab.
		
		// multiply by -1 because the entire game is flipped on the X axis somehow
		_currentAngle = _leverHandle.GetComponent<HingeJoint>().angle * -1;
		
		// in deadzone, lever isn't activating.
		// otherwise fire to reactors a factor of how far the lever is turned.
		_activationFactor = Mathf.Abs(_currentAngle) < _deadzone ? 0.0f : Mathf.Clamp(_currentAngle / (_turnAngle + _deadzone), -1.0f, 1.0f);
		
	}
	
	// sent by puppets when they grab something
	void OnGrab(PuppetController pup) {
		
		if (pup.gameObject == _ctx._player1 && _player1) {
			_grabbed = true;
		} else if (pup.gameObject == _ctx._player2 && _player2) {
			_grabbed = true;
		} else {
			// grabbed by the WRONG PUPPET!!!!!!!!! AHHHHHHHHHHHHHHH!!!!!!!!!!!!!!!!!!!!!
			pup.GetComponent<Rigidbody>().AddExplosionForce(10.0f, _leverBase.position, 5.0f, 3.0f);
		}
		
	}
	// sent by puppets when they release something
	void OnReleased(PuppetController pup) {
		_grabbed = false;
	}
}