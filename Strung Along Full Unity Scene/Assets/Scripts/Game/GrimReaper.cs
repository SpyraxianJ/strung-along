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
    public LogRecorder logRec;

    [Space]

    public AudioSource defeatAudio;

    void Start()
    {
		_ctx = GetComponent<GameStateManager>();
    }	
	
	public void Kill(PuppetController pup) {
		
		if (_puppetsCanDie) {
			
			Untangle(pup);
			
			// disable strings
			LineRenderer anchorLineVisual = pup.thisStringRoot.lineVisual;
			PuppetString puppetString = pup.secondPlayer ? pup.stringManager.string2Ref : pup.stringManager.string1Ref;
		
			anchorLineVisual.enabled = false;
			puppetString.GetComponent<CapsuleCollider>().enabled = false;
		
			Instantiate(_deathParticle, pup.transform.position, Quaternion.identity);
		
			// disable puppet
			GameObject puppet = pup.gameObject;
		
			puppet.SetActive(false);
		
			if (pup.secondPlayer) {
				_ctx._p2Alive = false;
			} else {
				_ctx._p1Alive = false;
			}

            defeatAudio.Play();

            // Hi Tim here, this is a little bit of code to record when players die
            try
            {
                if (logRec != null)
                {
                    if (pup.secondPlayer)
                    {
                        logRec.Death(_ctx._player2.transform.position, _ctx._player2.GetComponent<Rigidbody>().velocity, false);
                    }
                    else
                    {
                        logRec.Death(_ctx._player2.transform.position, _ctx._player2.GetComponent<Rigidbody>().velocity, true);
                    }
                }
            }
            catch (System.Exception)
            {
                Debug.LogWarning("No log recorder found");
            }
			
		}
		
		
		
	}
	
	public void Respawn(PuppetController pup) {
		
		Untangle(pup);
		
		GameObject puppet = pup.gameObject;
		
		// disable puppet: this is in case an already alive puppet is respawned, otherwise setting their position fails.
		puppet.SetActive(false);
		
		// set puppet velocity to zero
		Rigidbody pupBody = puppet.GetComponent<Rigidbody>();
		pupBody.velocity = Vector3.zero;
		
		// place puppet under their spawnpoint
		puppet.transform.position = pup.thisStringRoot.transform.position + new Vector3(0, -4, 0);
		
		// enable strings
		LineRenderer anchorLineVisual = pup.thisStringRoot.lineVisual;
		PuppetString puppetString = pup.secondPlayer ? pup.stringManager.string2Ref : pup.stringManager.string1Ref;
		
		anchorLineVisual.enabled = true;
		puppetString.GetComponent<CapsuleCollider>().enabled = true;
		
		// enable puppet
		puppet.SetActive(true);
		
		Instantiate(_respawnParticle, pup.transform.position, Quaternion.identity);
		
		if (pup.secondPlayer) {
			_ctx._p2Alive = true;
		} else {
			_ctx._p1Alive = true;
		}
		
	}
	
	void Untangle(PuppetController pup) {
		// untangle both puppets
		pup.stringManager.tangle = 0;
		pup.stringManager.bolConnected = false;
		
	}
	
	
}
