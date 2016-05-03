using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class RandomColour : NetworkBehaviour
{
    public SpriteRenderer[] spriteRenderers;

    public Color randomColor;

    void Start()
    {
        if (isLocalPlayer)
        {
            randomColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));

            CmdUpdateColor(randomColor);
        }

        UpdateColor();
    }

    [Command]
    void CmdUpdateColor(Color color)
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            randomColor = color;
            spriteRenderer.color = randomColor;
        }
    }

    void UpdateColor()
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = randomColor;
        }
    }
}
