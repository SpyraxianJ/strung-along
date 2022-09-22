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
		
		if (lerp != 0.0f && _oneShotRoutine == null) {
			_oneShotRoutine = StartCoroutine( Wiggle(_wiggleTime) );
		}
		
	}
	
	IEnumerator Wiggle(float duration) {
		_progress = 1.0f;
		float pingPong = Mathf.PingPong(Time.time, 0.05f);
		Vector3 wiggleAmount = new Vector3(0.25f, 0f, 0f);
		
		_target.transform.position = Vector3.Lerp(startPos + wiggleAmount, startPos - wiggleAmount, pingPong / 0.05f);
		yield return null;
		
		if (duration <= 0.0f) {
			_progress = 0.0f;
			_oneShotRoutine = StartCoroutine( Move() );
		} else {
			_oneShotRoutine = StartCoroutine( Wiggle(duration - Time.deltaTime) );
		}
		
	}
	
	IEnumerator Move() {
		_progress += Time.deltaTime / _duration;
		_progress = Mathf.Clamp01(_progress);
		_target.transform.position = Vector3.Lerp(startPos, endPos, _progress);
		yield return null;
		
		if (_progress == 1.0f) {
			StopCoroutine(_oneShotRoutine);
		} else {
			_oneShotRoutine = StartCoroutine( Move() );
		}
	}
	
	
	public override void Reset() {
		StopAllCoroutines();
		_oneShotRoutine = null;
		_progress = 0.0f;
		startPos = Vector3.zero;
		endPos = Vector3.zero;
	}
	
}
