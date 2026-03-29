using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_RuntimeActionPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject _root;
    [SerializeField] Button _blocker;        // ÀüÃ¼ È­¸é Åõ¸í ¹öÆ°
    [SerializeField] Button _restButton;
    [SerializeField] Button _exerciseButton;
    [SerializeField] Button _coffeeButton;
    [SerializeField] Button _trailButton;

    CharacterEntity _entity;

    void Awake()
    {

        GetComponent<Canvas>().worldCamera = Camera.main;

        _root.SetActive(false);

        _blocker.onClick.AddListener(Hide);
        _restButton.onClick.AddListener(() => OnActionSelected(RuntimeAction.Rest));
        _exerciseButton.onClick.AddListener(() => OnActionSelected(RuntimeAction.Exercise));
        _coffeeButton.onClick.AddListener(() => OnActionSelected(RuntimeAction.Coffee));
        _trailButton.onClick.AddListener(() => OnActionSelected(RuntimeAction.Trail));
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    //  Show / Hide
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void Show(CharacterEntity entity)
    {
        // ÀÌ¹Ì ¿­·ÁÀÖÀ¸¸é ´Ý±â
        if (_root.activeSelf)
        {
            Hide();
            return;
        }

        _entity = entity;
        RefreshButtons();

        _root.SetActive(true);
        _root.transform.localScale = Vector3.zero;
        _root.transform.DOScale(1f, 0.15f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    public void Hide()
    {
        _root.transform.DOScale(0f, 0.1f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => _root.SetActive(false));
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    //  ¹öÆ° »óÅÂ °»½Å
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void RefreshButtons()
    {
        SetButtonState(_restButton, RuntimeAction.Rest);
        SetButtonState(_exerciseButton, RuntimeAction.Exercise);
        SetButtonState(_coffeeButton, RuntimeAction.Coffee);
    }

    void SetButtonState(Button button, RuntimeAction action)
    {
        bool isActive = _entity.ActiveRuntime == action;
        bool isForced = isActive && _entity.IsForcedRuntime;
        var text = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        var image = button.GetComponent<Image>();

        if (text != null)
        {
            text.text = action switch
            {
                RuntimeAction.Rest => isActive ? "ÈÞ½Ä ²ô±â" : "ÈÞ½Ä",
                RuntimeAction.Exercise => isActive ? "Çï½º ²ô±â" : "Çï½º",
                RuntimeAction.Coffee => isActive ? "Ä¿ÇÇ ²ô±â" : "Ä¿ÇÇ",
                _ => ""
            };
        }

        if (image != null)
            image.color = isForced ? Color.red : Color.white;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    //  ¹öÆ° Å¬¸¯
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void OnActionSelected(RuntimeAction action)
    {
        if (_entity == null) return;

        if (_entity.ActiveRuntime == action)
        {
            if (_entity.State == CharacterState.Down) return;
            _entity.ClearRuntimeAction();
        }
        else
        {
            _entity.SetRuntimeAction(action);
        }

        RefreshButtons();
        Hide();  // ¼±ÅÃ ÈÄ ÆÐ³Î ´Ý±â
    }
}
