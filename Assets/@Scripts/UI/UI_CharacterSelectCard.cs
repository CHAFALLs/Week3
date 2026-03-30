using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CharacterSelectCard : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Image _portrait;
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _hpValue;
    [SerializeField] TextMeshProUGUI _planningValue;
    [SerializeField] TextMeshProUGUI _clientValue;
    [SerializeField] TextMeshProUGUI _artValue;
    [SerializeField] TextMeshProUGUI _traitsText;

    // ─────────────────────────────────────────────────
    //  Setup
    // ─────────────────────────────────────────────────
    public void Setup(CharacterData data)
    {
        _nameText.text = data.CharacterName;
        _portrait.sprite = data.Portrait;
        _portrait.preserveAspect = true;
        _hpValue.text = data.HP.ToString();
        _planningValue.text = data.Planning.ToString();
        _clientValue.text = data.Client.ToString();
        _artValue.text = data.Art.ToString();

        // 특성
        _traitsText.text = TraitHelper.GetTraitsString(data.Traits);

        // 카드 등장 연출
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

 
}
