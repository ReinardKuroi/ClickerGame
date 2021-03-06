﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Login : MonoBehaviour {

	public TextMeshProUGUI welcomeText;
	public GameObject continueButton;
	public GameObject loginButton;

	public void Awake () {
		OpenLoginMenu ();
		SoundManager.Instance.LevelMusic ("menu");
	}

	public void OpenLoginMenu () {
		Debug.Log ("Opened 'Login' Menu.");
		int activePlayer = GlobalData.Instance.LastActivePlayer;
		if (activePlayer == -1) {
			continueButton.GetComponent<Button> ().interactable = false;
			loginButton.GetComponent<Button> ().interactable = false;
			//no activeplayer found
			//can only create new
			//disable load button
			//disable continue button
			Debug.Log ("No active player found.");
			welcomeText.text = "WELCOME";
		} else {
			GlobalData.Instance.LastActivePlayer = activePlayer;
			PlayerData playerData = GlobalData.Instance.ActivePlayerData;
			Debug.Log ("Got active player " + playerData.name);
			welcomeText.text = playerData.name ;
			SoundManager.Instance.SetVolume ();
			continueButton.GetComponent<Button> ().interactable = true;
			loginButton.GetComponent<Button> ().interactable = true;
			//set text to welcome, playername
			//enable continue button
		}
	}

	public void OpenNewMenu () {
		Debug.Log ("Opened 'New' Menu.");
		//nothing?
	}

	public void OpenLoadMenu () {
		Debug.Log ("Opened 'Load' Menu.");
		//initialize button selector or something
		//like in button generator for levels
	}

	public void Continue () {
		Debug.Log ("Pressed 'Continue'.");
		LoadManager.Instance.LoadScene ("MainMenu");
	}
		
	public void Quit () {

		GameManager.Instance.Quit ();

		Application.Quit ();
	}

	public void Load () {
		Debug.Log ("Pressed 'Load'.");
		LoadManager.Instance.LoadScene ("MainMenu");
	}

	public void Create (TMP_InputField input) {
		Debug.Log ("Pressed 'New'.");
		string playerName = input.text;
		if (playerName.Length > 0) {
			GlobalData.Instance.CreateNewPlayerData (playerName);
			SoundManager.Instance.SetVolume ();
			LoadManager.Instance.LoadScene ("MainMenu");
		} else {
			Debug.Log ("Player name must not be empty.");
		}
	}
}