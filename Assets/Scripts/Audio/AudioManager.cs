using System.Collections;
using UnityEngine;

namespace Evetero
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sound Banks")]
        [SerializeField] private SoundBank defaultSoundBank;

        [Header("SFX Pool")]
        [SerializeField] private int sfxPoolSize = 8;

        // Volume state
        private float _masterVolume = 1f;
        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;
        private float _ambientVolume = 1f;

        // PlayerPrefs keys
        private const string KEY_MASTER  = "vol_master";
        private const string KEY_MUSIC   = "vol_music";
        private const string KEY_SFX     = "vol_sfx";
        private const string KEY_AMBIENT = "vol_ambient";

        // Ambient crossfade (ping-pong between two sources)
        private AudioSource _ambientA;
        private AudioSource _ambientB;
        private bool _ambientOnA = true;
        private Coroutine _crossfadeCoroutine;

        // Music
        private AudioSource _musicSource;

        // SFX pool
        private AudioSource[] _sfxPool;
        private int _sfxPoolIndex;

        // Active bank
        private SoundBank _activeSoundBank;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadVolumes();
            BuildSources();
            _activeSoundBank = defaultSoundBank;
        }

        // ── Volume API ─────────────────────────────────────────────────────────

        public float MasterVolume
        {
            get => _masterVolume;
            set { _masterVolume = Mathf.Clamp01(value); ApplyVolumes(); SaveVolumes(); }
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set { _musicVolume = Mathf.Clamp01(value); ApplyVolumes(); SaveVolumes(); }
        }

        public float SFXVolume
        {
            get => _sfxVolume;
            set { _sfxVolume = Mathf.Clamp01(value); ApplyVolumes(); SaveVolumes(); }
        }

        public float AmbientVolume
        {
            get => _ambientVolume;
            set { _ambientVolume = Mathf.Clamp01(value); ApplyVolumes(); SaveVolumes(); }
        }

        // ── SFX API ────────────────────────────────────────────────────────────

        public void PlaySFX(string id, SoundBank bank = null)
        {
            var entry = Resolve(id, bank);
            if (entry == null) { Debug.LogWarning($"[AudioManager] SFX '{id}' not found."); return; }
            PlayEntry(entry);
        }

        public void PlaySFXClip(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            var src = GetPooledSource();
            src.clip = clip;
            src.volume = volume * _sfxVolume * _masterVolume;
            src.pitch = pitch;
            src.Play();
        }

        // ── Ambient API ────────────────────────────────────────────────────────

        public void PlayAmbient(string id, float crossfadeDuration = 1f, SoundBank bank = null)
        {
            var entry = Resolve(id, bank);
            if (entry == null) { Debug.LogWarning($"[AudioManager] Ambient '{id}' not found."); return; }
            PlayAmbientClip(entry.clip, entry.volume, crossfadeDuration);
        }

        public void PlayAmbientClip(AudioClip clip, float volume = 1f, float crossfadeDuration = 1f)
        {
            if (_crossfadeCoroutine != null) StopCoroutine(_crossfadeCoroutine);
            _crossfadeCoroutine = StartCoroutine(CrossfadeAmbient(clip, volume, crossfadeDuration));
        }

        public void StopAmbient(float fadeDuration = 1f)
        {
            if (_crossfadeCoroutine != null) StopCoroutine(_crossfadeCoroutine);
            _crossfadeCoroutine = StartCoroutine(FadeOutAmbient(fadeDuration));
        }

        // ── Music API ──────────────────────────────────────────────────────────

        public void PlayMusic(string id, SoundBank bank = null)
        {
            var entry = Resolve(id, bank);
            if (entry == null) { Debug.LogWarning($"[AudioManager] Music '{id}' not found."); return; }
            _musicSource.clip = entry.clip;
            _musicSource.volume = entry.volume * _musicVolume * _masterVolume;
            _musicSource.pitch = 1f;
            _musicSource.Play();
        }

        public void StopMusic() => _musicSource.Stop();

        // ── Sound Bank Registration ────────────────────────────────────────────

        public void SetSoundBank(SoundBank bank) => _activeSoundBank = bank ?? defaultSoundBank;

        // ── Private Helpers ────────────────────────────────────────────────────

        private void BuildSources()
        {
            _ambientA    = CreateSource("Ambient_A", loop: true);
            _ambientB    = CreateSource("Ambient_B", loop: true);
            _musicSource = CreateSource("Music",     loop: true);

            _sfxPool = new AudioSource[sfxPoolSize];
            for (int i = 0; i < sfxPoolSize; i++)
                _sfxPool[i] = CreateSource($"SFX_{i}", loop: false);
        }

        private AudioSource CreateSource(string label, bool loop)
        {
            var go = new GameObject($"AudioSource_{label}");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.loop = loop;
            src.playOnAwake = false;
            return src;
        }

        private AudioSource GetPooledSource()
        {
            for (int i = 0; i < _sfxPool.Length; i++)
                if (!_sfxPool[i].isPlaying) return _sfxPool[i];

            // Round-robin fallback when all sources are busy
            var src = _sfxPool[_sfxPoolIndex];
            _sfxPoolIndex = (_sfxPoolIndex + 1) % _sfxPool.Length;
            return src;
        }

        private void PlayEntry(SoundEntry entry)
        {
            var src = GetPooledSource();
            src.clip   = entry.clip;
            src.volume = entry.volume * _sfxVolume * _masterVolume;
            src.pitch  = Random.Range(entry.pitchMin, entry.pitchMax);
            src.Play();
        }

        private SoundEntry Resolve(string id, SoundBank bank)
        {
            var b = bank ?? _activeSoundBank;
            return b?.Get(id);
        }

        private IEnumerator CrossfadeAmbient(AudioClip clip, float targetVolume, float duration)
        {
            var fadeOut = _ambientOnA ? _ambientA : _ambientB;
            var fadeIn  = _ambientOnA ? _ambientB : _ambientA;
            _ambientOnA = !_ambientOnA;

            float effectiveTarget = targetVolume * _ambientVolume * _masterVolume;
            float startVolume = fadeOut.volume;

            fadeIn.clip   = clip;
            fadeIn.volume = 0f;
            fadeIn.Play();

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float ratio = Mathf.Clamp01(t / duration);
                fadeOut.volume = Mathf.Lerp(startVolume, 0f, ratio);
                fadeIn.volume  = Mathf.Lerp(0f, effectiveTarget, ratio);
                yield return null;
            }

            fadeOut.Stop();
            fadeOut.clip = null;
            _crossfadeCoroutine = null;
        }

        private IEnumerator FadeOutAmbient(float duration)
        {
            var active = _ambientOnA ? _ambientA : _ambientB;
            float startVolume = active.volume;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                active.volume = Mathf.Lerp(startVolume, 0f, t / duration);
                yield return null;
            }

            active.Stop();
            active.clip = null;
            _crossfadeCoroutine = null;
        }

        private void LoadVolumes()
        {
            _masterVolume  = PlayerPrefs.GetFloat(KEY_MASTER, 1f);
            _musicVolume   = PlayerPrefs.GetFloat(KEY_MUSIC, 1f);
            _sfxVolume     = PlayerPrefs.GetFloat(KEY_SFX, 1f);
            _ambientVolume = PlayerPrefs.GetFloat(KEY_AMBIENT, 0.6f);
        }

        private void SaveVolumes()
        {
            PlayerPrefs.SetFloat(KEY_MASTER,  _masterVolume);
            PlayerPrefs.SetFloat(KEY_MUSIC,   _musicVolume);
            PlayerPrefs.SetFloat(KEY_SFX,     _sfxVolume);
            PlayerPrefs.SetFloat(KEY_AMBIENT, _ambientVolume);
            PlayerPrefs.Save();
        }

        private void ApplyVolumes()
        {
            _musicSource.volume = _musicVolume * _masterVolume;

            var activeAmbient = _ambientOnA ? _ambientA : _ambientB;
            if (activeAmbient.isPlaying)
                activeAmbient.volume = _ambientVolume * _masterVolume;
        }
    }
}
