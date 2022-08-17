using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineProp : MonoBehaviour
{
	public LayerMask _puppetMask;
	public Vector3 _detector = new Vector3(4.5f, 0.8f, 4.5f);
	
	GrimReaper _reaper;
	Material _mat;
	
	void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(transform.position, _detector);
	}
	
    void Start() {
        _reaper = GetComponentInParent<GrimReaper>();
    }
	
	void Update() {
		
	}
	

    void FixedUpdate() {
		CheckTouch();
    }
	
	void CheckTouch() {
		Collider[] hits;
		hits = Physics.OverlapBox(transform.position, _detector / 2f, transform.rotation, _puppetMask);
		
		foreach (Collider hit in hits) {
			
			_reaper.Kill( hit.GetComponent<PuppetController>() );
			
		}
	}
	
}