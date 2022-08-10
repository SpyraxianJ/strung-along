using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events;

public class GameStateManager : MonoBehaviour
{
	// state machine control
	LevelBaseState _currentState;
	public LevelEmptyState EmptyState = new LevelEmptyState();
	public LevelLoadingState LoadingState = new LevelLoadingState();
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
	public bool _p1Alive = true;
	public bool _p2Alive = true;
	//[HideInInspector]
	public Level _nextLevelToLoad = null;
	[HideInInspector]
	public Level _currentLevel = null;
	[HideInInspector]
	public float _totalPlaytime = 0.0f;
	[HideInInspector]
	public bool _quitting = false;
	[HideInInspector]
	public GrimReaper _reaper;
	[HideInInspector]
	public LevelDatabase _database;
	
	// events for camera
	public UnityEvent _OnPlayingStart;
	public UnityEvent _OnPlayingExit;
	
	
    void Start()
    {
		_reaper = GetComponent<GrimReaper>();
		_database = GetComponent<LevelDatabase>();
		
		
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
	
	// start the game from the first level!
	public void StartGame() {
		_nextLevelToLoad = _database.GetFirstLevel();
	}
	// start the game from a specific level.
	public void SetNextLevel(int act, int level) {
		_nextLevelToLoad = _database.GetByIndex(act, level);
	}
	// tell the game to quit. it'll wait until it's ready to do so.
	public void RequestQuit() {
		_quitting = true;
		_nextLevelToLoad = null;
	}
	// get the current level number.
	public int GetCurrentLevel() {
		return _database.GetLevelNum(_currentLevel);
	}
	public int GetCurrentAct() {
		return _database.GetActNum(_currentLevel);
	}
	public float GetPlaytime() {
		return _totalPlaytime;
	}
	public void SetPlaytime(float t) {
		_totalPlaytime = t;
	}
	
}
