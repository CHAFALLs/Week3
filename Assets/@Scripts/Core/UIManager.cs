using UnityEngine;

public class UIManager : SingletonBehaviour<UIManager>
{
    [Header("UI 컴포넌트")]
    [SerializeField] UI_MainMenuPopup _mainMenuPopup;
    [SerializeField] UI_GoalPopup _goalPopup;
    [SerializeField] UI_CharacterSelectPopup _characterSelectPopup;
    [SerializeField] UI_CharacterBar _characterBar;
    [SerializeField] UI_Hud _hud;
    [SerializeField] UI_EventPanel _eventPanel;
    [SerializeField] UI_MeetingPopup _meetingPopup;
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
        _meetingPopup.Init();
        _eventPanel.Init();
        // _dayEnd.Init();
        Debug.Log("[UIManager] GameUI Init 완료");
    }

    // ─────────────────────────────────────────────────
    //  팝업 호출
    // ─────────────────────────────────────────────────

    public void ShowGoal()
    {
        _goalPopup.Show();
    }

    public void ShowCharacterSelect()
    {
        _characterSelectPopup.Show();
    }

    public void ShowMeetingPopup()
    {
        _meetingPopup.Show(TimeManager.Instance.CurrentDayPhase);
    }

    public void Clear() { }
}
