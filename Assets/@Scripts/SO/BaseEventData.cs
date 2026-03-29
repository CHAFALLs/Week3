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
//  발생 조건
// ─────────────────────────────────────────────────────
[System.Serializable]
public class EventCondition
{
    public ConditionType Type;

    // HasTrait / TraitAndCondition 용
    public TraitType RequiredTrait;
    public float ConditionThreshold; // TraitAndCondition (0~100)

    // PhaseAndProgress / PhaseOnly 용
    public TimeManager.GamePhase RequiredPhase;
    public ProgressType RequiredProgressType;
    public float ProgressThreshold;
}

public enum ConditionType
{
    HasTrait,           // 특성 보유 캐릭터 존재
    TraitAndCondition,  // 특성 보유 + 컨디션 이하
    PhaseOnly,          // 특정 페이즈일 때만
    PhaseAndProgress,   // 특정 페이즈 + 진행도 이하
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
    public RuntimeAction ForcedRuntime; // 난 무조건 커피를 마셔야 겠어 같은.
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