using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// attached to the gameplay camera, this makes it follow the puppets
// when they cross into the deeper part of the stage.
public class StageCamFollow : MonoBehaviour
{
	public Transform _cameraTarget;
	public Vector3 _dollyVector = new Vector3(0, 1, 4);
	public float _stageFollowDepth = 9.0f;
	
	Vector3 _startPos;
	Vector3 _dollyPos;
	
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position + _dollyVector, 0.25f);
		Gizmos.DrawLine(transform.position, transform.position + _dollyVector);
		
		Gizmos.DrawWireSphere(_cameraTarget.position, 0.25f);
		Gizmos.DrawWireSphere(_cameraTarget.position + new Vector3(0, 0, _stageFollowDepth), 0.25f );
		Gizmos.DrawLine(_cameraTarget.position, _cameraTarget.position + new Vector3(0, 0, _stageFollowDepth) );
	}
	
    void Start() {
        _startPos = transform.position;
		_dollyPos = transform.position + _dollyVector;
    }

    void Update() {
		transform.position = Vector3.Lerp(_startPos, _dollyPos, _cameraTarget.position.z / _stageFollowDepth);
    }
}
