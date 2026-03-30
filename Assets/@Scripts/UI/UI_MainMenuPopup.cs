using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenuPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] Button _easyStartButton;    // 쉬움 모드 시작
    [SerializeField] Button _normalStartButton;  // 보통 모드 시작
    [SerializeField] Button _quitButton;         // 나가기

    void Awake()
    {
        _easyStartButton.onClick.AddListener(OnEasyStartClicked);
        _normalStartButton.onClick.AddListener(OnNormalStartClicked);
        _quitButton.onClick.AddListener(OnQuitClicked);
    }

    void Start()
    {
        Show();
    }

    // ─────────────────────────────────────────────────
    //  Show / Hide
    // ─────────────────────────────────────────────────
    void Show()
    {
        _canvasGroup.DOKill();
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
    }

    void Hide()
    {
        _canvasGroup.DOKill();
        _canvasGroup.DOFade(0f, 0.2f)
        .SetUpdate(true)
        .OnComplete(() =>
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        });
    }

    // ─────────────────────────────────────────────────
    //  버튼 이벤트
    // ─────────────────────────────────────────────────
    void OnEasyStartClicked()
    {
        GameManager.Instance.SetDifficulty(GameManager.Difficulty.Easy);
        Hide();
        UIManager.Instance.ShowGoal();
    }

    void OnNormalStartClicked()
    {
        GameManager.Instance.SetDifficulty(GameManager.Difficulty.Normal);
        Hide();
        UIManager.Instance.ShowGoal();
    }

    void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
