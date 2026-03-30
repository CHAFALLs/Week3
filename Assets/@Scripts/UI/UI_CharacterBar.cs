using UnityEngine;
using UnityEngine.UI;

public class UI_CharacterBar : MonoBehaviour
{

    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] UI_CharacterSlot[] _slots;  // Inspector에서 5개 연결
    [SerializeField] UI_CharacterDetailPopup _detailPopup;
    [SerializeField] Button _tipButton;

    void Awake()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    public void Init()
    {

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        _tipButton.onClick.AddListener(() =>
        UIManager.Instance.ShowTip(
            "조작 안내",
            "[기본 조작]\n" +
            "스페이스바: 일시정지 / 재개\n" +
            "WASD / 화살표: 카메라 이동\n" +
            "마우스 휠: 줌 인 / 아웃\n" +
            "ESC: 메인 메뉴로\n\n" +
            "[캐릭터 조작]\n" +
            "캐릭터 클릭: 실시간 행동 부여\n" +
             "상단 슬롯 클릭: 캐릭터 상세 정보\n\n" +
            "[이벤트]\n" +
            "우측 이벤트 알림 클릭: 이벤트 상세 내용 확인"
        ));

        var characters = CharacterManager.Instance.Characters;

        for (int i = 0; i < _slots.Length; i++)
        {
            if (i < characters.Count)
            {
                _slots[i].gameObject.SetActive(true);
                _slots[i].Init(characters[i], _detailPopup);
            }
            else
            {
                // 캐릭터 수보다 슬롯이 많으면 숨김
                _slots[i].gameObject.SetActive(false);
            }
        }
    }
}
