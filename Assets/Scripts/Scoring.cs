using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Scoring 
{
    public static int _score { get => PlayerPrefs.GetInt("Score",0); set => PlayerPrefs.SetInt("Score", value); }
    
}
