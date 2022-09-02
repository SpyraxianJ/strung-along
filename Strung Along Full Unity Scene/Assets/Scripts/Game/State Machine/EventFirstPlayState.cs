using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class EventFirstPlayState : GameState
{
    public override void EnterState(GameStateManager ctx) {
		Debug.Log("Playing first level intro cinematic.");
		
		ctx._lights.DimMains();
		ctx._camera.SwitchCam(2);
		
		ctx._cinematics.PlayCutscene(ctx._cinematics._intro);
	
		// teleport under their string roots
		ctx._player1.transform.position = ctx._player1.GetComponent<PuppetController>().thisStringRoot.transform.position + new Vector3(0, -4, 0);
		ctx._player2.transform.position = ctx._player2.GetComponent<PuppetController>().thisStringRoot.transform.position + new Vector3(0, -4, 0);

	}

	public override void UpdateState(GameStateManager ctx) {
		if (ctx._cinematics._director.state == PlayState.Playing) {
			
			ctx._cinematics.DisablePlayers(ctx._player1, ctx._player2);
			ctx._cinematics.ForceLook(ctx._player1, ctx._camera.Position());
			ctx._cinematics.ForceLook(ctx._player2, ctx._camera.Position());
			
			
		} else {
			ctx._lights.BrightenMains();
			ctx.SwitchState(ctx.PlayingState);
		}
		
		
		
		
	}
	
}
