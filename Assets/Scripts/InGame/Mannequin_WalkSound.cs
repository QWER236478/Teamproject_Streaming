using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mannequin_WalkSound : StateMachineBehaviour
{
    public AudioClip clip;
    public float volume = 1f;
    AudioSource src;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!src) src = animator.GetComponent<AudioSource>();
        if (src && clip)
        {
            src.clip = clip;
            src.loop = true;
            src.spatialBlend = 1f;
            src.volume = volume;
            src.Play();
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (src) src.Stop();
    }
}
