using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_CharacterSelectPopup : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] GameObject _root;

    [Header("카드")]
    [SerializeField] UI_CharacterSelectCard[] _cards;  // 5개

    [Header("버튼")]
    [SerializeField] Button _rerollButton;
    [SerializeField] Button _startButton;

    // ─────────────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────────────
    void Start()
    {

        _rerollButton.onClick.AddListener(OnReroll);
        _startButton.onClick.AddListener(OnStart);

        // CharacterManager 이벤트 구독
        CharacterManager.Instance.OnRerolled += RefreshCards;

        _root.SetActive(false);  // 기본 비활성화
    }

    // ─────────────────────────────────────────────────
    //  Show / Hide
    // ─────────────────────────────────────────────────
    public void Show()
    {
        CharacterManager.Instance.Reroll();

        _root.SetActive(true);
        _root.transform.localScale = Vector3.zero;
        _root.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);

    }

    void Hide()
    {
        _root.transform.DOScale(0f, 0.2f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => _root.SetActive(false));
    }

    // ─────────────────────────────────────────────────
    //  버튼 이벤트
    // ─────────────────────────────────────────────────
    void OnReroll()
    {
        // 카드 흔들기 연출
        foreach (var card in _cards)
            card.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f).SetUpdate(true);

        CharacterManager.Instance.Reroll();
    }

    void OnStart()
    {
        CharacterManager.Instance.ConfirmSelection();
        UIManager.Instance.InitGameUI();  // 게임 UI 초기화
        Hide();
        TimeManager.Instance.StartGame();
    }

    // ─────────────────────────────────────────────────
    //  카드 갱신
    // ─────────────────────────────────────────────────
    void RefreshCards(CharacterData[] data)
    {
        // data가 null이면 CharacterManager에서 직접 가져옴
        // (Start에서 OnRerolled 구독 전에 호출될 때)
        var selected = data;
        if (selected == null)
        {
            // 첫 Init 시 이미 Reroll 된 상태
            CharacterManager.Instance.Reroll();
            return;
        }

        for (int i = 0; i < _cards.Length; i++)
        {
            if (i < selected.Length)
            {
                _cards[i].gameObject.SetActive(true);
                _cards[i].Setup(selected[i]);
            }
            else
            {
                _cards[i].gameObject.SetActive(false);
            }
        }
    }
}
