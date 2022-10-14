using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AdminInputHandler : MonoBehaviour
{
	GameStateManager _ctx;
	PauseMenu _pause;
	
	[Tooltip("")]
	public float _titleScreenTimeout = 60f;
	float _timeoutTimer = 0f;
	
    // Start is called before the first frame update
    void Start()
    {
		_ctx = FindObjectOfType<GameStateManager>();
		_pause = FindObjectOfType<PauseMenu>();
    }

    // Update is called once per frame
    void Update()
    {
		if (_ctx._player1.GetComponent<PuppetController>().movePressed || _ctx._player2.GetComponent<PuppetController>().movePressed) {
			_timeoutTimer = 0;
		} else {
			_timeoutTimer += Time.deltaTime;
		}
		
		if (_timeoutTimer >= _titleScreenTimeout) {
			_timeoutTimer = 0;
			_ctx.QuitToTitle();
		}
		
    }
	
	void OnRestart(InputValue input) {
		Debug.Log(this + ": RESTARTING LEVEL");
		
		_ctx._reaper.Kill( _ctx._player1.GetComponent<PuppetController>() );
		_ctx._reaper.Kill( _ctx._player2.GetComponent<PuppetController>() );
	}
	
	void OnTitleScreen(InputValue input) {
		Debug.Log(this + ": RETURNING TO TITLE");
		
		_ctx.QuitToTitle();
	}
	
	void OnSkip(InputValue input) {
		Debug.Log(this + ": SKIPPING LEVEL");
		
		_ctx.RequestSkip();
	}
	
	void OnPause(InputValue input) {
		Debug.Log(this + ": PAUSED GAME");
		
		if (PauseMenu.GameIsPaused) _pause.Resume(); else _pause.Pause();
	}
	
	// fucking
	void OnLoad11(InputValue input) {
		_ctx.RequestAct(1);
		_ctx.RequestLevel(1);
	}
	void OnLoad12(InputValue input) {
		_ctx.RequestAct(1);
		_ctx.RequestLevel(2);
	}
	void OnLoad13(InputValue input) {
		_ctx.RequestAct(1);
		_ctx.RequestLevel(3);
	}
	void OnLoad21(InputValue input) {
		_ctx.RequestAct(2);
		_ctx.RequestLevel(1);
	}
	void OnLoad22(InputValue input) {
		_ctx.RequestAct(2);
		_ctx.RequestLevel(2);
	}
	void OnLoad23(InputValue input) {
		_ctx.RequestAct(2);
		_ctx.RequestLevel(3);
	}
	void OnLoad31(InputValue input) {
		_ctx.RequestAct(3);
		_ctx.RequestLevel(1);
	}
	void OnLoad32(InputValue input) {
		_ctx.RequestAct(3);
		_ctx.RequestLevel(2);
	}
	void OnLoad33(InputValue input) {
		_ctx.RequestAct(3);
		_ctx.RequestLevel(3);
	}
	
}
