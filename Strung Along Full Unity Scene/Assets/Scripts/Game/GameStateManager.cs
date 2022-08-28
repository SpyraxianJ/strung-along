using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
	// state machine control
	LevelBaseState _currentState;
	public LevelEmptyState EmptyState = new LevelEmptyState();
	public LevelLoadingState LoadingState = new LevelLoadingState();
	public LevelFirstPlayingState FirstPlayingState = new LevelFirstPlayingState();
	public LevelPlayingState PlayingState = new LevelPlayingState();
	public LevelUnloadingState UnloadingState = new LevelUnloadingState();
	
	public const float TOP_BOUNDARY = 24f; // the "top" of the level. Y coordinate.
	public const float BOTTOM_BOUNDARY = -10f;
	public const float SIDE_BOUNDARY = 34f; // the "side" of the level. X coordinate.
	
	public enum Direction {
		Top,
		Side,
		Bottom
	}
	
	[Header("References")]
	public GameObject _player1; // refs to the player objects
	public GameObject _player2;
	public StringRoot _p1Anchor; // refs to string anchor locations
	public StringRoot _p2Anchor;
	public StageCamController _camera;
	public StagelightController _lights;
	public CinematicsDirector _cinematics;
	
	[Header("Transition Settings")]
	[Tooltip("Time in seconds it takes for props to move into the level.")]
	public float _loadTime = 2.0f;
	[Tooltip("Time in seconds it takes for props to move out of the level.")]
	public float _unloadTime = 2.0f;
	[Header("Gameplay Settings")]
	[Tooltip("Time in seconds for puppets to respawn and level to reset after one of them dies.")]
	public float _deathTime = 2.0f;
	[Tooltip("Game speed slowdown when a puppet dies. So dramatic!")]
	public float _deathSlowdown = 0.5f;
	[Tooltip("Volume effect that fades in when a puppet dies. Also fades in any Image placed as a child.")]
	public Volume _deathEffect;
	
	
	[Header("Debug")]
	[Tooltip("it was getting annoying")]
	public bool _skipIntroCinematic = false;
	[HideInInspector]
	public bool _p1Alive = true;
	[HideInInspector]
	public bool _p2Alive = true;
	[HideInInspector]
	public Level _nextLevelToLoad = null;
	[HideInInspector]
	public Level _currentLevel = null;
	[HideInInspector]
	public float _totalPlaytime = 0.0f;
	[HideInInspector]
	public bool _interrupt = false;
	[HideInInspector]
	public GrimReaper _reaper;
	[HideInInspector]
	public LevelDatabase _database;


	
    void Start()
    {
		_reaper = GetComponent<GrimReaper>();
		_database = GetComponent<LevelDatabase>();
		//_camera = GetComponentInParent<StageCamController>();
		//_lights = 
		
		
        _currentState = EmptyState;
		_currentState.EnterState(this);
		
    }

    void Update()
    {
        _currentState.UpdateState(this);
    }
	
	public void SwitchState(LevelBaseState state) {
		_currentState = state;
		_currentState.EnterState(this);
	}
	
	// UI integration functions
	
	// START GAME
	// start the game from the first level!
	public void StartGame() {
		_nextLevelToLoad = _database.GetFirstLevel();
	}
	
	// QUIT LEVEL
	// tell the game to quit. this is checked in LevelPlayingState, and if it's
	// true, the level will end. the next level in line is set to null so that the
	// game essentially waits for the player to choose what to do next.
	public void RequestQuit() {
		_interrupt = _currentState == PlayingState ? true : false;
		_nextLevelToLoad = null;
	}
	
	// QUIT LEVEL (TO TITLE SCREEN)
	public void QuitToTitle() {
		_cinematics.PlayCutscene( _cinematics._fadeOut );
		Invoke("LoadTitleScene", (float)_cinematics._fadeOut.duration);
	}
	void LoadTitleScene() {
		SceneManager.LoadScene("TitleScreen");
	}
	
	// SKIP LEVEL
	// tell the game to quit, but don't prevent it from loading whatever level is
	// next in line. this simply acts as skipping the level!
	public void RequestSkip() {
		_interrupt = _currentState == PlayingState ? true : false;
	}
	
	// LEVEL SELECT
	// tell the game to quit whatever level is playing and load the
	// requested level.
	public void RequestLevel(int act, int level) {
		_interrupt = _currentState == PlayingState ? true : false;
		_nextLevelToLoad = _database.GetByIndex(act, level);
	}
	
	// CURRENT LEVEL INFO
	// current level number.
	public int GetCurrentLevel() {
		return _currentLevel == null ? -1 : _database.GetLevelNum(_currentLevel);
	}
	// current act number.
	public int GetCurrentAct() {
		return _currentLevel == null ? -1 : _database.GetActNum(_currentLevel);
	}
	// number of attempts at the current level.
	public int GetLevelAttempts() {
		return PlayingState.GetAttempts();
	}
	// playtime of the current level.
	public float GetLevelPlaytime() {
		return PlayingState.GetTime();
	}
	
	// GAME INFO
	// get the total time players have been in the game for.
	public float GetPlaytime() {
		return _totalPlaytime;
	}
	// set the total playtime. used for the save+load function.
	public void SetPlaytime(float t) {
		_totalPlaytime = t;
	}
	
}
