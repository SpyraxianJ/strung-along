using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CinematicsDirector : MonoBehaviour
{
	public PlayableAsset _intro;
	public PlayableAsset _actChange;
	public PlayableAsset _endOfGame;
	
	[HideInInspector]
	public PlayableDirector _director;
	
    // Start is called before the first frame update
    void Start() {
        _director = GetComponent<PlayableDirector>();
    }
	
	public void PlayCutscene(PlayableAsset cinematic) {
		_director.Stop();
		_director.playableAsset = cinematic;
		_director.RebuildGraph();
		_director.Play();
	}
	
}
