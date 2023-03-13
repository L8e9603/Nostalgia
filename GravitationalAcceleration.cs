using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitationalAcceleration : MonoBehaviour
{
    [Header("Gravitational Acceleration Mutiplyer")]
    public float gravity = -9.8f;
    [Range(-100f, 100f)]
    public float multuplyAmount = 1f;

    void Start()
    {
        Physics.gravity = new Vector3(0, gravity * multuplyAmount, 0);
    }

    void Update()
    {
        
    }
}
