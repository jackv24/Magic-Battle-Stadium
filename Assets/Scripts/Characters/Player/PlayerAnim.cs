/*
**  PlayerAnim.cs:  Called from playerinput, sends floats to mecanim for animating
*/

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerAnim : NetworkBehaviour
{
    public Animator animatorController;

    private PlayerInput input;
    private PlayerStats stats;

    void Awake()
    {
        input = GetComponent<PlayerInput>();
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (input)
        {
            FaceDirection(input.attackVector);
        }

        if (stats)
        {
            animatorController.SetBool("isAlive", stats.isAlive);
        }
    }

    public void FaceDirection(Vector2 input)
    {
        animatorController.SetFloat("x", input.x);
        animatorController.SetFloat("y", input.y);
    }
}
