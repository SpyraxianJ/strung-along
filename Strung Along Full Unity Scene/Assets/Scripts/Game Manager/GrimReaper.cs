using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrimReaper : MonoBehaviour
{
	public ParticleSystem _deathParticle;
	public ParticleSystem _respawnParticle;
	[HideInInspector]
	public bool _puppetsCanDie = false;
	GameStateManager _ctx;
	
    void Start()
    {
		_ctx = GetComponent<GameStateManager>();
    }	
	
	public void Kill(PuppetController pup) {
		
		if (_puppetsCanDie) {
			
			// untangle both puppets
			pup.stringManager.tangle = 0;
			pup.stringManager.bolConnected = false;
		
			// disable strings
			LineRenderer anchorLineVisual = pup.thisStringRoot.lineVisual;
			PuppetString puppetString = pup.secondPlayer ? pup.stringManager.string2Ref : pup.stringManager.string1Ref;
		
			anchorLineVisual.enabled = false;
			puppetString.GetComponent<CapsuleCollider>().enabled = false;
			puppetString.GetComponent<Renderer>().enabled = false;
		
			Instantiate(_deathParticle, pup.transform.position, Quaternion.identity);
		
			// disable puppet
			GameObject puppet = pup.gameObject;
		
			puppet.SetActive(false);
		
			if (pup.secondPlayer) {
				_ctx._p2Alive = false;
			} else {
				_ctx._p1Alive = false;
			}
			
		}
		
		
		
	}
	
	public void Respawn(PuppetController pup) {
		
		// place puppet under their spawnpoint
		GameObject puppet = pup.gameObject;
		puppet.transform.position = pup.thisStringRoot.transform.position + new Vector3(0, -4, 0);
		
		// set puppet velocity to zero
		Rigidbody pupBody = puppet.GetComponent<Rigidbody>();
		pupBody.velocity = Vector3.zero;
		
		// enable puppet
		puppet.SetActive(true);
		
		// enable strings
		LineRenderer anchorLineVisual = pup.thisStringRoot.lineVisual;
		PuppetString puppetString = pup.secondPlayer ? pup.stringManager.string2Ref : pup.stringManager.string1Ref;
		
		anchorLineVisual.enabled = true;
		puppetString.GetComponent<CapsuleCollider>().enabled = true;
		puppetString.GetComponent<Renderer>().enabled = true;
		
		Instantiate(_respawnParticle, pup.transform.position, Quaternion.identity);
		
		if (pup.secondPlayer) {
			_ctx._p2Alive = true;
		} else {
			_ctx._p1Alive = true;
		}
		
	}
	
	
}
