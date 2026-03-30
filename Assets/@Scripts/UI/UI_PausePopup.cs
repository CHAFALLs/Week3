using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_PausePopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] Button _mainMenuButton;
    [SerializeField] Button _cancelButton;

    bool _gameStarted = false;
    bool _isShowing = false;

    void Awake()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        _cancelButton.onClick.AddListener(Hide);

       
    }

    private void Start()
    {
        TimeManager.Instance.OnGameStart += () => _gameStarted = true;
    }

    void Update()
    {
        if (!_gameStarted) return;
        if (TimeManager.Instance.IsInMeeting) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isShowing) Hide();
            else Show();
        }
    }

    // ─────────────────────────────────────────────────
    //  Show / Hide
    // ─────────────────────────────────────────────────
    void Show()
    {
        _isShowing = true;
        TimeManager.Instance.Pause();

        _canvasGroup.DOKill();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.DOFade(1f, 0.2f).SetUpdate(true);
    }

    void Hide()
    {
        _isShowing = false;
        TimeManager.Instance.Resume();

        _canvasGroup.DOKill();
        _canvasGroup.DOFade(0f, 0.15f)
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
    void OnMainMenuClicked()
    {
        Destroy(BootingManager.Instance.gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
