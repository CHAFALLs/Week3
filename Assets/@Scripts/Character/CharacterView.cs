using DG.Tweening;
using UnityEngine;

public class CharacterView : MonoBehaviour
{
    CharacterEntity _entity;

    LocationPoint _currentLocation;
    Transform _currentSlot;

    // ─────────────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────────────
    public void Init(CharacterEntity entity)
    {
        _entity = entity;

        // 행동 변경 시 이동
        _entity.OnActionChanged += _ => OnActionChanged();

        // 초기 위치 → 교육장 고정 자리
        OccupyClassroom();
    }

    // ─────────────────────────────────────────────────
    //  행동 변경 → 이동
    // ─────────────────────────────────────────────────
    void OnActionChanged()
    {
        LocationPoint targetLocation;

        if (_entity.IsOnBreak && _entity.ActiveRuntime.HasValue)
            targetLocation = LocationManager.Instance.GetLocation(_entity.ActiveRuntime.Value);
        else
            targetLocation = LocationManager.Instance.GetLocation(_entity.AssignedAction);

        // 같은 장소면 이동 안 함
        if (targetLocation == _currentLocation) return;

        // 기존 슬롯 반납
        ReleaseCurrentSlot();

        // 새 슬롯 점유 — Mode로 Fixed/Dynamic 판단
        Transform targetSlot;

        if (targetLocation.Mode == LocationSlotMode.Fixed)
            targetSlot = targetLocation.GetFixedSlot(_entity.Index);
        else
            targetSlot = targetLocation.GetAvailableSlot();

        if (targetSlot == null) return;

        _currentLocation = targetLocation;
        _currentSlot = targetSlot;

        MoveTo(_currentSlot.position);
    }

    // ─────────────────────────────────────────────────
    //  교육장 초기 배치
    // ─────────────────────────────────────────────────
    void OccupyClassroom()
    {
        var classroom = LocationManager.Instance.GetLocation(AssignedAction.Meeting);
        var slot = classroom.GetFixedSlot(_entity.Index);

        if (slot == null) return;

        _currentLocation = classroom;
        _currentSlot = slot;

        transform.position = slot.position;
    }

    // ─────────────────────────────────────────────────
    //  이동
    // ─────────────────────────────────────────────────
    void MoveTo(Vector3 targetPos)
    {
        transform.DOKill();
        transform.DOMove(targetPos, 0.5f)
            .SetEase(Ease.InOutQuad);
    }

    void ReleaseCurrentSlot()
    {
        if (_currentLocation != null && _currentSlot != null)
        {
            _currentLocation.ReleaseSlot(_currentSlot);
            _currentLocation = null;
            _currentSlot = null;
        }
    }

    void OnDestroy()
    {
        ReleaseCurrentSlot();
    }
}
