/*
**  PlayerInput.cs: Gets input from the player and calls associated functions in PlayerMove and PlayerLook
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerInput : NetworkBehaviour
{
    //move Axis input for this player
    public Vector2 inputVector;
    //attack axis input
    public Vector2 attackVector;

    private PlayerMove playerMove;
    private PlayerStats stats;

    void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        stats = GetComponent<PlayerStats>();
    }

    void Start()
    {
        //If this is the local player, set the main camera's target
        if (isLocalPlayer)
            Camera.main.GetComponent<CameraFollow>().target = transform;
    }

    void Update()
    {
        if (!isLocalPlayer || !stats.isAlive)
            return;

        //Get and normalize input
        inputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        //Call player move
        playerMove.Move(inputVector.normalized);

        //Get attack vector (not normalised in this case)
        attackVector = new Vector2(Input.GetAxisRaw("Attack Horizontal"), Input.GetAxisRaw("Attack Vertical"));

        //Call player move to face direction
        if (attackVector == Vector2.zero)
            attackVector = inputVector;
    }
}
