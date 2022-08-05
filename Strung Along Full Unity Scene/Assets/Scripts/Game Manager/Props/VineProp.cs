using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineProp : MonoBehaviour
{
	public LayerMask _puppetMask;
	public Vector3 _detector = new Vector3(4.5f, 0.8f, 4.5f);
	
	GrimReaper _reaper;
	Material _mat;
	Color _glowColor;
	
	void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(transform.position, _detector);
	}
	
    void Start() {
        _reaper = GetComponentInParent<GrimReaper>();
		
		_mat = transform.GetChild(1).GetComponent<MeshRenderer>().material;
		_mat.EnableKeyword("_EMISSION");
		_glowColor = _mat.color;
    }
	
	void Update() {
		HandleGlow();
	}
	
	void HandleGlow() {
		_mat.SetColor("_EmissionColor", Color.Lerp(Color.black, _glowColor, Mathf.PingPong(Time.time / 2f, 1f) ) );
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
