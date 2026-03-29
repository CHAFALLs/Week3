using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_DayTransition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Image _blackPanel;

    [Header("설정")]
    [SerializeField] float _fadeInDuration = 0.5f;  // 검은 화면 덮이는 시간
    [SerializeField] float _holdDuration = 0.3f;  // 완전히 덮인 후 대기 시간
    [SerializeField] float _fadeOutDuration = 0.8f;  // 검은 화면 걷히는 시간

    void Awake()
    {
        // 초기에 완전 투명
        _blackPanel.color = new Color(0f, 0f, 0f, 0f);
        _blackPanel.gameObject.SetActive(false);
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
        _blackPanel.gameObject.SetActive(true);
        _blackPanel.color = new Color(0f, 0f, 0f, 0f);

        // 페이드 인 → 대기 → 페이드 아웃
        _blackPanel.DOFade(1f, _fadeInDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(_holdDuration, () =>
                {
                    // 페이드 아웃
                    _blackPanel.DOFade(0f, _fadeOutDuration)
                        .SetUpdate(true)
                        .OnComplete(() =>
                        {
                            _blackPanel.gameObject.SetActive(false);
                            // 걷힌 후 다음날 시작 → 캐릭터 이동 모습 보임
                            TimeManager.Instance.StartNextDay();
                        });
                }, true);
            });
    }
}
