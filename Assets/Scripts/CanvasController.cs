using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Threading.Tasks;
using UnityEditor.Search;
using UnityEditor.Tilemaps;
using Unity.VisualScripting;

//made by mistake 

public class CanvasController : MonoBehaviour
{

	[Header("Management Scripts")]
	[SerializeField] ColorDifferenceScript _colorDiffScript;

	[Header("Sound Button")]
	[SerializeField] Sprite[] MusicIcon;
	[SerializeField] Image icon;


	[Header("Canvas Options")]
	[SerializeField] GameObject MainCanvas;
    [SerializeField] GameObject OptionsCanvas;
    [SerializeField] GameObject GameCanvas;

	private int _gameCanvasOffset = 8;

	[Header("Buttons")]
	[SerializeField] Button PlayButton;
	[SerializeField] Button BackButton;
	[SerializeField] Button HintButton;

	[Header("Labels")]
	[SerializeField] TextMeshProUGUI HintLabel;

	[Header("Prefabs")]
	[SerializeField] GameObject HintPrefab;


	// Other private members

	private static int _hintsAvalaible;


	// Experiment
	[SerializeField] GameObject obj;
 
	private void Start()
	{
		//Setting Game Object State
		MainCanvas.SetActive(true);
		OptionsCanvas.SetActive(false);
		GameCanvas.transform.position -= _gameCanvasOffset * Vector3.left;

		//Setting Events 
		PlayButton.onClick.AddListener(() => GameScreenBtnEvents(State.GameScreen));
		BackButton.onClick.AddListener(() => GameScreenBtnEvents(State.MainScreen));
		GameManager.OnStateChangeAction += SwitchCanvas;
		HintButton.onClick.AddListener(HintButtonSequence);

		//Other Tasks
		icon.sprite = PlayerPrefs.GetInt("Music") == 0 ? MusicIcon[0] : MusicIcon[1];
		_hintsAvalaible = PlayerPrefs.GetInt("Hints", 3);
	}

	private void SwitchCanvas(State state)
	{
		if(state == State.MainScreen )
			GameCanvas.transform.position -= _gameCanvasOffset * Vector3.left;
		else if (state == State.GameScreen)
			GameCanvas.transform.position += _gameCanvasOffset * Vector3.left;
	}

	private async void HintButtonSequence()
	{
		if (HintButton.interactable)
			await ShowHint();
		
	}

	private async Task ShowHint()
	{
		HintButton.interactable = false;

		GameObject obj = null;
		if (_hintsAvalaible <= 0)
		{
			float animTime = 1f;
			HintLabel.rectTransform.DOAnchorPosY(HintLabel.rectTransform.anchoredPosition.y - 175, animTime);
			await Task.Delay(2000);
			HintLabel.rectTransform.DOAnchorPosY(HintLabel.rectTransform.anchoredPosition.y + 175, animTime);
			await Task.Delay((int)animTime*1000);
		}	
		else
		{
			_hintsAvalaible--;
			PlayerPrefs.SetInt("Hints", _hintsAvalaible);
			obj = _colorDiffScript.InitiateHintSequence(HintPrefab);
			await Task.Delay(3000);
			Destroy(obj);
		}
	
		HintButton.interactable = true;
	}

	private void GameScreenBtnEvents(State state)
	{
		GameManager.instance.UpdateGameState(state);
	}

	public void ToggleOptionsCanva() => OptionsCanvas.SetActive(!OptionsCanvas.activeSelf);

	public void ToggleSound()
	{
		SoundState currentState = SoundManager.instance._currentState;
		icon.sprite = currentState == 0 ? MusicIcon[1] : MusicIcon[0];
		SoundManager.instance.UpdateSoundState((SoundState)(((int)currentState) ^ 1));
	}



	async Task HintTextAnimation()
	{
		
		await Task.Delay(3);
		
	}

}
