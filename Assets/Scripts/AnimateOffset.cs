/*
 * AnimateOffset.cs - Animates the offset of a meshrenderer's material
 */

using UnityEngine;
using System.Collections;

public class AnimateOffset : MonoBehaviour
{
    //How much to offset the texture each second
    public Vector2 offset;
    private Vector2 currentOffset;

    private MeshRenderer rend;

    void Awake()
    {
        rend = GetComponent<MeshRenderer>();
    }

	void Update ()
    {
        if (rend)
        {
            currentOffset += offset * Time.deltaTime;

            rend.material.SetTextureOffset("_MainTex", currentOffset);
        }
	}
}
