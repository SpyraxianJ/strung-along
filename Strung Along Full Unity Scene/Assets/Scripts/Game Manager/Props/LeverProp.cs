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
	
	[Tooltip("Played for every 10 degrees the lever turns.\nThis means about 5 plays from neutral > fully active.")]
	public List<AudioClip> _turningSound;
	int _SFXincrement = 10; // every X degrees
	int _SFXticker = 0; // keeps track of how many multiples of _SFXincrement we are from start position
	
	GameStateManager _ctx;
	Transform _leverBase;
	Transform _leverHandle;
	AudioSource _speaker;
	Color _p2Color = new Color(0.02676473f, 0.3199194f, 0.7647059f);
	Color _eitherColor = new Color(0.8f, 0.8f, 0.8f);
	float _deadzone = 5.0f; // euler: leeway before activating
	float _turnAngle = 50.0f; // euler: maximum turn amount
	[Header("Debug")]
	public bool _grabbed = false; // true if a valid puppet is grabbing it.
	float _activationFactor = 0.0f; // 0 for no turn, 1 for full turn (max power), -1 for other direction
	
    // Start is called before the first frame update
    void Start()
    {
        _ctx = GetComponentInParent<GameStateManager>();
		_leverBase = transform.GetChild(0).GetChild(0);
		_leverHandle = transform.GetChild(0).GetChild(1);
		_speaker = _leverBase.GetComponent<AudioSource>();
		
		// change color of lever depending on who can use it.
		SetColor();
		// update spring. we turn spring off when its being grabbed.
		SetSpringy(_springy);
		
		// set lever centre of mass to the pivot point (which is just zero)
		_leverHandle.GetComponent<Rigidbody>().centerOfMass = Vector3.zero;
		
    }
	
	void SetColor() {
		// the material of the colored ball is the second slot in the MeshRenderer.
		Material[] handleMats = _leverHandle.GetComponent<MeshRenderer>().materials;
		if (_player1 && _player2) {
			handleMats[1].color = _eitherColor;
		} else if (_player2) {
			handleMats[1].color = _p2Color;
		}
		// by default the ball is red, so if only P1 can use it no need to change.
		_leverHandle.GetComponent<MeshRenderer>().materials = handleMats;
	}
	
	void SetSpringy(bool spring) {
		_leverHandle.GetComponent<HingeJoint>().useSpring = spring;
	}
	
	// handle physics based calculations
	void FixedUpdate() {
		CheckTurning();
		HandleSFX();
		
		bool actvRequirements = _grabbed && _activationFactor != 0.0f;
		
		if (actvRequirements) {
			foreach (Reaction react in _reactions) {
				react.Fire(Time.fixedDeltaTime * _activationFactor);
			}
		}
		
	}
	
	void CheckTurning() {
		
		Debug.DrawRay(_leverHandle.position, _leverBase.up, Color.red);
		Debug.DrawRay(_leverHandle.position, _leverHandle.up, Color.red);
		
		// multiply by -1 because the entire game is flipped on the X axis somehow
		float currentAngle = _leverHandle.GetComponent<HingeJoint>().angle * -1;
		
		// in deadzone, lever isn't activating.
		// otherwise fire to reactors a factor of how far the lever is turned.
		_activationFactor = Mathf.Abs(currentAngle) < _deadzone ? 0.0f : Mathf.Clamp(currentAngle / (_turnAngle + _deadzone), -1.0f, 1.0f);
		
	}
	
	void HandleSFX() {
		
		float absAngle = Mathf.Abs(_leverHandle.GetComponent<HingeJoint>().angle);
		int newTick = (int)absAngle / _SFXincrement;
		
		if (newTick != _SFXticker) {
			_speaker.Stop();
			_speaker.clip = _turningSound[Random.Range(0, _turningSound.Count)];
			_speaker.time = 0.4f; // the current lever sound has a long startup.
			_speaker.Play();
			
			_SFXticker = newTick;
			_speaker.pitch = 1.0f + (_SFXticker * 0.05f);
		}
		
		
	}
	
	// sent by puppets when they grab something
	void OnGrab(PuppetController pup) {
		
		if (pup.gameObject == _ctx._player1 && _player1) {
			_grabbed = true;
			SetSpringy(false);
		} else if (pup.gameObject == _ctx._player2 && _player2) {
			_grabbed = true;
			SetSpringy(false);
		} else {
			// grabbed by the WRONG PUPPET!!!!!!!!! AHHHHHHHHHHHHHHH!!!!!!!!!!!!!!!!!!!!!
			pup.GrabRelease();
			pup.GetComponent<Rigidbody>().AddExplosionForce(10.0f, _leverBase.position, 5.0f, 3.0f);
		}
		
	}
	// sent by puppets when they release something
	void OnReleased(PuppetController pup) {
		_grabbed = false;
		SetSpringy(_springy);
	}
}