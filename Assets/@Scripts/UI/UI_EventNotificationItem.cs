using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_EventNotificationItem : MonoBehaviour
{
    [SerializeField] Image _gradeIndicator;
    [SerializeField] TextMeshProUGUI _titleText;
    [SerializeField] Button _button;

    [Header("등급 색상")]
    [SerializeField] Color _criticalColor = new Color(0.9f, 0.2f, 0.2f);
    [SerializeField] Color _majorColor = new Color(0.9f, 0.7f, 0.0f);
    [SerializeField] Color _minorColor = new Color(0.6f, 0.6f, 0.6f);

    public BaseEventData GetData() => _data;

    BaseEventData _data;
    Action<BaseEventData> _onClicked;

    // ─────────────────────────────────────────────────
    //  Setup
    // ─────────────────────────────────────────────────
    public void Setup(BaseEventData data, Action<BaseEventData> onClicked)
    {
        _data = data;
        _onClicked = onClicked;

        _titleText.text = data.EventName;

        _gradeIndicator.color = data.Grade switch
        {
            EventGrade.Critical => _criticalColor,
            EventGrade.Major => _majorColor,
            EventGrade.Minor => _minorColor,
            _ => _minorColor
        };

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => _onClicked?.Invoke(_data));

        // 등장 연출
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, 0.15f)
            .SetEase(DG.Tweening.Ease.OutBack)
            .SetUpdate(true);
    }
}
