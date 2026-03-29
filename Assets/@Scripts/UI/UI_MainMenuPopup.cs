using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenuPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject _root;
    [SerializeField] Button _startButton;
    [SerializeField] Button _quitButton;

    void Awake()
    {
        _startButton.onClick.AddListener(OnStartClicked);
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
        _root.SetActive(true);
        _root.transform.localScale = Vector3.zero;
        _root.transform.DOScale(1f, 0.3f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    void Hide()
    {
        _root.transform.DOKill();
        _root.SetActive(false);
    }

    // ─────────────────────────────────────────────────
    //  버튼 이벤트
    // ─────────────────────────────────────────────────
    void OnStartClicked()
    {
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
