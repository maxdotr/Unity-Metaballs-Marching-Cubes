using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metaball : MonoBehaviour
{
    private Transform pos;

    public Vector3 position;
    public float radius;
    public float height;

    private float prevRadius;

    // Start is called before the first frame update
    void Start()
    {
        pos = GetComponent<Transform>();
        position = pos.position;

        prevRadius = radius;
    }

    // Update is called once per frame
    void Update()
    {
        if(radius != prevRadius){
            UpdateRadius();
        }

        prevRadius = radius;

        position = pos.position;
    }

    void UpdateRadius()
    {
        GetComponent<SphereCollider>().radius = 0.602165f * radius + 0.488931f;
    }
}
