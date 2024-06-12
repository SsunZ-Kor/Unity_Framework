using System;

namespace Game
{
    [Serializable]
    public class WeaponData : GameDataBase
    {
        public string Name;
        public string RootActionName;
        public int MaxBullet;
        public float BulletReloadTime;
        public float BulletReloadWaitTime;
        public int ATK;
    }
}
