using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class UGUIParentPerfectImage : UIBehaviour
{
    public enum ModeTypes
    {
        PerfectOnWidth,
        PerfectOnHeight,
        PerfectOnAuto,
    }

    public enum TargetType
    {
        Parent,
        Canvas,
    }

    [SerializeField]
    private ModeTypes _modeType = ModeTypes.PerfectOnAuto;
    [SerializeField]
    private TargetType _targetType = TargetType.Canvas;

    [SerializeField]
    private Image _uiImg_Target;

    protected override void Awake()
    {
        if (_uiImg_Target == null)
            _uiImg_Target = this.GetComponent(typeof(Image)) as Image;
    }

    protected override void OnEnable()
    {
        OnRectTransformDimensionsChange();
    }

    protected override void OnCanvasHierarchyChanged()
    {
        base.OnCanvasHierarchyChanged();

        OnRectTransformDimensionsChange();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        var rttr_Mine = this.transform as RectTransform;
        if (rttr_Mine == null)
            return;

        rttr_Mine.anchorMin = new Vector2(0.5f, 0.5f);
        rttr_Mine.anchorMax = new Vector2(0.5f, 0.5f);

        RectTransform rttr_Parent = null;
        if (_targetType == TargetType.Canvas)
        {
            rttr_Parent = rttr_Mine.GetComponentInParent(typeof(Canvas))?.transform as RectTransform;
        }
        else if (_targetType == TargetType.Parent)
        {
            rttr_Parent = this.transform.parent as RectTransform;
        }

        if (rttr_Parent == null)
            return;

        rttr_Mine.pivot = rttr_Parent.pivot;
        rttr_Mine.localPosition = Vector3.zero;

        float aspect_Parent = rttr_Parent.rect.width / rttr_Parent.rect.height;
        float aspect_Target = 1f;
        if (_uiImg_Target != null && _uiImg_Target.sprite != null)
        {
            aspect_Target = _uiImg_Target.mainTexture.width / (float)_uiImg_Target.mainTexture.height;
        }

        switch (_modeType)
        {
            case ModeTypes.PerfectOnWidth:
                {
                    rttr_Mine.sizeDelta = new Vector2(rttr_Parent.rect.width, rttr_Parent.rect.width / aspect_Target);
                }
                break;
            case ModeTypes.PerfectOnHeight:
                {
                    rttr_Mine.sizeDelta = new Vector2(rttr_Parent.rect.height * aspect_Target, rttr_Parent.rect.height);
                }
                break;
            case ModeTypes.PerfectOnAuto:
                {
                    // 높이에 맞춤
                    if (aspect_Target > aspect_Parent)
                    {
                        rttr_Mine.sizeDelta = new Vector2(rttr_Parent.rect.height * aspect_Target, rttr_Parent.rect.height);
                    }
                    // 너비에 맞춤
                    else
                    {
                        rttr_Mine.sizeDelta = new Vector2(rttr_Parent.rect.width, rttr_Parent.rect.width / aspect_Target);
                    }
                }
                break;
        }

        var newPos = rttr_Parent.position;
        newPos.z = this.transform.position.z;

        this.transform.position = newPos;
    }
}