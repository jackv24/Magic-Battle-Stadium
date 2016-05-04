/*
**  RandomColor.cs: Randomly sets the colour for the player on the provided array of sprite renderers,
**                  and updates the on the network.
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class RandomColour : NetworkBehaviour
{
    //The sprite renderers to colour
    public SpriteRenderer[] spriteRenderers;

    //The range in which the generated colour will lie
    public float minSaturation = 0.25f;
    public float maxSaturation = 0.75f;

    //The colour that is randomly generated
    //hooked to UpdateColour to change the sprite renderers when a player's colour changes
    [SyncVar(hook ="UpdateColor")]
    public Color randomColor;

    void Start()
    {
        //If this is the local player
        if (isLocalPlayer)
        {
            //Generate a random colour
            randomColor = new Color(Random.Range(minSaturation, maxSaturation), Random.Range(minSaturation, maxSaturation), Random.Range(minSaturation, maxSaturation));
            //Update the colour on the server
            CmdUpdateColor(randomColor);
        }
        else //If this is not the local player
            //Only update the local color
            UpdateColor(randomColor);
    }

    [Command]
    void CmdUpdateColor(Color color)
    {
        UpdateColor(color);
    }

    void UpdateColor(Color color)
    {
        randomColor = color;

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = randomColor;
        }
    }
}
