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
    [SerializeField] Button _tipButton;
    [SerializeField] Button _startButton;    // Morning / Lunch
    [SerializeField] Button _skipButton;     // Evening → 넘기기
    [SerializeField] Button _overtimeButton; // Evening → 야근하기

    public void Init()
    {
        _root.SetActive(false);

        _tipButton.onClick.AddListener(() =>
        UIManager.Instance.ShowTip(
            "회의 가이드",
            "[행동 선택 팁]\n" +
            "기획/클라/아트: 해당 스탯이 높을수록 진행도 기여가 높아요.\n" +
            "자기주도 학습: 진행도 기여는 절반이지만 컨디션 소모량이 적고 스탯이 성장해요.\n" +
            "장기적으로 효율이 올라가니 초반에 투자해보세요.\n\n" +
            "[페이즈별 진행도 가중치]\n" +
            "기획 페이즈:  기획 x1.0 | 클라 x0.7 | 아트 x0.7\n" +
            "개발 페이즈:  기획 x0.7 | 클라 x1.0 | 아트 x1.0\n" +
            "통합 페이즈:  기획 x0.5 | 클라 x1.2 | 아트 x0.8\n\n" +
            "[컨디션 관리]\n" +
            "컨디션이 낮으면 작업 효율이 떨어져요.\n" +
            "야근은 진행도를 더 올릴 수 있지만 컨디션 소모가 커요.\n" +
            "야근하지 않으면 전체 컨디션이 조금 회복돼요."
        ));

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
            c.ChangeCondition(20f);  // 숙면 회복 // TODO: 사실 이것도 CharacterManager쪽에서 함수 지원을 해주는게 맞음.
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
