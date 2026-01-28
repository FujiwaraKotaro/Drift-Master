using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static SoundManager Instance { get; private set; }

    [System.Serializable]
    public struct SoundData
    {
        public string name;
        public AudioClip clip;
    }

    [Header("BGM設定")]
    [SerializeField] private List<SoundData> bgmList;
    [SerializeField] private AudioSource bgmSource;

    [Header("SE設定")]
    [SerializeField] private List<SoundData> seList;
    [SerializeField] private AudioSource seSource;

    private void Awake()
    {
        // シングルトンの初期化処理
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移時に破棄されないようにする
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- BGM再生用メソッド ---
    public void PlayBGM(string name)
    {
        SoundData data = bgmList.Find(s => s.name == name);
        if (data.clip == null)
        {
            Debug.LogWarning($"BGM: {name} が見つかりませんでした");
            return;
        }

        // 既に同じBGMが再生中なら何もしない
        if (bgmSource.clip == data.clip && bgmSource.isPlaying) return;

        bgmSource.clip = data.clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // --- SE再生用メソッド ---
    public void PlaySE(string name)
    {
        SoundData data = seList.Find(s => s.name == name);
        if (data.clip == null)
        {
            Debug.LogWarning($"SE: {name} が見つかりませんでした");
            return;
        }

        // PlayOneShotを使用（同時に複数の音を重ねて再生可能）
        seSource.PlayOneShot(data.clip);
    }
}