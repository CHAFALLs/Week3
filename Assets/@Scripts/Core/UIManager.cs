using UnityEngine;

public class UIManager : SingletonBehaviour<UIManager>
{
    [Header("UI 컴포넌트")]
    [SerializeField] UI_CharacterSelectPopup _characterSelectPopup;
    [SerializeField] UI_CharacterBar _characterBar;
    [SerializeField] UI_Hud _hud;
    // [SerializeField] EventPanelUI      _eventPanel;
    // [SerializeField] MeetingPopupUI    _meetingPopup;
    // [SerializeField] DayEndUI          _dayEnd;

    // ─────────────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────────────
    public void Init()
    {
        // 캐릭터 선택 화면만 초기화
        // CharacterSelectPopup은 Start()에서 자체 초기화
        Debug.Log("[UIManager] Init 완료");
    }

    public void InitGameUI()
    {
        _characterBar.Init();
        _hud.Init();
        // _eventPanel.Init();
        // _meetingPopup.Init();
        // _dayEnd.Init();
        Debug.Log("[UIManager] GameUI Init 완료");
    }

    public void Clear() { }
}
