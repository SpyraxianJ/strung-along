using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// moves a prop to the target location.
// starts disabled. set the target position, then set MoveProp.enabled!
public class MoveProp : MonoBehaviour
{
	
	public Vector3 target; // the position to move this prop to
	public float moveSpeed = 0.2f; // how fast it moves
	public float maxSpeed = 20f; // maximum speed allowed
	public float targetMargin = 0.01f; // distance between prop and target before setting position
	
	private Vector3 velocity = Vector3.zero;
	
	
	void Awake() {
		enabled = false;
	}
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, moveSpeed, maxSpeed);
		
		if (Vector3.Distance(transform.position, target) < targetMargin) {
			// close enough, teleport it to final position.
			transform.position = target;
			
			// delete this component from the object
			Destroy(this);
		}
		
    }
}
