using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Hud : MonoBehaviour
{
    [SerializeField] CanvasGroup _canvasGroup;

    [Header("시간 정보")]
    [SerializeField] TextMeshProUGUI _dayText;
    [SerializeField] TextMeshProUGUI _phaseText;

    [Header("진행도 바")]
    [SerializeField] Slider _planningBar;
    [SerializeField] TextMeshProUGUI _planningText;

    [SerializeField] Slider _clientBar;
    [SerializeField] TextMeshProUGUI _clientText;

    [SerializeField] Slider _artBar;
    [SerializeField] TextMeshProUGUI _artText;

    [SerializeField] TextMeshProUGUI _nextMeetingText;

    // 일시정지
    [SerializeField] Button _pauseButton;
    [SerializeField] TextMeshProUGUI _pauseButtonText;

    bool _initialized = false;

    void Awake()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    // ─────────────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────────────
    public void Init()
    {
        _initialized = true;

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        // TimeManager 이벤트 구독
        TimeManager.Instance.OnDayEnd += _ => RefreshDay();
        TimeManager.Instance.OnGamePhaseChanged += _ => RefreshPhase();

        // GameManager 이벤트 구독
        GameManager.Instance.OnProgressChanged += OnProgressChanged;

        _pauseButton.onClick.AddListener(OnPauseButtonClicked);
        TimeManager.Instance.OnPauseChanged += RefreshPauseButton;

        Refresh();
    }

    // ─────────────────────────────────────────────────
    //  Update — 타이머 갱신
    // ─────────────────────────────────────────────────
    void Update()
    {
        if (!_initialized) return;
        RefreshNextMeeting();
    }

    // ─────────────────────────────────────────────────
    //  갱신
    // ─────────────────────────────────────────────────
    void Refresh()
    {
        RefreshDay();
        RefreshPhase();
        RefreshProgress();
    }

    void RefreshDay()
    {
        _dayText.text = $"D-{TimeManager.Instance.RemainingDays:D2}";
    }

    void RefreshPhase()
    {
        _phaseText.text = TimeManager.Instance.CurrentGamePhase switch
        {
            TimeManager.GamePhase.Planning => "기획 페이즈",
            TimeManager.GamePhase.Development => "개발 페이즈",
            TimeManager.GamePhase.Integration => "통합 페이즈",
            _ => ""
        };
    }

    void RefreshNextMeeting()
    {
        if (TimeManager.Instance.IsInMeeting)
        {
            _nextMeetingText.text = "회의 중";
            return;
        }

        _nextMeetingText.text = $"다음 회의까지 {TimeManager.Instance.RemainingPhaseTime}초";
    }

    void OnProgressChanged(ProgressType type, float value)
    {
        switch (type)
        {
            case ProgressType.Planning: SetBar(_planningBar, _planningText, value); break;
            case ProgressType.Client: SetBar(_clientBar, _clientText, value); break;
            case ProgressType.Art: SetBar(_artBar, _artText, value); break;
        }
    }

    void RefreshProgress()
    {
        SetBar(_planningBar, _planningText, GameManager.Instance.PlanningProgress);
        SetBar(_clientBar, _clientText, GameManager.Instance.ClientProgress);
        SetBar(_artBar, _artText, GameManager.Instance.ArtProgress);
    }

    void SetBar(Slider bar, TextMeshProUGUI text, float value)
    {
        bar.value = value;
        text.text = $"{Mathf.RoundToInt(value * 100f)}%";
    }

    void OnPauseButtonClicked()
    {
        if (TimeManager.Instance.IsPaused)
            TimeManager.Instance.Resume();
        else
            TimeManager.Instance.Pause();
    }

    void RefreshPauseButton(bool isPaused)
    {
        _pauseButtonText.text = isPaused ? "▶" : "⏸";
    }
}
