using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Game
{
    public static class MonoEx
    {
        public static void Reset(this Transform tr)
        {
            if (tr is RectTransform)
            {
                var rttr = tr as RectTransform;
                rttr.anchoredPosition = Vector2.zero;
            }
            else
            {
                tr.localPosition = Vector3.zero;
            }

            tr.localRotation = Quaternion.identity;
            tr.localScale = Vector3.one;
        }

        public static Transform FindDeep(this Transform tr, string childName)
        {
            var tr_ResultChild = tr.Find(childName);
            if (tr_ResultChild != null)
                return tr_ResultChild;

            for (int i = 0; i < tr.childCount; ++i)
            {
                var tr_child = tr.GetChild(i);
                tr_ResultChild = tr_child.FindDeep(childName);
                if (tr_ResultChild != null)
                    return tr_ResultChild;
            }

            return tr_ResultChild;
        }

        public static void SetLayer(this GameObject gameObject, int layer, bool bContainChildren = false)
        {
            if (gameObject == null)
                return;

            gameObject.layer = layer;
            if (!bContainChildren)
                return;

            foreach (Transform child in gameObject.transform)
                SetLayer(child.gameObject, layer, true);
        }

        public static void SetLayer(this GameObject gameObject, string layerName, bool bContainChildren = false)
        {
            SetLayer(gameObject, LayerMask.NameToLayer(layerName), bContainChildren);
        }

        public static void SetPosition(this Transform tr, Vector3 pos)
        {
            if(tr is RectTransform)
            {
                (tr as RectTransform).anchoredPosition = pos;
            }
            else
            {
                tr.position = pos;
            }
        }

        public static void SetRosition(this Transform tr, Quaternion rot)
        {
            tr.rotation = rot;
        }

        public static Vector3 Top(this CapsuleCollider capsuleCol)
        {
            Vector3 vUp = Vector3.zero;
            switch (capsuleCol.direction)
            {
                case 0: vUp = Vector3.right; break;
                case 1: vUp = Vector3.up; break;
                case 2: vUp = Vector3.forward; break;
            }

            var height = Mathf.Max(capsuleCol.height - capsuleCol.radius * 2f, 0f);
            return capsuleCol.center + (vUp * (height * 0.5f));
        }

        public static Vector3 Bottom(this CapsuleCollider capsuleCol)
        {
            Vector3 vDown = Vector3.zero;
            switch (capsuleCol.direction)
            {
                case 0: vDown = Vector3.left; break;
                case 1: vDown = Vector3.down; break;
                case 2: vDown = Vector3.back; break;
            }

            var height = Mathf.Max(capsuleCol.height - capsuleCol.radius * 2f, 0f);
            return capsuleCol.center + (vDown * (height * 0.5f));
        }

        public static Vector3 TopHemi(this CapsuleCollider capsuleCol)
        {
            Vector3 vUp = Vector3.zero;
            switch (capsuleCol.direction)
            {
                case 0: vUp = Vector3.right; break;
                case 1: vUp = Vector3.up; break;
                case 2: vUp = Vector3.forward; break;
            }

            var height = Mathf.Max(capsuleCol.height - capsuleCol.radius * 2f, 0f);
            return capsuleCol.center + (vUp * (height * 0.5f + capsuleCol.radius));
        }

        public static Vector3 BottomHemi(this CapsuleCollider capsuleCol)
        {
            Vector3 vDown = Vector3.zero;
            switch (capsuleCol.direction)
            {
                case 0: vDown = Vector3.left; break;
                case 1: vDown = Vector3.down; break;
                case 2: vDown = Vector3.back; break;
            }

            var height = Mathf.Max(capsuleCol.height - capsuleCol.radius * 2f, 0f);
            return capsuleCol.center + (vDown * (height * 0.5f + capsuleCol.radius));
        }

        public static Vector3 TopWorld(this CapsuleCollider capsuleCol)
        {
            return capsuleCol.transform.position + (capsuleCol.transform.rotation * capsuleCol.Top());
        }

        public static Vector3 BottomWorld(this CapsuleCollider capsuleCol)
        {
            return capsuleCol.transform.position + (capsuleCol.transform.rotation * capsuleCol.Bottom());
        }

        public static Vector3 TopHemiWorld(this CapsuleCollider capsuleCol)
        {
            return capsuleCol.transform.position + (capsuleCol.transform.rotation * capsuleCol.TopHemiWorld());
        }

        public static Vector3 BottomHemiWorld(this CapsuleCollider capsuleCol)
        {
            return capsuleCol.transform.position + (capsuleCol.transform.rotation * capsuleCol.BottomHemiWorld());
        }

        public delegate bool OverlapFilter(Collider col);

        private static int Overlap(
            this CapsuleCollider capsuleCol,
            Collider[] result,
            int layerMask = int.MaxValue,
            OverlapFilter filter = null
            )
        {
            int nbHits = 0;
            int count = Physics.OverlapCapsuleNonAlloc(
                        capsuleCol.TopWorld(),
                        capsuleCol.BottomWorld(),
                        capsuleCol.radius,
                        result,
                        layerMask,
                        QueryTriggerInteraction.Ignore);


            int resultCount = 0;
            for (int i = 0; i < count; i++)
            {
                if (filter != null && filter.Invoke(result[i]))
                {
                    result[i] = null;
                    continue;
                }

                result[resultCount] = result[i];
                result[i] = null;
                ++resultCount;
            }

            return nbHits;
        }
        
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            return GetOrAddComponent(go, typeof(T)) as T;
        }

        public static Component GetOrAddComponent(this GameObject go, System.Type t)
        {
            if (go == null)
                return null;

            var result = go.GetComponent(t);
            if (result != null)
                return result;

            return go.AddComponent(t);
        }

        public static T GetOrAddComponent<T>(this MonoBehaviour mono) where T : Component
        {
            return GetOrAddComponent(mono, typeof(T)) as T;
        }

        public static Component GetOrAddComponent(this MonoBehaviour mono, System.Type t)
        {
            if (mono == null)
                return null;

            var result = mono.GetComponent(t);
            if (result != null)
                return result;

            return mono.gameObject.AddComponent(t);
        }

        public static void GetFullPath(this GameObject go, ref StringBuilder stb)
        {
            if (stb == null)
                stb = new StringBuilder(100);
            else
                stb.Clear();

            var trParant = go.transform;
            while (trParant != null)
            {
                stb.Insert(0, trParant.parent != null ? $"/{trParant.name}" : trParant.name);
                trParant = trParant.parent;
            }
        }
    }
}