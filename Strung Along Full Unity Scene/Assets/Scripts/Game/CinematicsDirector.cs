using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CinematicsDirector : MonoBehaviour
{
	public PlayableAsset _fadeOut;
	public PlayableAsset _intro;
	public PlayableAsset _levelStart;
	public PlayableAsset _levelEnd;
	public PlayableAsset _actChange;
	public PlayableAsset _act3WallBreak;
	public PlayableAsset _endOfGame;
	
	[HideInInspector]
	public PlayableDirector _director;
	
    void Start() {
        _director = GetComponent<PlayableDirector>();
    }
	
	public bool Playing() {
		return _director.state == PlayState.Playing;
	}
	
	public void PlayCutscene(PlayableAsset cinematic) {
		SkipCutscene();
		_director.Stop();
		_director.playableAsset = cinematic;
		_director.RebuildGraph();
		_director.Play();
	}
	
	public void SkipCutscene() {
		if (Playing() ) {
			_director.time = _director.duration;
			_director.Evaluate();
		}
		
	}
	
	// during cinematics we often want the puppets to stand still and play an animation or something.
	public void DisablePlayers(GameObject player1, GameObject player2) {
		// disable movement.
		player1.GetComponent<PuppetController>().move = Vector2.zero;
		player2.GetComponent<PuppetController>().move = Vector2.zero;
		
		// they can still jump though because its funny
		player1.GetComponent<Rigidbody>().velocity.Scale( new Vector3(0f, 1f, 0f) );
		player2.GetComponent<Rigidbody>().velocity.Scale( new Vector3(0f, 1f, 0f) );
	}
	
	// make puppets turn towards an object.
	public void ForceLook(GameObject player, Vector3 lookAt) {
		lookAt.y = 0.0f;
		player.GetComponent<PuppetController>().visualReference.transform.LookAt(lookAt, Vector3.up);
	}
	
	// make puppets turn towards the audience (the front of the stage).
	public void ForceLook(GameObject player) {
		player.GetComponent<PuppetController>().visualReference.transform.LookAt(-Vector3.forward, Vector3.up);
	}
	
}
