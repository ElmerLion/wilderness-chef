using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AudioManager;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Ricimi;

public class AudioManager : MonoBehaviour {

    public static AudioManager Instance { get; private set; }

    public enum Sound {
        PlayerFootstep,
        FryingPanSizzle,
        MixerMixing,
        KnifeChopping,
        BushRustle,
        ChickenIdle,
        CowDamaged,
        BirdChirp,
        DeliverySuccess,
        ItemBurning,
        BeehiveBuzz,
        PickingPlant,
        RecipeCompleted,
        NewOrder,
        FailedOrder,
        BasicItemPickup,
        LiquidPickup,
        MeatPickup,
        WaterSplash,
        ThrowTrash,
        ChickenDamaged,
        CowIdle,
        RankUp,
        RankUp1,
        TimerTicking,
    }

    public enum Music {
        BackgroundMusic,
    }

    public enum AudioType {
        Master,
        Music,
        SFX,
        Environment,
    }

    [System.Serializable]
    public class SoundAudioClip {
        public AudioType audioType;
        public Sound sound;
        public bool repeat;
        public float audioPlayDistance;
        public List<AudioClip> audioClip;
    }

    [System.Serializable]
    public class MusicAudioClip {
        public Music music;
        public List<AudioClip> audioClip;
    }

    [System.Serializable]
    public class RandomIntervalAudio {
        public SoundAudioClip soundAudioClip;
        public float minInterval;
        public float maxInterval;
        public float timer;
        public float spawnDistanceToPlayer;
        public float volumeMultiplier = 1f;
    }

    [Header("Sound Effects")]
    public List<SoundAudioClip> soundAudioClipList;

    [Header("Music Tracks")]
    public List<MusicAudioClip> musicAudioClipList;

    [Header("Random Interval Audio")]
    public List<RandomIntervalAudio> randomIntervalAudioList;

    private Dictionary<Sound, List<AudioClip>> soundAudioClips;
    private Dictionary<Music, List<AudioClip>> musicAudioClips;
    private Dictionary<AudioType, float> audioTypeVolume;

    private AudioSource musicAudioSource;
    private Music currentMusic;
    private int currentTrackIndex = 0;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(this);

        soundAudioClips = new Dictionary<Sound, List<AudioClip>>();
        musicAudioClips = new Dictionary<Music, List<AudioClip>>();
        audioTypeVolume = new Dictionary<AudioType, float>();

        foreach (SoundAudioClip soundAudioClip in soundAudioClipList) {
            soundAudioClips[soundAudioClip.sound] = soundAudioClip.audioClip;
        }

        foreach (MusicAudioClip musicAudioClip in musicAudioClipList) {
            musicAudioClips[musicAudioClip.music] = musicAudioClip.audioClip;
        }

        audioTypeVolume[AudioType.Master] = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        audioTypeVolume[AudioType.Music] = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        audioTypeVolume[AudioType.SFX] = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        audioTypeVolume[AudioType.Environment] = PlayerPrefs.GetFloat("EnvironmentVolume", 0.5f);

        musicAudioSource = gameObject.AddComponent<AudioSource>();
        musicAudioSource.loop = true;
        SetVolume(audioTypeVolume[AudioType.Music], AudioType.Music);
    }


    private void Update() {
        if (!musicAudioSource.isPlaying) {
            PlayNextTrack();
        }

        PlayRandomIntervalAudio();
    }

    public void PlayRandomIntervalAudio() {
        foreach (RandomIntervalAudio randomIntervalAudio in randomIntervalAudioList) {
            randomIntervalAudio.timer -= Time.deltaTime;

            if (randomIntervalAudio.timer <= 0f) {
                randomIntervalAudio.timer = Random.Range(randomIntervalAudio.minInterval, randomIntervalAudio.maxInterval);

                Vector3 playerPos = Camera.main.transform.position;
                Vector3 spawnPos = new Vector3(playerPos.x + randomIntervalAudio.spawnDistanceToPlayer, playerPos.y, playerPos.z + randomIntervalAudio.spawnDistanceToPlayer);
                PlaySound(randomIntervalAudio.soundAudioClip, spawnPos, randomIntervalAudio.volumeMultiplier);
            }
        }
    }
    public AudioSource PlaySound(Sound sound, float volumeMultiplier = 1f, bool addRandomPitch = false) {
        return PlaySound(sound, Camera.main.transform.position, volumeMultiplier, addRandomPitch);
    }

    public AudioSource PlaySound(Sound sound, Vector3 position, float volumeMultiplier = 1f, bool addRandomPitch = false) {
        if (soundAudioClips.ContainsKey(sound)) {
            SoundAudioClip soundAudioClip = soundAudioClipList.Find(x => x.sound == sound);
            return PlaySound(soundAudioClip, position, volumeMultiplier);
        } else {
            Debug.LogWarning("Sound " + sound + " not found!");
            return null;
        }
    }

    public AudioSource PlaySound(SoundAudioClip soundAudioClip, Vector3 position, float volumeMultiplier = 1f, bool addRandomPitch = false) {
        float specificVolume = audioTypeVolume[soundAudioClip.audioType];
        float masterVolume = audioTypeVolume[AudioType.Master];
        float finalVolume = (specificVolume * masterVolume) * volumeMultiplier;

        if (finalVolume > 0f) {
            return PlayAudioClip(soundAudioClip, position, finalVolume, addRandomPitch);
        }
        return null;
    }

    public void PlayMusic(Music music) {
        if (music == currentMusic && musicAudioSource.isPlaying) {
            return;
        }

        if (musicAudioClips.ContainsKey(music)) {
            currentMusic = music;
            currentTrackIndex = Random.Range(0, musicAudioClips[music].Count);
            PlayCurrentTrack();
        } else {
            Debug.LogWarning("Music " + music + " not found!");
        }
    }

    private void PlayCurrentTrack() {
        if (musicAudioClips.ContainsKey(currentMusic)) {
            AudioClip clip = musicAudioClips[currentMusic][currentTrackIndex];
            musicAudioSource.clip = clip;

            musicAudioSource.volume = audioTypeVolume[AudioType.Music] * audioTypeVolume[AudioType.Master];
            musicAudioSource.Play();
        }
    }

    private void PlayNextTrack() {
        currentTrackIndex++;
        if (currentTrackIndex >= musicAudioClips[currentMusic].Count) {
            currentTrackIndex = 0;
        }
        PlayCurrentTrack();
    }

    public void StopMusic() {
        musicAudioSource.Stop();
    }

    public void SetVolume(float volume, AudioType audioType) {
        if (audioTypeVolume.ContainsKey(audioType)) {
            audioTypeVolume[audioType] = volume;
        } else {
            Debug.LogWarning("AudioType " + audioType + " not found!");
            return;
        }

        if (audioType == AudioType.Music || audioType == AudioType.Master) {
            musicAudioSource.volume = audioTypeVolume[AudioType.Music] * audioTypeVolume[AudioType.Master];
        }
    }

    private AudioSource PlayAudioClip(SoundAudioClip soundAudioClip, Vector3 position, float volume, bool addRandomPitch = false) {
        AudioClip clip = soundAudioClip.audioClip[Random.Range(0, soundAudioClip.audioClip.Count)];

        GameObject tempAudioSource = new GameObject("TempAudio_" + clip.name);
        tempAudioSource.transform.position = position;

        AudioSource audioSource = tempAudioSource.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;

        if (addRandomPitch) {
            audioSource.pitch = Random.Range(0.8f, 1.2f);
        } else {
            audioSource.pitch = 1f;
        }

        audioSource.spatialBlend = 1.0f;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = soundAudioClip.audioPlayDistance;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        audioSource.Play();
        if (!soundAudioClip.repeat) {
            Destroy(tempAudioSource, clip.length);
        } else {
            audioSource.loop = true;
        }

        return audioSource;
    }

    public AudioSource PlayLoopingSound(Sound sound, Transform parent, float volumeMultiplier = 1f) {
        // Look up your clip data
        SoundAudioClip soundData = soundAudioClipList.Find(x => x.sound == sound);
        if (soundData == null) {
            Debug.LogWarning($"[AudioManager] No SoundAudioClip for {sound}");
            return null;
        }

        // pick a random clip
        AudioClip clip = soundData.audioClip[Random.Range(0, soundData.audioClip.Count)];
        float volume = audioTypeVolume[soundData.audioType]
                     * audioTypeVolume[AudioType.Master]
                     * volumeMultiplier;

        // create a new GameObject as a child of 'parent'
        GameObject go = new GameObject($"LoopingSound_{sound}");
        go.transform.SetParent(parent, worldPositionStays: false);

        // attach and configure an AudioSource
        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.loop = true;
        src.volume = volume;
        src.spatialBlend = 1f;
        src.minDistance = 1f;
        src.maxDistance = soundData.audioPlayDistance;
        src.rolloffMode = AudioRolloffMode.Linear;

        src.Play();
        return src;
    }

    public void StopLoopingSound(AudioSource src) {
        if (src == null) return;
        src.Stop();
        Destroy(src.gameObject);
    }


    private void OnApplicationQuit() {
        PlayerPrefs.SetFloat("MasterVolume", audioTypeVolume[AudioType.Master]);
        PlayerPrefs.SetFloat("MusicVolume", audioTypeVolume[AudioType.Music]);
        PlayerPrefs.SetFloat("SFXVolume", audioTypeVolume[AudioType.SFX]);
        PlayerPrefs.SetFloat("EnvironmentVolume", audioTypeVolume[AudioType.Environment]);
    }

}
