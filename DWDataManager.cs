using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

public static class DWDataManager
{
    static string ConfigFilePath { get { return Application.persistentDataPath /*Application.dataPath*/ + "/dwconfig.xml"; } } //file path
    static Dictionary<Guid, DWPlayerData> _players = new Dictionary<Guid, DWPlayerData>(); // list of current players
    public static Guid ActivePlayerID { private set; get; } // current active player ID

    // Player list converted to an array for ease of access through the project
    public static DWPlayerData[] Players 
    {
        get { return _players.Values.ToArray(); }
    }

    //Active player data acquired from ID
    public static DWPlayerData ActivePlayerData
    {
        get {
            if (_players.ContainsKey(ActivePlayerID))
                return _players[ActivePlayerID];
            else
                return null;
        }
    }
    public static float SFXVolume = 1;
    public static float MusicVolume = 1;

    //Save Config
    public static void SaveConfig()
    {
        DWGameConfig config = new DWGameConfig();
        config.Players = _players.Values.ToArray();
        config.MusicVolume = MusicVolume;
        config.SFXVolume = SFXVolume;
        config.LastPlayerID = ActivePlayerID;
        XmlSerializer serializer = new XmlSerializer(typeof(DWGameConfig));
        using (FileStream stream = new FileStream(ConfigFilePath, FileMode.Create))
        {
            serializer.Serialize(stream, config);
        }
    }

    //Load Config
    public static void LoadConfig()
    {
        _players.Clear();
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                DWGameConfig config;
                XmlSerializer serializer = new XmlSerializer(typeof(DWGameConfig));
                using (FileStream stream = new FileStream(ConfigFilePath, FileMode.Open))
                {
                    config = serializer.Deserialize(stream) as DWGameConfig;
                }
                foreach (DWPlayerData p in config.Players)
                {
                    _players[p.ID] = p;
                }
                SetActivePlayer(config.LastPlayerID);
                MusicVolume = config.MusicVolume;
                SFXVolume = config.SFXVolume;
                //TODO broadcast the load config event
                return;
            }
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }        
        //else
        DWPlayerData pd = CreateNewPlayer("Damon", 1, 1, 1);
        SetActivePlayer(pd.ID);        
    }

    //Create New Player Data
    public static DWPlayerData CreateNewPlayer(string name, int strength, int agility,int vitality)
    {
        DWPlayerData player = new DWPlayerData() { ID = Guid.NewGuid(),
                                            Name = name,
                                            Strength = strength,
                                            Agility = agility,
                                            Vitality = vitality };
        _players[player.ID] = player;
        ActivePlayerID = player.ID;
        return player;
    }

    //Delete specifid player data
    public static void DeletePlayer(Guid playerID)
    {
        _players.Remove(playerID);
        if(ActivePlayerID == playerID)
        {            
            if(_players.Count > 0)
            {
                ActivePlayerID = _players.Keys.ToArray()[0];
            }
            else
            {
                ActivePlayerID = Guid.Empty;
            }
        }
    }

    //Set as active player
    public static bool SetActivePlayer(Guid playerID)
    {
        if (_players.ContainsKey(playerID))
        {
            ActivePlayerID = playerID;
            return true;
        }
        else
            return false;
    }
}

//Game Properties
[Serializable]
public class DWGameConfig
{
    public DWPlayerData[] Players;
    public Guid LastPlayerID;
    public float MusicVolume;
    public float SFXVolume;
}

//Player properties
[Serializable]
public class DWPlayerData
{
    public Guid ID;
    public string Name;
    public int Level = 1;
    public int Vitality;
    public int Agility;
    public int Strength;
    public DWGameState State = DWGameState.FirstPlay;
}

//Game State -- objects will be spawned based on game state, used for progression
public enum DWGameState
{
    FirstPlay,
    TutorialDone,
    AcquiredForestSword,
    AcquiredCaveSword,
    BothSword
}