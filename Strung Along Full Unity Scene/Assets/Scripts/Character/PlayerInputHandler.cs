using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput playerInput;
    private PuppetController player;

    // Start is called before the first frame update
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        var players = FindObjectsOfType<PuppetController>();
        var index = playerInput.playerIndex;
		
		// if admin is enabled, the keyboard is already "player 1". so increment both player indexes.
		if ( FindObjectOfType<AdminInputHandler>() ) {
			player = players.FirstOrDefault(p => p.GetPlayerIndex() + 1 == index);
		} else {
			player = players.FirstOrDefault(p => p.GetPlayerIndex() == index);
		}
		
    }

    public void OnMove(CallbackContext context)
    {
        if (context.performed)
        {
            player.move = context.ReadValue<Vector2>();
            player.movePressed = true;
        }
        else if (context.canceled)
        {
            player.move = Vector2.zero;
            player.movePressed = false;
        }
    }

    public void OnJump(CallbackContext context)
    {
        if (context.performed)
        {
            player.jumpPressed = context.ReadValueAsButton();
            if (player.jumpPressed && player.jumpReleased)
            {
                player.StartJump();
            }
            player.jumpReleased = false;
        }
        else if (context.canceled)
        {
            player.jumpReleased = true;
            player.jumpBoostTimer = 0;
        }
    }

    public void OnGrab(CallbackContext context)
    {
        if (context.performed)
        {
            player.grabPressed = context.ReadValueAsButton();
            player.GrabStart();
        }
        else if (context.canceled)
        {
            player.grabPressed = false;
            player.GrabRelease(false);
        }
    }
}
