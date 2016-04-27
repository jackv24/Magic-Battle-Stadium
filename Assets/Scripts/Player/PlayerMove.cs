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

    private enum Direction
    {
        Left, UpLeft, Up, UpRight, Right, DownRight, Down, DownLeft
    }

    public void Move(Vector2 input)
    {
        //Lerp move vector
        moveVector = Vector2.Lerp(moveVector, input * moveSpeed, acceleration);

        transform.Translate(moveVector * Time.deltaTime, Space.World);
    }

    public void FaceDirection(Vector2 input)
    {
        //Default look direction is down
        Direction lookDirection = Direction.Down;

        //Match vector values to direction
        if (input.x > 0)
            lookDirection = Direction.Right;
        else if (input.x < 0)
            lookDirection = Direction.Left;
        else if (input.y > 0)
            lookDirection = Direction.Up;
        else if (input.y < 0)
            lookDirection = Direction.Down;

        //Rotate player to face direction
        switch(lookDirection)
        {
            case Direction.Down:
                transform.eulerAngles = new Vector3(0, 0, 0);
                break;
            case Direction.Up:
                transform.eulerAngles = new Vector3(0, 0, 180);
                break;
            case Direction.Left:
                transform.eulerAngles = new Vector3(0, 0, 270);
                break;
            case Direction.Right:
                transform.eulerAngles = new Vector3(0, 0, 90);
                break;
        }
    }
}
