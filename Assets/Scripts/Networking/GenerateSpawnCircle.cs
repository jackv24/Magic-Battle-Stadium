using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerateSpawnCircle : MonoBehaviour
{
    public static GenerateSpawnCircle instance;

    public float radius = 3f;

    private void Awake()
    {
        instance = this;
    }

    public List<Transform> Generate(int amount)
    {
        List<Transform> positions = new List<Transform>();

        for (int i = 0; i < amount; i++)
        {
            float angle = (i / (float)amount) * 360;

            Vector3 center = transform.position;
            Vector3 pos;

            pos.x = center.x + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            pos.y = center.y + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            pos.z = center.z;

            GameObject obj = new GameObject("SpawnPoint");
            obj.transform.parent = transform;
            obj.transform.position = pos;

            positions.Add(obj.transform);
        }

        return positions;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
