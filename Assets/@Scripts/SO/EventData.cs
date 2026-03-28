using UnityEngine;

// ─────────────────────────────────────────────────────
//  공통 베이스
// ─────────────────────────────────────────────────────
public abstract class BaseEventData : ScriptableObject
{
    [Header("기본 정보")]
    public string EventName;
    public string Description;
    public EventGrade Grade;

    [Header("쿨타임 (초)")]
    public float Cooldown = 60f;

    [Header("발생 조건 (없으면 조건 없이 발동)")]
    public EventCondition[] Conditions;

    [Header("효과")]
    public EventEffect[] Effects;
}

// ─────────────────────────────────────────────────────
//  즉시 발동 — 조건 만족 순간 발동
// ─────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "ImmediateEvent", menuName = "Scriptable Objects/Event/ImmediateEvent")]
public class ImmediateEventData : BaseEventData { }

// ─────────────────────────────────────────────────────
//  랜덤 발동 — 주기적으로 확률 체크 후 발동
// ─────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "RandomEvent", menuName = "Scriptable Objects/Event/RandomEvent")]
public class RandomEventData : BaseEventData
{
    [Header("랜덤 발동 설정")]
    [Tooltip("확률 체크 주기 (초)")]
    public float TriggerInterval = 60f;

    [Tooltip("발동 확률 (0~1)")]
    [Range(0f, 1f)]
    public float TriggerChance = 0.3f;
}

// ─────────────────────────────────────────────────────
//  발생 조건
// ─────────────────────────────────────────────────────
[System.Serializable]
public class EventCondition
{
    public ConditionType Type;
    public TraitType RequiredTrait;
    public float ConditionThreshold; // TraitAndCondition (0~100)
    public CharacterState RequiredState;      // TraitAndState
}

public enum ConditionType
{
    HasTrait,           // 특성 보유 캐릭터 존재
    TraitAndCondition,  // 특성 보유 + 컨디션 이하
    TraitAndState,      // 특성 보유 + 특정 상태
}

// ─────────────────────────────────────────────────────
//  효과
// ─────────────────────────────────────────────────────
[System.Serializable]
public class EventEffect
{
    public EffectType Type;
    public EffectTarget Target;
    public int CharacterIndex;

    // Condition 변화용
    public int ConditionAmount;

    // Progress 변화용
    public ProgressType ProgressType;
    public float ProgressAmount;

    // ForceRuntime용
    public RuntimeAction ForcedRuntime;
}

public enum EffectType
{
    Condition,     // 컨디션 변화
    Progress,      // 진행도 변화
    ForceRuntime,  // RuntimeAction 강제 전환
}

public enum EffectTarget
{
    SingleCharacter,  // 특정 캐릭터
    AllCharacters,    // 전체
    TraitOwner,       // 조건을 만족한 캐릭터
}

public enum EventGrade
{
    Minor,
    Major,
    Critical,
}