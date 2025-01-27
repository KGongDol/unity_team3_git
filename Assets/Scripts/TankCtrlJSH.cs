﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TankCtrlJSH : MonoBehaviour
{
    // </ 이동 관련 변수
    float moveVelocity = 10.0f;  // 이동속도
    // </ Picking 관련 변수
    Vector3 moveDir = Vector3.zero;         // 이동 방향
    float rotSpeed = 7.0f;                  // 초당 회전 속도
    bool isMoveOn = false;                  // 이동 On/Off
    public Transform beginTarPos = null;    // 공격 탱크가 인스턴싱 될 때 지정하는 목적지
    Vector3 targetPos = Vector3.zero;       // 목적지
    double moveDurTime = 0.0f;              // 목표지점까지 도착하는데 걸리는 시간
    double addTimeCount = 0.0f;             // 누적 시간 카운트
    Vector3 startPos = Vector3.zero;
    Vector3 cacLenVec = Vector3.zero;
    Quaternion targetRot;
    // Picking 관련 변수 />

    Vector3 m_VecLen = Vector3.zero;
    // 이동 관련 변수 />

    // </ Navigation
    NavMeshAgent navAgent;
    NavMeshPath movePath;
    Vector3 pathEndPos = Vector3.zero;
    int curPathIndex = 1;
    // Navigation />

    void Start()
    {
        movePath = new NavMeshPath();
        navAgent = this.gameObject.GetComponent<NavMeshAgent>();
        navAgent.updateRotation = false;
        SetDestination(beginTarPos.position); // 최초 목적지 설정
    }

    void Update()
    {
        NavUpdate();
    }

    public void SetDestination(Vector3 a_SetTargetVec)
    {
        //Debug.Log(a_SelectObj);
        // 캐릭터들의 Hp바와 닉네임바 RaycastTarget을 모두 꺼주어야 피킹이 정상작동한다.
        // 그렇지 않으면 if(IsPointerOverUIObject() == false) 에 의해 막히게 된다.
        startPos = this.transform.position; // 출발 위치
        cacLenVec = a_SetTargetVec - startPos; // 현재지점과 목표지점사이의 거리 벡터

        if (cacLenVec.magnitude < 0.5f) // 근거리 피킹 스킵
            return;

        // 네비게이션 메쉬 길찾기를 이용할 때 코드
        float a_PathLen = 0.0f;
        if (MyNavCalcPath(startPos, a_SetTargetVec, ref a_PathLen) == false)
            return;

        a_SetTargetVec.y = this.transform.position.y; // 최종 목표 위치
        targetPos = a_SetTargetVec;                   // 최종 목표 위치
        isMoveOn = true;                              // 이동 OnOff

        moveDir = cacLenVec.normalized;
        // 네비게이션 메시 길찾기를 이용했을 때 거리 계산법
        moveDurTime = a_PathLen / moveVelocity; // 도착하는데 걸리는 시간 = 거리 / 속도
        addTimeCount = 0.0f;
    }

    void NavUpdate()
    {
        // 마우스 피킹 이동
        if (isMoveOn == true)
        {
            // 네비게이션 메시 길찾기를 이용할 때 코드
            isMoveOn = MoveToPath(); // 도착한 경우 false 리턴
        }
    }

    public bool MyNavCalcPath(Vector3 a_StartPos, Vector3 a_TargetPos, ref float a_PathLen)
    {
        // 경로 탐색 함수
        // 피킹이 발생된 상황이므로 초기화 하고 계산한다.
        movePath.ClearCorners(); // 경로 모두 제거
        curPathIndex = 1;        // 진행 인덱스 초기화
        pathEndPos = transform.position;

        if (navAgent == null || navAgent.enabled == false)
        {
            return false;
        }

        if (NavMesh.CalculatePath(a_StartPos, a_TargetPos, -1, movePath) == false)
        {
            // CalculatePath() 함수 계산이 끝나고 정상적으로 instance.final
            // 즉, 목적지까지 계산에 도달했다는 뜻
            // --> p.status == UnityEngine.AI.NavMeshPathStatus.PathComplete
            // 그럴 때, 정상적으로 타겟으로 설정해준다.는 뜻
            // 길찾기 실패 했을 때 점프하는 경향이 있다.
            Debug.Log("여기서 걸림");
            NavMeshHit hit;

            if (NavMesh.SamplePosition(a_TargetPos, out hit, 1.0f, NavMesh.AllAreas)) 
                // 갈 수 없는 위치를 전달했을 경우 갈 수 있는 가장 가까운 위치로 루트 검색
            {
                a_TargetPos = hit.position;
                MyNavCalcPath(a_StartPos, a_TargetPos, ref a_PathLen);
                // Debug.DrawRay(a_TargetPos, Vector3.up, Color.red, 100.0f);
            }
        }

        if (movePath.corners.Length < 2)
            return false;

        for (int i = 1; i < movePath.corners.Length; ++i)
        {
#if UNITY_EDITOR
            //맨마지막 인자(duration 라인을 표시하는 시간
            //Debug.DrawLine(movePath.corners[i], movePath.corners[i] + Vector3.up * i, Color.cyan, 100.0f);
#endif
            m_VecLen = movePath.corners[i] - movePath.corners[i - 1];
            m_VecLen.y = 0.0f;
            a_PathLen = a_PathLen + m_VecLen.magnitude;
        }

        if (a_PathLen <= 0.0f)
            return false;

        // 주인공이 마지막 위치에 도달했을 때 정확한 방향을 바라보게 하고 싶은 경우 때문에 계산해 놓는다.
        pathEndPos = movePath.corners[(movePath.corners.Length - 1)];

        return true;
    }

    // MoveToPath 관련 변수
    bool isSuccessed = true;
    Vector3 curCPos = Vector3.zero;
    Vector3 cacDestV = Vector3.zero;
    Vector3 targetDir;
    float cacSpeed = 0.0f;
    float nowStep = 0.0f;
    Vector3 velocity = Vector3.zero;
    Vector3 vTowardNom = Vector3.zero;
    int oldPathCount = 0;

    public bool MoveToPath(float overSpeed = 1.0f)
    {
        isSuccessed = true;

        if (movePath == null)
        {
            movePath = new NavMeshPath();
        }

        oldPathCount = curPathIndex;
        if (curPathIndex < movePath.corners.Length) // 최소 curPathIndex = 1 보다 큰 경우에
        {
            curCPos = this.transform.position;          // 현재 위치 업데이트
            cacDestV = movePath.corners[curPathIndex];  // 현재 이동해야할 꼭지점의 위치

            curCPos.y = cacDestV.y;         // 높이 오차가 있어서 도착 판정을 못하는 경우가 있다. ( 도착지점의 높이를 캐릭터의 높이에 넣음 )
            targetDir = cacDestV - curCPos; // 현재 이동해야할 목표지점 - 현재 위치 ( 위에서 높이 값을 맞춰줬으므로 같은 평면으로 놓고 구한 것이 된다. ) 
            targetDir.y = 0.0f;             // 한 번 더 평면처리 (쓸데없는 듯)
            targetDir.Normalize();          // 이동해야할 방향벡터 구하기

            cacSpeed = moveVelocity;         // 속력는 버퍼에 넣어 처리
            cacSpeed = cacSpeed * overSpeed; // 현재속도 * 배속 ( 기본배속 1.0f )

            nowStep = cacSpeed * Time.deltaTime; // ( 한 프레임에 이동할 거리 ) 이번에 이동했을 때 이 안으로만 들어와도...

            velocity = cacSpeed * targetDir; // 속도 = 크기 * 방향
            velocity.y = 0.0f;               // 속도 평면처리
            navAgent.velocity = velocity;    // 이동처리

            if ((cacDestV - curCPos).magnitude <= nowStep)   // 다음 지점까지 거리가 한 프레임에 이동할 거리보다 작아지면 중간점에 도착한 것으로 본다.
            {
                //movePath.corners[curPathIndex] = this.transform.position; // 코너의 위치를 캐릭터의 위치로 대체
                curPathIndex = curPathIndex + 1; // 다음 꼭지점 업데이트
            }

            addTimeCount = addTimeCount + Time.deltaTime; // 경과 시간 증가
            if (moveDurTime <= addTimeCount) // '실제 경과 시간'이 '예상 경과 시간'을 초과하면 '목표점에 도달'한 것으로 판정한다.
            {
                curPathIndex = movePath.corners.Length; // 이동종료 [ 현재 꼭지점 경로를 최종경로로 바꿔버림 => 다음 업데이트 때 동작 안한다. ]
            }
        }

        if (curPathIndex < movePath.corners.Length) // 목적지에 아직 도착하지 않았다면
        {
            // 캐릭터 회전 / 애니메이션 방향 조정
            vTowardNom = movePath.corners[curPathIndex] - this.transform.position; // 가야할 지점까지의 거리
            vTowardNom.y = 0.0f;
            vTowardNom.Normalize(); // 단위 벡터를 만든다.

            if (0.0001f < vTowardNom.magnitude) // 로테이션에서는 모두 들어가야 한다.
            {
                Quaternion targetRot = Quaternion.LookRotation(vTowardNom);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotSpeed);
            }
        }
        else // 최종 목적지에 도착한 경우 매 프레임 호출
        {
            if (oldPathCount < movePath.corners.Length) // 최종 목적지에 도착한 경우 한 번 발생시키기 위한 부분
            {
                ClearPath();
            }

            isSuccessed = false; // 아직 목적지에 도착하지 않았다면 다시 잡아 줄 것이기 때문에...
        }
        return isSuccessed;
    }

    void ClearPath()
    {
        isMoveOn = false;

        // 피킹을 위한 동기화
        pathEndPos = transform.position;
        //navAgent.velocity = Vector3.zero; // 목적지에 도착하면 즉시 멈춤

        if (0 < movePath.corners.Length)
        {
            movePath.ClearCorners();    // 경로 모두 제거
        }
        curPathIndex = 1; // 진행 인덱스 초기화
        // 피킹을 위한 동기화 부분
    }
}
