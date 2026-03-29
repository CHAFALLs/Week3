using UnityEngine;

public class BootingManager : SingletonBehaviour<BootingManager>
{
    void Start()
    {
        // Awake에서 모든 싱글톤 등록 완료 후
        TimeManager.Instance.Init();
        CharacterManager.Instance.Init();
        GameManager.Instance.Init();
        EventManagerEx.Instance.Init();
        UIManager.Instance.Init();
    }

    public static void Clear()
    {
        // 씬 전환 시 정리
    }
}
