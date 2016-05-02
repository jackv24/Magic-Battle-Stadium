/*
**  PlayerAnim.cs:  Called from playerinput, sends floats to mecanim for animating
*/

using UnityEngine;
using System.Collections;

public class PlayerAnim : MonoBehaviour
{
    public Animator animatorController;

    public void FaceDirection(Vector2 input)
    {
        animatorController.SetFloat("x", input.x);
        animatorController.SetFloat("y", input.y);
    }
}
