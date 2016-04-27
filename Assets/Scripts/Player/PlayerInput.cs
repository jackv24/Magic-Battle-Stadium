/*
**  PlayerInput.cs: Gets input from the player and calls associated functions in PlayerMove and PlayerLook
*/

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(PlayerLook))]
public class PlayerInput : MonoBehaviour
{
    //move Axis input for this player
    private Vector2 inputVector;

    private PlayerLook playerLook;
    private PlayerMove playerMove;

    void Awake()
    {
        playerLook = GetComponent<PlayerLook>();
        playerMove = GetComponent<PlayerMove>();
    }

    void Update()
    {
        //Get and normalize input
        inputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        inputVector.Normalize();

        //Call player move
        playerMove.Move(inputVector);
    }
}
