using UnityEngine;
using System;


public enum State
{
	MainScreen,
	GameScreen,
	InterstitialAd
}


public class GameManager : MonoBehaviour
{
    public static GameManager instance;
	public static event Action<State> OnStateChangeAction;
	public State _currentState;

	//------------------------Serealized Fields----------------------

	[SerializeField] AdManager _adManager;

	private GameManager()
	{

	}
	private void Awake()
	{
		
		instance = this;
		
	}

	private void Start()
	{
		UpdateGameState(State.MainScreen);
	}

	public void UpdateGameState(State newState)
	{
		_currentState = newState;
		switch (newState)
		{
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



	public void ShowBanner()
	{
		_adManager.LoadBannerAd();
	}

}


