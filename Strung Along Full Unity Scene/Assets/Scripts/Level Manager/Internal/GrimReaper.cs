using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrimReaper : MonoBehaviour
{
	public ParticleSystem deathParticle;
	public float deathWait = 2.0f;
	public ParticleSystem respawnParticle;
	
	private LevelManager manager;
	
    // Start is called before the first frame update
    void Start()
    {
		manager = GetComponent<LevelManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	
	public void kill(PuppetController pup) {
		
		// disable strings
		LineRenderer anchorLineVisual = pup.thisStringRoot.lineVisual;
		PuppetString puppetString = pup.secondPlayer ? pup.stringManager.string2Ref : pup.stringManager.string1Ref;
		
		anchorLineVisual.enabled = false;
		puppetString.GetComponent<CapsuleCollider>().enabled = false;
		puppetString.GetComponent<Renderer>().enabled = false;
		
		// play puppet death animation here
		
		StartCoroutine( waitForDeathAnim(pup)  );
		
	}
	
	IEnumerator waitForDeathAnim(PuppetController pup) {
		
		// yield return new WaitUntil() anim has finished
		
		Instantiate(deathParticle, pup.transform.position, Quaternion.identity);
		
		// disable puppet
		GameObject puppet = pup.gameObject;
		
		puppet.SetActive(false);
		
		yield return new WaitForSeconds(deathWait);
		
		if (pup.secondPlayer) {
			manager.p2.alive = false;
		} else {
			manager.p1.alive = false;
		}
		
	}
	
	public void respawn(PuppetController pup) {
		
		// place puppet under their spawnpoint
		GameObject puppet = pup.gameObject;
		puppet.transform.position = pup.thisStringRoot.transform.position + new Vector3(0, -4, 0);
		
		// set puppet velocity to zero
		Rigidbody pupBody = puppet.GetComponent<Rigidbody>();
		pupBody.velocity = Vector3.zero;
		
		// enable strings
		LineRenderer anchorLineVisual = pup.thisStringRoot.lineVisual;
		PuppetString puppetString = pup.secondPlayer ? pup.stringManager.string2Ref : pup.stringManager.string1Ref;
		
		anchorLineVisual.enabled = true;
		puppetString.GetComponent<CapsuleCollider>().enabled = true;
		puppetString.GetComponent<Renderer>().enabled = true;
		
		// enable puppet
		puppet.SetActive(true);
		
		Instantiate(respawnParticle, pup.transform.position, Quaternion.identity);
		
	}
	
	
}
