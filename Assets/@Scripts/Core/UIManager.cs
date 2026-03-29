using UnityEngine;

public class UIManager : SingletonBehaviour<UIManager>
{
    [Header("UI 闡ん凱お")]
    [SerializeField] UI_MainMenuPopup _mainMenuPopup;
    [SerializeField] UI_GoalPopup _goalPopup;
    [SerializeField] UI_CharacterSelectPopup _characterSelectPopup;
    [SerializeField] UI_CharacterBar _characterBar;
    [SerializeField] UI_Hud _hud;
    [SerializeField] UI_EventPanel _eventPanel;
    [SerializeField] UI_MeetingPopup _meetingPopup;
    [SerializeField] UI_DayTransition _dayTransition;
    // [SerializeField] DayEndUI          _dayEnd;

    // 式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式
    //  Init
    // 式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式
    public void Init()
    {
        Debug.Log("[UIManager] Init 諫猿");
    }

    public void InitGameUI()
    {
        _characterBar.Init();
        _hud.Init();
        _meetingPopup.Init();
        _eventPanel.Init();
        _dayTransition.Init();
        // _dayEnd.Init();
        Debug.Log("[UIManager] GameUI Init 諫猿");
    }

    // 式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式
    //  で機 ��轎
    // 式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式

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
