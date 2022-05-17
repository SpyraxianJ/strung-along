using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActvOnTouch : Activator
{
	[Tooltip("By default, only puppets can activate.\nYou can optionally add other colliders aside the puppets.")]
	public List<Collider> extraActivators;
	[Header("Touch Properties")]
	public float activationTime = 2f;
	public float resetTime = 0f;
	[Tooltip("e.g. if two puppets are touching, it activates twice as fast.")]
	public bool accumulative = true;
	public Vector3 touchDistance = new Vector3(0.5f, 1f, 0.5f);
	[Space]
	[Header("Debug")]
	public List<Collider> currentActivators;
	[Range(0,1)]
	public float lerpProgress = 0.0f;
	public List<Collider> currentHits;
	public Collider p1Collider;
	public Collider p2Collider;
	
	
	public override void checkErrors() {
		
	}
	
	public override void reset() {
		currentActivators.Clear();
		lerpProgress = 0.0f;
	}
	
	
    // Start is called before the first frame update
    void Start()
    {
        currentHits = new List<Collider>();
		currentActivators = new List<Collider>();
		p1Collider = manager.player1.GetComponent<Collider>();
		p2Collider = manager.player2.GetComponent<Collider>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		// reimplementing OnCollision because unity's suck ass
		handleTouches();
		
		float speedFactor;
		if (currentActivators.Count > 0) {
			float scaledActivationTime = accumulative ? activationTime / currentActivators.Count : activationTime;
			speedFactor = Time.deltaTime / scaledActivationTime;
			lerpProgress = lerpProgress < 1.0f ? lerpProgress += speedFactor : lerpProgress = 1.0f;
		}
		else {
			speedFactor = Time.deltaTime / resetTime;
			lerpProgress = lerpProgress > 0.0f ? lerpProgress -= speedFactor : lerpProgress = 0.0f;
		}
		
		fireAll(lerpProgress);
		
    }
	
	private void handleTouches() {
		Collider[] hitArray = Physics.OverlapBox(transform.position, transform.localScale / 2 + touchDistance, transform.rotation);
		
		List<Collider> newHits = new List<Collider>(hitArray);
		List<Collider> entries = newHits.Except(currentHits).ToList();
		List<Collider> exits = currentHits.Except(newHits).ToList();
		
		
		foreach (Collider entry in entries) {
			currentHits.Add(entry);
			onTouchEnter(entry);
		}
		foreach (Collider exit in exits) {
			currentHits.Remove(exit);
			onTouchExit(exit);
		}
		foreach (Collider hit in currentHits) {
			onTouchStay(hit);
		}
	}
	
	private void onTouchEnter(Collider other) {
		
		if (other == p1Collider && p1CanActivate) {
			currentActivators.Add(other);
			
		}
		if (other == p2Collider && p2CanActivate) {
			currentActivators.Add(other);
		}
		
		foreach (Collider otherActivator in extraActivators) {
			if (other == otherActivator) {
				currentActivators.Add(other);
			}
		}
		
		
		
	}
	private void onTouchStay(Collider other) {
		if (other == p1Collider && p1CanActivate) {
			//isActivating = true;
			
		}
		if (other == p2Collider && p2CanActivate) {
			//isActivating = true;
		}
		
		
		foreach (Collider otherActivator in extraActivators) {
			if (other == otherActivator) {
				//isActivating = true;
			}
		}
		
	}
	private void onTouchExit(Collider other) {
		currentActivators.Remove(other);
	}
	
	
	
}
