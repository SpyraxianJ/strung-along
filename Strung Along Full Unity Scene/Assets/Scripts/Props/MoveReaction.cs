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
	
	public override void Fire(float lerp) {
		if (_progress == 0.0f) {
			startPos = _target.transform.position;
			endPos = startPos + _moveVector;
		}
		
		
		_progress += lerp / _duration;
		_progress = Mathf.Clamp01(_progress);
		_target.transform.position = Vector3.Lerp(startPos, endPos, _progress);
	}
	
	public override void Reset() {
		_progress = 0.0f;
		//_target.transform.position = _target.GetComponent<StageProp>().originalPosition;
	}
	
}
