using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
	[Tooltip("Determines if this is the goal for Player 2. Otherwise Player 1.")]
	public bool _isPlayer2;
	public bool _isActive = false;
	
	LayerMask _puppetMask;
	
	GameStateManager _ctx;
	GameObject _targetPlayer; // reference to the player GameObject this goal waits for.
	float _lerp = 0.0f;
	
	float _lightIntensity = 100.0f;
	Vector2 _lightSize = new Vector2(50f, 60f);
	Vector2 _lightSizeActive = new Vector2(110f, 120f);
	bool _flicker = false;
	
	float _detectorSize = 1.5f;
	Vector3 _detector = new Vector3(1.5f, 1.5f, 1.5f);
	
	void OnDrawGizmos() {
		Color trans = new Color(0, 0, 0, 0.5f);
		Gizmos.color = _isPlayer2 ? Color.red - trans : Color.blue - trans;
		
		Gizmos.DrawCube(transform.position + Vector3.down * (_detectorSize / 2f), _detector);
		
	}
	
    void Start()
    {
        _ctx = GetComponentInParent<GameStateManager>();
		
		// the goal can only be triggered by the right player!
		_targetPlayer = _isPlayer2 ? _ctx._player2 : _ctx._player1;
		
    }

    void Update()
    {
        
		HandleLight();
		
    }
	
	void HandleLight() {
		Light spotLight = GetComponent<Light>();
		
		if (_isActive) {
			_lerp += Time.deltaTime * 3;
		} else {
			_lerp -= Time.deltaTime * 3;
		}
		
		if (_flicker) {
			spotLight.intensity = UnityEngine.Random.Range(0f, 50f);
			_lerp = 0.5f;
		} else {
			spotLight.intensity = _lightIntensity;
		}
		
		_lerp = Mathf.Clamp01(_lerp);
		spotLight.innerSpotAngle = Mathf.Lerp(_lightSize.x, _lightSizeActive.x, _lerp);
		spotLight.spotAngle = Mathf.Lerp(_lightSize.y, _lightSizeActive.y, _lerp);
		
		
	}
	
	void FixedUpdate() {
		
		CheckActive();
		
	}
	
	void CheckActive() {
		// first assume it's off
		_isActive = false;
		_flicker = false;
		
		Vector3 goalPos = transform.position + Vector3.down * (_detectorSize / 2f);
		Collider[] areaHits = Physics.OverlapBox(goalPos, _detector / 2, Quaternion.identity, _puppetMask);
		
		foreach (Collider hit in areaHits) {
			if (hit.gameObject == _targetPlayer) {
				// correct puppet!
				_isActive = true;
			} else {
				// wrong puppet!!!!!!!!!
				_flicker = true;
			}
			
			
		}
	}
	
	void Reset() {
		_isActive = false;
	}
	
	
}
