using System.Collections;
using UnityEngine;

namespace Game
{
    public class BGMManager : ManagerBase
    {
        #region define
        [System.Serializable]
        private class AudioModule
        {
            public static float VolumeFactor = 1f;

            [SerializeField]
            private AudioSource _audio;
            [SerializeField]
            private float _volume = 1f;
            public float Volume
            {
                get
                {
                    return _volume;
                }
                set
                {
                    _volume = value;

                    if (_audio != null)
                        _audio.volume = _volume * VolumeFactor;
                }
            }

            public bool IsPlaying
            {
                get
                {
                    if (_audio == null)
                        return false;

                    return _audio.isPlaying;
                }
            }

            public AudioClip Clip
            {
                get
                {
                    if (_audio == null)
                        return null;

                    return _audio.clip;
                }
                set
                {
                    if (_audio == null)
                        return;

                    _audio.clip = value;
                }
            }

            public bool Mute
            {
                get
                {
                    return _audio.mute;
                }
                set
                {
                    _audio.mute = value;
                }
            }

            public float PlaybackTime
            {
                get
                {
                    if (_audio.clip == null || !_audio.isPlaying)
                        return 0f;

                    return _audio.time;
                }
                set
                {
                    if (_audio.clip == null)
                        return;

                    _audio.time = Mathf.Clamp(value, 0f, _audio.clip.length);
                }
            }



            public AudioModule(AudioSource audio)
            {
                _audio = audio;
                _audio.loop = true;
            }

            public IEnumerator VolumeToDest(float volume, float time, System.Action _endCallback = null)
            {
                if (_audio == null)
                {
                    if (_endCallback != null)
                        _endCallback.Invoke();

                    yield break;
                }

                float srcVolume = Volume;
                float dstVolume = Mathf.Clamp01(volume);
                if (srcVolume == dstVolume)
                {
                    if (_endCallback != null)
                        _endCallback.Invoke();

                    yield break;
                }

                float timeInverse = 1f / time;
                float duration = 0f;
                while (true)
                {
                    yield return new WaitForFixedUpdate();

                    duration += Time.fixedUnscaledDeltaTime;
                    if (duration >= time)
                        break;

                    var factor = duration * timeInverse;
                    Volume = Mathf.Lerp(srcVolume, dstVolume, factor );
                }

                Volume = volume;

                if (_endCallback != null)
                    _endCallback.Invoke();
            }

            public void UpdateVolumeByFactor()
            {
                if (_audio != null)
                    _audio.volume = _volume * VolumeFactor;
            }

            public bool Play(AudioClip clip)
            {
                if (_audio == null)
                    return false;

                Clip = clip;
                if (Clip == null)
                {
                    _audio.Stop();
                    return false;
                }

                _audio.time = 0f;
                _audio.Play();
                return true;
            }

            public void Play(AudioClip clip, float playbackTime)
            {
                if (Play(clip))
                    PlaybackTime = Mathf.Clamp(playbackTime, 0f, clip.length);
            }

            public void Stop()
            {
                if (_audio == null)
                    return;

                Clip = null;
                _audio.Stop();
            }
        }

        public enum FadeType
        {
            None,
            FadeOut,
            FadeIn,
            FadeOutIn,
            CrossFade
        }
        #endregion

        [Header("Components")]
        [SerializeField]
        private AudioModule[] _audios;

        private bool _mute = false;
        public bool Mute {
            get
            {
                return _mute;
            }
            set
            {
                if (_mute == value)
                    return;

                _mute = value;

                if (_audios != null)
                {
                    for (int i = 0; i < _audios.Length; ++i)
                    {
                        var audioModule = _audios[i];
                        if (audioModule == null)
                            continue;

                        audioModule.Mute = value;
                    }
                }
            }
        }
        public float Volume
        {
            get
            {
                return AudioModule.VolumeFactor;
            }
            set
            {
                var val = Mathf.Clamp01(value);
                if (AudioModule.VolumeFactor == val)
                    return;

                AudioModule.VolumeFactor = val;
                if (_audios != null)
                {
                    for (int i = 0; i < _audios.Length; ++i)
                    {
                        var audioModule = _audios[i];
                        if (audioModule == null)
                            continue;

                        audioModule.UpdateVolumeByFactor();
                    }
                }
            }
        }

        public AudioClip audioClip { get; protected set; } = null;

        public bool IgnoreChangeBgm = false;

        public override IEnumerator Init_Async()
        {
            //Mute = !Managers.LocalData.Get_OnBGM();
            //Volume = Managers.LocalData.Get_BGMVolume();
            yield break;
        }

        private void Awake()
        {
            _audios = new AudioModule[2];

            for (int i = 0; i < _audios.Length; ++i)
            {
                var comp_Audio = this.gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
                var newAudioModule = new AudioModule(comp_Audio);

                _audios[i] = newAudioModule;
            }
        }

        private void PlayBGM(AudioClip clip, bool syncPrevClip = false)
        {
            this.audioClip = clip;

            var mainAudio = _audios[0];
            float syncTime = 0f;

            if (syncPrevClip && mainAudio.IsPlaying)
                syncTime = mainAudio.PlaybackTime;

            PlayBGM(clip, syncTime);
        }

        private void PlayBGM(AudioClip clip, float playbackTime)
        {
            this.audioClip = clip;

            var mainAudio = _audios[0];
            mainAudio.Play(clip, playbackTime);
            mainAudio.Volume = 1f;

            var subAudio = _audios[1];
            subAudio.Stop();
        }

        public void PlayBGM(AudioClip clip, FadeType fadeType, float fadeTime, bool syncPrevClip = false, bool bResetSameClip = false)
        {
            if (IgnoreChangeBgm)
                return;

            if (!bResetSameClip && (this.audioClip != null && clip != null)
                && (this.audioClip == clip || this.audioClip.name.CompareTo(clip.name) == 0))
                return;
            else
               this.audioClip = clip;

            StopAllCoroutines();

            var mainAudio = _audios[0];
            var subAudio = _audios[1];

            if (fadeTime <= 0f)
            {
                PlayBGM(clip);
                return;
            }

            switch (fadeType)
            {
                case FadeType.None:
                {
                    PlayBGM(clip);
                }
                break;

                case FadeType.FadeIn:
                {
                    mainAudio.Stop();
                    subAudio.Stop();

                    float syncTime = 0f;
                    if (syncPrevClip && mainAudio.IsPlaying)
                        syncTime = mainAudio.PlaybackTime;

                    mainAudio.Volume = 0f;
                    StartCoroutine(mainAudio.VolumeToDest(1f, fadeTime));
                    mainAudio.Play(clip, syncTime);
                }
                break;

                case FadeType.FadeOut:
                {
                    subAudio.Stop();

                    if (mainAudio.IsPlaying)
                    {
                        float syncTime = 0f;
                        if (syncPrevClip)
                            syncTime = mainAudio.PlaybackTime + fadeTime;

                        StartCoroutine(mainAudio.VolumeToDest(0f, fadeTime, () => { PlayBGM(clip, syncTime); }));
                    }
                    else
                    {
                        PlayBGM(clip);
                    }
                }
                break;

                case FadeType.FadeOutIn:
                {
                    subAudio.Stop();

                    if (mainAudio.IsPlaying)
                    {
                        fadeTime *= 0.5f;

                        float syncTime = 0f;
                        if (syncPrevClip)
                            syncTime = mainAudio.PlaybackTime + fadeTime;

                        System.Action outCallback = () =>
                        {
                            mainAudio.Stop();
                            mainAudio.Volume = 0f;
                            StartCoroutine(mainAudio.VolumeToDest(1f, fadeTime));
                            mainAudio.Play(clip, syncTime);
                        };

                        StartCoroutine(mainAudio.VolumeToDest(0f, fadeTime, outCallback));
                    }
                    else
                    {
                        PlayBGM(clip, FadeType.FadeIn, fadeTime, false, true);
                    }
                }
                break;

                case FadeType.CrossFade:
                {
                    if (mainAudio.IsPlaying)
                    {
                        float syncTime = 0f;
                        if (syncPrevClip)
                            syncTime = mainAudio.PlaybackTime;

                        var temp = _audios[0];
                        _audios[0] = _audios[1];
                        _audios[1] = temp;

                        mainAudio = _audios[0];
                        subAudio = _audios[1];

                        mainAudio.Volume = 0f;
                        StartCoroutine(mainAudio.VolumeToDest(1f, fadeTime));
                        StartCoroutine(subAudio.VolumeToDest(0f, fadeTime, () => subAudio.Stop()));

                        mainAudio.Play(clip, syncTime);
                    }
                    else
                    {
                        PlayBGM(clip, FadeType.FadeIn, fadeTime, false, true);
                    }
                }
                break;
            }
        }

        public void ChangeBgmTime(float time)
        {
            if (_audios[0].Clip == null)
                return;

            _audios[0].PlaybackTime = time;
        }
    }
}