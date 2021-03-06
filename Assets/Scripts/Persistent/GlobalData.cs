﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Shit to do:
//Remake button generator and apply to login screen and level selection: DONE
//remake loader so it actually works: DONE
//make a game engine : DONE
//add a delete player button to settings? : DONE
//add more states, e.g. Transition and Loading : No need
//Quit button for login screen? : DONE
//Make login screen fancier : DONE
//implement highscore system : DONE
//Start working on achievements system : DONE
//make loader fancier : DONE

//Modify level data to include music : NOPE
//Modify player data to include selected music : NOPE
//Add cool looking effects with wumbers on click : NOPE
//Add level intermezzo transition : NOPE

public class GlobalData : MonoBehaviour {

	public static GlobalData Instance { get; private set; }

	private List<LevelData> allLevelData;
	private List<AchievementData> allAchievementData;
	public List<PlayerData> allPlayerData;

	private int activePlayer;

	public static string levelDataFilename = "level.json";
	public static string achievementDataFilename = "achievement.json";
	public static string playerDataFilename = "player.json";

	public static string exposedMusicVolume = "MusicVolumeControl";
	public static string exposedSFXVolume = "SFXVolumeControl";

	void Awake () {
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad (gameObject);
		} else {
			Destroy (gameObject);
		}
		LoadGameData ();
	}

	//Active level get;set and unlock

	public LevelData ActiveLevelData {
		get {
			return allLevelData [allPlayerData[activePlayer].activeLevel];
		}
	}

	public int ActiveLevelIndex {
		get {
			return allPlayerData [activePlayer].activeLevel;
		}
		set {
			allPlayerData [activePlayer].activeLevel = value;
			SaveLoad.SaveToPersistent (ref allPlayerData, playerDataFilename);			
		}
	}

	//Sets bool in list unlockedLevels by name
	//in a list of all PlayerData by index activePlayer

	public void UnlockLevel (string name) {
		ScoreData scoreData = ActivePlayerData.scoreData.Find (item => item.levelName == name);
		scoreData.isUnlocked = true;
		string levelName = allLevelData.Find (item => item.levelName == name).levelShowName;
		GameManager.Instance.PushNotification (levelName);
		SaveLoad.SaveToPersistent (ref allPlayerData, playerDataFilename);
	}

	//Active player get;set and save

	public PlayerData ActivePlayerData {
		get {
			if (activePlayer == -1)
				return new PlayerData ();
			else
				return allPlayerData [activePlayer];
		}
		set {
			allPlayerData [activePlayer] = value;
			SaveLoad.SaveToPersistent (ref allPlayerData, playerDataFilename);
		}
	}

	//New player

	public void CreateNewPlayerData (string playerName) {
		PlayerData playerData = new PlayerData ();
		for (int i = 0; i < allLevelData.Count; i++) {
			LevelData levelData = allLevelData [i];
			playerData.scoreData.Add (new ScoreData (levelData) {
				index = i,
				isUnlocked = false,
			});
		}
		foreach (AchievementData data in allAchievementData) {
			playerData.unlockedAchievements.Add (new UnlockedAchievement (data.achievementName));
		}
		playerData.name = playerName;
		allPlayerData.Insert (0, playerData);
		LastActivePlayer = 0;
		if (playerName == "B") {
			foreach (ScoreData data in playerData.scoreData) {
				data.isUnlocked = true;
			}
		} else
			UnlockLevel ("Common");
	}

	//Delete player

	public void DeletePlayerData (int index) {
		Debug.Log (0);
		if (allPlayerData [index] != null) {
			Debug.Log (1);
			allPlayerData.RemoveAt (index);
			Debug.Log (2);
			LastActivePlayer = 0;
			Debug.Log (3);
		}
	}

	//LastActivePLayer

	public int LastActivePlayer {
		get {
			if (allPlayerData.Count != 0)
				return allPlayerData.FindIndex (item => item.isActive);
			else
				return -1;
		}
		set {
			if (allPlayerData.Count != 0) {
				foreach (PlayerData playerData in allPlayerData) {
					playerData.isActive = false;
				}
				activePlayer = value;
				allPlayerData [value].isActive = true;
			}
			SaveLoad.SaveToPersistent (ref allPlayerData, playerDataFilename);
		}
	}

	//Hghscore

	public int Highscore {
		get {
			ScoreData scoreData = ActivePlayerData.scoreData.Find (item => item.levelName == ActiveLevelData.levelName);
			int highscore = scoreData.highscore;
			return highscore;
		}
		set {
			int index = ActivePlayerData.scoreData.FindIndex (item => item.levelName == ActiveLevelData.levelName);
			ActivePlayerData.scoreData [index].highscore = value;
		}
	}

	//Achievement

	public List<AchievementData> Achievements {
		get {
			return allAchievementData;
		}
	}

	//SaveLoad

	public void ResetPlayerData () {
		allPlayerData.Clear ();
		allPlayerData = new List<PlayerData> ();
		SaveLoad.SaveToPersistent (ref allPlayerData, playerDataFilename);
	}

	public void LoadGameData () {
		SaveLoad.LoadFromAssets (ref allLevelData, levelDataFilename);
		SaveLoad.LoadFromAssets (ref allAchievementData, achievementDataFilename);
		SaveLoad.LoadFromPersistent (ref allPlayerData, playerDataFilename);
	}

	public void SaveGameData () {
		SaveLoad.SaveToPersistent (ref allPlayerData, playerDataFilename);
	}
}

[System.Serializable]
public class PlayerData {
	public string name;
	public bool isActive;
	public int activeLevel;
	public List<ScoreData> scoreData;
	public List<UnlockedAchievement> unlockedAchievements;
	public List<AudioSettings> audioSettings;
	public int totalScore;

	public PlayerData () {
		this.name = "";
		this.isActive = false;
		this.activeLevel = 0;
		this.scoreData = new List<ScoreData> ();
		this.unlockedAchievements = new List<UnlockedAchievement> ();
		this.audioSettings = new List<AudioSettings> {
			new AudioSettings (GlobalData.exposedMusicVolume),
			new AudioSettings (GlobalData.exposedSFXVolume)
		};
		this.totalScore = 0;
	}
	[SerializeField]
	public float PlayTime {
		get {
			float time = 0f;
			foreach (ScoreData data in this.scoreData) {
				time += data.playTime;
			}
			return time;
		}
	}
	[SerializeField]
	public int PlayCount {
		get {
			int count = 0;
			foreach (ScoreData data in this.scoreData) {
				count += data.playCount;
			}
			return count;
		}
	}
}

[System.Serializable]
public class AchievementData {
	public string achievementName;
	public string levelRestriction;
	public string triggerName;
	public int triggerValue;
	public string unlocks;
	public string description;
	public string achievementID;

	AchievementData () {
		this.achievementName = "";
		this.levelRestriction = "";
		this.triggerName = "";
		this.triggerValue = 0;
		this.unlocks = "none";
		this.description = "";
		this.achievementID = "";
	}
}

[System.Serializable]
public class LevelData {
	public string levelName;
	public int transitionSpeed;
	public string levelShowName;

	public int multiplierLimit;
	public float multiplierDynamic;
	public float clickDecay;
	public float clickWeight;
	public int critChance;
	public int critMultiplier;

	public LevelData () {
		this.levelName = "";
		this.transitionSpeed = 0;
		this.levelShowName = "";
		this.multiplierLimit = 0;
		this.multiplierDynamic = 0;
		this.clickDecay = 0;
		this.clickWeight = 0;
		this.critChance = 0;
		this.critMultiplier = 0;
	}
}

[System.Serializable]
public class AudioSettings {
	public string name;
	public float volume;
	public bool enabled;

	public AudioSettings (string name) {
		this.name = name;
		this.volume = 0;
		this.enabled = true;
	}
}

[System.Serializable]
public class UnlockedAchievement {
	public string achievementName;
	public bool isUnlocked;

	public UnlockedAchievement (string name) {
		this.achievementName = name;
		this.isUnlocked = false;
	}
}

[System.Serializable]
public class ScoreData {
	public string levelName;
	public string showName;
	public bool isUnlocked;
	public int index;
	public int highscore;
	public float playTime;
	public int playCount;
//	public int floorCount;

	public ScoreData (LevelData levelData) {
		this.levelName = levelData.levelName;
		this.showName = levelData.levelShowName;
		this.isUnlocked = false;
		this.index = 0;
		this.highscore = 0;
		this.playTime = 0f;
		this.playCount = 0;
//		this.floorCount = 0;
	}
}