using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_MeetingCard : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _conditionText;
    [SerializeField] TextMeshProUGUI _statsText;
    [SerializeField] TextMeshProUGUI _traitsText;
    [SerializeField] TMP_Dropdown _actionDropdown;

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
        _actionDropdown.onValueChanged.RemoveAllListeners();
        Refresh();
        _actionDropdown.onValueChanged.AddListener(OnDropdownChanged);
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
        _conditionText.text = $"{_entity.Condition} ({stateName})";
        _statsText.text = $"{_entity.HP} / {_entity.Planning} / {_entity.Client} / {_entity.Art}";

        // 특성
        var traits = "";
        foreach (var t in _entity.Traits)
            if (t != TraitType.None)
                traits += GetTraitName(t) + ", ";
        _traitsText.text = $"{traits.TrimEnd(',', ' ')}";

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
    //  드롭다운 선택 → 행동 배분
    // ─────────────────────────────────────────────────
    void OnDropdownChanged(int index)
    {
        if (_entity.State == CharacterState.Down) return;
        if (index < 0 || index >= _actionMap.Length) return;
        _entity.SetAssignedAction(_actionMap[index]);
    }

    // TODO: 이거 한 곳에서 관리하는 식으로 변경할 것
    string GetTraitName(TraitType trait) => trait switch
    {
        TraitType.Ace => "에이스",
        TraitType.Fragile => "허약 체질",
        TraitType.BurnoutProne => "번아웃 체질",
        TraitType.Overenthusiast => "의욕 과다",
        TraitType.Ideaman => "아이디어맨",
        TraitType.Troublemaker => "갈등 유발",
        _ => ""
    };
}
