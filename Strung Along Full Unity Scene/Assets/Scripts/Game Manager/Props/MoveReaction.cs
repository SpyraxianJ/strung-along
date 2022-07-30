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
	
	private float _currentProgress = 0.0f;
	
	public override void Fire(float lerp) {
		//do the move
		Debug.Log("fired!!!");
		
		Vector3 originalPos = _target.GetComponent<StageProp>().originalPosition;
		Vector3 newPos = originalPos + _moveVector;
		
		_currentProgress += lerp / _duration;
		_target.transform.position = Vector3.Lerp(originalPos, newPos, _currentProgress);
	}
	
}
