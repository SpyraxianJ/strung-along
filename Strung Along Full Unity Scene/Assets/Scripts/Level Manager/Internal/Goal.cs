using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// tracks whether the object is currently touched by the designated player.
//

public class Goal : MonoBehaviour
{
	
	public LevelManager manager;
	[Tooltip("Determines if this is the goal for Player 2. Otherwise Player 1.")]
	public bool isPlayer2;
	
	public static event Action<bool, bool> onPlayerGoal;
	[Header("Debug")]
	private Color originalColor;
	private GameObject targetPlayer; // reference to the player GameObject this goal waits for.
	
	
    // Start is called before the first frame update
    void Start()
    {
        
		// the goal can only be triggered by the right player!
		if (isPlayer2) {
			targetPlayer = manager.player2;
		} else {
			targetPlayer = manager.player1;
		}
		
		originalColor = gameObject.GetComponent<Renderer>().material.color;
		
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	void OnTriggerEnter(Collider other) {
		
		if (other.gameObject == targetPlayer) {
			onPlayerGoal?.Invoke(true, isPlayer2);
			gameObject.GetComponent<Renderer>().material.color = Color.green;
			StartCoroutine ( pulseAnimation() );
		}
		
	}
	
	void OnTriggerExit(Collider other) {
		
		if (other.gameObject == targetPlayer) {
			onPlayerGoal?.Invoke(false, isPlayer2);
			gameObject.GetComponent<Renderer>().material.color = originalColor;
		}
		
	}
	
	// make the goal object pulse larger then back to original size.
	// some visual feedback!
	IEnumerator pulseAnimation() {
		
		float pulseSize = 0.5f; // pulse to 50% larger on goal touch.
		float pulseIncrement = 0.05f; // how much to change size on each frame.
		
		Vector3 scaleChange = new Vector3(pulseIncrement, pulseIncrement, pulseIncrement);
		
		for (float scaleFactor = 0f; scaleFactor < pulseSize; scaleFactor += pulseIncrement) {
			
			gameObject.transform.localScale += scaleChange;
			yield return null;
		}
		for (float scaleFactor = pulseSize; scaleFactor > 0f; scaleFactor -= pulseIncrement) {
			
			gameObject.transform.localScale -= scaleChange;
			yield return null;
		}
	}
	
	
}
