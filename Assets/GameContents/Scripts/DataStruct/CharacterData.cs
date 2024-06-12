using System;

namespace Game
{
    [Serializable]
    public class CharacterData : GameDataBase
    {
        public string Name;
        public string WeaponDataName;
        public int MaxHp;
        public int MaxDodge;
        public float DodgeReloadTime;
        public float DodgeReloadWaitTime;
        public string ModelPrefabPath;
    }
}
