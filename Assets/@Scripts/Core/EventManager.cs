using UnityEngine;
using System;
using System.Collections.Generic;

public class EventManager : SingletonBehaviour<EventManager>
{
    // ─── 이벤트 데이터 목록 ───────────────────────────
    [SerializeField] ImmediateEventData[] _immediateEvents;
    [SerializeField] RandomEventData[] _randomEvents;

    // ─── Random 사이클 설정 ───────────────────────────
    [SerializeField] float _randomCycleInterval = 60f;  // 공용 체크 주기 (초)
    float _randomCycleTimer = 0f;

    // ─── 알림 목록 ────────────────────────────────────
    List<BaseEventData> _activeNotifications = new List<BaseEventData>();
    public IReadOnlyList<BaseEventData> ActiveNotifications => _activeNotifications;

    // ─── 쿨타임 관리 ──────────────────────────────────
    Dictionary<BaseEventData, float> _lastTriggeredTime = new Dictionary<BaseEventData, float>();

    // ─── 이벤트 ──────────────────────────────────────
    public event Action<BaseEventData> OnEventTriggered;
    public event Action<BaseEventData> OnEventResolved;

    const int MAX_MINOR_NOTIFICATIONS = 3;

    bool _initialized = false;

    // ─────────────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────────────
    public void Init()
    {
        if (_initialized) return;
        _initialized = true;

        // Immediate → 틱 시작 + 캐릭터 상태 변화 시 체크
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
            if (data.Grade == EventGrade.Minor &&
                _activeNotifications.Count >= MAX_MINOR_NOTIFICATIONS) continue;

            if (!CheckCooldown(data)) continue;

            if (UnityEngine.Random.value > data.TriggerChance) continue;

            var triggerChar = FindTriggerCharacter(data);
            if (data.Conditions != null && data.Conditions.Length > 0 && triggerChar == null) continue;

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
            if (data.Grade == EventGrade.Minor &&
                _activeNotifications.Count >= MAX_MINOR_NOTIFICATIONS) continue;

            if (!CheckCooldown(data)) continue;

            var triggerChar = FindTriggerCharacter(data);
            if (triggerChar == null) continue;

            TriggerEvent(data, triggerChar);
        }
    }

    // ─────────────────────────────────────────────────
    //  공통 유틸
    // ─────────────────────────────────────────────────
    bool CheckCooldown(BaseEventData data)
    {
        if (!_lastTriggeredTime.ContainsKey(data)) return true;
        return Time.time - _lastTriggeredTime[data] >= data.Cooldown;
    }

    CharacterEntity FindTriggerCharacter(BaseEventData data)
    {
        if (data.Conditions == null || data.Conditions.Length == 0)
            return null;

        foreach (var c in CharacterManager.Instance.Characters)
            if (CheckAllConditions(c, data.Conditions))
                return c;

        return null;
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
                c.HasTrait(condition.RequiredTrait),

            ConditionType.TraitAndCondition =>
                c.HasTrait(condition.RequiredTrait) &&
                c.Condition <= condition.ConditionThreshold,

            ConditionType.TraitAndState =>
                c.HasTrait(condition.RequiredTrait) &&
                c.State == condition.RequiredState,

            _ => false
        };
    }

    // ─────────────────────────────────────────────────
    //  이벤트 발생
    // ─────────────────────────────────────────────────
    void TriggerEvent(BaseEventData data, CharacterEntity triggerChar)
    {
        Debug.Log($"[EventManager] 이벤트 발생: {data.EventName}");

        _lastTriggeredTime[data] = Time.time;
        _activeNotifications.Add(data);
        OnEventTriggered?.Invoke(data);

        ApplyEffects(data, triggerChar);
    }

    void ApplyEffects(BaseEventData data, CharacterEntity triggerChar)
    {
        foreach (var effect in data.Effects)
        {
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

            case EffectType.Progress:
                GameManager.Instance.AddProgress(effect.ProgressType, effect.ProgressAmount);
                break;

            case EffectType.ForceRuntime:
                c.SetRuntimeAction(effect.ForcedRuntime);
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