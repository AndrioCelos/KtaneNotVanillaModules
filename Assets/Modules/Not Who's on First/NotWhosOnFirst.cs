using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using NotVanillaModulesLib;
using UnityEngine;

public class NotWhosOnFirst : NotVanillaModule<NotMemoryConnector> {
	private static readonly string[] displays = new[] {
		"BLANK", "C", "CEE", "DISPLAY", "FIRST", "HOLD ON", "LEAD", "LED", "LEED", "NO", "NOTHING", "OK", "OKAY", "READ", "RED", "REED",
		"SAY", "SAYS", "SEE", "THEIR", "THERE", "THEY ARE", "THEY’RE", "U", "UR", "YES", "YOU", "YOU ARE", "YOU’RE", "YOUR"
	};
	private static readonly string[] labels1 = new[] { "BLANK", "FIRST", "LEFT", "MIDDLE", "NO", "NOTHING", "OKAY", "PRESS", "READY", "RIGHT", "UHHH", "WAIT", "WHAT", "YES" };
	private static readonly string[] labels2 = new[] { "DONE", "HOLD", "LIKE", "NEXT", "SURE", "U", "UH HUH", "UH UH", "UR", "WHAT?", "YOU", "YOU ARE", "YOU'RE", "YOUR" };
	private static readonly char[,] defaultLetterTable = new[,] {
		{ 'Y','G','I','V','Z','D','U','Q','J','A','E','H','F','B','R','C','W','S','X','M','L','N','T','P','O' },
		{ 'U','W','Z','L','Y','G','C','P','D','T','S','Q','V','N','K','O','H','M','R','E','A','J','X','F','I' },
		{ 'A','I','S','Z','B','U','D','S','Y','K','A','N','O','J','X','R','M','Q','L','H','E','T','G','I','P' },
		{ 'E','M','V','V','X','C','S','W','M','K','U','F','N','J','Y','P','F','X','B','A','T','I','R','G','Q' },
		{ 'O','C','U','G','Z','A','O','W','P','C','B','L','F','T','V','U','M','Z','Y','D','J','M','X','K','I' },
		{ 'L','G','K','Y','C','Q','B','R','A','H','O','J','K','U','P','D','P','F','Z','S','I','T','L','X','G' },
		{ 'T','C','E','X','Q','A','L','N','D','U','F','K','Y','R','P','V','G','B','J','B','S','I','W','O','H' },
		{ 'Y','E','O','U','J','V','M','Z','P','R','W','L','D','A','C','G','R','S','K','X','F','N','B','T','Q' },
		{ 'C','H','F','V','O','N','T','L','R','U','J','Y','E','S','B','P','Q','A','W','M','D','Z','K','X','G' },
		{ 'Q','G','W','I','P','X','L','D','Z','Y','V','C','F','R','T','M','E','B','K','H','S','N','U','O','A' },
		{ 'J','Z','U','L','X','P','I','V','G','Y','T','E','F','B','O','R','C','N','S','W','K','H','M','Q','D' },
		{ 'I','R','B','D','M','G','Y','J','T','O','Q','U','R','C','N','A','E','P','H','V','I','K','W','Z','S' },
		{ 'E','Z','E','S','X','P','W','L','J','D','A','V','U','R','C','H','N','G','B','I','H','O','T','Q','K' },
		{ 'O','D','X','P','U','J','K','Z','O','L','B','T','X','G','E','D','R','Q','H','V','I','C','I','W','Y' },
		{ 'K','S','U','Q','U','T','H','O','K','J','O','L','J','T','G','Z','V','C','F','M','T','U','L','X','R' },
		{ 'K','Y','B','B','C','T','B','C','I','M','R','Q','S','O','X','U','C','F','Z','D','F','K','T','Q','N' },
		{ 'S','K','U','W','L','P','E','H','S','Q','M','N','I','V','A','Y','X','G','J','W','Z','F','H','K','B' },
		{ 'K','D','B','E','V','U','Q','T','E','M','M','Z','H','S','I','W','Y','B','M','J','U','T','N','Z','Q' },
		{ 'J','F','N','P','M','O','I','A','J','Y','D','M','I','Q','H','X','C','K','L','Y','O','R','W','U','G' },
		{ 'S','G','I','Y','D','H','B','N','W','M','X','Q','U','P','Z','O','E','L','A','K','J','C','F','R','V' },
		{ 'Y','S','P','O','A','M','L','T','N','E','R','F','V','X','B','C','D','H','Q','J','I','K','Z','W','G' },
		{ 'J','F','X','G','R','Z','E','A','M','S','E','V','O','F','Q','D','S','N','B','W','P','I','Y','C','L' },
		{ 'G','A','F','L','Y','D','N','G','Z','R','O','A','P','Y','U','Q','H','K','D','Y','W','N','F','V','W' },
		{ 'N','V','M','Z','H','T','N','F','R','J','C','O','D','A','W','Y','W','J','L','H','A','B','Z','E','H' },
		{ 'P','D','L','Z','X','I','A','J','P','F','N','V','R','E','G','N','P','D','L','S','T','M','H','S','N' },
		{ 'E','R','W','L','G','H','Q','A','S','B','T','V','X','I','C','E','C','F','V','P','A','X','E','V','W' }
	};
	private readonly Dictionary<string, int[]> defaultStage5Table = new Dictionary<string, int[]> {
		{ "READY",   new[] { 1, 2, 3, 4, 5, 6 } },
		{ "FIRST",   new[] { 4, 5, 6, 2, 1, 3 } },
		{ "NO",      new[] { 2, 6, 4, 1, 3, 5 } },
		{ "BLANK",   new[] { 3, 1, 5, 2, 4, 6 } },
		{ "NOTHING", new[] { 5, 4, 1, 2, 3, 6 } },
		{ "YES",     new[] { 6, 3, 4, 5, 2, 1 } },
		{ "WHAT",    new[] { 2, 3, 6, 1, 5, 4 } },
		{ "UHHH",    new[] { 5, 1, 4, 3, 6, 2 } },
		{ "LEFT",    new[] { 2, 4, 6, 1, 5, 3 } },
		{ "RIGHT",   new[] { 1, 6, 3, 4, 5, 2 } },
		{ "MIDDLE",  new[] { 2, 3, 1, 5, 4, 6 } },
		{ "OKAY",    new[] { 3, 6, 2, 5, 4, 1 } },
		{ "WAIT",    new[] { 6, 1, 3, 4, 2, 5 } },
		{ "PRESS",   new[] { 4, 2, 5, 6, 1, 3 } },
		{ "YOU",     new[] { 3, 4, 6, 1, 2, 5 } },
		{ "YOU ARE", new[] { 2, 6, 3, 5, 1, 4 } },
		{ "YOUR",    new[] { 4, 1, 5, 6, 3, 2 } },
		{ "YOU'RE",  new[] { 5, 4, 2, 1, 6, 3 } },
		{ "UR",      new[] { 1, 3, 2, 4, 5, 6 } },
		{ "U",       new[] { 6, 3, 4, 2, 1, 5 } },
		{ "UH HUH",  new[] { 3, 2, 4, 1, 5, 6 } },
		{ "UH UH",   new[] { 5, 4, 3, 6, 1, 2 } },
		{ "WHAT?",   new[] { 4, 1, 2, 3, 5, 6 } },
		{ "DONE",    new[] { 2, 6, 5, 3, 1, 4 } },
		{ "NEXT",    new[] { 5, 2, 4, 3, 6, 1 } },
		{ "HOLD",    new[] { 4, 5, 6, 1, 2, 3 } },
		{ "SURE",    new[] { 3, 4, 1, 6, 2, 5 } },
		{ "LIKE",    new[] { 1, 4, 6, 5, 3, 2 } },
	};

	public ReadOnlyCollection<string> Labels { get; private set; }

	private readonly HashSet<int> correctButtons = new HashSet<int>();
	private readonly string[] buttonLabels = new string[6];
	private readonly int[] rememberedPositions = new int[6];
	private readonly string[] rememberedLabels = new string[6];
	private int stage2Sum;

	public int StagesCompleted { get; private set; }

	public void Awake() { this.Labels = Array.AsReadOnly(this.buttonLabels); }

	public override void Start() {
		base.Start();
		this.Connector.KMBombModule.OnActivate = this.Connector.Activate;
		this.Connector.ButtonPressed += this.Connector_ButtonPressed;
		this.Connector.ButtonsSunk += this.Connector_ButtonsSunk;
		this.SetRandomTexts();
		this.SetUpStage(1);
	}

	private void Connector_ButtonsSunk(object sender, EventArgs e) {
		this.SetRandomTexts();
		this.SetUpStage(this.StagesCompleted + 1);
	}

	private void Connector_ButtonPressed(object sender, KeypadButtonEventArgs e) {
		if (this.Solved || !this.Connector.InputValid) return;
		if (this.correctButtons.Contains(e.ButtonIndex)) {
			this.Connector.Stage = ++this.StagesCompleted;
			if (this.StagesCompleted >= 5) this.Disarm();
			else {
				if (this.StagesCompleted == 3 || this.StagesCompleted == 4) {
					this.rememberedLabels[this.StagesCompleted - 1] = this.buttonLabels[e.ButtonIndex];
					this.rememberedPositions[this.StagesCompleted - 1] = e.ButtonIndex;
				}
				this.Connector.AnimateButtons();
			}
		} else {
			this.Connector.KMBombModule.HandleStrike();
			this.Connector.Stage = this.StagesCompleted = 0;
			this.Connector.AnimateButtons();
		}
	}

	private void SetRandomTexts() {
		this.Connector.DisplayText = displays.PickRandom();
		var list = UnityEngine.Random.Range(0, 2) == 0 ? labels1 : labels2;
		list.Shuffle();
		for (int i = 0; i < 6; ++i) {
			this.buttonLabels[i] = list[i];
			this.Connector.SetButtonLabel(i, list[i]);
		}
	}

	private void SetUpStage(int stage) {
		this.correctButtons.Clear();
		switch (stage) {
			case 1:
				var correctButton = this.Stage1Button();
				this.correctButtons.Add(correctButton);
				this.Log("Stage 1: display is '{0}'. The correct button is {1} ('{2}').",
					this.Connector.DisplayText, DescribeButton(correctButton), this.buttonLabels[correctButton]);
				this.rememberedLabels[0] = this.buttonLabels[correctButton];
				this.rememberedPositions[0] = correctButton;
				break;
			case 2:
				this.stage2Sum = 0;
				var yLetters = this.Connector.DisplayText.Where(char.IsLetter).ToList();
				var xLetters = this.rememberedLabels[0].Where(char.IsLetter).ToList();
				var letters = new char[Math.Max(yLetters.Count, xLetters.Count)];
				for (int i = 0; i < letters.Length; ++i) {
					var xLetter = xLetters[i % xLetters.Count];
					if (xLetter > 'J') --xLetter;
					var letter = defaultLetterTable[yLetters[i % yLetters.Count] - 'A', xLetter - 'A'];
					letters[i] = letter;
					this.stage2Sum += letter - 'A' + 1;
				}
				this.stage2Sum = this.stage2Sum % 60 + 1;
				correctButton = Stage2Check(this.stage2Sum);
				this.correctButtons.Add(correctButton);
				this.Log("Stage 2: display is '{0}'. The final sum is {1} (from letters {2}). The correct button is {3} ('{4}').",
					this.Connector.DisplayText, this.stage2Sum, letters.Join(", "), DescribeButton(correctButton), this.buttonLabels[correctButton]);
				this.rememberedLabels[1] = this.buttonLabels[correctButton];
				this.rememberedPositions[1] = correctButton;
				break;
			case 3:
				var buttonIndex = this.Stage1Button();
				this.rememberedLabels[4] = this.buttonLabels[buttonIndex];
				this.rememberedPositions[4] = buttonIndex;
				var description = this.Stage3Check(buttonIndex);
				this.Log("Stage 3: display is '{0}'. The reference button is {1}. The correct button is {2}.",
					this.Connector.DisplayText, DescribeButton(buttonIndex), description);
				break;
			case 4:
				var visitedButtons = new HashSet<int>();
				var pos = this.rememberedPositions[4];
				do {
					switch (this.buttonLabels[pos]) {
						case "WHAT?": case "PRESS": case "YOU": case "LEFT": case "WAIT": case "OKAY": case "NO":  // Up
							pos -= 2; if (pos < 0) pos += 6; break;
						case "WHAT": case "UH HUH": case "UR": case "NEXT": case "NOTHING": case "FIRST": case "YOU ARE":  // Left
						case "BLANK": case "RIGHT": case "SURE": case "YOU'RE": case "READY": case "U": case "UH UH":  // Right
							pos ^= 1; break;
						default:  // Down
							pos += 2; if (pos >= 6) pos -= 6; break;
					}
				} while (visitedButtons.Add(pos));
				this.rememberedLabels[5] = this.buttonLabels[pos];
				this.rememberedPositions[5] = pos;
				description = this.Stage3Check(pos);
				this.Log("Stage 4: display is '{0}'. The reference button is {1}. The correct button is {2}.",
					this.Connector.DisplayText, DescribeButton(pos), description);
				break;
			case 5:
				var sum = this.stage2Sum;
				for (int i = 0; i < 6; ++i)
					sum += this.defaultStage5Table[this.rememberedLabels[i]][this.rememberedPositions[i]];
				sum = sum % 60 + 1;
				correctButton = Stage2Check(sum);
				this.correctButtons.Add(correctButton);
				this.Log("Stage 5: display is '{0}'. The correct button is {1} ('{2}').",
					this.Connector.DisplayText, DescribeButton(correctButton), this.buttonLabels[correctButton]);
				break;
			default: throw new ArgumentOutOfRangeException("stage");
		}
	}

	private static int Stage2Check(int n) {
		switch (n) {
			case 52: case 3: case 5: case 44: case 2: case 16: case 46: case 60: case 29: case 45: return 0;
			case 37: case 4: case 59: case 10: case 24: case 47: case 43: case 38: case 39: case 53: return 1;
			case 9: case 19: case 51: case 40: case 14: case 17: case 6: case 7: case 26: case 31: return 2;
			case 18: case 13: case 20: case 57: case 55: case 56: case 11: case 30: case 1: case 27: return 3;
			case 48: case 49: case 23: case 35: case 34: case 36: case 58: case 22: case 33: case 21: return 4;
			case 28: case 42: case 12: case 54: case 25: case 15: case 50: case 41: case 8: case 32: return 5;
			default: throw new ArgumentOutOfRangeException("n");
		}
	}

	private int Stage1Button() {
		switch (this.Connector.DisplayText) {
			case "THERE": case "NOTHING": case "UR": case "YOUR": case "SAY": return 0;
			case "SEE": case "HOLD ON": case "REED": case "YES": case "LEAD": return 1;
			case "FIRST": case "SAYS": case "C": case "THEIR": case "U": return 2;
			case "THEY ARE": case "RED": case "DISPLAY": case "BLANK": case "OK": return 3;
			case "THEY’RE": case "CEE": case "YOU’RE": case "YOU ARE": case "LED": return 4;
			case "READ": case "NO": case "OKAY": case "LEED": case "YOU": return 5;
			default: throw new InvalidOperationException("Unknown display string");
		}
	}

	private string Stage3Check(int referenceButtonIndex) {
		var label = this.buttonLabels[referenceButtonIndex];
		bool a = referenceButtonIndex % 2 == 0;
		bool b = label.Length % 2 == 0;
		bool c = this.Connector.DisplayText.Count(l => l == 'A' || l == 'E' || l == 'I' || l == 'O' || l == 'U') % 2 != 0;
		bool d;
		switch (this.stage2Sum) {
			case 2: case 3: case 5: case 7: case 11: case 13: case 17: case 19: case 23: case 29:
			case 31: case 37: case 41: case 43: case 47: case 53: case 59: d = true; break;
			default: d = false; break;
		}
		if (a) {
			if (b) {
				if (c) {
					if (d) { this.correctButtons.Add(4); return DescribeButton(4); }
					else { this.correctButtons.Add(2); return DescribeButton(2); }
				} else {
					if (d) { this.correctButtons.Add(3); return DescribeButton(3); }
					else { this.correctButtons.Add(1); return DescribeButton(1); }
				}
			} else {
				if (c) {
					if (d) { this.correctButtons.Add(0); this.correctButtons.Add(1); this.correctButtons.Add(2); this.correctButtons.Add(3); return "any button not on the bottom row"; }
					else { this.correctButtons.Add(2); this.correctButtons.Add(3); return "any button on the middle row"; }
				} else {
					if (d) { this.correctButtons.Add(referenceButtonIndex ^ 1); return DescribeButton(referenceButtonIndex ^ 1); }
					else { this.correctButtons.Add(referenceButtonIndex); return DescribeButton(referenceButtonIndex); }
				}
			}
		} else {
			if (b) {
				if (c) {
					if (d) { this.correctButtons.Add(2); this.correctButtons.Add(3); this.correctButtons.Add(4); this.correctButtons.Add(5); return "any button not on the top row"; }
					else { this.correctButtons.Add(0); this.correctButtons.Add(2); this.correctButtons.Add(4); return "any button on the left column"; }
				} else {
					if (d) { this.correctButtons.Add(4); this.correctButtons.Add(5); return "any button on the bottom row"; }
					else { this.correctButtons.Add(1); this.correctButtons.Add(3); this.correctButtons.Add(5); return "any button on the right column"; }
				}
			} else {
				if (c) {
					if (d) { this.correctButtons.Add(5); return DescribeButton(5); }
					else { this.correctButtons.Add(0); this.correctButtons.Add(1); return "any button on the top row"; }
				} else {
					if (d) {
						this.correctButtons.Add(0); this.correctButtons.Add(1); this.correctButtons.Add(2); this.correctButtons.Add(3); this.correctButtons.Add(4); this.correctButtons.Add(5);
						return "any button";
					} else { this.correctButtons.Add(0); return DescribeButton(0); }
				}
			}
		}
	}

	private static string DescribeButton(int index) {
		switch (index) {
			case 0: return "the top left button";
			case 1: return "the top right button";
			case 2: return "the middle left button";
			case 3: return "the middle right button";
			case 4: return "the bottom left button";
			case 5: return "the bottom right button";
			default: throw new ArgumentOutOfRangeException("index");
		}
	}

	// Twitch Plays support
	public static readonly string TwitchHelpMessage
		= "!{0} what? - press the button that says 'WHAT?'; the phrase must match exactly | !{0} press 3 - press the third button in English reading order";
	public IEnumerator ProcessTwitchCommand(string command) {
		int n;
		command = command.Trim();
		if (command.StartsWith("press ", StringComparison.InvariantCultureIgnoreCase)) {
			if (!int.TryParse(command.Substring(6).TrimStart(), out n) || n < 1 || n > 6) yield break;
			yield return null;
			this.Connector.TwitchPress(n - 1);
		} else {
			command = Regex.Replace(command, @"\s+", " ").Trim('\'', '"').ToUpperInvariant();
			n = Array.IndexOf(this.buttonLabels, command);
			if (n < 0) {
				yield return string.Format("sendtochaterror The label '{0}' is not present on the module.", command);
			} else {
				yield return null;
				this.Connector.TwitchPress(n);
			}
		}
	}

	public IEnumerator TwitchHandleForcedSolve() {
		while (!this.Solved) {
			yield return new WaitWhile(() => this.Connector.Animating);
			this.Connector.TwitchPress(this.correctButtons.First());
		}
		yield break;
	}
}
