using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameEndPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] TextMeshProUGUI _gradeText;
    [SerializeField] TextMeshProUGUI _commentText;

    [Header("진행도")]
    [SerializeField] Slider _planningBar;
    [SerializeField] TextMeshProUGUI _planningText;
    [SerializeField] Slider _clientBar;
    [SerializeField] TextMeshProUGUI _clientText;
    [SerializeField] Slider _artBar;
    [SerializeField] TextMeshProUGUI _artText;
    [SerializeField] Slider _averageBar;
    [SerializeField] TextMeshProUGUI _averageText;

    [Header("버튼")]
    [SerializeField] Button _restartButton;

    void Awake()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        _restartButton.onClick.AddListener(OnRestartClicked);
    }

    public void Init()
    {
        TimeManager.Instance.OnGameEnd += Show;
    }

    // ─────────────────────────────────────────────────
    //  Show
    // ─────────────────────────────────────────────────
    void Show()
    {
        float planning = GameManager.Instance.PlanningProgress;
        float client = GameManager.Instance.ClientProgress;
        float art = GameManager.Instance.ArtProgress;
        float average = (planning + client + art) / 3f;

        // 진행도 바
        SetBar(_planningBar, _planningText, planning);
        SetBar(_clientBar, _clientText, client);
        SetBar(_artBar, _artText, art);
        SetBar(_averageBar, _averageText, average);

        // 등급
        string grade = GetGrade(average);
        string comment = GetComment(grade);
        _gradeText.text = grade;
        _commentText.text = comment;

        // 등장
        _canvasGroup.DOKill();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.DOFade(1f, 0.5f).SetUpdate(true);
    }

    // ─────────────────────────────────────────────────
    //  등급 / 총평
    // ─────────────────────────────────────────────────
    string GetGrade(float average)
    {
        if (average >= 0.8f) return "S";
        if (average >= 0.6f) return "A";
        if (average >= 0.4f) return "B";
        if (average >= 0.2f) return "C";
        return "D";
    }

    string GetComment(string grade) => grade switch
    {
        "S" => "완벽한 팀워크!\n정글을 정복했습니다!",
        "A" => "훌륭한 성과!\n다음엔 더 잘할 수 있어요.",
        "B" => "선방했습니다.\n아쉬운 부분이 있네요.",
        "C" => "많이 힘드셨죠?\n다음엔 더 잘할 수 있어요.",
        "D" => "정글이 당신을\n삼켜버렸습니다...",
        _ => ""
    };

    void SetBar(Slider bar, TextMeshProUGUI text, float value)
    {
        bar.value = value;
        text.text = $"{Mathf.RoundToInt(value * 100f)}%";
    }

    // ─────────────────────────────────────────────────
    //  다시하기
    // ─────────────────────────────────────────────────
    void OnRestartClicked()
    {
        // Good
        Destroy(BootingManager.Instance.gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
