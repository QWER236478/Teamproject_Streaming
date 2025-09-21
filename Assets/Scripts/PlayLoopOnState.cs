using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayLoopOnState : StateMachineBehaviour
{
    public AudioClip clip;                   // mannequinChaser_walk
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0f, 1f)] public float spatialBlend = 0f; // 0=2D, 1=3D
    public float minDistance = 6f;          // 3D�� ���� ����
    public float maxDistance = 24f;

    AudioSource src;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // �ִϸ����Ͱ� �޸� ������Ʈ���� AudioSource Ȯ��/������ ����
        if (!src) src = animator.GetComponent<AudioSource>();
        if (!src) src = animator.gameObject.AddComponent<AudioSource>();

        src.playOnAwake = false;
        src.loop = true;
        src.clip = clip;
        src.volume = volume;
        src.spatialBlend = spatialBlend;
        src.minDistance = minDistance;
        src.maxDistance = maxDistance;
        src.dopplerLevel = 0f;

        if (clip) src.Play();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (src && src.isPlaying) src.Stop();
    }
}