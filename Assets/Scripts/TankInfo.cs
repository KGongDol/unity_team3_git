using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TankType
{
    Normal, // 일반차량
    Speed,  // 가볍고 빠른차량
    Repair, // 수리차량
    Solid,  // 튼튼한차량
    Cannon, // 장거리 미사일
    Count,
}

public class TankInfo : MonoBehaviour
{
    public TankType m_Type = TankType.Normal;
    public float maxHp;   // 최대체력
    public float speed;   // 이동속도
    public float atk;     // 공격력
    public float attRate; // 공격속도
    public float skillCool; // 스킬 쿨타임

    public void TankInit() // 탱크의 기본정보 세팅
    {
        switch(m_Type)
        { 
            case TankType.Normal: // 일반차량
            {
                maxHp = 100.0f;
                speed = 5.0f;
                atk = 10.0f;
                attRate = 3.0f;
                skillCool = 5.0f;
                break;
            }
            case TankType.Speed: // 빠르게 이동하면서 기관총을 쏘는 차량
            {
                maxHp = 70.0f;
                speed = 10.0f;
                atk = 2.0f;
                attRate = 0.5f;
                skillCool = 2.0f;
                break;
            }
            case TankType.Repair: // 다른 탱크를 수리하는 차량
            {
                maxHp = 80.0f;
                speed = 5.0f;
                atk = 8.0f;
                attRate = 3.0f;
                skillCool = 5.0f;
                break;
            }
            case TankType.Solid: // 다른 차량을 보호하며 천천히 전진하는 차량
            {
                maxHp = 200.0f;
                speed = 2.0f;
                atk = 3.0f;
                attRate = 4.0f;
                skillCool = 10.0f;
                break;
            }
            case TankType.Cannon: // 멀리까지 공격이 가능한 차량
            {
                maxHp = 150.0f;
                speed = 4.0f;
                atk = 20.0f;
                attRate = 5.0f;
                skillCool = 8.0f;
                break;
            }
        }
    }
}