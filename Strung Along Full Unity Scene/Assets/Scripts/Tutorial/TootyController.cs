using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TootyController : MonoBehaviour
{
	public float _runSpeed = 5f;
	public Vector3 _home = new Vector3(15.5f, 0f, -5f);
	[Space]
	public Animator _animator;
	
	[HideInInspector]
	public bool _inPlace = false;
	
	
    // Start is called before the first frame update
    void Start()
    {
        transform.position = _home;
    }

    // Update is called once per frame
    void Update()
    {
		
    }
	
	public void RunToPosition(Vector3 point) {
		StopAllCoroutines();
		_inPlace = false;
		_animator.SetBool("running", true);
		StartCoroutine( RunRoutine(point) );
	}
	
	IEnumerator RunRoutine(Vector3 point) {
		
		transform.position = new Vector3(transform.position.x, point.y, transform.position.z);
		
		transform.LookAt(point, Vector3.up);
		transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
		
		while ( Vector3.Distance(transform.position, point) > 0.001f ) {
			transform.position = Vector3.MoveTowards(transform.position, point, _runSpeed * Time.deltaTime);
			yield return null;
		}
		
		transform.eulerAngles = Vector3.zero;
		_inPlace = true;
		_animator.SetBool("running", false);
	}
	
	public void RunHome() {
		RunToPosition(_home);
	}
	
	public void GrappleToPosition(Vector3 point) {
		StopAllCoroutines();
		_inPlace = false;
		_animator.SetBool("grappling", true);
		StartCoroutine( GrappleRoutine(point) );
	}
	
	IEnumerator GrappleRoutine(Vector3 point) {
		
		transform.position = new Vector3(point.x, 14f, point.z); // put him in the rafters
		
		while ( Vector3.Distance(transform.position, point) > 0.001f ) {
			transform.position = Vector3.MoveTowards(transform.position, point, _runSpeed * Time.deltaTime);
			transform.Rotate(Vector3.up, 180f * Time.deltaTime);
			yield return null;
		}
		
		transform.eulerAngles = Vector3.zero;
		_inPlace = true;
		_animator.SetBool("grappling", false);
	}
	
	IEnumerator GrappleRoutine() {
		
		Vector3 point = new Vector3(transform.position.x, 14f, transform.position.z);
		
		while ( Vector3.Distance(transform.position, point) > 0.001f ) {
			transform.position = Vector3.MoveTowards(transform.position, point, _runSpeed * Time.deltaTime);
			transform.Rotate(Vector3.up, 180f * Time.deltaTime);
			yield return null;
		}
		
		transform.eulerAngles = Vector3.zero;
		transform.position = _home;
		_inPlace = true;
		_animator.SetBool("grappling", false);
	}
	
	public void GrappleHome() {
		StopAllCoroutines();
		_inPlace = false;
		_animator.SetBool("grappling", true);
		StartCoroutine( GrappleRoutine() );
	}
	
	public void Facepalm() {
		_animator.Play("Facepalm");
	}
	
	public void Cheer(bool cheer) {
		_animator.SetBool("cheer", cheer);
	}
	
	void OnGrab(PuppetController pup) {
		pup.GrabRelease(false);
		_animator.Play("Poke");
		
	}
	
	void OnReleased(PuppetController pup) {
		
	}
	
	
	
}
