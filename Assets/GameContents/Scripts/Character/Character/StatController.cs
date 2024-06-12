using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Character
{

    public class StatController
    {
        public enum HitReactionType
        { 
            Normal,
            Guard,
            SuperArmor,
        }

        public CharacterObject _owner;

        public HitReactionType HitReaction;

        public int ATK { get; private set; }
        public int CrrHP { get; private set; }
        public int MaxHP { get; private set; }
        public float CrrHpFactor => CrrHP / (float)MaxHP;

        public int CrrDodgeCount => crrDodgeCount;
        public int CrrBulletCount => crrBulletCount;

        private int maxDodgeCount;
        private int crrDodgeCount;
        private float dodgeReloadTime;
        private float crrDodgeReloadTime;
        private float dodgeReloadWaitTime;
        private float crrDodgeReloadWaitTime;
        
        private int maxBulletCount;
        private int crrBulletCount;
        private float bulletReloadTime;
        private float crrBulletReloadTime;
        private float bulletReloadWaitTime;
        private float crrBulletReloadWaitTime;

        public StatController(CharacterObject owner)
        {
            _owner = owner;
        }

        public void Init()
        {
            var charData = _owner.CharacterData;
            if (charData != null)
            {
                MaxHP = charData.MaxHp;
                CrrHP = charData.MaxHp;

                maxDodgeCount = charData.MaxDodge;
                crrDodgeCount = charData.MaxDodge;

                dodgeReloadTime = charData.DodgeReloadTime;
                dodgeReloadWaitTime = charData.DodgeReloadWaitTime;
                crrDodgeReloadTime = 0f;
            }

            var weaponData = _owner.WeaponData;
            if (weaponData != null)
            {
                ATK = weaponData.ATK;

                maxBulletCount = weaponData.MaxBullet;
                crrBulletCount = weaponData.MaxBullet;
                bulletReloadTime = weaponData.BulletReloadTime;
                bulletReloadWaitTime = weaponData.BulletReloadWaitTime;
                crrBulletReloadTime = 0f;
            }
        }

        public void OnUpdate()
        {
            if (_owner == null || _owner.IsNetChar)
                return;

            // Update Dodge
            OnUpdate_Reload(ref crrDodgeCount, ref maxDodgeCount, ref crrDodgeReloadWaitTime, ref dodgeReloadTime, ref crrDodgeReloadTime);

            // Update Bullet
            OnUpdate_Reload(ref crrBulletCount, ref maxBulletCount, ref crrBulletReloadWaitTime, ref bulletReloadTime, ref crrBulletReloadTime);
        }

        public void OnUpdate_Reload(ref int crr, ref int max, ref float waitTime, ref float reloadTime, ref float crrReloadTime)
        {
            if (crr >= max)
                return;

            if (waitTime > 0f)
            {
                waitTime -= Time.deltaTime;
                return;
            }

            crrReloadTime -= Time.deltaTime;
            if (crrReloadTime > 0f)
                return;

            while (crrReloadTime <= 0f && crr < max)
            {
                crrReloadTime += reloadTime;
                ++crr;
            }
        }

        public void SetHp(int Hp)
        {
            CrrHP = Mathf.Clamp(Hp, 0, MaxHP);
        }

        public void AddHp(int addHp)
        {
            SetHp(CrrHP + addHp);
        }

        public void AddDodgeCount(int addDodgeCount)
        {
            crrDodgeCount += addDodgeCount;
            crrDodgeCount = Mathf.Clamp(crrDodgeCount, 0, maxDodgeCount);

            crrDodgeReloadWaitTime = dodgeReloadWaitTime;
        }

        public void AddBulletCount(int addBulletCount)
        {
            crrBulletCount += addBulletCount;
            crrBulletCount = Mathf.Clamp(crrBulletCount, 0, maxBulletCount);

            crrBulletReloadWaitTime = bulletReloadWaitTime;
        }
    }
}