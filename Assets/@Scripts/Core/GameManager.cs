using UnityEngine;
using System;

public class GameManager : SingletonBehaviour<GameManager>
{
    // ─── 난이도 ───────────────────────────────────────
    public enum Difficulty { Easy, Normal }
    public Difficulty CurrentDifficulty { get; private set; } = Difficulty.Easy;

    public void SetDifficulty(Difficulty difficulty)
    {
        CurrentDifficulty = difficulty;
        Debug.Log($"[GameManager] 난이도 설정 → {difficulty}");
    }


    // ─── 완성도 (0~1) ─────────────────────────────────
    public float PlanningProgress { get; private set; }
    public float ClientProgress { get; private set; }
    public float ArtProgress { get; private set; }

    // 최종 점수 = 세 진행도 평균
    public float FinalScore =>
        (PlanningProgress + ClientProgress + ArtProgress) / 3f;

    // ─── 이벤트 ──────────────────────────────────────
    public event Action<ProgressType, float> OnProgressChanged;
    public event Action<EndingType> OnGameEnd;

    bool _initialized = false;

    // ─────────────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────────────
    
    public void Init()
    {
        if (_initialized) return;
        _initialized = true;

        PlanningProgress = 0f;
        ClientProgress = 0f;
        ArtProgress = 0f;

        TimeManager.Instance.OnGameEnd += HandleGameEnd;

        Debug.Log("[GameManager] Init 완료");
    }

    // ─────────────────────────────────────────────────
    //  진행도 추가
    // ─────────────────────────────────────────────────
    public void AddProgress(ProgressType type, float amount)
    {
        float weighted = ApplyPhaseWeight(type, amount);

        switch (type)
        {
            case ProgressType.Planning:
                PlanningProgress = Mathf.Clamp01(PlanningProgress + weighted);
                break;
            case ProgressType.Client:
                ClientProgress = Mathf.Clamp01(ClientProgress + weighted);
                break;
            case ProgressType.Art:
                ArtProgress = Mathf.Clamp01(ArtProgress + weighted);
                break;
        }

        OnProgressChanged?.Invoke(type, GetProgress(type));
    }

    // 페이즈별 가중치
    float ApplyPhaseWeight(ProgressType type, float amount)
    {
        var phase = TimeManager.Instance.CurrentGamePhase;

        float weight = (phase, type) switch
        {
            (TimeManager.GamePhase.Planning, ProgressType.Planning) => 1.0f,
            (TimeManager.GamePhase.Planning, ProgressType.Client) => 0.7f,
            (TimeManager.GamePhase.Planning, ProgressType.Art) => 0.7f,

            (TimeManager.GamePhase.Development, ProgressType.Planning) => 0.7f,
            (TimeManager.GamePhase.Development, ProgressType.Client) => 1.0f,
            (TimeManager.GamePhase.Development, ProgressType.Art) => 1.0f,

            (TimeManager.GamePhase.Integration, ProgressType.Planning) => 0.5f,
            (TimeManager.GamePhase.Integration, ProgressType.Client) => 1.2f,
            (TimeManager.GamePhase.Integration, ProgressType.Art) => 0.8f,

            _ => 1.0f
        };

        return amount * weight;
    }

    // ─────────────────────────────────────────────────
    //  완성도 조회 유틸
    // ─────────────────────────────────────────────────
    public float GetProgress(ProgressType type) => type switch
    {
        ProgressType.Planning => PlanningProgress,
        ProgressType.Client => ClientProgress,
        ProgressType.Art => ArtProgress,
        _ => 0f
    };

    // ─────────────────────────────────────────────────
    //  엔딩 판정
    // ─────────────────────────────────────────────────
    void HandleGameEnd()
    {
        var ending = JudgeEnding();
        OnGameEnd?.Invoke(ending);
        Debug.Log($"[GameManager] 게임 종료 → {ending} " +
                  $"(최종 점수: {FinalScore * 100f:F1}%)");
    }

    EndingType JudgeEnding()
    {
        float score = FinalScore;
        int downCount = CountDownCharacters();

        if (score >= 0.9f && downCount == 0) return EndingType.Perfect;
        if (score >= 0.7f && downCount >= 2) return EndingType.Burnout;
        if (score >= 0.7f) return EndingType.Normal;
        return EndingType.Fail;
    }

    int CountDownCharacters()
    {
        int count = 0;
        foreach (var c in CharacterManager.Instance.Characters)
            if (c.State == CharacterState.Down)
                count++;
        return count;
    }
}

// ─────────────────────────────────────────────────────
//  Enum 정의
// ─────────────────────────────────────────────────────
public enum ProgressType
{
    Planning,
    Client,
    Art,
}

public enum EndingType
{
    Perfect,  // 90%+ & 쓰러진 캐릭터 없음
    Normal,   // 70%+
    Burnout,  // 70%+ & 2명 이상 Down
    Fail,     // 70% 미만
}