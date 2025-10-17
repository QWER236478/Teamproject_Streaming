using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CorridorTrigger : MonoBehaviour
{
    private LoopManager manager;
    private bool triggered = false;

    void Start()
    {
        manager = FindObjectOfType<LoopManager>();
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        manager.SpawnNext();
    }
}