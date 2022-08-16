using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
when a player grabs something, these 3 messages are sent to the grabbed object:
OnGrab for the frame it is grabbed
OnGrabbing for every frame it remains grabbed
OnRelease for the frame grab ends

a grabbed object might need a separate collider object so the player grab raycast
can hit reliably. this class lets a separate collider propagate the relevant messages
to the right gameobject when it's grabbed.

for example the lever: i have a Grab Handle gameobject that's nice and big, easy to grab.
but the Lever parent gameobject is where all the scripts run. this class goes on the
Grab Handle and tells the Lever what's goin on.
**/
[RequireComponent(typeof(Collider))]
public class GrabListener : MonoBehaviour
{
	public GameObject _listener;
	
	void Start() {
		GetComponent<Collider>().isTrigger = true;
	}

	void OnGrab(PuppetController pup) {
		_listener.SendMessage("OnGrab", pup);
	}
	
	void OnGrabbing(PuppetController pup) {
		_listener.SendMessage("OnGrabbing", pup);
	}
	
	void OnReleased(PuppetController pup) {
		transform.localPosition = Vector3.zero;
		_listener.SendMessage("OnReleased", pup);
	}
	
}
