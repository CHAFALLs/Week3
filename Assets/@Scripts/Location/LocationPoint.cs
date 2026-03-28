using UnityEngine;

public enum LocationSlotMode
{
    Fixed,    // 고정 자리 (교육장 책상 — 캐릭터 인덱스로 지정)
    Dynamic,  // 선착순 빈 자리 (헬스장, 숙소 등)
}

public class LocationPoint : MonoBehaviour
{
    [SerializeField] LocationSlotMode _mode;
    [SerializeField] Transform[] _points;

    public LocationSlotMode Mode => _mode;

    bool[] _occupied;

    void Awake()
    {
        _occupied = new bool[_points.Length];
    }

    // ─────────────────────────────────────────────────
    //  Fixed 모드 — 인덱스로 슬롯 지정
    // ─────────────────────────────────────────────────
    public Transform GetFixedSlot(int index)
    {
        if (_mode != LocationSlotMode.Fixed)
        {
            Debug.LogWarning($"[{name}] Fixed 모드가 아닙니다");
            return null;
        }

        if (index < 0 || index >= _points.Length)
        {
            Debug.LogWarning($"[{name}] 잘못된 슬롯 인덱스: {index}");
            return null;
        }

        _occupied[index] = true;
        return _points[index];
    }

    // ─────────────────────────────────────────────────
    //  Dynamic 모드 — 빈 자리 반환
    // ─────────────────────────────────────────────────
    public Transform GetAvailableSlot()
    {
        if (_mode != LocationSlotMode.Dynamic)
        {
            Debug.LogWarning($"[{name}] Dynamic 모드가 아닙니다");
            return null;
        }

        for (int i = 0; i < _points.Length; i++)
        {
            if (!_occupied[i])
            {
                _occupied[i] = true;
                return _points[i];
            }
        }

        Debug.LogWarning($"[{name}] 빈 슬롯 없음");
        return null;
    }

    // ─────────────────────────────────────────────────
    //  슬롯 반납 (공통)
    // ─────────────────────────────────────────────────
    public void ReleaseSlot(Transform slot)
    {
        for (int i = 0; i < _points.Length; i++)
        {
            if (_points[i] == slot)
            {
                _occupied[i] = false;
                return;
            }
        }
    }
}
