using UnityEngine;
using System;

public class CharacterEntity
{
    // ─── 기본 정보 ────────────────────────────────────
    public CharacterData Data { get; private set; }
    public string Name => Data.CharacterName;
    public TraitType[] Traits => Data.Traits;
    public int Index { get; private set; } // 게임 시작과 동시에 번호를 매김.

    // ─── 런타임 스탯 (1~10, 성장 가능) ───────────────
    public int HP { get; private set; }  // 체력 — 컨디션 소모 속도에 영향
    public int Planning { get; private set; }  // 기획 능력
    public int Client { get; private set; }  // 클라 능력
    public int Art { get; private set; }  // 아트 능력

    // ─── 컨디션 (0~100) ───────────────────────────────
    // 수치가 곧 상태를 결정
    // 100~41 : Normal / 40~11 : Sick / 10~0 : Down
    public int Condition { get; private set; }

    // ─── 상태 (컨디션에서 자동 계산) ─────────────────
    // TODO: 수치도 따로 빼야 될 수 있음.
    public CharacterState State
    {
        get
        {
            if (Condition > 40) return CharacterState.Normal;
            if (Condition > 10) return CharacterState.Sick;
            return CharacterState.Down;
        }
    }

    // ─── 가용 여부 ────────────────────────────────────
    public bool CanBeAssigned => State != CharacterState.Down;  // 회의 배분 가능

    // ─── 행동 ─────────────────────────────────────────
    public AssignedAction AssignedAction { get; private set; }
    public RuntimeAction? ActiveRuntime { get; private set; }
    public bool IsOvertime { get; private set; }
    public bool IsOnBreak => ActiveRuntime != null; // 일 안하고 딴 짓중.
    public bool IsForcedRuntime { get; private set; }  // 강제 행동 여부

    [Header("강제 행동 해제 패널티")]
    const int ForceBreakPenalty = 15;

    // 현재 뭐하는지 (UI 말풍선용)
    public string GetCurrentBehaviorName()
    {
        if (State == CharacterState.Down) return "휴식 중 (다운)";
        if (ActiveRuntime != null) return ActiveRuntime.ToString();
        return AssignedAction.ToString();
    }

    // ─── 이벤트 ──────────────────────────────────────
    public event Action<CharacterEntity> OnStatChanged;    // 스탯/컨디션 변경
    public event Action<CharacterEntity, CharacterState> OnStateChanged;   // 상태 변경
    public event Action<CharacterEntity> OnActionChanged;  // 행동 변경

    CharacterState _prevState = CharacterState.Normal; // 컨디션이 바뀔 때마다 상태 체크를 하는데 그때마다 invoke되는 것을 막기 위함 (정확히 상태가 바뀌었을 때만 invoke 되도록)

    // ─────────────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────────────
    public void Init(CharacterData data, int index)
    {
        Data = data;
        Index = index;

        HP = data.HP;
        Planning = data.Planning;
        Client = data.Client;
        Art = data.Art;

        Condition = 100;
        _prevState = CharacterState.Normal;
        AssignedAction = AssignedAction.Planning;
        ActiveRuntime = null;
        IsOvertime = false;

        Debug.Log($"[CharacterEntity] {Name} Init 완료 " +
                  $"HP:{HP} 기획:{Planning} 클라:{Client} 아트:{Art}");
    }

    // ─────────────────────────────────────────────────
    //  컨디션 변경
    // ─────────────────────────────────────────────────
    public void ChangeCondition(int amount)
    {
        Condition = Mathf.Clamp(Condition + amount, 0, 100);
        OnStatChanged?.Invoke(this);

        // 상태 변화 감지
        CheckStateChanged();

        // Down 진입 시 강제 Rest 전환
        if (State == CharacterState.Down && !IsOnBreak)
        {
            ActiveRuntime = RuntimeAction.Rest;
            OnActionChanged?.Invoke(this);
            Debug.Log($"[{Name}] 다운 → 강제 휴식 전환");
        }
    }

    void CheckStateChanged()
    {
        var currentState = State;
        if (currentState == _prevState) return;

        Debug.Log($"[{Name}] 상태 변경 → {_prevState} → {currentState}");
        OnStateChanged?.Invoke(this, currentState);
        _prevState = currentState;
    }

    // ─────────────────────────────────────────────────
    // 컨디션 소모량 계산 — 체력 낮을수록 더 깎임
    // HP 10 → 0.5배 / HP 5 → 1배 / HP 1 → ~1.9배
    // ─────────────────────────────────────────────────
   
    public int GetConditionDrain(int baseDrain)
    {
        float multiplier = 2f - (HP / 10f);
        return Mathf.RoundToInt(baseDrain * multiplier);
    }

    // ─────────────────────────────────────────────────
    //  작업 효율
    //  효율 = (스탯 / 10) × 상태 보정
    // ─────────────────────────────────────────────────
    public float GetEfficiency()
    {
        // Down이거나 인터럽트 중이면 메인 작업 효율 0
        if (State == CharacterState.Down) return 0f;
        if (IsOnBreak) return 0f;

        float statRatio = AssignedAction switch
        {
            AssignedAction.Planning => Planning / 10f,
            AssignedAction.Client => Client / 10f,
            AssignedAction.Art => Art / 10f,
            AssignedAction.SelfStudy_Planning => Planning / 10f,
            AssignedAction.SelfStudy_Client => Client / 10f,
            AssignedAction.SelfStudy_Art => Art / 10f,
            _ => 0f
        };

        // 상태 보정
        float stateMultiplier = State switch
        {
            CharacterState.Normal => 1.0f,
            CharacterState.Sick => 0.5f,  // 아프면 효율 절반
            _ => 0f
        };

        return statRatio * stateMultiplier;
    }

    // ─────────────────────────────────────────────────
    //  행동 설정
    // ─────────────────────────────────────────────────

    // 회의에서 배분
    public void SetAssignedAction(AssignedAction action)
    {
        if (!CanBeAssigned) return;
        AssignedAction = action;
        ActiveRuntime = null;
        OnActionChanged?.Invoke(this);
        Debug.Log($"[{Name}] 배분 → {AssignedAction}");
    }

    // 실시간 중 토글 TODO: 아래 코드와 공통된 부분은 따로 빼주는게 정석이긴 함...
    public void SetRuntimeAction(RuntimeAction action)
    {
        IsForcedRuntime = false;
        ActiveRuntime = action;
        OnActionChanged?.Invoke(this);
        Debug.Log($"[{Name}] 런타임 시작 → {action}");
    }

    public void SetForcedRuntimeAction(RuntimeAction action)
    {
        IsForcedRuntime = true;
        ActiveRuntime = action;
        OnActionChanged?.Invoke(this);
        Debug.Log($"[{Name}] 강제 런타임 시작 → {action}");
    }

    public void ClearRuntimeActionForMeeting()
    {
        if (ActiveRuntime == null) return;
        IsForcedRuntime = false;
        ActiveRuntime = null;
        OnActionChanged?.Invoke(this);
        Debug.Log($"[{Name}] 회의 소집 → 런타임 해제 (패널티 없음)");
    }

    // 런타임 종료 → 메인 작업 복귀
    public void ClearRuntimeAction()
    {
        if (ActiveRuntime == null) return;

        // 강제 행동 해제 시 패널티
        if (IsForcedRuntime)
        {
            Debug.Log($"[{Name}] 강제 행동 강제 해제 → 패널티 -{ForceBreakPenalty}");
            ChangeCondition(-ForceBreakPenalty);
        }

        IsForcedRuntime = false;
        ActiveRuntime = null;
        OnActionChanged?.Invoke(this);
        Debug.Log($"[{Name}] 런타임 종료 → {AssignedAction} 복귀");
    }

    // 야근 설정
    public void SetOvertime(bool value)
    {
        if (!CanBeAssigned) return;
        IsOvertime = value;
        Debug.Log($"[{Name}] 야근: {value}");
    }

    // ─────────────────────────────────────────────────
    //  스탯 성장 (1~10 클램프)
    // ─────────────────────────────────────────────────
    public void GrowStat(StatType stat, int amount = 1)
    {
        switch (stat)
        {
            case StatType.HP: HP = Mathf.Clamp(HP + amount, 1, 10); break;
            case StatType.Planning: Planning = Mathf.Clamp(Planning + amount, 1, 10); break;
            case StatType.Client: Client = Mathf.Clamp(Client + amount, 1, 10); break;
            case StatType.Art: Art = Mathf.Clamp(Art + amount, 1, 10); break;
        }

        OnStatChanged?.Invoke(this);
        Debug.Log($"[{Name}] {stat} 성장 → {GetStat(stat)}");
    }

    // 스탯 조회 유틸
    public int GetStat(StatType stat) => stat switch
    {
        StatType.HP => HP,
        StatType.Planning => Planning,
        StatType.Client => Client,
        StatType.Art => Art,
        _ => 0
    };

    // ─────────────────────────────────────────────────
    //  특성 유틸
    // ─────────────────────────────────────────────────
    public bool HasTrait(TraitType trait)
    {
        foreach (var t in Traits)
            if (t == trait) return true;
        return false;
    }
}

// ─────────────────────────────────────────────────────
//  Enum 정의
// ─────────────────────────────────────────────────────
public enum CharacterState
{
    Normal,     // 컨디션 100~41  — 모든 행동 가능, 효율 100%
    Sick,       // 컨디션  40~11  — 모든 행동 가능, 메인 작업 효율 50%
    Down,       // 컨디션  10~ 0  — 메인 작업 불가, 강제 Rest
}

public enum StatType
{
    HP,
    Planning,
    Client,
    Art,
}

// 회의에서 배분 — 시간대 메인 작업
public enum AssignedAction
{
    Planning,            // 기획
    Art,                 // 아트
    Client,              // 클라 개발
    SelfStudy_Planning,  // 기획 공부
    SelfStudy_Client,    // 클라 공부
    SelfStudy_Art,       // 아트 공부
}

// 실시간 중 토글 — 끄면 메인 작업 복귀
public enum RuntimeAction
{
    Rest,       // 휴식 (컨디션 회복)
    Exercise,   // 헬스 (HP 성장)
    Coffee,     // 커피 (컨디션 일시 ↑, 이후 ↓)
}


// TODO: 아니면 한번 넉다운 되면 못 되돌리는 식으로 할까? 그러면 아플때는 진행도가 떨어지는 식으로??? 고민이네 이건 기획적인 부분이라.
