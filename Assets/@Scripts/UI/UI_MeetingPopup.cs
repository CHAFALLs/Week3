using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MeetingPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject _root;
    [SerializeField] TextMeshProUGUI _titleText;
    [SerializeField] UI_MeetingCard[] _cards;
    [SerializeField] Button _startButton;

    public void Init()
    {
        _root.SetActive(false);
        _startButton.onClick.AddListener(OnStartClicked);

        // 회의 시작 이벤트 구독
        TimeManager.Instance.OnMeetingStart += Show;
    }

    // ─────────────────────────────────────────────────
    //  Show / Hide
    // ─────────────────────────────────────────────────
    void Show(TimeManager.DayPhase phase)
    {
        _titleText.text = phase switch
        {
            TimeManager.DayPhase.Morning => "아침 회의(점심까지 할 작업을 정해주세요.)",
            TimeManager.DayPhase.Lunch => "점심 회의(저녁까지 할 작업을 정해주세요.)",
            TimeManager.DayPhase.Evening => "저녁 회의(밤새 할 작업을 정해주세요.)",
            _ => "회의"
        };

        // 카드 세팅
        var characters = CharacterManager.Instance.Characters;
        for (int i = 0; i < _cards.Length; i++)
        {
            if (i < characters.Count)
            {
                _cards[i].gameObject.SetActive(true);
                _cards[i].Setup(characters[i]);
            }
            else
            {
                _cards[i].gameObject.SetActive(false);
            }
        }

        // 팝업 등장
        _root.SetActive(true);
        _root.transform.localScale = Vector3.zero;
        _root.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    void Hide()
    {
        _root.transform.DOScale(0f, 0.15f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => _root.SetActive(false));
    }

    // ─────────────────────────────────────────────────
    //  시작 버튼
    // ─────────────────────────────────────────────────
    void OnStartClicked()
    {
        Hide();
        TimeManager.Instance.EndMeeting();
    }
}
