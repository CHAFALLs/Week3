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
        _hpValue.text = data.HP.ToString();
        _planningValue.text = data.Planning.ToString();
        _clientValue.text = data.Client.ToString();
        _artValue.text = data.Art.ToString();

        // 특성
        var traits = "";
        foreach (var t in data.Traits)
            if (t != TraitType.None)
                traits += $"{GetTraitName(t)}\n";
        _traitsText.text = traits.TrimEnd();

        // 카드 등장 연출
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    // 이거 계속 보이는데 손좀 봐야될지도???
    string GetTraitName(TraitType trait) => trait switch
    {
        TraitType.Ace => "에이스",
        TraitType.Fragile => "허약 체질",
        TraitType.BurnoutProne => "번아웃 체질",
        TraitType.Overenthusiast => "의욕 과다",
        TraitType.Ideaman => "아이디어맨",
        TraitType.Troublemaker => "갈등 유발",
        _ => ""
    };
}
