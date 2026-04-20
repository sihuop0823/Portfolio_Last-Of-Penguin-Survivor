using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace GlobalAudio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("BGM Settings")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private List<AudioClip> bgmList = new List<AudioClip>();
        [Range(0f, 1f)][SerializeField] private float bgmVolume = 1f;

        [Header("SFX Settings")]
        [SerializeField] private AudioSource sfxSource;
        [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

        // SettingManager에서 접근 가능
        public float BGMVolume => bgmVolume;
        public float SFXVolume => sfxVolume;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            // 저장된 볼륨 불러오기
            bgmVolume = PlayerPrefs.GetFloat(AudioVolume.BGM, 1f);
            sfxVolume = PlayerPrefs.GetFloat(AudioVolume.SFX, 1f);
        }

        private void Start()
        {
            AudioPlayer.SetSFXVolume(sfxVolume);
            PlayBGMByIndex(0);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode _) // LoadSceneMode 변수 안씀. 틀 맞추려고 쓴거
        {
            // _ 는 안쓰는 변수입니다 오인 X
            switch (scene.name)
            {
                case SceneName.Lobby:
                    PlayBGMByIndex(0);
                    break;
                case SceneName.Game:
                case SceneName.MultiGame:
                    PlayBGMByIndex(1);
                    break;

                default:
                    break;
            }
        }

        public void PlayBGMByIndex(int index)
        {
            if (bgmSource == null || bgmList.Count == 0)
                return;

            if (index < 0 || index >= bgmList.Count)
            {
                Debug.LogWarning($"AudioManager: 잘못된 인덱스 {index}");
                return;
            }

            AudioClip clip = bgmList[index];
            if (clip == null)
                return;

            if (bgmSource.clip == clip && bgmSource.isPlaying)
                return;

            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            if (bgmSource != null)
                bgmSource.Stop();
        }

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);

            if (bgmSource != null)
                bgmSource.volume = bgmVolume;

            // 볼륨 저장
            PlayerPrefs.SetFloat(AudioVolume.BGM, bgmVolume);
            PlayerPrefs.Save();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            AudioPlayer.SetSFXVolume(sfxVolume);

            // 볼륨 저장
            PlayerPrefs.SetFloat(AudioVolume.SFX, sfxVolume);
            PlayerPrefs.Save();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
        }

        public void StopSFX(AudioClip clip)
        {
            if(sfxSource != null)
                sfxSource.Stop();
        }
    }



    public static class AudioVolume
    {
        public const string BGM = "BGMVolume";
        public const string SFX = "SFXVolume";

        // 오디오 이름을 바꿨을 때 불상사가 생기지 않게 상수로 만들어두었습니다
    }
}
