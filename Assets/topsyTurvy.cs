using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class topsyTurvy : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
	public KMBombModule module;

	public KMSelectable button;
	public TextMesh[] screenTexts;
	public Color[] textColors;
	public Color solvedColor;

	private int displayIndex;
	private string correctWord;
	private int currentPos;
	private int solution;
	private List<string> decoyWords = new List<string>();
	private Coroutine cycle;

	private static readonly string[] displayWords = new string[16] { "Topsy", "Robot", "Cloud", "Round", "Quilt", "Found", "Plaid", "Curve", "Water", "Ovals", "Verse", "Sandy", "Frown", "Windy", "Curse", "Ghost" };
	private static readonly string[][] answerWords = new string[16][] {
		new string[5] { "Round", "Ghost", "Plaid", "Frown", "Cloud" },
		new string[5] { "Topsy", "Ghost", "Curse", "Sandy", "Frown" },
		new string[5] { "Plaid", "Verse", "Sandy", "Quilt", "Water" },
		new string[5] { "Found", "Water", "Ovals", "Sandy", "Frown" },
		new string[5] { "Cloud", "Topsy", "Windy", "Frown", "Ghost" },
		new string[5] { "Curse", "Curve", "Sandy", "Robot", "Water" },
		new string[5] { "Topsy", "Curve", "Ovals", "Round", "Cloud" },
		new string[5] { "Topsy", "Quilt", "Water", "Windy", "Curse" },
		new string[5] { "Verse", "Windy", "Plaid", "Found", "Quilt" },
		new string[5] { "Cloud", "Windy", "Topsy", "Robot", "Sandy" },
		new string[5] { "Topsy", "Ovals", "Water", "Curse", "Round" },
		new string[5] { "Ghost", "Frown", "Ovals", "Found", "Robot" },
		new string[5] { "Quilt", "Cloud", "Windy", "Curse", "Plaid" },
		new string[5] { "Robot", "Round", "Curse", "Topsy", "Frown" },
		new string[5] { "Ghost", "Sandy", "Verse", "Plaid", "Topsy" },
		new string[5] { "Robot", "Water", "Quilt", "Sandy", "Frown" }
	};

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved;

    void Awake()
    {
    	moduleId = moduleIdCounter++;
		button.OnInteract += delegate () { PressButton(); return false; };
		button.OnInteractEnded += delegate () { ReleaseButton(); };
    }

    void Start()
    {
		displayIndex = rnd.Range(0,16);
		Debug.LogFormat("[Topsy Turvy #{0}] The displayed word is {1}.", moduleId, displayWords[displayIndex].ToLowerInvariant());
		correctWord = answerWords[displayIndex].PickRandom();
		Debug.LogFormat("[Topsy Turvy #{0}] You need to submit {1}.", moduleId, correctWord.ToLowerInvariant());
		decoyWords = displayWords.Where(w => !answerWords[displayIndex].Contains(w) && w != displayWords[displayIndex]).ToList();
		decoyWords.Add(correctWord);
		decoyWords.Shuffle();
		solution = decoyWords.IndexOf(correctWord);
		screenTexts[0].text = displayWords[displayIndex];
		screenTexts[0].color = textColors.PickRandom();
		screenTexts[1].text = "";
    }

	void PressButton()
	{
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, button.transform);
		if (moduleSolved)
			return;
		cycle = StartCoroutine(CycleWords());
	}

	void ReleaseButton()
	{
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
		if (moduleSolved)
			return;
		StopCoroutine(cycle);
		var submitted = currentPos - 1;
		if (submitted != solution)
		{
			module.HandleStrike();
			Debug.LogFormat("[Topsy Turvy #{0}] You released the button when the displayed word was {1}. That was incorrect. Strike!", moduleId, decoyWords[submitted].ToLowerInvariant());
			Debug.LogFormat("[Topsy Turvy #{0}] Resetting...", moduleId);
			Start();
		}
		else
		{
			moduleSolved = true;
			StartCoroutine(Solve());
			Debug.LogFormat("[Topsy Turvy #{0}] You released the button when the displayed word was {1}. That was correct. Module solved!", moduleId, correctWord.ToLowerInvariant());
		}
	}

	IEnumerator CycleWords()
	{
		var count = decoyWords.Count();
		screenTexts[0].text = "";
		currentPos = 0;
		while (true)
		{
			screenTexts[1].color = textColors.PickRandom();
			screenTexts[1].text = decoyWords[currentPos];
			currentPos = (currentPos + 1) % count;
			yield return new WaitForSeconds(1f);
		}
	}

	IEnumerator Solve()
	{
		var messages = new string[3][]
		{
			new string[2] { "Good", "Job" },
			new string[2] { "Nice", "Work" },
			new string[2] { "We're", "Done" }
		};
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				screenTexts[j].text = messages[i][j];
				screenTexts[j].color = textColors.PickRandom();
				yield return new WaitForSeconds(.25f);
			}
		}
		yield return new WaitForSeconds(1f);
		foreach (TextMesh screenText in screenTexts)
			screenText.color = solvedColor;
		module.HandlePass();
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
	}
}
