using System;
using UnityEngine;

public class TimeManager : SingletonBehaviour<TimeManager>
{
    [Header("시간 설정")]
    [SerializeField] float _phaseDuration = 26f;

    [Header("페이즈 경계 (일수 기준)")]
    [SerializeField] int _devStartDay = 4;
    [SerializeField] int _integStartDay = 9;
    [SerializeField] int _totalDays = 10;

    // ─── 상태 ────────────────────────────────────────
    public int Day { get; private set; }
    public DayPhase CurrentDayPhase { get; private set; }
    public GamePhase CurrentGamePhase { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsInMeeting { get; private set; } // 회의 진입중.
    public float PhaseProgress => _timer / _phaseDuration; // 0~1, UI용
    public int RemainingDays => _totalDays - Day + 1;

    // ─── 이벤트 ──────────────────────────────────────
    public event Action<DayPhase> OnMeetingStart;      // 회의 시작 (자동 일시정지)
    public event Action<DayPhase> OnPhaseStart;        // 실시간 진행 시작
    public event Action<bool> OnPauseChanged;      // 일시정지 상태 변경
    public event Action<int> OnDayEnd;            // 날 종료
    public event Action<GamePhase> OnGamePhaseChanged;  // 기획→개발→통합 전환
    public event Action OnGameEnd;           // 게임 종료

    // ─── 내부 ────────────────────────────────────────
    float _timer = 0f;
    bool _initialized = false;

    // ─── Enum ────────────────────────────────────────
    public enum DayPhase { Morning, Lunch, Evening }
    public enum GamePhase { Planning, Development, Integration }

    // ─────────────────────────────────────────────────
    //   Init
    // ─────────────────────────────────────────────────

    public void Init()
    {
        if (_initialized) return;
        _initialized = true;

        Day = 1;
        CurrentDayPhase = DayPhase.Morning;
        CurrentGamePhase = GamePhase.Planning;
        IsPaused = true;   // 캐릭터 선택 전까지 일시정지
        IsInMeeting = false;
        _timer = 0f;

        Debug.Log("[TimeController] Init 완료");
    }

    // ─────────────────────────────────────────────────
    //  Update
    // ─────────────────────────────────────────────────
    void Update()
    {
        if (!_initialized) return;

        // 스페이스바 일시정지 (회의 중엔 불가) TODO: new input으로 할 것임.
        if (Input.GetKeyDown(KeyCode.Space) && !IsInMeeting)
            SetPause(!IsPaused);

        if (IsPaused || IsInMeeting) return;

        _timer += Time.deltaTime;
        if (_timer >= _phaseDuration)
        {
            _timer = 0f;
            AdvancePhase();
        }
    }

    // ─────────────────────────────────────────────────
    //  회의
    // ─────────────────────────────────────────────────
    void StartMeeting()
    {
        IsInMeeting = true;
        SetPause(true);
        OnMeetingStart?.Invoke(CurrentDayPhase);
        Debug.Log($"[회의 시작] {Day}일차 {CurrentDayPhase}");
    }

    // UI에서 행동 배분 완료 후 호출
    public void EndMeeting()
    {
        if (!IsInMeeting) return;

        IsInMeeting = false;
        SetPause(false);
        OnPhaseStart?.Invoke(CurrentDayPhase);
        Debug.Log($"[진행 시작] {Day}일차 {CurrentDayPhase}");
    }

    // ─────────────────────────────────────────────────
    //  시간 진행
    // ─────────────────────────────────────────────────
    void AdvancePhase()
    {
        int next = (int)CurrentDayPhase + 1;

        if (next > (int)DayPhase.Evening)
        {
            EndDay();
        }
        else
        {
            CurrentDayPhase = (DayPhase)next;
            StartMeeting();
        }
    }

    void EndDay()
    {
        OnDayEnd?.Invoke(Day);
        Debug.Log($"[날 종료] {Day}일차");

        Day++;

        if (Day > _totalDays)
        {
            EndGame();
            return;
        }

        UpdateGamePhase();

        CurrentDayPhase = DayPhase.Morning;
        StartMeeting();
    }

    void UpdateGamePhase()
    {
        GamePhase next;

        if (Day < _devStartDay)
            next = GamePhase.Planning;
        else if (Day < _integStartDay)
            next = GamePhase.Development;
        else
            next = GamePhase.Integration;

        if (next == CurrentGamePhase) return;

        CurrentGamePhase = next;
        OnGamePhaseChanged?.Invoke(CurrentGamePhase);
        Debug.Log($"[페이즈 전환] {CurrentGamePhase}");
    }

    void EndGame()
    {
        SetPause(true);
        OnGameEnd?.Invoke();
        Debug.Log("[게임 종료]");
    }

    // ─────────────────────────────────────────────────
    //  일시정지
    // ─────────────────────────────────────────────────
    void SetPause(bool pause)
    {
        if (IsPaused == pause) return;
        IsPaused = pause;
        Time.timeScale = pause ? 0f : 1f;
        OnPauseChanged?.Invoke(IsPaused);
    }

    public void StartGame()
    {
        StartMeeting();  // 여기서 호출
    }

    // 외부 강제 일시정지 (이벤트 팝업 등)
    public void Pause() => SetPause(true);
    public void Resume() => SetPause(false);
}
