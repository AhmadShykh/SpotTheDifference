using System;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Purchasing;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class LibraryScript : MonoBehaviour
{
    [Header("Library Image Settings")]
    [SerializeField] Button _leftBtn;
    [SerializeField] Button _rightBtn;
    [SerializeField] Image _image;
    [SerializeField] TextMeshProUGUI _msgLbl;

    private int _imageIndex = -1;

    string[] fileInfo;

	private void Awake()
	{
		_leftBtn.onClick.AddListener(() => ShowImageSeqeuence(false));
		_rightBtn.onClick.AddListener(() => ShowImageSeqeuence(true));
	}

	private void ShowImageSeqeuence(bool v)
	{
		//ImageDestroySequence();

		_leftBtn.interactable = true;
		_rightBtn.interactable = true;

		ApplyImage(v);
		if (_imageIndex == 0)
			_leftBtn.interactable = false;
		if (_imageIndex == fileInfo.Length - 1)
			_rightBtn.interactable = false;
		Resources.UnloadUnusedAssets();
	}


	private void OnEnable()
	{
		fileInfo = PlayerPrefs.GetString("Library", "").Split(";");
		if (fileInfo[0] == "")
		{
			_rightBtn.interactable = _leftBtn.interactable = false;
			_msgLbl.enabled = true;
			_msgLbl.text = "No Images Available";
		}
		else
		{
			_msgLbl.enabled = false;
			_msgLbl.text = "";
			ShowImageSeqeuence(true);
		}
			
	}


	private void ApplyImage(bool right) // true = go right, false = go left
	{
        if (right)
            _imageIndex++;
        else
			_imageIndex--;

		AsyncOperationHandle<Texture2D> textureAsync = Addressables.LoadAssetAsync<Texture2D>(fileInfo[_imageIndex]);
		textureAsync.WaitForCompletion();
		Texture2D imageTexture = textureAsync.Result;

		Texture2D myTexture = Instantiate(imageTexture) as Texture2D;
		Sprite sprite = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height), Vector2.one * 0.5f);
		_image.sprite = sprite;

		Addressables.Release(textureAsync);

	}


	void ImageDestroySequence()
    {
		if (_image.sprite != null)
		{
			// Deleting Both because I am creating both new for an image
			Destroy(_image.sprite.texture);
			Destroy(_image.sprite);
		}
	}
}
