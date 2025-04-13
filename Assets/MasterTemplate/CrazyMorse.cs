using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class CrazyMorse : MonoBehaviour {
	public KMBombInfo Bomb;
	public KMAudio Audio;
	
	static int ModuleIdCounter = 1;
	int ModuleId;
	private bool ModuleSolved;

	public KMSelectable button;
	public TextMesh text;

	const float BETWEEN_ITERATIONS_TIME = 0.25f;
	const float BETWEEN_LETTERS_TIME = 0.75f;
	const float BETWEEN_CYCLES_TIME = 1.5f;
	readonly string[] TEXTS = new string[] { "Dot", "Dash", "Dit", "Dah", ".", "-", "▄", "▄▄▄" };
	readonly float[] TIMES = new float[] { 0.25f, 0.75f };

	struct CycleIteration {
		public string Text;
		public float Time;

		public CycleIteration(string text, float time) {
			Text = text;
			Time = time;
		}
	}

	CycleIteration[] cycle = new CycleIteration[8];

	void Awake () {
		ModuleId = ModuleIdCounter++;
		/*
		foreach (KMSelectable object in keypad) {
			object.OnInteract += delegate () { keypadPress(object); return false; };
		}
		*/
		
		//button.OnInteract += delegate () { buttonPress(); return false; };
		
	}
	
	void Start () {
		for (int i = 0; i < 8; i++) {
			cycle[i] = new CycleIteration(TEXTS[Rnd.Range(0, 7)], TIMES[Rnd.Range(0, 2)]);
		}
		StartCoroutine(CycleCoroutine());
	}
	
	void Update () {
		
	}

	IEnumerator CycleCoroutine() {
		while (true) {
			for (int i = 0; i < cycle.Length; i++) {
				yield return new WaitForSeconds(ProcessIteration(cycle[i]));
				text.text = "";
				yield return new WaitForSeconds(BETWEEN_ITERATIONS_TIME);
			}
			yield return new WaitForSeconds(BETWEEN_CYCLES_TIME);
		}
	}

	float ProcessIteration(CycleIteration iteration) {
		text.text = iteration.Text;
		return iteration.Time;
	}

#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414
	
	IEnumerator ProcessTwitchCommand (string Command) {
		yield return null;
	}
	
	IEnumerator TwitchHandleForcedSolve () {
		yield return null;
	}
}
