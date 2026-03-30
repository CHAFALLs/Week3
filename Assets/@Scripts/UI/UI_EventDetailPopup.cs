using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_EventDetailPopup : MonoBehaviour
{
    [SerializeField] GameObject _root;
    [SerializeField] TextMeshProUGUI _titleText;
    [SerializeField] TextMeshProUGUI _descriptionText;
    [SerializeField] TextMeshProUGUI _gradeText;
    [SerializeField] Button _closeButton;
    [SerializeField] Button _blocker;

    BaseEventData _current;

    void Awake()
    {
        _root.SetActive(false);
        _closeButton.onClick.AddListener(Hide);
        _blocker.onClick.AddListener(Hide);
        TimeManager.Instance.OnMeetingStart += _ => Hide();
    }

    // ─────────────────────────────────────────────────
    //  Show / Hide
    // ─────────────────────────────────────────────────
    public void Show(BaseEventData data)
    {
        _current = data;

        _titleText.text = data.EventName;
        _descriptionText.text = data.Description;
        _gradeText.text = data.Grade switch
        {
            EventGrade.Critical => "대형",
            EventGrade.Major => "중규모",
            EventGrade.Minor => "소규모",
            _ => ""
        };

        _root.SetActive(true);
        _root.transform.localScale = Vector3.zero;
        _root.transform.DOScale(1f, 0.2f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    public void Hide()
    {
        // 알림 해제
        if (_current != null)
            EventManagerEx.Instance.ResolveNotification(_current);

        _root.transform.DOScale(0f, 0.15f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => _root.SetActive(false));
    }
}
