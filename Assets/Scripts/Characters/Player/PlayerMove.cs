/*
**  PlayerMove.cs: Contains functions which are called from PlayerInput, in order to move the player
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerMove : NetworkBehaviour
{
    //How fast the player moves
    [SyncVar]
    public float moveSpeed = 10.0f;

    //How fast the player reaches top speed
    [Range(0, 1f)]
    public float acceleration = 0.25f;

    private Vector2 moveVector;

    private Rigidbody2D body;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();

        if (isLocalPlayer)
            //Set stats on server (sent to clients via SyncVars)
            CmdSetSpeed(PlayerPrefs.GetInt("AttackSet", 0));
    }

    [Command]
    void CmdSetSpeed(int setIndex)
    {
        AttackSet attackSet = GameManager.instance.attackSets[setIndex];

        moveSpeed = attackSet.moveSpeed;
    }

    public void Move(Vector2 input)
    {
        //Lerp move vector
        moveVector = Vector2.Lerp(moveVector, input * moveSpeed, acceleration);

        body.velocity = moveVector;
    }
}
