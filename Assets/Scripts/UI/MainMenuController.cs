﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour {

	void Start () {
		//ADD INITIALIZATION I DONT CARE HOW OR WHY FUCK YOU FUTURE ME

		//greeting text or something
		//like, show player name and stats
		Debug.Log("Player #" + GlobalData.Instance.LastActivePlayer.ToString () + ", name: " + GlobalData.Instance.ActivePlayerData.name);
	}

	public void Quit () {

		GameManager.Instance.Quit ();

		Application.Quit ();
	}

	public void Reset () {
		GlobalData.Instance.DeletePlayerData (GlobalData.Instance.LastActivePlayer);
		LoadManager.Instance.LoadScene ("Login");
	}

	public void Play () {
		LevelData levelData = GlobalData.Instance.ActiveLevelData;
		GameManager.Instance.Play ();
		LoadManager.Instance.LoadScene (levelData.levelName);
	}

	public void Logout () {
		LoadManager.Instance.LoadScene ("Login");
	}
}
