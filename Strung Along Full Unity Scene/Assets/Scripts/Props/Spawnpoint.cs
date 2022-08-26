using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoint : MonoBehaviour
{	
	[HideInInspector]
	public bool _isPlayer2 = false;
	//float _distanceFromGround; // obsolete: for the previous sphere approach to string lengths
	[Header("Per-Level String Properties")]
	public bool _elasticString = false;
	public float _stringLength = 12f;
	
	
	void OnDrawGizmos() {
		Gizmos.color = _isPlayer2 ? Color.red : Color.blue;
		Vector3 direction = transform.TransformDirection(Vector3.down) * _stringLength;
		Gizmos.DrawRay(transform.position, direction);
	}
	
	void OnDrawGizmosSelected() {
		Vector3 point = new Vector3(transform.position.x, 0f, transform.position.z);
		//_distanceFromGround = Vector3.Distance(transform.position, point);
		
		
		RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, _stringLength);
		
		foreach (RaycastHit hit in hits) {
			if (hit.transform.IsChildOf(gameObject.transform.parent) ) {
				//_distanceFromGround = hit.distance;
				point = hit.point;
				break;
			}
		}
		
		//float radius;
		//radius = Mathf.Sqrt(_stringLength * _stringLength - _distanceFromGround * _distanceFromGround);
		
		Gizmos.matrix *= Matrix4x4.Translate( new Vector3(0, point.y + 0.1f, 0) );
		Gizmos.matrix *= Matrix4x4.Scale( new Vector3(1, 0, 1) );
		
		Gizmos.color = _isPlayer2 ? Color.red : Color.blue;
		//Gizmos.DrawWireSphere(point, radius);
		Gizmos.DrawSphere(point, 0.5f);
		
	}
	
    // Start is called before the first frame update
    void Start()
    {
		// these are intended as a guide during the Editor.
		// you can't see them during gameplay!
		Destroy(gameObject.GetComponent<Renderer>() );
		
    }
	
}
