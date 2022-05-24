using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

Plays a particle system on fire.
It's best to use a Particle System prefab object for the Particle field. 

*/

public class ReactParticle : Reactor
{
	
	[Header("Particle Properties")]
	[Tooltip("Particle System to play. Check the Prefabs!")]
	public ParticleSystem particle;
	private ParticleSystem particleInstance;
	[Tooltip("Turn Looping particles on and off with each fire.")]
	public bool toggle = false;
	[Tooltip("If true, particle emission rotates with the object.")]
	public bool useLocalRotation = false;
	[Tooltip("If true, particle size is scaled with the object.")]
	public bool useLocalScale = true;
	
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
		
    }

    // Update is called once per frame
    void Update()
    {
		
		if (particleInstance != null) {
			updatePosition();
		}
		
		
    }
	
	private void updatePosition() {
		if (targetObject.transform.hasChanged && particleInstance.isPlaying) {
			
			particleInstance.transform.position = targetObject.transform.position;
			if (useLocalRotation) {
				particleInstance.transform.rotation = targetObject.transform.rotation;
			}
			if (useLocalScale) {
				particleInstance.transform.localScale = targetObject.transform.lossyScale;
			}
			
		}
		
	}
	
	
	public override void fire(float progress) {
		
		if (progress == 1.0f && ready && targetObject.activeSelf) {
			ready = false;
			
			if (particleInstance == null) {
				particleInstance = Instantiate(particle, targetObject.transform.position, Quaternion.identity);
			}
			
			if ( !particleInstance.isEmitting ) {
				particleInstance.Play();
			} else if (toggle) {
				particleInstance.Stop();
			}
			
		} else if (progress < 1.0f) {
			ready = true;
		}
		
	}
	
	public override void reset() {
		
	}
	
	public override void checkErrors() {
		if (particle == null) {
			Debug.LogError(this + ": no reference to the Particle System to use.");
		}
	}
	
	
}
