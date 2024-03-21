using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class SoundManager : MonoBehaviour
{
	[SerializeField] AudioMixerGroup MusicGroup;

	public static SoundManager instance;
	public static Action<State> OnStateChangeAction;
	public SoundState _currentState {
		get {
			return PlayerPrefs.GetInt("Music", 0) == 0 ? SoundState.MusicOn : SoundState.MusicOff;
		}
		private set
		{
			int musicVol = (value == SoundState.MusicOn) ? 0 : -80;
			PlayerPrefs.SetInt("Music", musicVol);
			MusicGroup.audioMixer.SetFloat("MusicVolume", musicVol);
		}
	}

		
	private SoundManager()
	{

	}

	private void Start()
	{
		if (instance == null)
		{
			instance = this;
			_currentState = PlayerPrefs.GetInt("Music", 0) == 0 ? SoundState.MusicOn : SoundState.MusicOff;
			DontDestroyOnLoad(this);
		}
		else
			Destroy(this);
	}

	public void UpdateSoundState(SoundState newState)
	{
		_currentState = newState;
	}
}


public enum SoundState
{
	MusicOn,
	MusicOff,
}