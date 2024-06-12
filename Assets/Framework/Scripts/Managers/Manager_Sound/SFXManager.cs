using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public enum SFXType
    {
        _2D,
        _3D
    }


    public class SFXManager : ManagerBase
    {

        private PoolBase Pool_SFX;

        private bool _mute = false;
        public bool Mute
        {
            get
            {
                return _mute;
            }
            set
            {
                if (Pool_SFX == null)
                    return;

                if (_mute == value)
                    return;

                _mute = value;

                var list_SfxObj = Pool_SFX.GetAllItem();
                for (int i = 0; i < list_SfxObj.Count; ++i)
                {
                    var sfxObj = list_SfxObj[i] as SFXObject;
                    if (sfxObj == null)
                        continue;

                    sfxObj.AudioSource.mute = !value;
                }
            }
        }

        private float _volume = 1f;

        public float Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                if (_volume == value)
                    return;

                _volume = value;

                if (SFXDeco.set_AudioSource != null)
                {
                    foreach (var audioSource in SFXDeco.set_AudioSource)
                    {
                        if (audioSource != null)
                            audioSource.volume = _volume;
                    }
                }

                //var list_SfxObj = Pool_SFX.GetAllItem();
                //for (int i = 0; i < list_SfxObj.Count; ++i)
                //{
                //    var sfxObj = list_SfxObj[i] as SFXObject;
                //    if (sfxObj == null)
                //        continue;
                //
                //    sfxObj.AudioSource.volume = value;
                //}
            }
        }

        public override IEnumerator Init_Async()
        {
            // 임시 프리펩 생성
            var prf_SfxObj = Resources.Load<GameObject>("System/SfxObject");
            if (prf_SfxObj == null)
            {
                prf_SfxObj = new GameObject("SfxObject");
                prf_SfxObj.GetOrAddComponent<SFXObject>();
                Pool_SFX.Init(prf_SfxObj, 32);
                GameObject.Destroy(prf_SfxObj);
            }
            else
            {
                prf_SfxObj.GetOrAddComponent<SFXObject>();
                Pool_SFX.Init(prf_SfxObj, 32);
            }

            //Mute = !Managers.LocalData.Get_OnSFX();
            //Volume = Managers.LocalData.Get_SFXVolume();
            yield break;
        }

        public void Awake()
        {
            Pool_SFX = this.gameObject.AddComponent(typeof(PoolBase)) as PoolBase;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vLocalPos">로컬 좌표, tr_Parent == null이면 월드 좌표</param>
        /// <param name="qLocalRot">로컬 회전, tr_Parent == null이면 월드 회전</param>
        /// <param name="vLocalScale">로컬 스케일, tr_Parent == null이면 월드 스케일</param>
        /// <param name="tr_Parent">부모 Transform</param>
        /// <param name="clip">재생할 AudioClip</param>
        /// <param name="isLoof">반복 재생 여부</param>
        /// <param name="playTimeOffset">빨리감기 Time</param>
        /// <returns></returns>
        public SFXObject PlaySFX(
            Vector3 vLocalPos, 
            Quaternion qLocalRot, 
            Vector3 vLocalScale, 
            Transform tr_Parent, 
            AudioClip clip,
            SFXType sfxType,
            bool isLoof,
            float playTimeOffset = 0f)
        {
            if (clip == null)
                return null;

            var sfx = Pool_SFX.Pop(PoolBase.PopOptionForNotEnough.Force) as SFXObject;

            sfx.GenerateObject(vLocalPos, qLocalRot, vLocalScale, tr_Parent);
            sfx.PlaySFX(clip, sfxType, isLoof, playTimeOffset, null);
            sfx.AudioSource.volume = this.Volume;
            return sfx;
        }

        public SFXObject PlaySFX(
            Vector3 vLocalPos,
            Quaternion qLocalRot,
            Vector3 vLocalScale,
            Transform tr_Parent,
            AudioClip clip,
            SFXType sfxType,
            System.Action<SFXObject> endCallback,
            float playTimeOffset = 0f)
        {
            if (clip == null)
            {
                endCallback?.Invoke(null);
                return null;
            }

            var sfx = Pool_SFX.Pop(PoolBase.PopOptionForNotEnough.Force) as SFXObject;

            sfx.GenerateObject(vLocalPos, qLocalRot, vLocalScale, tr_Parent);
            sfx.PlaySFX(clip, sfxType, false, playTimeOffset, endCallback);
            sfx.AudioSource.volume = this.Volume;
            return sfx;
        }

        public SFXObject PlaySFX(
            Transform tr_Parent,
            AudioClip clip,
            SFXType sfxType,
            bool isLoof,
            float playTimeOffset = 0f)
        {
            return PlaySFX(Vector3.zero, Quaternion.identity, Vector3.one,  tr_Parent, clip, sfxType, isLoof, playTimeOffset);
        }

        public SFXObject PlaySFX(
            Transform tr_Parent,
            AudioClip clip,
            SFXType sfxType,
            System.Action<SFXObject> endCallback,
            float playTimeOffset = 0f)
        {
            return PlaySFX(Vector3.zero, Quaternion.identity, Vector3.one, tr_Parent, clip, sfxType, endCallback, playTimeOffset);
        }

        public SFXObject PlaySFX(AudioClip clip, SFXType sfxType)
        {
            return PlaySFX(Vector3.zero, Quaternion.identity, Vector3.one, this.transform, clip, sfxType, false, 0.0f);
        }

        public SFXObject PlaySFX(AudioClip clip, SFXType sfxType, System.Action<SFXObject> endCallback)
        {
            return PlaySFX(Vector3.zero, Quaternion.identity, Vector3.one, this.transform, clip, sfxType, endCallback, 0.0f);
        }


        public void RetrieveAllItems()
        {
#if DevClient
            Debug.Log("SFXManager->RetrieveAllItems");
#endif
            Pool_SFX.RetrieveAllItems();
        }
    }
}
