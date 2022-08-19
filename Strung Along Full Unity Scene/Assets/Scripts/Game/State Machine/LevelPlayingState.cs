using UnityEngine;

public class LevelPlayingState : LevelBaseState
{
	float _deathTimer = 0.0f;
	int _attemptCount = 0;
	float _levelTime = 0.0f;
	
	public override void EnterState(GameStateManager ctx) {
		Debug.Log("Level Start!");
		Reset(ctx._currentLevel);
		_attemptCount = 1;
		_levelTime = 0.0f;
		
		ctx._reaper.Respawn(ctx._player1.GetComponent<PuppetController>() );
		ctx._reaper.Respawn(ctx._player2.GetComponent<PuppetController>() );
		ctx._reaper._puppetsCanDie = true;
		ctx._OnPlayingStart?.Invoke();
	}
	
	public override void UpdateState(GameStateManager ctx) {
		// to win, both goals need to be active and both puppets need to be alive.
		bool winCondition = ctx._currentLevel._p1Goal._isActive && ctx._currentLevel._p2Goal._isActive && ctx._p1Alive && ctx._p2Alive;
		// to lose, either puppet must die.
		bool failCondition = !ctx._p1Alive || !ctx._p2Alive;
		
		if (failCondition) {
			// a puppet has died!
			Time.timeScale = ctx._deathSlowdown; // slow down the game
			_deathTimer += Time.unscaledDeltaTime; // count death timer
			ctx._deathEffect.weight += (Time.unscaledDeltaTime / ctx._deathTime); // fade in death screen effect
			ctx._deathEffect.GetComponent<CanvasGroup>().alpha += (Time.unscaledDeltaTime / ctx._deathTime); // fade in death image
			
			if (_deathTimer >= ctx._deathTime) {
				// death timer reached, respawn the lads.
				Reset(ctx._currentLevel);
				_attemptCount++;
				ctx._reaper.Respawn(ctx._player1.GetComponent<PuppetController>() );
				ctx._reaper.Respawn(ctx._player2.GetComponent<PuppetController>() );
				
				// clear all those effects from before.
				Time.timeScale = 1.0f;
				_deathTimer = 0.0f;
				ctx._deathEffect.weight = 0.0f;
				ctx._deathEffect.GetComponent<CanvasGroup>().alpha = 0.0f;
			}
			
		} else if (winCondition) {
			// invoke any win stuff!
			ctx._reaper._puppetsCanDie = false;
			ctx._OnPlayingExit?.Invoke();
			ctx.SwitchState(ctx.UnloadingState);
			
		} else if (ctx._interrupt) {
			// the player has requested the game end. rude.
			ctx._interrupt = false;
			ctx._reaper._puppetsCanDie = false;
			ctx._OnPlayingExit?.Invoke();
			ctx.SwitchState(ctx.UnloadingState);
			
		}
		
		ctx._totalPlaytime += Time.unscaledDeltaTime;
		_levelTime += Time.unscaledDeltaTime;
	}
	
	
	void Reset(Level level) {
		
		level._p1Goal._isActive = false;
		level._p2Goal._isActive = false;
		
		level.FreezeAll();
		level.UnfreezeAll();
		
		foreach (StageProp prop in level._props) {
			prop.transform.position = prop.originalPosition;
		}
	}
	
	public int GetAttempts() {
		return _attemptCount;
	}
	
	public float GetTime() {
		return _levelTime;
	}
	
}
