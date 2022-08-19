using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppetAudio : MonoBehaviour
{

    public PuppetController thisPuppet;

    public List<AudioClip> footsteps;
    public List<AudioClip> landings;
    public List<AudioClip> jumps;

    public AudioSource puppetAudioSource;

    [Space]

    public Vector3 lastPos;
    float stepDistanceLeft;
    public float stepDistance;

    [Space]

    public Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    // Update is called once per frame
    void FixedUpdate()
    {
        if (thisPuppet.isGrounded)
        {
            stepDistanceLeft -= (lastPos - thisPuppet.transform.position).magnitude;
        }

        lastPos = thisPuppet.transform.position;

        if (stepDistanceLeft < 0) {
            stepDistanceLeft = stepDistance;
            Step();
        }
    }

    public void Jump()
    {
        puppetAudioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        puppetAudioSource.clip = jumps[Random.Range(0, jumps.Count)];
        puppetAudioSource.Play();
    }

    public void Step()
    {
        puppetAudioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        puppetAudioSource.clip = footsteps[Random.Range(0, footsteps.Count)];
        puppetAudioSource.Play();
    }

    public void Land()
    {
        puppetAudioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        puppetAudioSource.clip = landings[Random.Range(0, landings.Count)];
        puppetAudioSource.Play();
    }

}
