using DG.Tweening;
using UnityEngine;

public class UI_DayTransition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CanvasGroup _canvasGroup;


    [Header("설정")]
    [SerializeField] float _fadeInDuration = 0.5f;  // 검은 화면 덮이는 시간
    [SerializeField] float _holdDuration = 0.3f;  // 완전히 덮인 후 대기 시간
    [SerializeField] float _fadeOutDuration = 0.8f;  // 검은 화면 걷히는 시간

    void Awake()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
    }

    public void Init()
    {
        TimeManager.Instance.OnDayEnd += _ => PlayTransition();
    }

    // ─────────────────────────────────────────────────
    //  트랜지션 재생
    // ─────────────────────────────────────────────────
    void PlayTransition()
    {
        _canvasGroup.DOKill();
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = true;

        _canvasGroup.DOFade(1f, _fadeInDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(_holdDuration, () =>
                {
                    _canvasGroup.DOFade(0f, _fadeOutDuration)
                        .SetUpdate(true)
                        .OnComplete(() =>
                        {
                            _canvasGroup.blocksRaycasts = false;
                            TimeManager.Instance.StartNextDay();
                        });
                }, true);
            });
    }
}
