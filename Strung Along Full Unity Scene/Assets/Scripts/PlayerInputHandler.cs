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
        Debug.Log(players);
        var index = playerInput.playerIndex;
        player = players.FirstOrDefault(p => p.GetPlayerIndex() == index);
    }

    public void OnMove(CallbackContext context)
    {
        player.move = context.ReadValue<Vector2>();
    }
}
