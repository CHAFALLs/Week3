using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Hud : MonoBehaviour
{
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

    // ─────────────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────────────
    public void Init()
    {
        // TimeManager 이벤트 구독
        TimeManager.Instance.OnDayEnd += _ => RefreshDay();
        TimeManager.Instance.OnGamePhaseChanged += _ => RefreshPhase();

        // GameManager 이벤트 구독
        GameManager.Instance.OnProgressChanged += OnProgressChanged;

        Refresh();
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
}
