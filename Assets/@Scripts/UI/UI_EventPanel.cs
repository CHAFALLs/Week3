using System.Collections.Generic;
using UnityEngine;

public class UI_EventPanel : MonoBehaviour
{
    [SerializeField] Transform _listContainer;
    [SerializeField] UI_EventNotificationItem _itemPrefab;
    [SerializeField] UI_EventDetailPopup _detailPopup;

    List<UI_EventNotificationItem> _activeItems = new List<UI_EventNotificationItem>();

    // ─────────────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────────────
    public void Init()
    {
        EventManagerEx.Instance.OnEventTriggered += OnEventTriggered;
        EventManagerEx.Instance.OnEventResolved += OnEventResolved;
    }

    // ─────────────────────────────────────────────────
    //  이벤트 발생 → 알림 추가
    // ─────────────────────────────────────────────────
    void OnEventTriggered(BaseEventData data)
    {
        var item = Instantiate(_itemPrefab, _listContainer);
        item.Setup(data, OnItemClicked);
        _activeItems.Add(item);
    }

    // ─────────────────────────────────────────────────
    //  알림 클릭 → 상세 팝업
    // ─────────────────────────────────────────────────
    void OnItemClicked(BaseEventData data)
    {
        _detailPopup.Show(data);
    }

    // ─────────────────────────────────────────────────
    //  이벤트 해제 → 알림 제거
    // ─────────────────────────────────────────────────
    void OnEventResolved(BaseEventData data)
    {
        // 해당 데이터의 아이템 찾아서 제거
        var item = _activeItems.Find(i => i.GetData() == data);
        if (item == null) return;

        _activeItems.Remove(item);
        Destroy(item.gameObject);
    }
}
