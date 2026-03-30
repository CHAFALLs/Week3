using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MeetingCard : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _conditionText;
    [SerializeField] TextMeshProUGUI _statsText;
    [SerializeField] TextMeshProUGUI _traitsText;
    [SerializeField] TMP_Dropdown _actionDropdown;
    [SerializeField] Image _portrait;

    CharacterEntity _entity;

    static readonly AssignedAction[] _actionMap = new[]
    {
        AssignedAction.Planning,
        AssignedAction.Client,
        AssignedAction.Art,
        AssignedAction.SelfStudy_Planning,
        AssignedAction.SelfStudy_Client,
        AssignedAction.SelfStudy_Art,
    };

    // ─────────────────────────────────────────────────
    //  Setup
    // ─────────────────────────────────────────────────
    public void Setup(CharacterEntity entity)
    {
        _entity = entity;
        _portrait.sprite = _entity.Data.Portrait;
        _portrait.preserveAspect = true;
        Refresh();
    }

    // ─────────────────────────────────────────────────
    //  갱신
    // ─────────────────────────────────────────────────
    void Refresh()
    {
        _nameText.text = _entity.Name;

        string stateName = _entity.State switch
        {
            CharacterState.Normal => "보통",
            CharacterState.Sick => "아픔",
            CharacterState.Down => "다운",
            _ => ""
        };
        _conditionText.text = $"컨디션: {(int)_entity.Condition} ({stateName})";
        _statsText.text = $"{_entity.HP} / {_entity.Planning} / {_entity.Client} / {_entity.Art}";

        // 특성
        _traitsText.text = TraitHelper.GetTraitsString(_entity.Traits);

        RefreshDropdown();
    }

    void RefreshDropdown()
    {
        _actionDropdown.ClearOptions();

        // Down 상태 → 옵션 하나만
        if (_entity.State == CharacterState.Down)
        {
            _actionDropdown.AddOptions(new List<string> { "휴식 중 (다운)" });
            _actionDropdown.interactable = false;
            return;
        }

        // 정상/아픔 → 일반 옵션
        _actionDropdown.AddOptions(new List<string>
        {
            "기획",
            "클라 개발",
            "아트",
            "기획 공부",
            "클라 공부",
            "아트 공부",
        });
        _actionDropdown.interactable = true;

        // 현재 배분된 행동으로 초기값 설정
        int currentIndex = System.Array.IndexOf(_actionMap, _entity.AssignedAction);
        _actionDropdown.value = currentIndex >= 0 ? currentIndex : 0;
    }

    // ─────────────────────────────────────────────────
    //  Start 클릭 시 선택값 반영
    // ─────────────────────────────────────────────────
    public void ApplySelection()
    {
        if (_entity.State == CharacterState.Down) return;

        int index = _actionDropdown.value;
        if (index < 0 || index >= _actionMap.Length) return;
        _entity.SetAssignedAction(_actionMap[index]);
    }

    
}
