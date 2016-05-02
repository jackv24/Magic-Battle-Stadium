/*
**  PlayerMove.cs: Contains functions which are called from PlayerInput, in order to move the player
*/

using UnityEngine;
using System.Collections;

public class PlayerMove : MonoBehaviour
{
    //How fast the player moves
    public float moveSpeed = 10.0f;

    //How fast the player reaches top speed
    [Range(0, 1f)]
    public float acceleration = 0.25f;

    private Vector2 moveVector;

    public void Move(Vector2 input)
    {
        //Lerp move vector
        moveVector = Vector2.Lerp(moveVector, input * moveSpeed, acceleration);

        transform.Translate(moveVector * Time.deltaTime, Space.World);
    }
}
