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

	public KMSelectable Button;
	public TextMesh Text;

	private const float TIME_LONG = 1.5f;
	private const float TIME_SHORT = 0.5f;
	private const float TIME_BETWEEN_SYMBOLS = 0.5f;
	private const float TIME_AFFIXES = 3f;
	private const float TIME_SPACES = 1.5f;

	private class Symbol {
		public string Text;
		public float Time;

		public Symbol(string text, float time) {
			Text = text;
			Time = time;
		}
	}
	private class Space : Symbol {
		public int Shift;

		public Space(string text, int shift) : base(text, TIME_SPACES) {
			Shift = shift;
		}
	}
	enum Orderings {
		Normal,
		ReverseLetters,
		ReverseSymbols
	}
	private class Affix : Symbol {
		public Orderings Ordering;

		public Affix(string text, Orderings ordering) : base(text, TIME_AFFIXES) {
			Ordering = ordering;
		}
	}
	private class Word {
		public string Text;
		public char Letter;

		public Word(string text, char letter) {
			Text = text;
			Letter = letter;
		}
	}

	// Table 1
	private readonly Symbol[] SYMBOL_DOTS = {
		new Symbol("Dot", TIME_LONG), new Symbol("Um", TIME_LONG), new Symbol("Echo", TIME_LONG),
		new Symbol("Tango", TIME_SHORT),
		new Symbol("Dit", TIME_SHORT), new Symbol("Dit", TIME_LONG), new Symbol("Iddy", TIME_SHORT), new Symbol("Up", TIME_SHORT),
		new Symbol("Dah", TIME_SHORT), new Symbol("Dah", TIME_LONG), new Symbol("Umpty", TIME_SHORT), new Symbol("Down", TIME_SHORT),
		new Symbol("Short", TIME_SHORT), new Symbol("Short", TIME_LONG), new Symbol("Period", TIME_SHORT), new Symbol("Period", TIME_LONG), new Symbol("Top", TIME_LONG),
		new Symbol("Long", TIME_SHORT),
		new Symbol(".", TIME_SHORT), new Symbol("🡩", TIME_LONG),
		new Symbol("Boop", TIME_SHORT), new Symbol("-", TIME_SHORT), new Symbol("🡫", TIME_SHORT), new Symbol("🡫", TIME_LONG),
		new Symbol("Up", TIME_SHORT), new Symbol("Up", TIME_SHORT),
		new Symbol("Zero", TIME_LONG), new Symbol("E", TIME_LONG),
		new Symbol("One", TIME_SHORT), new Symbol("long circle", TIME_SHORT),
		new Symbol("Low", TIME_SHORT), new Symbol("0", TIME_SHORT),
		new Symbol("High", TIME_LONG), new Symbol("long square", TIME_SHORT), new Symbol("long square", TIME_LONG), new Symbol("1", TIME_SHORT)
	};
	private readonly Symbol[] SYMBOL_DASHES = {
		new Symbol("Dot", TIME_SHORT), new Symbol("Um", TIME_SHORT), new Symbol("Echo", TIME_SHORT),
		new Symbol("Dash", TIME_SHORT), new Symbol("Dash", TIME_LONG), new Symbol("Uhh", TIME_SHORT), new Symbol("Uhh", TIME_LONG), new Symbol("Tango", TIME_LONG),
		new Symbol("Iddy", TIME_LONG), new Symbol("Up", TIME_LONG),
		new Symbol("Umpty", TIME_LONG), new Symbol("Down", TIME_LONG),
		new Symbol("Top", TIME_SHORT),
		new Symbol("Long", TIME_LONG), new Symbol("Hyphen", TIME_SHORT), new Symbol("Hyphen", TIME_LONG), new Symbol("Bottom", TIME_SHORT), new Symbol("Bottom", TIME_LONG),
		new Symbol("Beep", TIME_SHORT), new Symbol("Beep", TIME_LONG),new Symbol(".", TIME_LONG), new Symbol("🡩", TIME_SHORT),
		new Symbol("Boop", TIME_LONG), new Symbol("-", TIME_LONG),
		new Symbol("Zero", TIME_SHORT), new Symbol("short circle", TIME_SHORT), new Symbol("short circle", TIME_LONG), new Symbol("E", TIME_SHORT),
		new Symbol("One", TIME_LONG), new Symbol("long circle", TIME_LONG), new Symbol("T", TIME_SHORT), new Symbol("T", TIME_LONG),
		new Symbol("Low", TIME_LONG), new Symbol("short square", TIME_SHORT), new Symbol("short square", TIME_LONG), new Symbol("0", TIME_LONG),
		new Symbol("High", TIME_SHORT), new Symbol("1", TIME_LONG)
	};
	// Table 2
	private readonly Space[] SPACES = {
		new Space("", -1), new Space("Umm", 0), new Space("Then next", 0),
		new Space("And", 2), new Space("Uhhh", 0), new Space("Space", -1),
		new Space("Then", 2), new Space("Next", -2), new Space("Gap", 1),
		new Space("And then", -2), new Space("Next letter", -1),new Space("Pause", -2),
	};
	// Table 3
	private readonly Affix[] AFFIXES = {
		new Affix("End phrase", Orderings.Normal), new Affix("In words", Orderings.ReverseSymbols), new Affix("Long space", Orderings.ReverseSymbols),
		new Affix("End quote", Orderings.ReverseLetters), new Affix("Got that?", Orderings.Normal), new Affix("Long gap", Orderings.Normal),
		new Affix("End sequence", Orderings.ReverseLetters), new Affix("That's all", Orderings.ReverseSymbols), new Affix("Long pause", Orderings.ReverseLetters)
	};
	// Table 4
	private readonly Word[] WORDS = {
		new Word("WORD", 'L'), new Word("COLD", 'V'), new Word("FOAL", 'X'), new Word("LAIR", 'D'), new Word("MRRP", 'G'), new Word("REAP", 'D'), new Word("TIDY", 'O'),
		new Word("ACID", 'U'), new Word("COLE", 'G'), new Word("FOND", 'O'), new Word("LEER", 'R'), new Word("PEAK", 'N'), new Word("REEK", 'N'), new Word("TRUE", 'E'),
		new Word("AGES", 'Z'), new Word("COLT", 'F'), new Word("FONT", 'X'), new Word("LIAR", 'A'), new Word("PEAR", 'B'), new Word("REEL", 'N'), new Word("VIVA", 'V'),
		new Word("ANTS", 'M'), new Word("CULT", 'Q'), new Word("FOUL", 'W'), new Word("LIRE", 'U'), new Word("PEEK", 'R'), new Word("SAIL", 'D'), new Word("WAND", 'N'),
		new Word("AVID", 'G'), new Word("DIVA", 'N'), new Word("GORP", 'U'), new Word("LOAF", 'T'), new Word("PEEL", 'U'), new Word("SEWN", 'Q'), new Word("WANT", 'V'),
		new Word("BLEW", 'K'), new Word("FILL", 'O'), new Word("KEEL", 'R'), new Word("LYRE", 'D'), new Word("PEER", 'F'), new Word("SFGH", 'S'), new Word("WILL", 'O'),
		new Word("BLUE", 'I'), new Word("FINS", 'U'), new Word("KEEP", 'H'), new Word("MEOW", 'J'), new Word("QUIT", 'G'), new Word("SIGH", 'O'), new Word("YOLO", 'S'),
		new Word("COAL", 'H'), new Word("FLEW", 'R'), new Word("KILL", 'T'), new Word("MILL", 'C'), new Word("REAL", 'J'), new Word("STAN", 'L'), new Word("ZULU", 'A'),
	};
	// Table 5
	private readonly int[] DOT_OCCURENCES = { 8, 1, 7, 5 };
	private readonly int[] DASH_OCCURENCES = { 9, 6, 3, 4 };
	private readonly int[] SUBMIT_OCCURENCES = { 5, 1, 3, 2 };
	// Appendix
	private readonly string[] MORSE_CODE = new string[26] { ".-", "-...", "-.-.", "-..", ".", "..-.", "--.", "....", "..", ".---", "-.-", ".-..", "--", "-.", "---", ".--.", "--.-", ".-.", "...", "-", "..-", "...-", ".--", "-..-", "-.--", "--.." };

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
		Word cipherWord = WORDS[Rnd.Range(0, WORDS.Length)];

		List<Symbol> phrase = new List<Symbol>();
		List<Space> spaces = new List<Space>();
		List<int> positionsOfSpaces = new List<int>();

		Affix affix = AFFIXES[Rnd.Range(0, AFFIXES.Length)];
		bool reverseLetters = affix.Ordering == Orderings.ReverseLetters;
		bool reverseSymbols = affix.Ordering == Orderings.ReverseSymbols || affix.Ordering == Orderings.ReverseLetters;

		Debug.Log(reverseLetters);
		Debug.Log(reverseSymbols);

		Debug.Log(cipherWord.Text);
		for ( int charIter = 0; charIter < cipherWord.Text.Length; charIter++ ) {
			int charPlace = reverseLetters ? cipherWord.Text.Length - charIter - 1 : charIter;
			string morseEquivalent = MORSE_CODE[cipherWord.Text[charPlace] - 'A'];

			Debug.Log("Letter: " + morseEquivalent);

			for (int symbolIter = 0; symbolIter < morseEquivalent.Length; symbolIter++) {
				int symbolPlace = reverseSymbols ? morseEquivalent.Length - symbolIter - 1 : symbolIter;
				char value = morseEquivalent[symbolPlace];
				Symbol symbol;
				if (value == '.') {
					symbol = SYMBOL_DOTS[Rnd.Range(0, SYMBOL_DOTS.Length)];
				} else {
					symbol = SYMBOL_DASHES[Rnd.Range(0, SYMBOL_DASHES.Length)];
				}
				Debug.Log("Symbol: " + symbol.Text + " " + symbol.Time.ToString() + " (" + value + ")");
				phrase.Add(symbol);
			}
			if ( charIter != cipherWord.Text.Length - 1 ) {
				Space space = SPACES[Rnd.Range(0, SPACES.Length)];
				Debug.Log("Space: " + space.Text);
				spaces.Add(space);
				positionsOfSpaces.Add(phrase.Count - space.Shift);
			} else {
				Debug.Log(affix.Text);
				phrase.Add(affix);
			}
		}
		// handle space shifts
		List<int> positionsAddedSoFar = new List<int>();
		int phraseLengthWithoutSpaces = phrase.Count;
		for ( int i = 0; i < spaces.Count;  i++ ) {
			Space space = spaces[i];
			int position = positionsOfSpaces[i];
			if (position <= 0) position += phraseLengthWithoutSpaces - 1;
			if (position >= phraseLengthWithoutSpaces - 1) position -= phraseLengthWithoutSpaces - 1;
			// need to add in the right place though the list size is changing
			positionsAddedSoFar.Add(position);
			foreach (int positionAddedSoFar in positionsAddedSoFar) if (positionAddedSoFar <= position) position += 1;
			position--; // to account for it finding itself
			phrase.Insert(position, space);
		}

		StartCoroutine(CycleCoroutine(phrase));
	}
	
	void Update () {
		
	}

	IEnumerator CycleCoroutine(List<Symbol> phrase) {
		while (true) {
			for ( int i = 0; i < phrase.Count; i++ ) {
				Text.text = phrase[i].Text;
				yield return new WaitForSeconds(phrase[i].Time);
				Text.text = "";
				yield return new WaitForSeconds(TIME_BETWEEN_SYMBOLS);
			}
		}
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
