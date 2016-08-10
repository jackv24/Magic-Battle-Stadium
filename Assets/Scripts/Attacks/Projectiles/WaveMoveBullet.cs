/*
 *  WaveMoveBullet.cs: A bullet that follows a sine wave pattern in the direction it was shot
 */

using UnityEngine;
using System.Collections;

public class WaveMoveBullet : Bullet
{
    //How fast to oscillate
    public float frequency = 5f;

    //The size of the oscillation
    public float magnitude = 2f;
    
    private float targetMagnitude = 0;
    private float newMagnitude = 0;

    //Can add randomness to the magnitude
    public float randomness = 0f;
    private float flipTime = 0;
    //For lerping between random magnitudes (stops jerkiness)
    [Range(0, 1f)]
    public float smoothing = 0.5f;

    private Vector3 pos;
    //The time this bullet has existed (for sine function)
    private float awakeTime = 0;

    public override void Start()
    {
        base.Start();

        pos = transform.position;

        //Fixes wrong direction issue
        transform.Rotate(0, 0, -90);
    }

    public override void Update()
    {
        pos += transform.up * Time.deltaTime * moveSpeed;
        awakeTime += Time.deltaTime;

        //Only change magnitude at the end of every period
        if (awakeTime > flipTime)
        {
            //Next time to randomise is 1 period later
            flipTime = awakeTime + 1 / frequency;

            targetMagnitude = Random.Range(magnitude - randomness, magnitude + randomness);
        }

        //Lerp between magnitudes
        newMagnitude = Mathf.Lerp(newMagnitude, targetMagnitude, 0.5f);

        //Set position with Sine function
        transform.position = pos + transform.right * Mathf.Sin(awakeTime * frequency) * newMagnitude;
    }
}
