using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

[ExecuteAlways]
public class UGUICanvasPerfect : UIBehaviour {

    [SerializeField]
    private bool ExcuteAwake = true;
    [SerializeField]
    private bool ExcuteEnable = false;
    [SerializeField]
    private bool ExcuteDimensionsChange = false;

    [SerializeField]
    private RectTransform _rttr_Canvas = null;
    [SerializeField]
    private CanvasScaler _scaler = null;

    [SerializeField]
    private Vector2 _maxResolution = Vector2.positiveInfinity;
    [SerializeField]
    private bool _useSafeArea = true;

#if UNITY_EDITOR

    [UnityEditor.MenuItem("Utils/Safe Area Test On")]
    public static void OnSafeAreaTest()
    {
        PlayerPrefs.SetInt("OnSafeTest", 0);
        PlayerPrefs.Save();
    }

    [UnityEditor.MenuItem("Utils/Safe Area Test Off")]
    public static void OffSafeAreaTest()
    {
        PlayerPrefs.DeleteKey("OnSafeTest");
        PlayerPrefs.Save();
    }

#endif

    protected override void Awake()
    {
        base.Awake();

        var rttr_Mine = this.transform as RectTransform;
        if (rttr_Mine == null)
            return;

        var vAnchor = Vector2.one * 0.5f;
        rttr_Mine.anchorMin = vAnchor;
        rttr_Mine.anchorMax = vAnchor;

        if (ExcuteAwake)
            Update_SizeDelta();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (ExcuteEnable)
            Update_SizeDelta();
        //StartCoroutine(Cor_Update_SizeDelta());
    }
    
    //protected override void OnDisable()
    //{
    //    StopAllCoroutines();
    //}

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        //if (this.isActiveAndEnabled)
        //    StartCoroutine(Cor_Update_SizeDelta());
        if (ExcuteDimensionsChange)
            Update_SizeDelta();
    }

    //private IEnumerator Cor_Update_SizeDelta()
    //{
    //    yield return new WaitForEndOfFrame();
    //    Update_SizeDelta();
    //}

    public void Update_SizeDelta()
    {
        // Canvas의 RectTransform 찾기
        if (_rttr_Canvas == null)
        {
            var canvas = GetComponentInParent(typeof(Canvas)) as Canvas;
            if (canvas == null)
                return;

            _rttr_Canvas = canvas.transform as RectTransform;
        }

        // CanvasScaler 찾기
        if (_scaler == null)
        {
            _scaler = GetComponentInParent(typeof(CanvasScaler)) as CanvasScaler;
            if (_scaler == null)
                return;
        }

        // Holder의 Anchor 다시 잡기
        var rttr_Mine = this.transform as RectTransform;
        if (rttr_Mine == null)
            return;

        var vCanvasPos = _rttr_Canvas.position;
        var vCanvasSize = _rttr_Canvas.sizeDelta;
        if (_useSafeArea)
        {
            var rt_ScreenArea = new Rect(0, 0, Screen.width, Screen.height);

            var factorX_ScreenToCanvas = vCanvasSize.x / rt_ScreenArea.width;
            var factorY_ScreenToCanvas = vCanvasSize.y / rt_ScreenArea.height;

            var rt_SafeArea = Screen.safeArea;

#if UNITY_EDITOR
            if (PlayerPrefs.HasKey("OnSafeTest"))
                rt_SafeArea = new Rect(132, 63, 2172,1062);
#endif

            var posOffset = rt_SafeArea.center - rt_ScreenArea.center;
            var newSizeDelta = rt_SafeArea.size;

            posOffset.x *= factorX_ScreenToCanvas;
            posOffset.y *= factorY_ScreenToCanvas;
            newSizeDelta.x *= factorX_ScreenToCanvas;
            newSizeDelta.y *= factorY_ScreenToCanvas;

            if (newSizeDelta.x >= 2340f)
                newSizeDelta.x = 2340f;
            if (newSizeDelta.y >= _maxResolution.y)
                newSizeDelta.y = _maxResolution.y;


            rttr_Mine.sizeDelta = newSizeDelta;
            rttr_Mine.position = vCanvasPos;
            rttr_Mine.anchoredPosition = posOffset;
        }
        else
        {
            if (vCanvasSize.x >= 2340f)
                vCanvasSize.x = 2340f;
            if (vCanvasSize.y >= _maxResolution.y)
                vCanvasSize.y = _maxResolution.y;

            rttr_Mine.sizeDelta = vCanvasSize;
            rttr_Mine.position = vCanvasPos;
        }
    }
}
