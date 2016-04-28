/*
**  PlayerInput.cs: Gets input from the player and calls associated functions in PlayerMove and PlayerLook
*/

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(PlayerAnim))]
public class PlayerInput : MonoBehaviour
{
    //move Axis input for this player
    private Vector2 inputVector;
    //attack axis input
    private Vector2 attackVector;

    private PlayerMove playerMove;
    private PlayerAnim playerAnim;

    void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        playerAnim = GetComponent<PlayerAnim>();
    }

    void Update()
    {
        //Get and normalize input
        inputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        //Call player move
        playerMove.Move(inputVector.normalized);

        //Get attack vector (not normalised in this case)
        attackVector = new Vector2(Input.GetAxisRaw("AttackHorizontal"), Input.GetAxisRaw("AttackVertical"));
        
        //Call player move to face direction
        if (attackVector != Vector2.zero)
            playerAnim.FaceDirection(attackVector);
        else //if no attack direction is input, just face movement direction
            playerAnim.FaceDirection(inputVector);
    }
}
