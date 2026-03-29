using DG.Tweening;
using System;
using UnityEngine;

public class CharacterView : MonoBehaviour
{
    CharacterEntity _entity;

    LocationPoint _currentLocation;
    Transform _currentSlot;

    public event Action<CharacterView> OnArrivedAtMeeting;

    // ─────────────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────────────
    public void Init(CharacterEntity entity)
    {
        _entity = entity;

        // 행동 변경 시 이동
        _entity.OnActionChanged += _ => OnActionChanged();

        // 회의 시작 → 회의실로 이동
        TimeManager.Instance.OnMeetingStart += _ => MoveToMeetingRoom();
        // 회의 끝 → 원래 행동 위치로 복귀
        TimeManager.Instance.OnPhaseStart += _ => OnActionChanged();

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
        var classroom = LocationManager.Instance.GetLocation(AssignedAction.Planning);
        var slot = classroom.GetFixedSlot(_entity.Index);

        if (slot == null) return;

        _currentLocation = classroom;
        _currentSlot = slot;

    }

    // ─────────────────────────────────────────────────
    //  이동
    // ─────────────────────────────────────────────────
    void MoveToMeetingRoom()
    {
        // 회의 소집 시 RuntimeAction 강제 해제 (패널티 없이)
        if (_entity.IsOnBreak)
            _entity.ClearRuntimeActionForMeeting();

        var meetingRoom = LocationManager.Instance.GetMeetingRoom();

        // 이미 회의실에 있으면 바로 도착 처리 (TODO: 꼼수)
        if (meetingRoom == _currentLocation)
        {
            OnArrivedAtMeeting?.Invoke(this);
            return;
        }

        ReleaseCurrentSlot();

        var slot = meetingRoom.GetFixedSlot(_entity.Index);
        if (slot == null) return;

        _currentLocation = meetingRoom;
        _currentSlot = slot;

        transform.DOKill();
        transform.DOMove(_currentSlot.position, 0.5f)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(true)
            .OnComplete(() => OnArrivedAtMeeting?.Invoke(this));
    }

    void MoveTo(Vector3 targetPos)
    {
        transform.DOKill();
        transform.DOMove(targetPos, 0.5f)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(true);
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
