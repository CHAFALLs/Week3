using UnityEngine;

public class LocationManager : SingletonBehaviour<LocationManager>
{
    [SerializeField] LocationPoint _classroom;  // Fixed
    [SerializeField] LocationPoint _meetingRoom;    // Fixed
    [SerializeField] LocationPoint _cafeteria;   // Dynamic
    [SerializeField] LocationPoint _dormitory;  // Dynamic
    [SerializeField] LocationPoint _gym;        // Dynamic
    [SerializeField] LocationPoint _trail;      // Dynamic

    public void Init()
    {
        Debug.Log("[LocationManager] Init 완료");
    }

    // 회의실 접근자
    public LocationPoint GetMeetingRoom() => _meetingRoom;

    // AssignedAction 기준 장소 반환
    public LocationPoint GetLocation(AssignedAction action) => action switch
    {
        AssignedAction.Planning
        or AssignedAction.Client
        or AssignedAction.Art
        or AssignedAction.SelfStudy_Planning
        or AssignedAction.SelfStudy_Client
        or AssignedAction.SelfStudy_Art => _classroom,
        _ => _classroom
    };

    // RuntimeAction 기준 장소 반환
    public LocationPoint GetLocation(RuntimeAction action) => action switch
    {
        RuntimeAction.Rest => _dormitory,
        RuntimeAction.Exercise => _gym,
        RuntimeAction.Coffee => _cafeteria,
        _ => _classroom
    };
}
