using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// tracks whether the object is currently touched by the designated player.
//

public class Goal : MonoBehaviour
{
	
	
	[Tooltip("Determines if this is the goal for Player 2. Otherwise Player 1.")]
	public bool isPlayer2;
	
	public bool _isActive = false;
	
	[Header("Debug")]
	private GameStateManager _ctx;
	private Color _originalColor;
	private GameObject _targetPlayer; // reference to the player GameObject this goal waits for.
	
	
    // Start is called before the first frame update
    void Start()
    {
        
		// the goal can only be triggered by the right player!
		if (isPlayer2) {
			_targetPlayer = _ctx._player2;
		} else {
			_targetPlayer = _ctx._player1;
		}
		
		_originalColor = gameObject.GetComponent<Renderer>().material.color;
		
    }

    // Update is called once per frame
    void Update()
    {
        
		if (_isActive) {
			gameObject.GetComponent<Renderer>().material.color = Color.green;
		} else {
			gameObject.GetComponent<Renderer>().material.color = _originalColor;
		}
		
    }
	
	void OnTriggerEnter(Collider other) {
		if (other.gameObject == _targetPlayer) {
			_isActive = true;
		}
		
	}
	
	void OnTriggerStay(Collider other) {
		if (other.gameObject == _targetPlayer) {
			_isActive = true;
		}
	}
	
	void OnTriggerExit(Collider other) {
		if (other.gameObject == _targetPlayer) {
			_isActive = false;
		}
		
	}
	
	void OnDisable() {
		_isActive = false;
	}
	
	
}
