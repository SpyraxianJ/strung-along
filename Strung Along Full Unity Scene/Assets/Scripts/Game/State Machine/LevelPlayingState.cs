using UnityEngine;

public class LevelPlayingState : GameState
{
	float _deathTimer = 0.0f;
	int _attemptCount = 0;
	float _levelTime = 0.0f;
	
	public override void EnterState(GameStateManager ctx) {
		Debug.Log("Level Start!");
		PlaySetup(ctx);
	}
	
	void PlaySetup(GameStateManager ctx) {
		// reset the level to starting parameters
		Reset(ctx);
		_attemptCount = 1;
		_levelTime = 0.0f;
		
		// set puppets up for gameplay
		ctx._reaper.Respawn(ctx._player1.GetComponent<PuppetController>() );
		ctx._reaper.Respawn(ctx._player2.GetComponent<PuppetController>() );
		ctx._reaper._puppetsCanDie = true;
		ctx._player1.GetComponent<PuppetController>().gridManager = ctx._currentLevel._grid;
		ctx._player2.GetComponent<PuppetController>().gridManager = ctx._currentLevel._grid;
		
		// activate gameplay camera
		ctx._camera.SwitchCam(1);
		
		// play the tutorial for this level
		ctx._tutorial.PlayTutorial(ctx._currentLevel._tutorial);
		// enable/disable slingshotting for this level based on the flag
		ctx._player1.GetComponent<PuppetController>().canSlingshot = ctx._currentLevel._tutorial._canSlingshot;
		
	}
	
	public override void UpdateState(GameStateManager ctx) {
		if ( Fail(ctx) ) {
			// a puppet has died!
			Time.timeScale = ctx._deathSlowdown; // slow down the game
			_deathTimer += Time.unscaledDeltaTime; // count death timer
			ctx._deathEffect.weight += (Time.unscaledDeltaTime / ctx._deathTime); // fade in death screen effect
			ctx._deathEffect.GetComponent<CanvasGroup>().alpha += (Time.unscaledDeltaTime / ctx._deathTime); // fade in death image
			
			if (_deathTimer >= ctx._deathTime) {
				// death timer reached, respawn the lads.
				Reset(ctx);
				_attemptCount++;
				ctx._reaper.Respawn(ctx._player1.GetComponent<PuppetController>() );
				ctx._reaper.Respawn(ctx._player2.GetComponent<PuppetController>() );
				
				// clear all those effects from before.
				Time.timeScale = 1.0f;
				_deathTimer = 0.0f;
				ctx._deathEffect.weight = 0.0f;
				ctx._deathEffect.GetComponent<CanvasGroup>().alpha = 0.0f;
			}
			
		} else if ( Win(ctx) ) {
			// invoke any win stuff!
			ctx._currentLevelWon = true;
			PlayExit(ctx);
		} else if (ctx._interrupt) {
			// the player has requested the game end. rude.
			ctx._interrupt = false;
			PlayExit(ctx);
		}
		
		ctx._totalPlaytime += Time.unscaledDeltaTime;
		_levelTime += Time.deltaTime;
	}
	
	void PlayExit(GameStateManager ctx) {
		ctx._reaper._puppetsCanDie = false;
		ctx._player1.GetComponent<PuppetController>().gridManager = null;
		ctx._player2.GetComponent<PuppetController>().gridManager = null;
		ctx._camera.SwitchCam(0);
		
		// reset slingshot ability
		ctx._player1.GetComponent<PuppetController>().canSlingshot = true;
		
		ctx.SwitchState(ctx.EndState);
	}
	
	// to win, both goals need to be active and both puppets need to be alive.
	bool Win(GameStateManager ctx) {
		return ctx._currentLevel._p1Goal._isActive && ctx._currentLevel._p2Goal._isActive && ctx._p1Alive && ctx._p2Alive;
	}
	// to lose, either puppet must die.
	bool Fail(GameStateManager ctx) {
		return !ctx._p1Alive || !ctx._p2Alive;
	}
	
	void Reset(GameStateManager ctx) {
		ctx._currentLevel.Reset(); // reset all props
		ctx._p1Anchor.transform.position = ctx._currentLevel._p1Spawn.GetComponent<StageProp>().originalPosition;
		ctx._p2Anchor.transform.position = ctx._currentLevel._p2Spawn.GetComponent<StageProp>().originalPosition; // make sure anchors are reset too
		ctx._tutorial.ResetTutorial();
	}
	
	public int GetAttempts() {
		return _attemptCount;
	}
	
	public float GetTime() {
		return _levelTime;
	}
	
}
