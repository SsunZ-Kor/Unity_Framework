using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways()]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimation : MonoBehaviour
{
    public enum WrapMode
    {
        Once,
        Loop,
        PingPong,
    }

    [Header("Cpomponents")]
    [SerializeField]
    private SpriteRenderer _renderer = null;

    [Header("Options")]
    [SerializeField]
    private WrapMode _defaultWrapMode = WrapMode.Once;
    [SerializeField]
    private bool _playOnAwake = true;

    [Header("Sprites")]
    [Range(1f, 120f)]
    [SerializeField]
    private float _sample = 30f;
    [SerializeField]
    private Sprite[] _sprites = null;

    public WrapMode DefaultWrapMode { get { return _defaultWrapMode; } set { _defaultWrapMode = value; } }
    public WrapMode wrapMode { get; set; }
    public bool playOnAwake { get { return _playOnAwake; } set { _playOnAwake = value; } }

    public bool isPlaying { get; set; } = false;
    public float time { get; set; } = 0f;
    public float length => _sprites == null ? 0f : _sprites.Length / _sample;
    public float normalizedSpeed { get; set; } = 1f;

#if UNITY_EDITOR
    private double playStartTime = 0f;
#endif

    // 이하 외부 접근 X
    private int _currIdx = -1;

    private void Awake()
    {
        if (_renderer == null)
            _renderer = this.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
    }

    private void OnEnable()
    {
        if (Application.isPlaying && _playOnAwake)
            Play();
    }

    public void Play(bool reset = true)
    {
        Play(_defaultWrapMode, reset);
    }

    public void Play(WrapMode mode, bool reset = true)
    {
        isPlaying = true;
        this.wrapMode = mode;
        if (reset)
        {
            _currIdx = -1;
            time = 0f;
#if UNITY_EDITOR
            playStartTime = UnityEditor.EditorApplication.timeSinceStartup;
#endif
        }
    }

    public void Stop(bool showFirstFrame = false)
    {
        isPlaying = false;
        if (showFirstFrame)
        {
            if (_renderer != null && _sprites != null && _sprites.Length > 0)
                _renderer.sprite = _sprites[0];
        }
    }

    public void Update()
    {
        if (!isPlaying || _sprites == null || _sprites.Length <= 0)
            return;

        var newIdx = 0;
        switch(wrapMode)
        {
            case WrapMode.Once:
            {
                if (time >= length)
                {
                    newIdx = _sprites.Length - 1;
                    isPlaying = false;
                }
                else
                {
                    newIdx = Mathf.FloorToInt(time * _sample * normalizedSpeed);
                }
            }
            break;
            case WrapMode.Loop:
            {
                newIdx = Mathf.FloorToInt(time * _sample * normalizedSpeed) % _sprites.Length;
            }
            break;
            case WrapMode.PingPong:
            {
                newIdx = Mathf.FloorToInt(time * _sample * normalizedSpeed);
                if (_sprites.Length > 3)
                {
                    var pingpongFrame = _sprites.Length - 2;
                    var totalFrame = _sprites.Length + pingpongFrame;

                    newIdx %= totalFrame;
                    if (newIdx >= _sprites.Length)
                        newIdx = totalFrame - newIdx;
                }
                else
                {
                    newIdx = Mathf.FloorToInt(time * _sample * normalizedSpeed) % _sprites.Length;
                }
            }
            break;
        }

        ChangeIdx(newIdx);
#if UNITY_EDITOR
        if (!Application.isPlaying)
            time = (float)(UnityEditor.EditorApplication.timeSinceStartup - playStartTime);
        else
#endif
            time += UnityEngine.Time.deltaTime;
    }

    private void ChangeIdx(int idx)
    {
        if (_currIdx == idx)
            return;

        _currIdx = idx;

        _renderer.sprite = _sprites[idx];

    }

}
