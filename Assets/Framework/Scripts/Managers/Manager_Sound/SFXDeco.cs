using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(AudioSource))]
    public class SFXDeco : MonoBehaviour
    {
        public static HashSet<AudioSource> set_AudioSource;

        [SerializeField]
        private AudioSource _audioSource;

        private void Awake()
        {
            if (_audioSource == null)
            {
                _audioSource = this.GetComponent<AudioSource>();
                if (_audioSource == null)
                    return;
            }

            if (set_AudioSource == null)
                set_AudioSource = new HashSet<AudioSource>();

            if (!set_AudioSource.Contains(_audioSource))
                set_AudioSource.Add(_audioSource);

            if (Managers.IsValid && Managers.SFX != null)
                _audioSource.volume = Managers.SFX.Volume;
        }

        private void OnDestroy()
        {
            if (_audioSource == null)
                return;

            if (set_AudioSource.Contains(_audioSource))
                set_AudioSource.Remove(_audioSource);

        }
    }
}

