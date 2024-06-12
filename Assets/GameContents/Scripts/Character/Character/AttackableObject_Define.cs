using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Character
{
    public partial class AttackableObject
    {
        public enum HitType
        { 
            None,
            Move,
            AttachToMe,
            AttachToOther,
        }

        [System.Serializable]
        public class AttackData
        {
            public int damageWeight;

            // Hit 이동 or 물리 이펙트
            public HitType hitType;
            public Vector3 hitOffset;      // hit direction
            public float hitPower;
            public string TrName_Attach;
            
            // Hit 그래픽 & 사운드 이펙트
            public string hitFxDir;     // hit fx 폴더 경로
            public string hitFxName;    // hit fx 프리펩명(확장자 없음)
            public string hitSfxDir;    // hit sfx 폴더 경로
            public string hitSfxName;   // hit sfx 프리펩명(확장자 없음)

#if UNITY_EDITOR
            public void OnGUI()
            {
                EditorGUILayout.BeginVertical("Box");
                {
                    damageWeight = EditorGUILayout.IntField("DamageWeight", damageWeight);

                    EditorGUILayout.BeginHorizontal();
                    {
                        hitType = (HitType)EditorGUILayout.EnumPopup("HitEffect", hitType);
                        switch (hitType)
                        {
                            case HitType.Move:
                                hitOffset = EditorGUILayout.Vector3Field("Direction", hitOffset);
                                hitPower = EditorGUILayout.FloatField("Power", hitPower);
                                break;
                            case HitType.AttachToMe:
                            case HitType.AttachToOther:
                                TrName_Attach = EditorGUILayout.TextField("BoneName", TrName_Attach);
                                hitOffset = EditorGUILayout.Vector3Field("PosOffset", hitOffset);
                                break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        hitFxDir = EditorGUILayout.TextField("Hit Fx Directory", hitFxDir);
                        hitFxName = EditorGUILayout.TextField("Hit Fx Name", hitFxName);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        hitSfxDir = EditorGUILayout.TextField("Hit Sfx Directory", hitFxDir);
                        hitSfxName = EditorGUILayout.TextField("Hit Sfx Name", hitFxName);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
#endif

            public AttackData Clone()
            {
                return new AttackData()
                {
                    damageWeight = damageWeight,

                    hitType = this.hitType,
                    hitOffset = this.hitOffset,
                    hitPower = this.hitPower,
                    TrName_Attach = (string)this.TrName_Attach.Clone(),

                    hitFxDir = (string)this.hitFxDir.Clone(),
                    hitFxName = (string)this.hitFxName.Clone(),
                    hitSfxDir = (string)this.hitSfxDir.Clone(),
                    hitSfxName = (string)this.hitSfxName.Clone(),
                };
            }
        }

        public class AttackRuntime
        {
            public CharacterObject _owner { get; private set; }
            public AttackData Data { get; private set; }

            public GameObject prf_HitFx { get; private set; }
            public int idx_HitFx { get; private set; }

            public AudioClip clip_HitSfx { get; private set; }
            public int idx_HitSfx { get; private set; }

            public void Init(CharacterObject owner, AttackData data)
            {
                _owner = owner;
                Data = data;

                // Hit 이펙트 세팅
                if (!string.IsNullOrWhiteSpace(Data.hitFxDir) && !string.IsNullOrWhiteSpace(Data.hitFxName))
                {
                    var hitFxPath = System.IO.Path.Combine(Data.hitFxDir, Data.hitFxName);

                    prf_HitFx = Resources.Load<GameObject>(hitFxPath);
                    if (prf_HitFx != null)
                    {
                        Managers.FX.RegistFX(prf_HitFx);
                        idx_HitFx = _owner.RegistHitFx(prf_HitFx.name);
                    }
                }

                // Hit 사운드 이펙트 세팅
                if (!string.IsNullOrWhiteSpace(Data.hitSfxDir) && !string.IsNullOrWhiteSpace(Data.hitSfxName))
                {
                    var hitSfxPath = System.IO.Path.Combine(Data.hitSfxDir, Data.hitSfxName);

                    clip_HitSfx = Resources.Load<AudioClip>(hitSfxPath);
                    if (clip_HitSfx != null)
                    {
                        idx_HitSfx = _owner.RegistHitSfx(clip_HitSfx);
                    }
                }
            }

            public void OnFinalize()
            {
                if (prf_HitFx != null && Managers.IsValid)
                    Managers.FX.RemoveFX(prf_HitFx.name);

                prf_HitFx = null;
                clip_HitSfx = null;
            }
        }
    }
}
