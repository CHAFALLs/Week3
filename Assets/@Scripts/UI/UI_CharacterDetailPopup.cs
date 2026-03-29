using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UI_CharacterDetailPopup : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] GameObject _root;

    [Header("기본 정보")]
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _conditionText;
    [SerializeField] Slider _conditionBar;
    [SerializeField] TextMeshProUGUI _actionText;

    [Header("스탯")]
    [SerializeField] TextMeshProUGUI _hpText;
    [SerializeField] TextMeshProUGUI _planningText;
    [SerializeField] TextMeshProUGUI _clientText;
    [SerializeField] TextMeshProUGUI _artText;

    [Header("특성")]
    [SerializeField] TextMeshProUGUI _traitText;

    [Header("버튼")]
    [SerializeField] Button _closeButton;

    CharacterEntity _current;

    void Awake()
    {
        _root.SetActive(false);
        _closeButton.onClick.AddListener(Hide);
    }

    // ─────────────────────────────────────────────────
    //  Show / Hide
    // ─────────────────────────────────────────────────
    public void Show(CharacterEntity entity)
    {
        // 기존 구독 해제
        if (_current != null)
            _current.OnStatChanged -= OnStatChanged;

        _current = entity;
        _current.OnStatChanged += OnStatChanged;

        Refresh();

        _root.SetActive(true);
        _root.transform.localScale = Vector3.zero;
        _root.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void Hide()
    {
        if (_current != null)
            _current.OnStatChanged -= OnStatChanged;

        _root.transform.DOScale(0f, 0.15f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => _root.SetActive(false));
    }

    // ─────────────────────────────────────────────────
    //  갱신
    // ─────────────────────────────────────────────────
    void OnStatChanged(CharacterEntity entity) => Refresh();

    void Refresh()
    {
        if (_current == null) return;

        // 기본 정보
        _nameText.text = _current.Name;
        _conditionText.text = $"{(int)_current.Condition}%";
        _conditionBar.value = _current.Condition / 100f;
        _actionText.text = _current.GetCurrentBehaviorName();

        // 스탯
        _hpText.text = _current.HP.ToString();
        _planningText.text = _current.Planning.ToString();
        _clientText.text = _current.Client.ToString();
        _artText.text = _current.Art.ToString();

        // 특성
        _traitText.text = TraitHelper.GetTraitsString(_current.Traits);
    }

    
}