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
        // シングルトンの初期化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンを跨いでも破棄されない
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
            Debug.LogWarning($"BGM: {name} が見つかりません");
            return;
        }

        // すでに同じBGMが流れている場合は何もしない
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
            Debug.LogWarning($"SE: {name} が見つかりません");
            return;
        }

        // PlayOneShotで再生（複数の音が重なっても大丈夫）
        seSource.PlayOneShot(data.clip);
    }
}