using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEditor.SceneTemplate;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
	public static Action<State> OnStateChangeAction;
	public State _currentState;

	//------------------------Serealized Fields----------------------

	[SerializeField] AdManager _adManager;

	private GameManager()
	{

	}
	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			UpdateGameState(State.MainScreen);
			DontDestroyOnLoad(this);
		}
		else
			Destroy(this);
	}

	public void UpdateGameState(State newState)
	{

		_currentState = newState;
		switch (newState)
		{
			case State.MainScreen:
				StartMainScreenSequnence();
				break;
			case State.GameScreen:
				StartGameScreenSequnence();
				break;
			case State.InterstitialAd:
				StartInterstitialAdSequence();
				break;
		}

		OnStateChangeAction?.Invoke(_currentState);
	}

	private void StartInterstitialAdSequence()
	{
		_adManager.LoadInterstitialAd();
		_adManager.ShowInterstitialAd();
	}

	private void StartMainScreenSequnence()
	{
		
	}

	private void StartGameScreenSequnence()
	{
		
	}
}

public enum State
{
	MainScreen,
	GameScreen,
	InterstitialAd
}

