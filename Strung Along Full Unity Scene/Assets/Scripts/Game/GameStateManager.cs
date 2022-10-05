using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
	// state machine
	GameState _currentState;
	public GameEmptyState EmptyState = new GameEmptyState();
	
	// level loop states
	public LevelLoadingState LoadingState = new LevelLoadingState();
	public LevelStartState StartState = new LevelStartState();
	public LevelPlayingState PlayingState = new LevelPlayingState();
	public LevelEndState EndState = new LevelEndState();
	public LevelUnloadingState UnloadingState = new LevelUnloadingState();
	
	// event states
	public EventIntroState IntroState = new EventIntroState();
	public EventActChangeState ActChangeState = new EventActChangeState();
	public EventWallBreakState WallBreakState = new EventWallBreakState();
	public EventGameEndState GameEndState = new EventGameEndState();
	
	
	public const float TOP_BOUNDARY = 24f; // the "top" of the level. Y coordinate.
	public const float BOTTOM_BOUNDARY = -10f;
	public const float SIDE_BOUNDARY = 34f;// the "side" of the level. X coordinate.

    public LogRecorder logRec;

    public enum Direction {
		Top,
		Left,
		Right,
		Scrolling,
		Bottom
	}
	
	[Header("References")]
	public GameObject _player1; // refs to the player objects
	public GameObject _player2;
	public StringRoot _p1Anchor; // refs to string anchor locations
	public StringRoot _p2Anchor;
	[HideInInspector] public GrimReaper _reaper;
	[HideInInspector] public LevelDatabase _database;
	[HideInInspector] public StageCamController _camera;
	[HideInInspector] public StagelightController _lights;
	[HideInInspector] public CinematicsDirector _cinematics;
	[HideInInspector] public TutorialListener _tutorial;
	
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
	[Tooltip("Duration of victory fanfare after completing a level.")]
	public float _victoryTime = 3.0f;
	
	[Header("no time for this fancy bullshit")]
	[Tooltip("Instead of loading 1-1, show level select on game start.")]
	public bool _levelSelectOnStart = false;
	public bool _skipIntro = false;
	public bool _skipLevelSwitch = false;
	public bool _skipActChanges = false;
	
	[HideInInspector] public bool _playIntro = true;
	[HideInInspector] public bool _actChange = false;
	[HideInInspector] public bool _gameEnd = false;
	[HideInInspector] public bool _interrupt = false;
	[HideInInspector] public bool _p1Alive = true;
	[HideInInspector] public bool _p2Alive = true;
	
	[HideInInspector] public Level _nextLevelToLoad = null;
	[HideInInspector] public Level _currentLevel = null;
	[HideInInspector] public bool _currentLevelWon = false;
	[HideInInspector] public int _currentAct = 0;
	[HideInInspector] public float _totalPlaytime = 0.0f;
	

    void Start() {
		_reaper = GetComponent<GrimReaper>();
		_database = GetComponent<LevelDatabase>();
		_camera = FindObjectOfType<StageCamController>();
		_lights = FindObjectOfType<StagelightController>();
		_tutorial = FindObjectOfType<TutorialListener>();
		
		
        _currentState = EmptyState;
		_currentState.EnterState(this);
		
    }

    void Update() {
        _currentState.UpdateState(this);
    }
	
	public void SwitchState(GameState state) {
		_currentState = state;
		_currentState.EnterState(this);
	}
	
	// UI integration functions
	
	// START GAME
	// start the game from the first level!
	public void StartGame() {
		if (_levelSelectOnStart) {
			_skipIntro = true;
			PauseMenu menu = FindObjectOfType<PauseMenu>();
			menu.Pause();
			menu.OpenLevelSelectUI();
		} else {
			_nextLevelToLoad = _database.GetFirstLevel();
		}
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
	
	// SKIP LEVEL OR CUTSCENE
	// tell the game to quit, but don't prevent it from loading whatever level is
	// next in line. this simply acts as skipping the level!
	// it also skips cutscenes, if one is playing.
	public void RequestSkip() {
		_interrupt = _currentState == PlayingState ? true : false;
		_cinematics.SkipCutscene();
	}
	
	// LEVEL SELECT
	// tell the game to quit whatever level is playing and load the
	// requested level.
	public void RequestAct(int act)
    {
		_currentAct = act;
    } 

	public void RequestLevel(int level) {
		_interrupt = _currentState == PlayingState ? true : false;
		_nextLevelToLoad = _database.GetByIndex(_currentAct, level);
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
