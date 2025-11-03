using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Twins : MonoBehaviour
{
    public enum ActionState
    {
        Idle, Walk, Run, Attack
    }
    public ActionState actionState = ActionState.Idle; //액션 상태

    public float searchRange; //감지 거리
    public float attackRange; //공격 거리
    private NavMeshAgent agent; //내브메시 에이전트
    public Animator animator; //애니메이터
    public GameObject target; //타겟
    public float walkSpeed; //걷기 속도
    public float runSpeed; //달리기 속도

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void TitanAction()
    {
        switch (actionState)
        {
            case ActionState.Idle:
                {
                    if (target)
                    {
                        TwinsAnimationOn(1); //걷기 애니메이션 실행
                        agent.isStopped = false; //이동 중지 해제
                        agent.SetDestination(target.transform.position); //타겟의 위치로 이동
                        actionState = ActionState.Walk;
                    }
                    break;
                }
            case ActionState.Walk:
                {
                    //자신(거인)과 타겟과의 거리를 float형으로 반환
                    float dist = Vector3.Distance(transform.position, target.transform.position);

                    //타겟과의 거리가 공격 범위 안에 들어올 경우
                    if (dist <= attackRange)
                    {
                        agent.isStopped = true; //이동 중지
                        TwinsAnimationOn(2); //공격 애니메이션
                        actionState = ActionState.Attack;
                    }
                    break;
                }
            case ActionState.Attack:
                {
                    {                      

                    }
                    break;
                }
        }
    }
    void TwinsAnimationOn(int i) //거인 애니메이션 함수
    {
        animator.SetInteger("TwinsState", i);
    }
}
        
