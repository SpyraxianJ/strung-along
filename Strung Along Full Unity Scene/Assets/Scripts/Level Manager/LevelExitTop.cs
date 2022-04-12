using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// causes a prop to exit the stage by moving vertically upwards until a threshold boundary.
public class LevelExitTop : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{
		
	}
	
	// Update is called once per frame
	void Update()
	{
		float deltaSpeed = LevelManager.EXIT_SPEED * Time.deltaTime;
		transform.position = transform.position + Vector3.up * deltaSpeed;
		
		if (transform.position.y > LevelManager.TOP_BOUNDARY) {
			// disable the object from the scene
			this.gameObject.SetActive(false);
			// delete this component from the object
			Destroy(this);
		}
			
		
	}
	
}
