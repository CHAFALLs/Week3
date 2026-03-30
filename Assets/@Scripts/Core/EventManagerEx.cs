using UnityEngine;
using System;
using System.Collections.Generic;

public class EventManagerEx : SingletonBehaviour<EventManagerEx>
{
    // ─── 이벤트 데이터 목록 ───────────────────────────
    [SerializeField] ImmediateEventData[] _immediateEvents;
    [SerializeField] RandomEventData[] _randomEvents;

    // ─── Random 사이클 설정 ───────────────────────────
    [SerializeField] float _randomCycleInterval = 10f;  // 공용 체크 주기 (초)
    float _randomCycleTimer = 0f;

    // ─── 알림 목록 ────────────────────────────────────
    List<BaseEventData> _activeNotifications = new List<BaseEventData>();
    public IReadOnlyList<BaseEventData> ActiveNotifications => _activeNotifications;

    // ─── 쿨타임 관리 ──────────────────────────────────
    Dictionary<BaseEventData, float> _lastTriggeredTime = new Dictionary<BaseEventData, float>();

    // ─── 이벤트 ──────────────────────────────────────
    public event Action<BaseEventData> OnEventTriggered;
    public event Action<BaseEventData> OnEventResolved;

    const int MAX_NOTIFICATIONS = 3; // 최대 알림 표기 수.

    bool _initialized = false;

    // ─────────────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────────────
    public void Init()
    {
        if (_initialized) return;
        _initialized = true;

        // Immediate → 페이즈 시작 + 캐릭터 상태 변화 시 체크 (TODO 손보기)
        TimeManager.Instance.OnPhaseStart += CheckImmediateEvents;
        CharacterManager.Instance.OnCharacterDown += _ => CheckImmediateEvents(TimeManager.Instance.CurrentDayPhase);
        CharacterManager.Instance.OnCharacterRecovered += _ => CheckImmediateEvents(TimeManager.Instance.CurrentDayPhase);

        _randomCycleTimer = 0f;

        Debug.Log("[EventManager] Init 완료");
    }

    // ─────────────────────────────────────────────────
    //  Update — Random 사이클 타이머
    // ─────────────────────────────────────────────────
    void Update()
    {
        if (!_initialized) return;
        if (TimeManager.Instance.IsPaused) return;

        _randomCycleTimer += Time.deltaTime;
        if (_randomCycleTimer < _randomCycleInterval) return;

        _randomCycleTimer = 0f;
        TriggerRandomCycle();
    }

    // ─────────────────────────────────────────────────
    //  Random 사이클 — 후보 수집 후 하나만 선택 발동
    // ─────────────────────────────────────────────────
    void TriggerRandomCycle()
    {
        if (_randomEvents == null || _randomEvents.Length == 0) return;

        var candidates = new List<(RandomEventData data, CharacterEntity triggerChar)>();

        foreach (var data in _randomEvents)
        {
            // 쿨타임 안 됐으면 스킵
            if (!CheckCooldown(data)) continue;

            // 확률 체크 실패하면 스킵
            if (UnityEngine.Random.value > data.TriggerChance) continue;

            // 조건 불충족하면 스킵
            if (!CheckEventConditions(data, out var triggerChar)) continue;

            candidates.Add((data, triggerChar));
        }

        if (candidates.Count == 0) return;

        // 후보 중 하나만 랜덤 선택
        var selected = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        TriggerEvent(selected.data, selected.triggerChar);
    }

    // ─────────────────────────────────────────────────
    //  Immediate 이벤트 체크
    // ─────────────────────────────────────────────────
    void CheckImmediateEvents(TimeManager.DayPhase phase)
    {
        foreach (var data in _immediateEvents)
        {
            if (!CheckCooldown(data)) continue;
            if (UnityEngine.Random.value > data.TriggerChance) continue;
            if (!CheckEventConditions(data, out var triggerChar)) continue;

            TriggerEvent(data, triggerChar);
        }
    }

    // ─────────────────────────────────────────────────
    //  조건 체크 — 발동 가능 여부 + 트리거 캐릭터 반환
    //  조건 없으면 무조건 통과 (triggerChar = null)
    //  캐릭터 조건 → 만족하는 첫 번째 캐릭터 반환
    //  페이즈 조건 → 캐릭터 불필요, triggerChar = null
    // ─────────────────────────────────────────────────
    bool CheckEventConditions(BaseEventData data, out CharacterEntity triggerChar)
    {
        triggerChar = null;

        if (data.Conditions == null || data.Conditions.Length == 0)
            return true;

        // 조건 타입에 따라 분기
        bool needsCharacter = false;
        foreach (var condition in data.Conditions)
        {
            if (condition.Type == ConditionType.HasTrait ||
                condition.Type == ConditionType.TraitAndCondition)
            {
                needsCharacter = true;
                break;
            }
        }

        if (needsCharacter)
        {
            // 캐릭터 기반 조건 — 만족하는 캐릭터 탐색
            foreach (var c in CharacterManager.Instance.Characters)
            {
                if (CheckAllConditions(c, data.Conditions))
                {
                    triggerChar = c;
                    return true;
                }
            }
            return false;
        }
        else
        {
            // 게임 상태 기반 조건 — 캐릭터 불필요
            return CheckAllConditions(null, data.Conditions);
        }
    }

    bool CheckAllConditions(CharacterEntity c, EventCondition[] conditions)
    {
        foreach (var condition in conditions)
            if (!CheckCondition(c, condition)) return false;
        return true;
    }

    bool CheckCondition(CharacterEntity c, EventCondition condition)
    {
        return condition.Type switch
        {
            ConditionType.HasTrait =>
                c != null && c.HasTrait(condition.RequiredTrait),

            ConditionType.TraitAndCondition =>
                c != null &&
                c.HasTrait(condition.RequiredTrait) &&
                Compare(c.Condition, condition.ConditionThreshold, condition.Compare),

            ConditionType.PhaseOnly =>
                TimeManager.Instance.CurrentGamePhase == condition.RequiredPhase,

            ConditionType.PhaseAndProgress =>
                TimeManager.Instance.CurrentGamePhase == condition.RequiredPhase &&
                Compare(
                    GameManager.Instance.GetProgress(condition.RequiredProgressType) * 100f,
                    condition.ProgressThreshold,
                    condition.Compare),

            ConditionType.AverageCondition =>
                Compare(GetAverageCondition(), condition.ConditionThreshold, condition.Compare),

            _ => false
        };
    }

    float GetAverageCondition()
    {
        var characters = CharacterManager.Instance.Characters;
        if (characters.Count == 0) return 0f;

        float total = 0f;
        foreach (var c in characters)
            total += c.Condition;
        return total / characters.Count;
    }

    bool Compare(float value, float threshold, CompareType compareType) => compareType switch
    {
        CompareType.LessOrEqual => value <= threshold,
        CompareType.GreaterOrEqual => value >= threshold,
        _ => false
    };

    // ─────────────────────────────────────────────────
    //  공통 유틸
    // ─────────────────────────────────────────────────
    bool CheckCooldown(BaseEventData data)
    {
        if (!_lastTriggeredTime.ContainsKey(data)) return true;
        return Time.time - _lastTriggeredTime[data] >= data.Cooldown;
    }

    // ─────────────────────────────────────────────────
    //  이벤트 발생
    // ─────────────────────────────────────────────────
    void TriggerEvent(BaseEventData data, CharacterEntity triggerChar)
    {
        Debug.Log($"[EventManager] 이벤트 발생: {data.EventName}");

        // 최대치 초과 시 가장 오래된 알림 제거
        if (_activeNotifications.Count >= MAX_NOTIFICATIONS)
        {
            var oldest = _activeNotifications[0];
            _activeNotifications.RemoveAt(0);
            OnEventResolved?.Invoke(oldest);
        }


        _lastTriggeredTime[data] = Time.time;
        _activeNotifications.Add(data);
        OnEventTriggered?.Invoke(data);

        ApplyEffects(data, triggerChar);
    }

    void ApplyEffects(BaseEventData data, CharacterEntity triggerChar)
    {
        foreach (var effect in data.Effects)
        {

            // Progress는 타겟 무관하게 한 번만 적용
            if (effect.Type == EffectType.Progress)
            {
                GameManager.Instance.AddProgress(effect.ProgressType, effect.ProgressAmount);
                continue;
            }

            switch (effect.Target)
            {
                case EffectTarget.SingleCharacter:
                    ApplyEffect(effect, CharacterManager.Instance.Get(effect.CharacterIndex));
                    break;

                case EffectTarget.AllCharacters:
                    foreach (var c in CharacterManager.Instance.Characters)
                        ApplyEffect(effect, c);
                    break;

                case EffectTarget.TraitOwner:
                    ApplyEffect(effect, triggerChar);
                    break;
            }
        }
    }

    void ApplyEffect(EventEffect effect, CharacterEntity c)
    {
        if (c == null) return;

        switch (effect.Type)
        {
            case EffectType.Condition:
                c.ChangeCondition(effect.ConditionAmount);
                break;

            //case EffectType.Progress:
            //    GameManager.Instance.AddProgress(effect.ProgressType, effect.ProgressAmount);
            //    break; // 한번만 작동하게 하기 위해 약간의 수정을 함.

            case EffectType.ForceRuntime:
                c.SetForcedRuntimeAction(effect.ForcedRuntime);
                break;
        }
    }

    // ─────────────────────────────────────────────────
    //  알림 해제
    // ─────────────────────────────────────────────────
    public void ResolveNotification(BaseEventData data)
    {
        if (_activeNotifications.Remove(data))
        {
            OnEventResolved?.Invoke(data);
            Debug.Log($"[EventManager] 알림 해제: {data.EventName}");
        }
    }
}