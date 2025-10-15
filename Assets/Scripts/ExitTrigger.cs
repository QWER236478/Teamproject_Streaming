using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    private LoopManager lm;

    void Start()
    {
        lm = FindObjectOfType<LoopManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            lm.OnExitTrigger();
        }
    }
}