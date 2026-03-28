using UnityEngine;

public class UI_CharacterBar : MonoBehaviour
{
    [SerializeField] UI_CharacterSlot[] _slots;  // Inspector에서 5개 연결
    [SerializeField] UI_CharacterDetailPopup _detailPopup;

    public void Init()
    {
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
