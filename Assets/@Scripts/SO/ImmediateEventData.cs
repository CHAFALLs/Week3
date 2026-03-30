using UnityEngine;

[CreateAssetMenu(fileName = "ImmediateEvent", menuName = "Scriptable Objects/Event/ImmediateEvent")]
public class ImmediateEventData : BaseEventData 
{
    [Header("발동 확률 (0~1, 1이면 무조건 발동)")]
    [Range(0f, 1f)]
    public float TriggerChance = 1f;  // 기본값 1 → 기존 동작 유지
}

