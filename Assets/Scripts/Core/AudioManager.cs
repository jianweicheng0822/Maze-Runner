using System.Collections.Generic;
using UnityEngine;

namespace MazeRunner.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Music")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameMusic;

        [Header("SFX Pool")]
        [SerializeField] private int sfxPoolSize = 10;

        private readonly Queue<AudioSource> sfxPool = new Queue<AudioSource>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitSFXPool();
        }

        private void InitSFXPool()
        {
            for (int i = 0; i < sfxPoolSize; i++)
            {
                var go = new GameObject($"SFX_{i}");
                go.transform.SetParent(transform);
                var source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                sfxPool.Enqueue(source);
            }
        }

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            var source = GetSFXSource();
            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = 0f;
            source.Play();
        }

        public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;
            var source = GetSFXSource();
            source.transform.position = position;
            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = 1f;
            source.Play();
        }

        private AudioSource GetSFXSource()
        {
            var source = sfxPool.Dequeue();
            sfxPool.Enqueue(source);
            return source;
        }

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null) return;
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        public void PlayMenuMusic() => PlayMusic(menuMusic);
        public void PlayGameMusic() => PlayMusic(gameMusic);

        public void StopMusic()
        {
            if (musicSource != null) musicSource.Stop();
        }

        public void SetMusicVolume(float volume)
        {
            if (musicSource != null) musicSource.volume = volume;
        }

        public void SetSFXVolume(float volume)
        {
            // SFX volume stored for future use when SFX audio source is added
        }
    }
}
