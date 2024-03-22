using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Threading.Tasks;

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
		PlayButton.onClick.AddListener(() => GameScreenBtnEvents(State.GameScreen, _gameCanvasOffset));
		BackButton.onClick.AddListener(() => GameScreenBtnEvents(State.MainScreen, -_gameCanvasOffset));
		HintButton.onClick.AddListener(HintButtonSequence);

		//Other Tasks
		icon.sprite = PlayerPrefs.GetInt("Music") == 0 ? MusicIcon[0] : MusicIcon[1];
		_hintsAvalaible = PlayerPrefs.GetInt("Hints", 3);
	}

	private void HintButtonSequence()
	{
		if (_hintsAvalaible <= 0)
			HintTextAnimation();
		else
		{
			_hintsAvalaible--;
			_colorDiffScript.InitiateHintSequence(HintPrefab);
		}
	}

	private void GameScreenBtnEvents(State state, int multiplier)
	{
		GameCanvas.transform.position += multiplier * Vector3.left ;
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
		HintLabel.rectTransform.DOAnchorPosY(HintLabel.rectTransform.anchoredPosition.y - 10, 1f);
		await Task.Delay(3);
		HintLabel.rectTransform.DOAnchorPosY(HintLabel.rectTransform.anchoredPosition.y + 10, 1f);
	}

}
