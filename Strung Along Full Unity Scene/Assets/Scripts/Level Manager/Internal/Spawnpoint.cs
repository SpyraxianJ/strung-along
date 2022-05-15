using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoint : MonoBehaviour
{	
	public bool isPlayer2 = false;
	[SerializeField]
	private float distanceFromGround;
	[Space]
	[Header("Per-Level String Properties")]
	public bool elasticString = false;
	public float stringLength = 12f;
	
	
	void OnDrawGizmos() {
		Gizmos.color = isPlayer2 ? Color.red : Color.blue;
		Vector3 direction = transform.TransformDirection(Vector3.down) * stringLength;
		Gizmos.DrawRay(transform.position, direction);
	}
	
	void OnDrawGizmosSelected() {
		Vector3 point = new Vector3(transform.position.x, 0f, transform.position.z);
		distanceFromGround = Vector3.Distance(transform.position, point);
		float radius;
		
		RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, stringLength);
		
		foreach (RaycastHit hit in hits) {
			if (hit.transform.IsChildOf(gameObject.transform.parent) ) {
				distanceFromGround = hit.distance;
				point = hit.point;
				break;
			}
		}
		
		radius = Mathf.Sqrt(stringLength * stringLength - distanceFromGround * distanceFromGround);
		
		Gizmos.matrix *= Matrix4x4.Translate( new Vector3(0, point.y + 0.1f, 0) );
		Gizmos.matrix *= Matrix4x4.Scale( new Vector3(1, 0, 1) );
		
		Gizmos.color = isPlayer2 ? Color.red : Color.blue;
		Gizmos.DrawWireSphere(point, radius);
		Gizmos.DrawSphere(point, 0.25f);
		
	}
	
    // Start is called before the first frame update
    void Start()
    {
		// these are intended as a guide during the Editor.
		// you can't see them during gameplay!
		Destroy(gameObject.GetComponent<Renderer>() );
		
    }

    // Update is called once per frame
    void Update()
    {
		
    }
}
