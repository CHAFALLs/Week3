using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string CharacterName;
    public Sprite Portrait;         // 상단 캐릭터 바 아이콘
    public GameObject CharacterPrefab; // 실제 인게임 프리팹 

    [Header("스탯 (1~10)")]
    [Range(1, 10)] public int HP;        // 체력 — 컨디션 소모 속도에 영향
    [Range(1, 10)] public int Planning;  // 기획 능력
    [Range(1, 10)] public int Client;    // 클라 개발 능력
    [Range(1, 10)] public int Art;       // 아트 능력

    public TraitType[] Traits; // 특성들
}


// TODO: 예시
public enum TraitType
{
    None,
    Ace,            // 에이스
    Fragile,        // 허약 체질
    BurnoutProne,   // 번아웃 체질
    Overenthusiast, // 의욕 과다
    Ideaman,        // 아이디어맨
    Troublemaker,   // 갈등 유발
    CoffeeAddict,   // 커피 중독자
    MoodMaker,      // 분위기 메이커
    Loner,          // 고독을 즐김
    GlassMental,    // 유리 멘탈
    Optimist,       // 긍정형 인간
    MorningPerson,  // 아침형 인간
    NightOwl,       // 야행성
    Workaholic,     // 일중독
    Pessimist,      // 부정형 인간
    GymRat,         // 헬창
}