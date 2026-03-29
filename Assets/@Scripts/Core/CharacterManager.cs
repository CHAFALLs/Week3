using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterManager : SingletonBehaviour<CharacterManager>
{
    // ─── 캐릭터 풀 ────────────────────────────────────
    [SerializeField] CharacterData[] _characterPool;  // 전체 풀
    [SerializeField] int _teamSize = 5;   // 선택 인원

    CharacterData[] _selectedData;        // 현재 선택된 캐릭터

    List<CharacterEntity> _characters = new List<CharacterEntity>();
    public IReadOnlyList<CharacterEntity> Characters => _characters;

    // ─── 이벤트 ──────────────────────────────────────
    public event Action<CharacterData[]> OnRerolled;          // 다시 굴리기
    public event Action<CharacterEntity> OnCharacterDown;       // 캐릭터 다운
    public event Action<CharacterEntity> OnCharacterRecovered;  // 다운 → Sick 회복

    private bool _initialized = false;
    private int _arrivedAtMeetingCount = 0; // 회의실에 모두 다 도착했는지 체크용.

    // ─────────────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────────────

    public void Init()
    {
        if (_initialized) return;
        _initialized = true;

        // 캐릭터 선택 화면에서 보여줄 첫 랜덤 선택
        Reroll();

        Debug.Log("[CharacterManager] Init 완료");
    }

    #region 캐릭터 랜덤 선택

    public void Reroll()
    {
        _selectedData = SelectRandom(_characterPool, _teamSize);
        OnRerolled?.Invoke(_selectedData);
        Debug.Log("[CharacterManager] 캐릭터 재선택");
    }

    CharacterData[] SelectRandom(CharacterData[] pool, int count)
    {
        var list = new List<CharacterData>(pool);

        // Fisher-Yates 셔플
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        var result = new CharacterData[Mathf.Min(count, list.Count)];
        for (int i = 0; i < result.Length; i++)
            result[i] = list[i];

        return result;
    }

    // ─────────────────────────────────────────────────
    //  선택 확정 → 게임 시작
    // ─────────────────────────────────────────────────
    public void ConfirmSelection()
    {
        LoadCharacters(_selectedData);
        SpawnCharacters(_selectedData);

        // 틱 처리 구독 (게임 시작 후부터)
        TimeManager.Instance.OnPhaseStart += OnPhaseTick;
        TimeManager.Instance.OnPhaseEnd += OnPhaseEnd;

        Debug.Log("[CharacterManager] 팀 구성 확정");
    }

    #endregion

    void SpawnCharacters(CharacterData[] dataArray)
    {
        var classroom = LocationManager.Instance.GetLocation(AssignedAction.Planning);

        for (int i = 0; i < dataArray.Length; i++)
        {
            var slot = classroom.GetFixedSlot(i);
            if (slot == null)
            {
                Debug.LogWarning($"[CharacterManager] 교육장 슬롯 부족 (index: {i})");
                break;
            }

            var go = Instantiate(
                dataArray[i].CharacterPrefab,
                slot.position,
                Quaternion.identity);

            go.name = dataArray[i].CharacterName;

            var view = go.GetComponent<CharacterView>();
            var handler = go.GetComponent<CharacterClickHandler>();

            if (view != null)
            {
                view.Init(_characters[i]);
                view.OnArrivedAtMeeting += OnCharacterArrivedAtMeeting;
            }

            handler?.Init(_characters[i]);
            Debug.Log($"[CharacterManager] {dataArray[i].CharacterName} 스폰 완료");
        }
    }

    void OnCharacterArrivedAtMeeting(CharacterView view)
    {
        _arrivedAtMeetingCount++;

        // 회의 참여 가능한 캐릭터 전원 도착 시 팝업
        if (_arrivedAtMeetingCount >= GetAvailable().Count)
        {
            _arrivedAtMeetingCount = 0;
            UIManager.Instance.ShowMeetingPopup();
        }
    }

    public void LoadCharacters(CharacterData[] dataArray)
    {
        _characters.Clear();

        foreach (var (data, index) in dataArray.Select((d, i) => (d, i)))
        {
            var entity = new CharacterEntity();
            entity.Init(data, index);

            entity.OnStateChanged += (c, newState) =>
            {
                if (newState == CharacterState.Down)
                    OnCharacterDown?.Invoke(c);
                else if (newState == CharacterState.Sick)
                    OnCharacterRecovered?.Invoke(c);
            };

            _characters.Add(entity);
            Debug.Log($"[CharacterManager] {data.CharacterName} 로드 완료");
        }
    }

    // ─────────────────────────────────────────────────
    //  캐릭터 조회
    // ─────────────────────────────────────────────────
    public CharacterEntity Get(int index)
    {
        if (index < 0 || index >= _characters.Count)
        {
            Debug.LogWarning($"[CharacterManager] 잘못된 인덱스: {index}");
            return null;
        }
        return _characters[index];
    }

    // 회의 참여 가능한 캐릭터 (Down 제외)
    public List<CharacterEntity> GetAvailable()
    {
        var result = new List<CharacterEntity>();
        foreach (var c in _characters)
            if (c.CanBeAssigned)
                result.Add(c);
        return result;
    }

    // ─────────────────────────────────────────────────
    //  Update — 진행도 실시간 누적
    // ─────────────────────────────────────────────────
    void Update()
    {
        if (_characters.Count == 0) return;
        if (TimeManager.Instance == null) return;
        if (TimeManager.Instance.IsPaused) return;
        if (TimeManager.Instance.IsInMeeting) return;

        foreach (var c in _characters)
        {
            ProcessWorkProgress(c);
            ProcessCondition(c);
        }
    }

    // 컨디션 처리
    void ProcessCondition(CharacterEntity c)
    {
        if (c.IsOnBreak)
        {
            switch (c.ActiveRuntime)
            {
                case RuntimeAction.Rest:
                    c.ChangeCondition(Mathf.RoundToInt(8f * Time.deltaTime));
                    break;
                case RuntimeAction.Exercise:
                    c.ChangeCondition(Mathf.RoundToInt(-2f * Time.deltaTime));
                    break;
                case RuntimeAction.Coffee:
                    c.ChangeCondition(Mathf.RoundToInt(6f * Time.deltaTime));
                    // TODO: 다음 틱 디버프
                    break;
            }
            return;
        }

        if (c.State == CharacterState.Down) return;

        float drain = c.AssignedAction switch
        {
            AssignedAction.Planning
            or AssignedAction.Client
            or AssignedAction.Art => 4f,
            AssignedAction.SelfStudy_Planning
            or AssignedAction.SelfStudy_Client
            or AssignedAction.SelfStudy_Art => 3f,
            _ => 1f
        };

        if (c.IsOvertime) drain += 4f;
        if (c.State == CharacterState.Sick) drain += 2f;
        if (c.HasTrait(TraitType.BurnoutProne) && c.IsOvertime) drain += 4f;

        c.ChangeCondition(-c.GetConditionDrain(Mathf.RoundToInt(drain * Time.deltaTime)));
    }

    // 작업 진행도 처리
    void ProcessWorkProgress(CharacterEntity c)
    {
        if (c.IsOnBreak) return;
        if (c.State == CharacterState.Down) return;

        float efficiency = c.GetEfficiency();
        if (efficiency <= 0f) return;

        // 초당 조금씩 누적
        float amount = efficiency * 0.005f * Time.deltaTime;

        ProgressType type = c.AssignedAction switch
        {
            AssignedAction.Planning => ProgressType.Planning,
            AssignedAction.Client => ProgressType.Client,
            AssignedAction.Art => ProgressType.Art,
            _ => ProgressType.Planning
        };

        // SelfStudy는 진행도 기여 없음
        if (c.AssignedAction == AssignedAction.SelfStudy_Planning ||
            c.AssignedAction == AssignedAction.SelfStudy_Client ||
            c.AssignedAction == AssignedAction.SelfStudy_Art) return;

        GameManager.Instance.AddProgress(type, amount);
    }

    // ─────────────────────────────────────────────────
    //  틱 처리 (OnPhaseStart 구독 → 구간 시작 시 호출)
    // ─────────────────────────────────────────────────
    public void OnPhaseTick(TimeManager.DayPhase phase)
    {
        foreach (var c in _characters)
        {
            ProcessTraitEffects(c); // 틱 시작 시 효과 발동하는 특성이 있다면.
        }
    }

    // ─────────────────────────────────────────────────
    //  OnPhaseEnd — 스탯 성장 (페이즈 종료 시)
    // ─────────────────────────────────────────────────
    void OnPhaseEnd(TimeManager.DayPhase phase)
    {
        foreach (var c in _characters)
            ProcessStatGrowth(c);
    }

    void ProcessStatGrowth(CharacterEntity c)
    {
        if (c.State == CharacterState.Down) return;

        switch (c.AssignedAction)
        {
            case AssignedAction.SelfStudy_Planning:
                c.GrowStat(StatType.Planning);
                break;
            case AssignedAction.SelfStudy_Client:
                c.GrowStat(StatType.Client);
                break;
            case AssignedAction.SelfStudy_Art:
                c.GrowStat(StatType.Art);
                break;
        }

        // 헬스 → HP 성장
        if (c.ActiveRuntime == RuntimeAction.Exercise)
            c.GrowStat(StatType.HP);
    }


    // ─────────────────────────────────────────────────
    //  특성 효과 처리
    // ─────────────────────────────────────────────────
    void ProcessTraitEffects(CharacterEntity c)
    {
        if (c.State == CharacterState.Down) return;

        // 에이스 — 컨디션 20% 이하 시 경고
        if (c.HasTrait(TraitType.Ace) && c.Condition <= 20)
        {
            Debug.LogWarning($"[{c.Name}] 에이스 과로 위험!");
            // TODO: EventManager 트리거
        }

        // 허약 체질 — 매 틱 추가 컨디션 소모
        if (c.HasTrait(TraitType.Fragile))
            c.ChangeCondition(-5);

        // 아이디어맨 — 기획 페이즈에서 랜덤 보너스
        if (c.HasTrait(TraitType.Ideaman) &&
            TimeManager.Instance.CurrentGamePhase == TimeManager.GamePhase.Planning &&
            UnityEngine.Random.value < 0.15f)
        {
            GameManager.Instance.AddProgress(ProgressType.Planning, 0.05f);
            Debug.Log($"[{c.Name}] 아이디어 보너스!");
        }

        // 의욕 과다 — 클라 작업 시 오버엔지니어링 확률
        if (c.HasTrait(TraitType.Overenthusiast) &&
            c.AssignedAction == AssignedAction.Client &&
            UnityEngine.Random.value < 0.1f)
        {
            GameManager.Instance.AddProgress(ProgressType.Client, -0.05f);
            Debug.Log($"[{c.Name}] 오버엔지니어링 발생!");
        }
    }

    protected override void Dispose()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnPhaseStart -= OnPhaseTick;
            TimeManager.Instance.OnPhaseEnd -= OnPhaseEnd;
        }
        base.Dispose();
    }
}