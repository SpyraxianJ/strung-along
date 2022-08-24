using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelFirstPlayingState : LevelBaseState
{
	CinematicIntro _cinematic;
	
	
    public override void EnterState(GameStateManager ctx) {
		Debug.Log("Playing first level intro cinematic.");
		
		_cinematic = ctx.gameObject.AddComponent<CinematicIntro>();
		_cinematic.Play();
		ctx._camera.SwitchCam(2);
		
	}

	public override void UpdateState(GameStateManager ctx) {
		
		
		if (_cinematic) {
			// disable movement.
			ctx._player1.GetComponent<PuppetController>().move = Vector2.zero;
			ctx._player2.GetComponent<PuppetController>().move = Vector2.zero;
			
			// make players look forwards.
			ctx._player1.GetComponent<PuppetController>().visualReference.transform.LookAt(Vector3.zero, Vector3.up);
			ctx._player2.GetComponent<PuppetController>().visualReference.transform.LookAt(Vector3.zero, Vector3.up);
			
		} else {
			ctx.SwitchState(ctx.PlayingState);
		}
		
		
		
		
	}
	
}
