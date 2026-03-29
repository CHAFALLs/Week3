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

    [Header("버튼")]
    [SerializeField] Button _startButton;    // Morning / Lunch
    [SerializeField] Button _skipButton;     // Evening → 넘기기
    [SerializeField] Button _overtimeButton; // Evening → 야근하기

    public void Init()
    {
        _root.SetActive(false);
        _startButton.onClick.AddListener(OnStartClicked);
        _skipButton.onClick.AddListener(OnSkipClicked);
        _overtimeButton.onClick.AddListener(OnOvertimeClicked);

    }

    // ─────────────────────────────────────────────────
    //  Show / Hide
    // ─────────────────────────────────────────────────
    public void Show(TimeManager.DayPhase phase)
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

        // Evening이면 넘기기/야근하기, 아니면 시작
        bool isEvening = phase == TimeManager.DayPhase.Evening;
        _startButton.gameObject.SetActive(!isEvening);
        _skipButton.gameObject.SetActive(isEvening);
        _overtimeButton.gameObject.SetActive(isEvening);

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
    // Morning / Lunch → 시작
    void OnStartClicked()
    {

        // 전체 카드 선택값 한번에 반영
        foreach (var card in _cards)
            if (card.gameObject.activeSelf)
                card.ApplySelection();

        Hide();
        TimeManager.Instance.EndMeeting();
    }

    // Evening → 넘기기 (야근 없음, 컨디션 회복, 다음날)
    void OnSkipClicked()
    {

        // 전원 야근 해제 + 컨디션 회복
        foreach (var c in CharacterManager.Instance.Characters)
        {
            c.SetOvertime(false);
            c.ChangeCondition(30);  // 숙면 회복 // TODO: 사실 이것도 CharacterManager쪽에서 함수 지원을 해주는게 맞음.
        }

        // 애니메이션 없이 즉시 닫기 (TODO: 수정 필요.)
        _root.transform.DOKill();
        _root.SetActive(false);

        TimeManager.Instance.SkipToNextDay();
    }

    // Evening → 야근하기
    void OnOvertimeClicked()
    {
        ApplyAllCards();

        // 전원 야근 설정
        foreach (var c in CharacterManager.Instance.Characters)
            c.SetOvertime(true);

        Hide();
        TimeManager.Instance.EndMeeting();
    }

    // 모든 카드 반영
    void ApplyAllCards()
    {
        foreach (var card in _cards)
            if (card.gameObject.activeSelf)
                card.ApplySelection();
    }

}
