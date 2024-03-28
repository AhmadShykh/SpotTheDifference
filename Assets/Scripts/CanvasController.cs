using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Threading.Tasks;


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

	[Header("Buttons")]
	[SerializeField] Button PlayButton;
	[SerializeField] Button BackButton;
	[SerializeField] Button HintButton;
	[SerializeField] Button SoundButton;
	[SerializeField] Button QuitButton;

	[Header("Labels")]
	[SerializeField] TextMeshProUGUI HintLabel;
	[SerializeField] TextMeshProUGUI ScoreLabel;

	[Header("Prefabs")]
	[SerializeField] GameObject HintPrefab;


	private int _gameCanvasOffset = 8;
	private int _maxShowHintTime = 3;

	// Other private members

	private static int _hintsAvalaible;



	// Experiment


	private void Awake()
	{
		//Setting Events and listeners
		PlayButton.onClick.AddListener(() => GameScreenBtnEvents(State.GameScreen));
		BackButton.onClick.AddListener(() => GameScreenBtnEvents(State.MainScreen));
		QuitButton.onClick.AddListener(QuitApplication);
		SoundButton.onClick.AddListener(ToggleSound);
		HintButton.onClick.AddListener(HintButtonSequence);
		GameManager.OnStateChangeAction += SwitchCanvas;
	}

	private void QuitApplication() => Application.Quit();

	private void OnDestroy()
	{
		GameManager.OnStateChangeAction -= SwitchCanvas;
	}
	private void Start()
	{
		// Setting Default Positions
		GameCanvas.transform.position = new Vector3(_gameCanvasOffset, GameCanvas.transform.position.y);
		//Other Tasks
		icon.sprite = PlayerPrefs.GetInt("Music") == 0 ? MusicIcon[0] : MusicIcon[1];
	}

	private void SwitchCanvas(State state)
	{
		if (state == State.MainScreen)
		{
			ScoreLabel.text = String.Format($"Score: {Scoring._score}");
			GameCanvas.transform.DOMoveX(_gameCanvasOffset, 1);
		}
		else if (state == State.GameScreen)
		{
			_hintsAvalaible = PlayerPrefs.GetInt("Hints", 3);
			GameCanvas.transform.DOMoveX(0, 1);
		}
			
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
			await Task.Delay(_maxShowHintTime*1000);
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
