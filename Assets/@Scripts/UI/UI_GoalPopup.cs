using DG.Tweening;
using TMPro;
using UnityEngine;

public class UI_GoalPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject _root;
    [SerializeField] TextMeshProUGUI _hintText;  // "아무 키나 눌러 시작"

    bool _isReady = false;  // 팝업이 다 열린 후에만 입력 받기

    void Awake()
    {
        _root.SetActive(false);
    }

    // ─────────────────────────────────────────────────
    //  Show / Hide
    // ─────────────────────────────────────────────────
    public void Show()
    {
        _isReady = false;
        _root.SetActive(true);
        _root.transform.localScale = Vector3.zero;
        _root.transform.DOScale(1f, 0.3f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _isReady = true;
                // 힌트 텍스트 깜빡이기
                _hintText.DOFade(0f, 0.8f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);
            });
    }

    void Hide()
    {
        _hintText.DOKill();
        _root.transform.DOKill();
        _root.SetActive(false);
    }

    // ─────────────────────────────────────────────────
    //  아무 키나 누르면 캐릭터 선택으로
    // ─────────────────────────────────────────────────
    void Update()
    {
        if (!_isReady) return;
        if (!_root.activeSelf) return;
        if (Input.anyKeyDown)
        {
            Hide();
            UIManager.Instance.ShowCharacterSelect();
        }
    }
}
