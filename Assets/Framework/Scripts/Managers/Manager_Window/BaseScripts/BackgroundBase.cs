using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class BackgroundBase : MonoBehaviour
    {
        [Header("Gyro Settings")]
        [Range(0f, 2.7f)]
        [SerializeField]
        protected float _camDownSize_H = 1f;
        protected float _camDownSize_W = 1f;
        [SerializeField]
        [Range(0f, 90f)]
        protected float _maxAngle = 15f;

        protected int maxChildCount = 0;
        protected Quaternion _qStartAtt;
        protected Quaternion _qStartAttInverse;
        protected SpriteRenderer[] _childSprite;
        protected Color[] _originColor_childSprite;
        protected Transform[] _tr_Children;

        protected Color _color = Color.white;
        protected Quaternion qLastUpdateAtt;

        public float CamSize => 5.4f - _camDownSize_H;

        public Color Color
        {
            get { return _color; }
            set
            {
                if (_color == value)
                    return;

                _color = value;

                if (_childSprite == null)
                    return;

                for (int i = 0; i < _childSprite.Length; ++i)
                    _childSprite[i].color = _originColor_childSprite[i] * _color;
            }
        }
        public float Alpha
        {
            get { return _color.a; }
            set
            {
                var newColor = _color;
                newColor.a = value;

                Color = newColor;
            }
        }

        protected virtual void Awake()
        {
            maxChildCount = this.transform.childCount;
            _camDownSize_W = _camDownSize_H / Screen.height * Screen.width;

            _childSprite = this.GetComponentsInChildren<SpriteRenderer>();
            _originColor_childSprite = new Color[_childSprite.Length];
            for (int i = 0; i < _originColor_childSprite.Length; ++i)
            {
                _originColor_childSprite[i] = _childSprite[i].color;
            }

            _tr_Children = new Transform[this.transform.childCount];
            for (int i = 0; i < this.transform.childCount; ++i)
                _tr_Children[i] = this.transform.GetChild(i);

            System.Array.Sort(_tr_Children, (x, y) => x.transform.localPosition.z.CompareTo(y.transform.localPosition.z));

            // 스케일 조절
            var factor = 1f - (_camDownSize_H / 5.4f);
            for (int i = 0; i < this.transform.childCount; ++i)
            {
                var tr = _tr_Children[i];
                tr.localScale *= Mathf.Lerp(factor, 1f, (i + 1) / (float)maxChildCount);
            }
        }

        protected virtual void OnEnable()
        {
            ResetChild();

#if !UNITY_EDITOR
            if (!SystemInfo.supportsGyroscope)
                return;

            _qStartAtt = Input.gyro.attitude;
#else
            _qStartAtt = Quaternion.identity;
#endif
            _qStartAttInverse = Quaternion.Inverse(_qStartAtt);
        }

        protected virtual void Update()
        {
            if (_camDownSize_H == 0 || _maxAngle == 0)
                return;

#if !UNITY_EDITOR
            if (!SystemInfo.supportsGyroscope)
                return;
#endif

#if !UNITY_EDITOR
            var qCurrAtt = Input.gyro.attitude;
#else
            var mousePos = Managers.UI.ConvertScreenToCanvasPoint(Input.mousePosition);
            //Debug.Log($"Mouse Position : {mousePos}");
            var rect = Managers.UI._rttr_CanvasRoot.rect;

            if (mousePos.x < rect.x)
                mousePos.x = rect.x;
            else if(mousePos.x > rect.xMax)
                mousePos.x = rect.xMax;

            if (mousePos.y < rect.y)
                mousePos.y = rect.y;
            else if (mousePos.y > rect.yMax)
                mousePos.y = rect.yMax;

            var vFactor = new Vector2(mousePos.x / (rect.width * 0.5f), mousePos.y / (rect.height * 0.5f));
            vFactor *= _maxAngle;
            var qCurrAtt = Quaternion.Euler(vFactor.y ,vFactor.x, 0f);
#endif
            if (qLastUpdateAtt == qCurrAtt)
                return;

            var vOffset = Vector2.zero;
            if (_qStartAtt != qCurrAtt)
            {
                var fGapAngle = Quaternion.Angle(_qStartAtt, qCurrAtt);
                if (fGapAngle > _maxAngle)
                {
                    _qStartAtt = Quaternion.Slerp(qCurrAtt, _qStartAtt, _maxAngle / fGapAngle);
                    _qStartAttInverse = Quaternion.Inverse(_qStartAtt);

                    fGapAngle = _maxAngle;
                }

                var qGap = _qStartAttInverse * qCurrAtt;
                var vGap = qGap * Vector3.forward;

                vOffset.x = Mathf.Clamp01(Vector3.Angle(Vector3.forward, new Vector3(vGap.x, 0f, vGap.z).normalized) / _maxAngle);
                if (vGap.x > 0f) vOffset.x *= -1f;
                vOffset.y = Mathf.Clamp01(Vector3.Angle(Vector3.forward, new Vector3(0f, vGap.y, vGap.z).normalized) / _maxAngle);
                if (vGap.y > 0f) vOffset.y *= -1f;
           
                vOffset.x *= _camDownSize_W;
                vOffset.y *= -_camDownSize_H;

                for (int i = 0; i < _tr_Children.Length; ++i)
                {
                    var newPos = new Vector3(vOffset.x, vOffset.y, 0);
                    newPos = newPos * (i + 1) / transform.childCount;
                    newPos.z = _tr_Children[i].localPosition.z;

                    _tr_Children[i].localPosition = Vector3.Lerp(_tr_Children[i].localPosition, newPos, Time.deltaTime * 10f);
                }

                qLastUpdateAtt = qCurrAtt;
            }
            else
            {
                for (int i = 0; i < _tr_Children.Length; ++i)
                {
                    var newPos = Vector3.zero;
                    newPos.z = _tr_Children[i].localPosition.z;

                    _tr_Children[i].localPosition = Vector3.Lerp(_tr_Children[i].localPosition, newPos, Time.deltaTime * 10f);
                }
            }
        }

        protected virtual void ResetChild()
        {
            for (int i = 0; i < _tr_Children.Length; ++i)
            {
                var newPos = new Vector3(0f, 0f, _tr_Children[i].localPosition.z);//  transform.GetChild(i).localPosition.z);
                _tr_Children[i].localPosition = newPos;
            }
        }
    }
}
