using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// causes a prop to enter the stage by moving vertically down.
// has no function if the prop is already at the target location.
public class LevelEnterTop : MonoBehaviour
{
	
	private Vector3 target;
	
    // Start is called before the first frame update
    void Start()
    {
        this.target = gameObject.GetComponent<StageProp>().originalPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float deltaSpeed = LevelManager.ENTRY_SPEED * Time.deltaTime;
		transform.position = Vector3.MoveTowards(transform.position, target, deltaSpeed);
		
		if (Vector3.Distance(transform.position, target) < 0.001f) {
			// close enough, teleport it to final position.
			transform.position = target;
			// delete this component from the object
			Destroy(this);
		}
		
    }
}
