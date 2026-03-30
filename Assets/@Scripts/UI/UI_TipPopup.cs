using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_TipPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] TextMeshProUGUI _titleText;
    [SerializeField] TextMeshProUGUI _contentText;
    [SerializeField] Button _closeButton;
    [SerializeField] Button _background;

    void Awake()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        _closeButton.onClick.AddListener(Hide);
        _background.onClick.AddListener(Hide);
    }

    // 式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式
    //  Show / Hide
    // 式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式
    public void Show(string title, string content)
    {
        _titleText.text = title;
        _contentText.text = content;

        _canvasGroup.DOKill();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.DOFade(1f, 0.2f).SetUpdate(true);
    }

    public void Hide()
    {
        _canvasGroup.DOKill();
        _canvasGroup.DOFade(0f, 0.15f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            });
    }
}
