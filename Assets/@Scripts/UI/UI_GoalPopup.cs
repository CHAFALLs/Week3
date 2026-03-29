using DG.Tweening;
using TMPro;
using UnityEngine;

public class UI_GoalPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] TextMeshProUGUI _hintText;  // "아무 키나 눌러 시작"

    bool _isReady = false;  // 팝업이 다 열린 후에만 입력 받기

    void Awake()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    // ─────────────────────────────────────────────────
    //  Show / Hide
    // ─────────────────────────────────────────────────
    public void Show()
    {
        _isReady = false;
        _canvasGroup.DOKill();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.DOFade(1f, 0.3f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _isReady = true;
                _hintText.DOFade(0f, 0.8f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);
            });
    }

    void Hide()
    {
        _isReady = false;
        _hintText.DOKill();
        _canvasGroup.DOKill();
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = 0f;
    }

    // ─────────────────────────────────────────────────
    //  아무 키나 누르면 캐릭터 선택으로
    // ─────────────────────────────────────────────────
    void Update()
    {
        if (!_isReady) return;
        if (_canvasGroup.alpha <= 0f) return;
        if (Input.anyKeyDown)
        {
            Hide();
            UIManager.Instance.ShowCharacterSelect();
        }
    }
}
