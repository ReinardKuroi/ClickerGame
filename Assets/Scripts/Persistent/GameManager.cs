﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;

public class GameManager : MonoBehaviour {

	[HideInInspector]
	public FSM.Process game;
	[HideInInspector]
	public ScoreController scoreController;

	private InputManager inputManager;
	private Subject achievementObserver;
	private Resolution _res;

	//Singleton
	public static GameManager Instance { get; private set; }
	void Awake () {
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad (gameObject);
		} else {
			Destroy (gameObject);
		}
		if (game == null)
			game = new FSM.Process ();
		if (inputManager == null)
			inputManager = new InputManager ();
		if (achievementObserver == null) {
			achievementObserver = new Subject ();
		}
		_res = new Resolution (Screen.width, Screen.height);
		Debug.Log (_res.height + " X " + _res.width);
	}

	public int Score {
		get { return scoreController.Score; }
	}

	public int Multiplier {
		get { return scoreController.Multiplier; }
	}

	public float Weightbar {
		get { return scoreController.Weightbar; }
	}

	//highscore code

	public void SetHighscore () {
		if (Score > GlobalData.Instance.Highscore)
			GlobalData.Instance.Highscore = Score;
	}

	//achievement

	public void CheckAchievement (AchievementData achievement) {
		PlayerData player = GlobalData.Instance.ActivePlayerData;
		LevelData level = GlobalData.Instance.ActiveLevelData;

		if (player.unlockedAchievements.Find (item => item.achievementName == achievement.achievementName).isUnlocked) {
			//skip
		} else {

			//if this achievement is unlocked -- do not check
			if ((achievement.levelRestriction == level.levelName) || (achievement.levelRestriction == "none")) {

				switch (achievement.triggerName) {
				case "Score":
					{
						if (Score >= achievement.triggerValue) {
							UnlockAchievement (achievement);
						}
						break;}
				case "Time":
					{
						if (achievement.levelRestriction == "none") {
							float time = player.PlayTime;
							if (time >= achievement.triggerValue) {
								UnlockAchievement (achievement);
							}
						} else if (player.scoreData.Find(item => item.levelName == level.levelName).playTime >= achievement.triggerValue) {
							UnlockAchievement (achievement);
						}
						break;}
				case "Playcount":
					{
						if (achievement.levelRestriction == "none") {
							int count = player.PlayCount;
							if (count >= achievement.triggerValue) {
								UnlockAchievement (achievement);
							}
						} else if (player.scoreData.Find (item => item.levelName == level.levelName).playCount >= achievement.triggerValue) {
							UnlockAchievement (achievement);
						}
						break;}
				case "Multi":
					{
						if (Multiplier >= achievement.triggerValue) {
							UnlockAchievement (achievement);
						}
						break;}
				case "Totalscore":
					{
						if (player.totalScore >= achievement.triggerValue)
							UnlockAchievement (achievement);
						break;}
				default:
					{break;}
				}
			}
		}
	}

	public void UnlockAchievement (AchievementData achievement) {
		PlayerData playerData = GlobalData.Instance.ActivePlayerData;
		playerData.unlockedAchievements.Find (item => item.achievementName == achievement.achievementName).isUnlocked = true;
		if (achievement.unlocks == "none") {
			//skip
		} else {
			GlobalData.Instance.UnlockLevel (achievement.unlocks);
		}
		GlobalData.Instance.ActivePlayerData = playerData;
		GlobalData.Instance.SaveGameData ();
		Debug.Log ("Achievement unlocked: " + achievement.achievementName);
	}

	//public actions for game state

	public void Pause () {
		if (game.MoveNext (FSM.Action.Pause) == FSM.State.Paused) {
			Debug.Log ("Game paused.");
		}
	}

	public void Resume () {
		if (game.MoveNext (FSM.Action.Resume) == FSM.State.Active) {
			Debug.Log ("Game resumed.");
		}
	}

	public void Play () {
		if (game.MoveNext (FSM.Action.Begin) == FSM.State.Active) {
			if (scoreController == null)
				scoreController = gameObject.AddComponent<ScoreController> () as ScoreController;
			else
				scoreController = gameObject.GetComponent<ScoreController> ();
			scoreController.Set ();
			achievementObserver = new Subject ();
			foreach (AchievementData data in GlobalData.Instance.Achievements)
				achievementObserver.AddObserver (new Achievement (data));
			SoundManager.Instance.StopMusic ();
			SoundManager.Instance.LevelMusic (GlobalData.Instance.ActiveLevelData.levelName);
			GlobalData.Instance.ActivePlayerData.scoreData.Find (item => item.levelName == GlobalData.Instance.ActiveLevelData.levelName).playCount += 1;
			Debug.Log ("Game started.");
		}
	}

	public void Stop () {
		if (game.MoveNext (FSM.Action.End) == FSM.State.Inactive) {
			if (scoreController)
				Destroy (scoreController);
			if (achievementObserver != null)
				achievementObserver = null;
			GlobalData.Instance.ActivePlayerData.totalScore += Score;
			GlobalData.Instance.SaveGameData ();
			SoundManager.Instance.StopMusic ();
			SoundManager.Instance.LevelMusic ("menu");
			Debug.Log ("Game ended.");
		}
	}

	public void Quit () {
		if (game.MoveNext (FSM.Action.Exit) == FSM.State.Terminated) {
			GlobalData.Instance.SaveGameData ();
			Debug.Log ("Exiting game.");
		}
	}

	public FSM.State GetState () {
		return game.CurrentState;
	}

	//cycle

	void Update () {
		if (game.CurrentState == FSM.State.Active) {
			inputManager.HandleInput ();
			achievementObserver.Notify ();
			GlobalData.Instance.ActivePlayerData.scoreData.Find(item => item.levelName == GlobalData.Instance.ActiveLevelData.levelName).playTime += Time.deltaTime;
		}
		Resolution res = new Resolution (Screen.width, Screen.height);
		if (res != _res) {
			Debug.Log ("Resolution changed");
			foreach (Camera camera in GameObject.FindObjectsOfType<Camera> ()) {
				GameObject g =	camera.gameObject;
				FixedAspect script = g.GetComponent<FixedAspect> ();
				script.UpdateCrop ();
			}
			_res = res;
		}
	}

	private class Achievement : Observer {

		private delegate void DelegateVoid (AchievementData data);
		private DelegateVoid achievementCheck;
		private AchievementData achievementData;

		public Achievement (AchievementData data) {
			achievementCheck = GameManager.Instance.CheckAchievement;
			this.achievementData = data;
		}

		public override void OnNotify () {
			achievementCheck (achievementData);
		}
	}

	private struct Resolution {
		public int width;
		public int height;
		public Resolution (int width, int height) {
			this.width = width;
			this.height = height;
		}
		public static bool operator ==(Resolution r, Resolution _r) {
			if ((r.width == _r.width) && (r.height == _r.height))
				return true;
			else return false;
		}
		public static bool operator !=(Resolution r, Resolution _r) {
			if ((r.width == _r.width) && (r.height == _r.height))
				return false;
			else return true;
		}
	}
}