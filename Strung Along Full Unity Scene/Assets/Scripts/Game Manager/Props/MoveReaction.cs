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
	
	
	public override void fire() {
		//do the move
		Debug.Log("fired!!!");
	}
	
}
