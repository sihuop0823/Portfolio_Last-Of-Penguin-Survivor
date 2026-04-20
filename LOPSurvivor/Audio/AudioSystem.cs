using UnityEngine;

namespace GlobalAudio
{
    public static class AudioPlayer
    {
        /// <summary>
        /// 특정 오브젝트에 AudioSource를 붙이고 오디오 재생합니다
        /// </summary>
        /// <param name="target">오디오가 재생될 GameObject</param>
        /// <param name="clip">재생할 AudioClip</param>
        /// <param name="volume">볼륨 (기본값 1f)</param>
        /// 

        private static float soundVolume = 1f;
        private static float muteMultiplier = 1f;

        public static void PlaySound(GameObject target, AudioClip clip, float volume = 1f)
        {
            if (target == null || clip == null)
            {
                //Debug.LogWarning("AudioPlayer.PlaySound 호출 실패: target 또는 clip이 null입니다.");
                return;
            }

            float finalVolume = soundVolume * volume * muteMultiplier;

            if (finalVolume <= 0.001f) return;

            AudioSource source = target.GetComponent<AudioSource>();
            if (source == null)
            {
                source = target.AddComponent<AudioSource>();
            }

            source.PlayOneShot(clip, finalVolume);
        }
        /// <summary>
        /// SFX 볼륨 설정 (0~1)
        /// </summary>
        public static void SetSFXVolume(float volume)
        {
            soundVolume = Mathf.Clamp01(volume);

            // Clamp01은 그냥 값 0 ~ 1 사이 매개변수 하나만 받는 자동 제한 함수입니다.
            // 네, 저도 만들 당시에 몰랐으니 혹시 모르시면 참고 하시라구요..
        }

        public static void SetMuteState(bool isMuted)
        {
            muteMultiplier = isMuted ? 0f : 1f;
        }

        public static float GetSFXVolume() => soundVolume;
    }
}