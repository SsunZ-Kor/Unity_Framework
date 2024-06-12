using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Character
{
    public partial class CharacterObject
    {
        protected Dictionary<string, FXObject> dic_ActiveFX { get; private set; } = new Dictionary<string, FXObject>();
        protected Dictionary<string, SFXObject> dic_ActiveSFX { get; private set; } = new Dictionary<string, SFXObject>();

        /// <summary>
        /// 루프 형 이펙트 등록
        /// </summary>
        public void RegistFXObj(string uniqueKey, FXObject fxObj)
        {
            if (fxObj != null)
                dic_ActiveFX.AddOrRefresh(uniqueKey, fxObj);
        }

        /// <summary>
        /// 루프 형 이펙트 제거
        /// </summary>
        public void RemoveFXObj(string uniqueKey)
        {
            FXObject fxObj;
            if (dic_ActiveFX.TryGetValue(uniqueKey, out fxObj))
            {
                dic_ActiveFX.Remove(uniqueKey);
                if (fxObj != null)
                    fxObj.ReturnToPool();
            }
        }

        public void RemoveAllFxObj()
        {
            foreach(var pair in dic_ActiveFX)
                pair.Value.ReturnToPool();

            dic_ActiveFX.Clear();
        }

        /// <summary>
        /// 루프 형 사운드 이펙트 등록
        /// </summary>
        /// <param name="uniqueKey"></param>
        /// <param name="sfxObj"></param>
        public void RegistSFXObj(string uniqueKey, SFXObject sfxObj)
        {
            if (sfxObj != null)
                dic_ActiveSFX.AddOrRefresh(uniqueKey, sfxObj);
        }

        /// <summary>
        /// 루프 형 사운드 이펙트 제거
        /// </summary>
        public void RemoveSFXObj(string uniqueKey)
        {
            SFXObject sfxObj;
            if (dic_ActiveSFX.TryGetValue(uniqueKey, out sfxObj))
            {
                dic_ActiveSFX.Remove(uniqueKey);
                if (sfxObj != null)
                    sfxObj.ReturnToPool();
            }
        }

        public void RemoveAllSfxObj()
        {
            foreach (var pair in dic_ActiveSFX)
                pair.Value.ReturnToPool();

            dic_ActiveSFX.Clear();
        }
    }
}