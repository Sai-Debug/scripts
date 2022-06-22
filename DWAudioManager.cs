using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using ImpactCombo = System.Tuple<DWWeaponType, DWHandlerObjectMaterial>;
public enum BGM { MainMenu, Tutorial, Level1, Level2, Level3, GameOver, Winning };
public enum AudioType { ButtonClick, Interact, Cook };

public class DWAudioManager : SimpleSingleton<DWAudioManager>
{
    private void Awake()
    {
        InitSingleton();
        Core.SubscribeEvent(DWData.OnPlayerInCombat, OnPlayerInCombat);
    }
    private void OnDestroy()
    {
        UninitSingleton();
        Core.UnsubscribeEvent(DWData.OnPlayerInCombat, OnPlayerInCombat);
    }

    [Header("Audio Sources")]
    public AudioSource AmbientAudio;
    public AudioSource ActionAudio;
    public AudioSource StingSource;

    [Header("Audio Snapshots")]
    public AudioMixerSnapshot AmbientSnapshot;
    public AudioMixerSnapshot ActionSnapshot;

    [Header("Ambient Music")]    
    public AudioClip[] IntroClips;
    public AudioClip[] PrologueClips;
    public AudioClip[] AltarMusicClips;
    public AudioClip[] CaveMusicClips;
    public AudioClip[] ForestMusicClips;

    [Header("Action Music")]
    public AudioClip[] ActionMusicClips;

    [Header("Impact Audio")]
    public DWImpactSound[] ImpactSound;

    [Header("Weapon Swoosh Audio")]
    public DWSwooshSound[] WeaponSwoosh;

    [Header("Music Transition Time")]
    public float TransitionIn = 1f;
    public float TransitionOut = 1f;

    [Header("Other Sound FX")]
    public AudioClip[] SFXclips;

    //[Header("Foot Steps - Ordered")]
    //public AudioClip[] FootSteps;
    [Header("Footsteps Audio")]
    public DWTerrainSound[] FootSteps;

    public DWMusicType StartMusicType = DWMusicType.Intro;
    Dictionary<ImpactCombo, DWImpactSound> _impactSounds = new Dictionary<ImpactCombo, DWImpactSound>();
    Dictionary<DWWeaponType, DWSwooshSound> _swooshSounds = new Dictionary<DWWeaponType, DWSwooshSound>();
    Dictionary<DWTerrainType, DWTerrainSound> _terrainSounds = new Dictionary<DWTerrainType, DWTerrainSound>();
    // Start is called before the first frame update
    void Start()
    {
        DWDataManager.LoadConfig();
        SetVolume();
        foreach (DWImpactSound dis in ImpactSound)
        {
            _impactSounds[new ImpactCombo(dis.WeaponType, dis.TargetMaterial)] = dis;
        }
        foreach(DWSwooshSound s in WeaponSwoosh)
        {
            _swooshSounds[s.WeaponType] = s;
        }
        foreach(DWTerrainSound ts in FootSteps)
        {
            _terrainSounds[ts.TerrainType] = ts;
        }
        PlayMusic(StartMusicType, true);
    }
    // Update is called once per frame
    void Update()
    {

    }
    void OnPlayerInCombat(object sender, object[] args)
    {
        if ((bool)args[0])
            PlayRandomActionMusic();
        else
            PlayMusic(DWMusicType.Altar); //TODO locational music
    }
    public void SetVolume()
    {
        StingSource.volume = DWDataManager.SFXVolume;
        AmbientAudio.volume = ActionAudio.volume = DWDataManager.MusicVolume;
    }
    public void PlayMusic(DWMusicType id, bool force = false)
    {
        if (!AmbientAudio.isPlaying || force)
        {
            AudioClip[] clips = null;
            switch(id)
            {
                case DWMusicType.Altar:
                    clips = AltarMusicClips; break;
                case DWMusicType.Cave:
                    clips = CaveMusicClips; break;
                case DWMusicType.Forest:
                    clips = ForestMusicClips; break;
                case DWMusicType.Intro:
                    clips = IntroClips; break;
                case DWMusicType.Prologue:
                    clips = PrologueClips; break;
            }
            int randClip = Random.Range(0, clips.Length);
            AmbientAudio.clip = clips[randClip];
            AmbientAudio.Stop();

            AmbientAudio.Play();
            AmbientSnapshot.TransitionTo(TransitionIn);
        }
    }
    public void PlayRandomActionMusic() // Combat Music
    {
        if (!ActionAudio.isPlaying)
        {
            int randClip = Random.Range(0, ActionMusicClips.Length);
            ActionAudio.clip = ActionMusicClips[randClip];
            //if (ActionAudio.isPlaying)
            //    ActionAudio.Stop();

            //ActionAudio.Play();
            ActionSnapshot.TransitionTo(TransitionOut);
            ActionAudio.Play();
        }
    }
    public void PlayImpact(DWWeaponType weaponType, DWHandlerObjectMaterial entityMaterial, Vector3 pos) // Weapon Impacts
    {
        ImpactCombo ic = new ImpactCombo(weaponType, entityMaterial);
        DWImpactSound imp = null;
        if (_impactSounds.TryGetValue(ic, out imp))
        {
            int idx = Random.Range(0, imp.Clips.Length);
            AudioSource.PlayClipAtPoint(imp.Clips[idx], pos, DWDataManager.SFXVolume);
        }
        //StingSource.Play();
    }
    //public void PlayFootStep(Vector3 pos, int idx)
    //{
    //    AudioClip clip = FootSteps[0];
    //    if (FootSteps.Length > idx)
    //        clip = FootSteps[idx];
    //    AudioSource.PlayClipAtPoint(clip, pos, DWDataManager.SFXVolume);
    //}
    public void PlayFootStep(DWTerrainType type, Vector3 pos)
    {
        DWTerrainSound _ts = null;
        if(!_terrainSounds.TryGetValue(type, out _ts))
        {
            _ts = _terrainSounds[DWTerrainType.Plain];//assumed to be there
        }
        var clips = _ts.Clips;
        int idx = Random.Range(0, clips.Length);
        var clip = clips[idx];
        AudioSource.PlayClipAtPoint(clip, pos);
    }
    public void PlaySwoosh(DWWeaponType type, Vector3 position)
    {
        DWSwooshSound ss = null;
        if(!_swooshSounds.TryGetValue(type, out ss))
        {
            ss = _swooshSounds[DWWeaponType.Blade];//assumed to be there
        }
        var clips = ss.Clips;
        int idx = Random.Range(0, clips.Length);
        var clip = clips[idx];
        AudioSource.PlayClipAtPoint(clip, position);
    }
    public void PlaySound(AudioType type, Vector3? pos = null)
    {
        if ((int)type < SFXclips.Length)
        {
            Vector3 at;
            if (pos == null)
            {
                at = Camera.main.transform.position;
            }
            else
            {
                at = pos.Value;
            }
            AudioSource.PlayClipAtPoint(SFXclips[(int)type], at, DWDataManager.SFXVolume);
        }
    }
}

public enum DWMusicType
{
    Intro,
    Altar,
    Cave,
    Forest,
    Prologue
}

public enum DWTerrainType
{
    Plain,
    Bricks,
    DrySoil,
    Gravel,
    Sand,
    Rocky,
    Clay,
    Field,
    Forest
}

[System.Serializable]
public class DWImpactSound
{
    public DWWeaponType WeaponType;
    public DWHandlerObjectMaterial TargetMaterial;
    public AudioClip[] Clips;
}

[System.Serializable]
public class DWSwooshSound
{
    public DWWeaponType WeaponType;
    public AudioClip[] Clips;
}

[System.Serializable]
public class DWTerrainSound
{
    public DWTerrainType TerrainType;
    public AudioClip[] Clips;
}