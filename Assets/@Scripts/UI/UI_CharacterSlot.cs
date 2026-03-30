using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_CharacterSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _actionIconText;  // 이모지 아이콘
    [SerializeField] Image _portrait;
    [SerializeField] Image _stateIndicator;  // 상태 색상 표시

    // 상태별 색상
    [Header("State Colors")]
    [SerializeField] Color _normalColor = new Color(0.2f, 0.8f, 0.4f);  // 초록
    [SerializeField] Color _sickColor = new Color(1.0f, 0.7f, 0.0f);  // 노랑
    [SerializeField] Color _downColor = new Color(0.8f, 0.2f, 0.2f);  // 빨강

    CharacterEntity _entity;
    UI_CharacterDetailPopup _detailPopup;
    CharacterState _prevState = CharacterState.Normal; // 한번만 흔들기 위함.

    public void Init(CharacterEntity entity, UI_CharacterDetailPopup detailPopup)
    {
        _entity = entity;
        _detailPopup = detailPopup;

        _portrait.sprite = entity.Data.Portrait;
        _portrait.preserveAspect = true;

        RefreshState();
        Refresh();

        // 이벤트 구독
        _entity.OnStatChanged += _ => Refresh();
        _entity.OnActionChanged += _ => Refresh();
        _entity.OnStateChanged += (_, __) => RefreshState();
    }

    // ─────────────────────────────────────────────────
    //  갱신
    // ─────────────────────────────────────────────────
    void Refresh()
    {
        _nameText.text = _entity.Name;
    }

    void RefreshState()
    {
        _stateIndicator.color = _entity.State switch
        {
            CharacterState.Normal => _normalColor,
            CharacterState.Sick => _sickColor,
            CharacterState.Down => _downColor,
            _ => _normalColor
        };

        // Down 상태 진입 시 한 번만 흔들기
        if (_entity.State == CharacterState.Down && _prevState != CharacterState.Down)
        {
            transform.DOKill();
            transform.DOShakePosition(0.3f, strength: 5f, vibrato: 10);
        }

        _prevState = _entity.State;
    }

    // ─────────────────────────────────────────────────
    //  클릭 → 상세 팝업
    // ─────────────────────────────────────────────────
    public void OnPointerClick(PointerEventData eventData)
    {
        _detailPopup.Show(_entity);
    }
}
