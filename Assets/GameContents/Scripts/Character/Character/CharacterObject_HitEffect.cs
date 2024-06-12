using BubbleFighter.Network.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Character
{
    public partial class CharacterObject
    {
        protected long nextProjUniqueKey = 0;

        protected List<Transform> list_TrParent = new List<Transform>();
        protected List<string> list_ProjName = new List<string>();
        protected List<string> list_HitFxName = new List<string>();
        protected List<AudioClip> list_HitSfxClip = new List<AudioClip>();

        public long GetProjectileUniqueKey()
        {
            return nextProjUniqueKey++;
        }

        public int RegistParent(Transform tr)
        {
            list_TrParent.Add(tr);
            return list_TrParent.Count - 1;
        }

        public Transform GetParent(int parnetTrIndex)
        {
            if (!list_TrParent.CheckIndex(parnetTrIndex))
                return null;

            return list_TrParent[parnetTrIndex];
        }

        public int RegistProj(string projName)
        {
            list_ProjName.Add(projName);
            return list_ProjName.Count - 1;
        }

        public string GetProjName(int projIndex)
        {
            if (!list_ProjName.CheckIndex(projIndex))
                return null;

            return list_ProjName[projIndex];
        }

        public int RegistHitFx(string hitFxName)
        {
            list_HitFxName.Add(hitFxName);
            return list_HitFxName.Count - 1;
        }

        public string GetHitFxName(int idx)
        {
            if (!list_HitFxName.CheckIndex(idx))
                return null;

            return list_HitFxName[idx];
        }

        public int RegistHitSfx(AudioClip hitSfxClip)
        {
            list_HitSfxClip.Add(hitSfxClip);
            return list_HitSfxClip.Count - 1;
        }
        
        public AudioClip GetHitSfxClip(int idx)
        {
            if (!list_HitSfxClip.CheckIndex(idx))
                return null;

            return list_HitSfxClip[idx];
        }

        public void RemoveAllProjInfo()
        {
            list_ProjName.Clear();
            list_HitFxName.Clear();
            list_HitSfxClip.Clear();
        }
    }
}