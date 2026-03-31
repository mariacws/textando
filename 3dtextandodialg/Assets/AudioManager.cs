using System.Collections.Generic;
using UnityEngine;

// Ensure Unity exposes this in the Add Component menu and that an AudioSource exists
[AddComponentMenu("Audio/AudioManager")]
[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private bool persistBetweenScenes = true;
    [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;

    // 2D system source used for background/looping 2D sounds
    private AudioSource systemSource;

    // Active 3D looping sources
    private List<AudioSource> activeSources = new List<AudioSource>();

    // Simple singleton for easy access
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton handling
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (persistBetweenScenes)
            DontDestroyOnLoad(gameObject);

        // Ensure there's a system (2D) AudioSource on this GameObject
        // RequireComponent above guarantees one will be present when added via Add Component
        systemSource = GetComponent<AudioSource>();
        if (systemSource == null)
            systemSource = gameObject.AddComponent<AudioSource>();

        ConfigureSystemSource();

        // Initialize list
        if (activeSources == null)
            activeSources = new List<AudioSource>();
    }

    private void ConfigureSystemSource()
    {
        systemSource.playOnAwake = false;
        systemSource.spatialBlend = 0f; // fully 2D
        systemSource.loop = false;
        systemSource.volume = masterVolume;
    }

    private void Update()
    {
        // Clean up any destroyed/null references in activeSources
        if (activeSources != null && activeSources.Count > 0)
            activeSources.RemoveAll(s => s == null);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // -------------------- 2D (system) APIs --------------------

    // Play a looping 2D clip on the system source
    public void Play2DLoop(AudioClip clip, float volume = 1f, bool loop = true)
    {
        if (clip == null)
            return;

        systemSource.clip = clip;
        systemSource.volume = Mathf.Clamp01(volume) * masterVolume;
        systemSource.loop = loop;
        systemSource.spatialBlend = 0f;
        systemSource.Play();
    }

    public void Stop2D()
    {
        if (systemSource.isPlaying)
            systemSource.Stop();
    }

    public void Pause2D()
    {
        if (systemSource.isPlaying)
            systemSource.Pause();
    }

    public void Resume2D()
    {
        // UnPause will resume only if paused
        systemSource.UnPause();
    }

    // Play a one-shot 2D sound using the system source
    public void Play2DOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
            return;

        systemSource.PlayOneShot(clip, Mathf.Clamp01(volume) * masterVolume);
    }

    // -------------------- 3D APIs --------------------

    // Play a looping 3D AudioSource at a position. Returns the created AudioSource so caller can control it.
    public AudioSource Play3DLoop(AudioClip clip, Vector3 position, float volume = 1f, bool loop = true,
        float spatialBlend = 1f, float minDistance = 1f, float maxDistance = 500f)
    {
        if (clip == null)
            return null;

        GameObject go = new GameObject("3D_AudioSource_" + clip.name);
        go.transform.position = position;
        go.transform.parent = transform; // keep hierarchy tidy

        AudioSource src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.clip = clip;
        src.volume = Mathf.Clamp01(volume) * masterVolume;
        src.loop = loop;
        src.spatialBlend = Mathf.Clamp01(spatialBlend); // 1 = fully 3D
        src.minDistance = Mathf.Max(0.01f, minDistance);
        src.maxDistance = Mathf.Max(src.minDistance, maxDistance);
        src.rolloffMode = AudioRolloffMode.Logarithmic;

        src.Play();

        activeSources.Add(src);
        return src;
    }

    // Stop and destroy a specific 3D loop source
    public void Stop3D(AudioSource src)
    {
        if (src == null)
            return;

        if (activeSources.Contains(src))
            activeSources.Remove(src);

        if (src.isPlaying)
            src.Stop();

        Destroy(src.gameObject);
    }

    // Pause a 3D source
    public void Pause3D(AudioSource src)
    {
        if (src == null)
            return;

        if (src.isPlaying)
            src.Pause();
    }

    // Resume a paused 3D source
    public void Resume3D(AudioSource src)
    {
        if (src == null)
            return;

        src.UnPause();
    }

    // Play a one-shot 3D sound at a position (temporary source)
    public void Play3DOneShot(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null)
            return;

        AudioSource.PlayClipAtPoint(clip, position, Mathf.Clamp01(volume) * masterVolume);
    }

    // Stop and destroy all active 3D loop sources
    public void StopAll3D()
    {
        if (activeSources == null)
            return;

        for (int i = activeSources.Count - 1; i >= 0; i--)
        {
            AudioSource src = activeSources[i];
            if (src == null)
                continue;

            if (src.isPlaying)
                src.Stop();

            Destroy(src.gameObject);
        }

        activeSources.Clear();
    }

    // Expose a copy of the active sources list for inspection (more compatible than IReadOnlyList)
    public List<AudioSource> GetActive3DSources()
    {
        // Return a shallow copy so callers cannot mutate the internal list directly
        return new List<AudioSource>(activeSources);
    }
}
