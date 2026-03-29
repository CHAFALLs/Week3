using UnityEngine;

public class CharacterClickHandler : MonoBehaviour
{
    [SerializeField] UI_RuntimeActionPanel _runtimePanel;
    CharacterEntity _entity;

    public void Init(CharacterEntity entity)
    {
        _entity = entity;
    }

    void OnMouseDown()
    {
        if (TimeManager.Instance.IsInMeeting) return;
        _runtimePanel.Show(_entity);
    }
}
