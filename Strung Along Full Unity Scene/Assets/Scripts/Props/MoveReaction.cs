using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoveReaction : Reaction
{
	[Tooltip("The object we want to move.")]
	public Transform _target;
	[Tooltip("Where to move the object to, relative.")]
	public Vector3 _moveVector = Vector3.zero;
	[Tooltip("Time in seconds to reach new position.")]
	public float _duration = 2.0f;
	
	Coroutine _oneShotRoutine = null;
	float _wiggleTime = 2.0f;
	float _progress = 0.0f;
	Vector3 startPos = Vector3.zero;
	Vector3 endPos = Vector3.zero;
	
	void OnDrawGizmos() {
		if (_target != null) {
			Gizmos.DrawLine(transform.position, _target.position);
		}
		
	}
	
	void OnDrawGizmosSelected() {
		if (_target != null) {
			Gizmos.color = Color.green;
			Gizmos.DrawLine(_target.position, _target.position + _moveVector);
			Gizmos.DrawWireMesh(_target.GetComponentInChildren<MeshFilter>().sharedMesh, _target.position + _moveVector, _target.rotation, _target.lossyScale);
		}
		
	}
	
	void Start() {
		Spawnpoint isSpawn;
		if (_target.TryGetComponent<Spawnpoint>(out isSpawn)) {
			GameStateManager ctx = GetComponentInParent<GameStateManager>();
			if (isSpawn._isPlayer2) {
				_target = ctx._p2Anchor.transform;
			} else {
				_target = ctx._p1Anchor.transform;
			}
			
		}
		
	}
	
	/**
	
	// old implementation that required the puppet the continue holding the lever
	// to move the object. this has been replaced with only requiring the initial
	// crank of the lever, and the rest is automatic.
	public override void Fire(float lerp) {
		if (_progress == 0.0f) {
			startPos = _target.transform.position;
			endPos = startPos + _moveVector;
		}
		
		
		_progress += lerp / _duration;
		_progress = Mathf.Clamp01(_progress);
		_target.transform.position = Vector3.Lerp(startPos, endPos, _progress);
	}
	**/
	
	public override void Fire(float lerp) {
		if (_progress == 0.0f) {
			startPos = _target.transform.position;
			endPos = startPos + _moveVector;
		}
		
		if (lerp > 0.0f && _oneShotRoutine == null && _progress == 0.0f) {
			// move from start position to target position
			_oneShotRoutine = StartCoroutine( WiggleMove(_wiggleTime, false) );
		} else if (lerp < 0.0f && _oneShotRoutine == null && _progress == 1.0f) {
			// move from target position back to start position
			_oneShotRoutine = StartCoroutine( WiggleMove(_wiggleTime, true) );
		}
		
	}
	
	IEnumerator WiggleMove(float duration, bool reverse) {
		_progress = 1.0f;
		float pingPong = 0.0f;
		Vector3 wiggleAmount = new Vector3(0.25f, 0f, 0f);
		
		while (duration > 0.0f) {
			if (reverse) {
				_target.transform.position = Vector3.Lerp(endPos + wiggleAmount, endPos - wiggleAmount, pingPong / 0.05f);
			} else {
				_target.transform.position = Vector3.Lerp(startPos + wiggleAmount, startPos - wiggleAmount, pingPong / 0.05f);
			}
			
			pingPong = Mathf.PingPong(Time.time, 0.05f);
			duration -= Time.deltaTime;
			yield return null;
			
		}
		
		_oneShotRoutine = StartCoroutine( Move(reverse) );
	}
	
	IEnumerator Move(bool reverse) {
		_progress = reverse ? 1.0f : 0.0f;
		
		if (reverse) {
			while (_progress != 0.0f) {
				_progress -= Time.deltaTime / _duration;
				_progress = Mathf.Clamp01(_progress);
				_target.transform.position = Vector3.Lerp(startPos, endPos, _progress);
				yield return null;
			}
		} else {
			while (_progress != 1.0f) {
				_progress += Time.deltaTime / _duration;
				_progress = Mathf.Clamp01(_progress);
				_target.transform.position = Vector3.Lerp(startPos, endPos, _progress);
				yield return null;
			}
		}
		
		_oneShotRoutine = null;
	}
	
	
	public override void Reset() {
		StopAllCoroutines();
		_oneShotRoutine = null;
		_progress = 0.0f;
		startPos = Vector3.zero;
		endPos = Vector3.zero;
	}
	
}
