using UnityEngine;

[CreateAssetMenu(fileName = "RandomEvent", menuName = "Scriptable Objects/Event/RandomEvent")]
public class RandomEventData : BaseEventData
{
    [Header("·£´ý ¹ßµ¿ ¼³Á¤")]
    [Tooltip("¹ßµ¿ È®·ü (0~1)")]
    [Range(0f, 1f)]
    public float TriggerChance = 0.3f;
}

