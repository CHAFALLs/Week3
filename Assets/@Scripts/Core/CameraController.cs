using DG.Tweening;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] float _baseMoveSpeed = 10f;   // 기본 이동 속도
    [SerializeField] float _edgeScrollSize = 20f;  // 화면 가장자리 스크롤 감지 크기 (px)

    [Header("줌 설정")]
    [SerializeField] float _zoomSpeed = 2f;
    [SerializeField] float _minZoom = 3f;
    [SerializeField] float _maxZoom = 12f;

    [Header("경계 설정 (나중에 맵 크기에 맞게 조정)")]
    [SerializeField] bool _useBounds = false;
    [SerializeField] float _boundMinX = -20f;
    [SerializeField] float _boundMaxX = 20f;
    [SerializeField] float _boundMinY = -20f;
    [SerializeField] float _boundMaxY = 20f;

    [Header("인트로 줌 설정")]
    [SerializeField] float _introStartZoom = 3f;
    [SerializeField] float _introDuration = 1.5f;

    Camera _cam;
    float _targetZoom;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _targetZoom = _cam.orthographicSize;
    }

    void Start()
    {
        TimeManager.Instance.OnGameStart += PlayIntro;
    }

    void Update()
    {
        if (TimeManager.Instance == null) return;

        HandleMove();
        HandleZoom();
    }

    // ─────────────────────────────────────────────────
    //  인트로 줌 연출
    // ─────────────────────────────────────────────────
    void PlayIntro()
    {
        _cam.orthographicSize = _introStartZoom;
        _targetZoom = _maxZoom;

        DOTween.To(
            () => _cam.orthographicSize,
            x => _cam.orthographicSize = x,
            _maxZoom,
            _introDuration)
            .SetEase(Ease.OutQuad);
    }

    // ─────────────────────────────────────────────────
    //  이동
    // ─────────────────────────────────────────────────
    void HandleMove()
    {
        Vector3 dir = Vector3.zero;

        // WASD / 화살표
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) dir.y += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) dir.y -= 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) dir.x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) dir.x += 1f;

        // 줌 레벨에 비례한 속도 (멀수록 빠르게)
        float speed = _baseMoveSpeed * (_cam.orthographicSize / _maxZoom);
        transform.position += dir.normalized * speed * Time.unscaledDeltaTime;

        // 경계 클램프
        if (_useBounds)
        {
            var pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, _boundMinX, _boundMaxX);
            pos.y = Mathf.Clamp(pos.y, _boundMinY, _boundMaxY);
            transform.position = pos;
        }
    }

    // ─────────────────────────────────────────────────
    //  줌
    // ─────────────────────────────────────────────────
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Approximately(scroll, 0f)) return;

        _targetZoom -= scroll * _zoomSpeed * 10f;
        _targetZoom = Mathf.Clamp(_targetZoom, _minZoom, _maxZoom);

        // 부드럽게 줌
        _cam.orthographicSize = Mathf.Lerp(
            _cam.orthographicSize,
            _targetZoom,
            Time.unscaledDeltaTime * 10f);
    }

}
