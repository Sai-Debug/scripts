using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class DWGameManager : SimpleSingleton<DWGameManager>
{
    [Header("Common")]
    public Transform RebornLocation;
    public Transform RebornCameraLocation;

    [Header("Weapons")]
    public DWWeapon DefaultSword;
    public DWWeapon DarkSword;
    public DWWeapon HolySword;

    [Header("First Play and Tutorial")]
    public Transform FirstPlayLocation;
    public Transform FirstPlayCameraLocation;
    public Transform TutorialStartTrigger;
    public Transform TutorialControlTrigger;
    public Transform TutorialSpawnCamera;
    public Transform TutorialSpawner;
    public int TutorialComboNeeded = 2;
    int TutorialComboCount = 2;

    [Header("Tutorial End")]
    public Transform tutorialEndTrigger;

    [Header("Cliff")]
    public Transform CliffTriggers;

    [Header("Cave")]
    public Transform CaveTriggers;

    [Header("Altar")]
    public Transform AltarTriggers;

    [HideInInspector]
    public IDWEntity Player;
    
    private void Awake()
    {
        InitSingleton();
        Core.SubscribeEvent(DWData.OnSetPlayer, OnSetPlayer);
        Core.SubscribeEvent(DWData.OnPause, OnPause);
        Core.SubscribeEvent("tutorial_combo_start", OnTutorialComboStart);
        Core.SubscribeEvent(DWData.OnHealthUpdate, OnHealthUpdate);
    }
    private void OnDestroy()
    {
        UninitSingleton();
        Core.UnsubscribeEvent(DWData.OnPause, OnPause);
        Core.UnsubscribeEvent(DWData.OnSetPlayer, OnSetPlayer);
        Core.UnsubscribeEvent("tutorial_combo_start", OnTutorialComboStart);
        Core.UnsubscribeEvent(DWData.OnHealthUpdate, OnHealthUpdate);
    }
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        //Invoke("FireGameState", 2);
    }
    
    void OnHealthUpdate(object sender, object[] args)
    {
        int current = (int)args[0];
        if(current == 0)
        {
            StartCoroutine(ExecuteWithDelay(() =>
            {
                UIDialogYesNo dok = DWUIManager.Instance.Open<UIDialogYesNo>(GameUI.DialogOK);
                dok.Label = "You died.";
                dok.Button1Label.text = "Reborn";
                dok.OnButtonClick += (buttontype) =>
                {
                    DWUILevelLoading.LoadScene("level01", "Reborn...", "");
                    return true;
                };
            }, 3));
        }
    }
    void OnTutorialComboStart(object sender, object[] args)
    {
        if(args!=null && args.Length > 0)
            Core.BroadcastEvent("tutorial_rat_spawn", this, args[0]);
        else
            Core.BroadcastEvent("tutorial_rat_spawn", this, 1);
        Core.BroadcastEvent(DWData.OnActivatePlayer, this, false);
        Core.BroadcastEvent(DWData.OnSetFreeRoamCamera, this, true, TutorialSpawnCamera.position, TutorialSpawnCamera.rotation);
        StartCoroutine(ExecuteWithDelay(() =>
        {
            Core.BroadcastEvent(DWData.OnSetFreeRoamCamera, this, false);
            Core.BroadcastEvent(DWData.OnActivatePlayer, this, true);
        }, 5));
    }
    void OnSetPlayer(object sender, object[]args)
    {
        if (args != null && args.Length > 0)
        {
            Player = (IDWEntity)args[0];
            Player.ControllerCallback += OnPlayerActionCallback;
            DWUIManager.Instance.OpenReplace(GameUI.GameHUD);
            FireGameState();
            Core.BroadcastEvent(DWData.OnHealthUpdate, this, Player.CurrentHealth, Player.MaxHealth);
            Core.BroadcastEvent(DWData.OnAmuletUpdate, this, Player.CurrentEXP, Player.MaxEXP);
        }
    }
    void OnPlayerActionCallback(DWEntityCallbackEvent callback, object[] args)
    {
        switch(callback)
        {
            case DWEntityCallbackEvent.KilledTarget:
                string targetName = (string)args[0];
                if (targetName == "Giant Rat")
                {
                    if (Player.QuickHit.ComboCount >= TutorialComboCount)
                    {
                        switch (TutorialComboNeeded)
                        {
                            case 0:
                                {
                                    DWDataManager.ActivePlayerData.State = DWGameState.TutorialDone;
                                    DWDataManager.SaveConfig();
                                    FireGameState();
                                    DWDialogueSystem.Instance.PlayScene(DWData.TutorialEnd, false);
                                    break;
                                }
                            case 1:
                                {
                                    Core.BroadcastEvent("tutorial_combo_start", this, 1);
                                    DWDialogueSystem.Instance.PlayScene(DWData.TutorialComboSuccess2, false);
                                    break;
                                }
                            case 2:
                                {
                                    Core.BroadcastEvent("tutorial_combo_start", this, 1);
                                    DWDialogueSystem.Instance.PlayScene(DWData.TutorialComboSuccess1, false);
                                    break;
                                }
                        }
                        TutorialComboNeeded--;
                    }
                    else
                    {
                        //show dialog telling player how to chain combo
                        DWDialogueSystem.Instance.PlayScene(DWData.TutorialComboFail, false);
                        Core.BroadcastEvent("tutorial_combo_start", this, 1);
                    }      
                }
                break;
        }
    }
    void OnPause(object sender, object[] args)
    {

    }
    IEnumerator ExecuteWithDelay(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    void TurnOffStuff()
    {
        // --- Tutorial Triggers ---
        TutorialStartTrigger.gameObject.SetActive(false);
        tutorialEndTrigger.gameObject.SetActive(false);
        TutorialControlTrigger.gameObject.SetActive(false);
        tutorialEndTrigger.gameObject.SetActive(false);

        // --- Cave Triggers ---
        CaveTriggers.gameObject.SetActive(false);

        // --- Cliff Triggers --- 
        CliffTriggers.gameObject.SetActive(false);

        // --- Altar Triggers ---
        AltarTriggers.gameObject.SetActive(false);

        if (Player.ControllerCallback != null)
            Player.ControllerCallback -= OnPlayerActionCallback;
    }
    public void FireGameState()
    {
        TurnOffStuff();
        DWDataManager.LoadConfig();
        DWDataManager.SaveConfig();
        DWPlayerData pd = DWDataManager.ActivePlayerData;
        DWAudioManager.Instance.PlayMusic(DWMusicType.Altar, true);
        switch (pd.State)
        {
            case DWGameState.FirstPlay:
                {
                    DWWeapon[] weapons = new DWWeapon[]
                       {
                        Instantiate(DefaultSword),
                        Instantiate(DefaultSword)
                       };
                    Player.SetWeapons(weapons);
                    Player.ExecuteAction(DWEntityAction.SetTransform, FirstPlayLocation.position, FirstPlayLocation.rotation);
                    Core.BroadcastEvent(DWData.OnSetCameraTransform, this, FirstPlayCameraLocation.position, FirstPlayCameraLocation.rotation);
                    StartCoroutine(ExecuteWithDelay(() =>
                    {
                        DWDialogueSystem.Instance.PlayScene(DWData.IslandArrival);
                    }, 2));
                    TutorialStartTrigger.gameObject.SetActive(true);
                    TutorialControlTrigger.gameObject.SetActive(true);
                    TutorialSpawner.gameObject.SetActive(true);
                    Player.ControllerCallback += OnPlayerActionCallback;
                }
                break;
            case DWGameState.TutorialDone:
                {
                    DWWeapon[] weapons = new DWWeapon[]
                           {
                        Instantiate(DefaultSword),
                        Instantiate(DefaultSword)
                           };
                    Player.SetWeapons(weapons);
                    Player.ExecuteAction(DWEntityAction.SetTransform, RebornLocation.position, RebornLocation.rotation);
                    Core.BroadcastEvent(DWData.OnSetCameraTransform, this, RebornCameraLocation.position, RebornCameraLocation.rotation);
                    DWDialogueSystem.Instance.PlayScene("tutorial_end", true, ()=>
                    {
                        DWUIManager.Instance.Open(GameUI.RebornUI);
                    });
                    return;
                    //tutorialEndTrigger.gameObject.SetActive(true);
                }
                //break;
            case DWGameState.AcquiredForestSword:
                {
                    DWWeapon[] weapons = new DWWeapon[]
                    {
                        Instantiate(DefaultSword),
                        Instantiate(HolySword)
                    };
                    Player.SetWeapons(weapons);
                    CliffTriggers.gameObject.SetActive(false);
                }
                break;
            case DWGameState.AcquiredCaveSword:
                {
                    DWWeapon[] weapons = new DWWeapon[]
                    {
                        Instantiate(DarkSword),
                        Instantiate(DefaultSword)
                    };
                    Player.SetWeapons(weapons);
                    CaveTriggers.gameObject.SetActive(false);
                }
                break;
            case DWGameState.BothSword:
                {
                    DWWeapon[] weapons = new DWWeapon[]
                        {
                        Instantiate(DarkSword),
                        Instantiate(HolySword)
                        };
                    Player.SetWeapons(weapons);
                    AltarTriggers.gameObject.SetActive(true);
                }
                break;
        }

        if(pd.State != DWGameState.FirstPlay)
        {
            Player.ExecuteAction(DWEntityAction.SetTransform, RebornLocation.position, RebornLocation.rotation);
            Core.BroadcastEvent(DWData.OnSetCameraTransform, this, RebornCameraLocation.position, RebornCameraLocation.rotation);
            StartCoroutine(ExecuteWithDelay(() =>
            {
                DWUIManager.Instance.Open(GameUI.RebornUI);
            }, 2));
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
