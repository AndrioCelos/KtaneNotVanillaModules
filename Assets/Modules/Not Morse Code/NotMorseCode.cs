using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NotVanillaModulesLib;
using Random = UnityEngine.Random;
using System;
using System.Linq;

public class NotMorseCode : NotVanillaModule<NotMorseCodeConnector> {
	private static readonly int[] possibleFrequencies = new[] {
		502, 505, 512, 515,	522, 525, 532, 535,	542, 545, 552, 555,
		562, 565, 572, 575,	582, 585, 592, 595,	600
	};
	private static readonly Dictionary<char, Symbol[]> codeTable = new Dictionary<char, Symbol[]>() {
		{ 'A', new[] { Symbol.Dot, Symbol.Dash } },
		{ 'B', new[] { Symbol.Dash, Symbol.Dot, Symbol.Dot, Symbol.Dot } },
		{ 'C', new[] { Symbol.Dash, Symbol.Dot, Symbol.Dash, Symbol.Dot } },
		{ 'D', new[] { Symbol.Dash, Symbol.Dot, Symbol.Dot } },
		{ 'E', new[] { Symbol.Dot } },
		{ 'F', new[] { Symbol.Dot, Symbol.Dot, Symbol.Dash, Symbol.Dot } },
		{ 'G', new[] { Symbol.Dash, Symbol.Dash, Symbol.Dot } },
		{ 'H', new[] { Symbol.Dot, Symbol.Dot, Symbol.Dot, Symbol.Dot } },
		{ 'I', new[] { Symbol.Dot, Symbol.Dot } },
		{ 'J', new[] { Symbol.Dot, Symbol.Dash, Symbol.Dash, Symbol.Dash } },
		{ 'K', new[] { Symbol.Dash, Symbol.Dot, Symbol.Dash } },
		{ 'L', new[] { Symbol.Dot, Symbol.Dash, Symbol.Dot, Symbol.Dot } },
		{ 'M', new[] { Symbol.Dash, Symbol.Dash } },
		{ 'N', new[] { Symbol.Dash, Symbol.Dot } },
		{ 'O', new[] { Symbol.Dash, Symbol.Dash, Symbol.Dash } },
		{ 'P', new[] { Symbol.Dot, Symbol.Dash, Symbol.Dash, Symbol.Dot } },
		{ 'Q', new[] { Symbol.Dash, Symbol.Dash, Symbol.Dot, Symbol.Dash } },
		{ 'R', new[] { Symbol.Dot, Symbol.Dash, Symbol.Dot } },
		{ 'S', new[] { Symbol.Dot, Symbol.Dot, Symbol.Dot } },
		{ 'T', new[] { Symbol.Dash } },
		{ 'U', new[] { Symbol.Dot, Symbol.Dot, Symbol.Dash } },
		{ 'V', new[] { Symbol.Dot, Symbol.Dot, Symbol.Dot, Symbol.Dash } },
		{ 'W', new[] { Symbol.Dot, Symbol.Dash, Symbol.Dash } },
		{ 'X', new[] { Symbol.Dash, Symbol.Dot, Symbol.Dot, Symbol.Dash } },
		{ 'Y', new[] { Symbol.Dash, Symbol.Dot, Symbol.Dash, Symbol.Dash } },
		{ 'Z', new[] { Symbol.Dash, Symbol.Dash, Symbol.Dot, Symbol.Dot } },
	};

	private static readonly string[][] defaultColumns = new[] {
		new[] { "shelf", "twine", "null", "drive", "shell", "year", "shall" },
		new[] { "pet", "pounds", "possum", "honey", "eggplant", "hive", "brother" },
		new[] { "hive", "query", "sister", "ying", "pit", "guidance", "anew" },
		new[] { "brother", "yeast", "coolant", "beef", "null", "pence", "swine" },
		new[] { "yang", "twine", "pit", "anew", "yeast", "shill", "shell" },
		new[] { "shill", "eggplant", "year", "pet", "coolant", "drive", "possum" },
		new[] { "guidance", "honey", "swine", "shelf", "shall", "query", "beef" }
	};

	public int WordsCorrectlySubmitted { get; private set; }

	private int[] frequencies;
	private int channelIndex;
	private string[] words;
	private int[] correctChannels;
	private Coroutine playWordCoroutine;
	private bool activated;

	private const float DotLength = 0.25f;

	public override void Start () {
		base.Start();
		this.Connector.KMBombModule.OnActivate = this.KMBombModule_OnActivate;
		this.Connector.DownPressed += this.Connector_DownPressed;
		this.Connector.UpPressed += this.Connector_UpPressed;
		this.Connector.SubmitPressed += this.Connector_SubmitPressed;

		var frequencies = new List<int>(possibleFrequencies);
		this.frequencies = new int[5];
		for (int i = 0; i < 5; ++i) {
			var j = Random.Range(0, frequencies.Count);
			this.frequencies[i] = frequencies[j];
			frequencies.RemoveAt(j);
		}
		Array.Sort(this.frequencies);

		var columnIndex = Random.Range(0, 7);
		var column = defaultColumns[columnIndex];

		this.correctChannels = Enumerable.Range(0, 5).ToArray();
		this.correctChannels.Shuffle();

		var indices = Enumerable.Range(0, 7).ToList();
		indices.RemoveAt(Random.Range(0, 7));
		indices.RemoveAt(Random.Range(0, 6));

		this.words = new string[5];
		for (int i = 0; i < 5; ++i) this.words[this.correctChannels[i]] = column[indices[i]];

		this.Log("The transmitted words are: " + Enumerable.Range(0, 5).Select(i => string.Format("3.{0} MHz: {1}", this.frequencies[i], this.words[i])).Join(", "));
		this.Log("The words should be submitted in this order: " + this.correctChannels.Select(i => string.Format("3.{0} MHz", this.frequencies[i])).Join(", "));

		this.ChangeChannel();
	}

	public override void Disarm() {
		base.Disarm();
		if (this.playWordCoroutine != null) {
			this.StopCoroutine(this.playWordCoroutine);
			this.Connector.SetLight(false);
			this.playWordCoroutine = null;
		}
	}

	private void Connector_SubmitPressed(object sender, EventArgs e) {
		if (this.Solved) return;

		var word = this.words[this.channelIndex];
		var correctChannel = this.correctChannels[this.WordsCorrectlySubmitted];
		if (this.channelIndex == correctChannel) {
			this.Log("3.{0} MHz ({1}) was submitted. That was correct.", this.frequencies[this.channelIndex], word);
			++this.WordsCorrectlySubmitted;
			if (this.WordsCorrectlySubmitted >= 5) this.Disarm();
		} else {
			this.Log("3.{0} MHz ({1}) was submitted. That was incorrect: the next channel is 3.{2} MHz ({3}).",
				this.frequencies[this.channelIndex], word, this.frequencies[correctChannel], this.words[correctChannel]);
			this.Connector.KMBombModule.HandleStrike();
		}
	}

	private void KMBombModule_OnActivate() {
		this.Connector.Activate();
		this.activated = true;
		if (!this.Solved) this.playWordCoroutine = this.StartCoroutine(this.PlayWord(this.words[this.channelIndex]));
	}

	private void Connector_DownPressed(object sender, EventArgs e) {
		if (this.channelIndex > 0) {
			--this.channelIndex;
			this.ChangeChannel();
		}
	}

	private void Connector_UpPressed(object sender, EventArgs e) {
		if (this.channelIndex < 4) {
			++this.channelIndex;
			this.ChangeChannel();
		}
	}

	private void ChangeChannel() {
		this.Connector.SetSlider(this.frequencies[this.channelIndex]);
		this.Connector.SetDisplay(this.frequencies[this.channelIndex].ToString());
		if (this.activated && !this.Solved) {
			this.StopCoroutine(this.playWordCoroutine);
			this.playWordCoroutine = this.StartCoroutine(this.PlayWord(this.words[this.channelIndex]));
		}
	}

	public IEnumerator PlayWord(string word) {
		while (true) {
			foreach (var c in word) {
				var code = codeTable[char.ToUpper(c)];
				foreach (var symbol in code) {
					this.Connector.SetLight(true);
					yield return new WaitForSeconds(symbol == Symbol.Dot ? DotLength : DotLength * 3);
					this.Connector.SetLight(false);
					yield return new WaitForSeconds(DotLength);
				}
				yield return new WaitForSeconds(DotLength * 3);  // 4 dots total
			}
			yield return new WaitForSeconds(DotLength * 6);  // 10 dots total
		}
	}

	// Twitch Plays support
	public static readonly string TwitchHelpMessage
		= "!{0} down | !{0} up 2 | !{0} tune 3 - moves to the 3rd lowest frequency | !{0} submit 3.573 | !{0} xt 573 - submits 3.573 MHz | !{0} submit 2 - submits the 2nd lowest frequency";
	public IEnumerator ProcessTwitchCommand(string command) {
		var tokens = command.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
		if (tokens.Length == 0 || tokens.Length > 3 || (tokens.Length == 3 && !tokens[2].EqualsIgnoreCase("MHz"))) yield break;
		switch (tokens[0].ToLowerInvariant()) {
			case "down": case "left":
				int n = 1;
				if (tokens.Length == 2) {
					if (!int.TryParse(tokens[1], out n) || n < 1 || n > 9) yield break;
				}
				yield return null;
				for (; n > 0 && this.channelIndex > 0; --n) {
					this.Connector.TwitchMoveDown();
					yield return new WaitForSeconds(0.1f);
				}
				break;
			case "up": case "right":
				n = 1;
				if (tokens.Length == 2) {
					if (!int.TryParse(tokens[1], out n) || n < 1 || n > 9) yield break;
				}
				yield return null;
				for (; n > 0 && this.channelIndex < 4; --n) {
					this.Connector.TwitchMoveUp();
					yield return new WaitForSeconds(0.1f);
				}
				break;
			case "tune":
				if (tokens.Length != 2) yield break;
				if (!int.TryParse(tokens[1], out n) || n < 1 || n > 5) yield break;
				yield return null;
				foreach (var o in this.TwitchTuneTo(n)) yield return o;
				break;
			case "submit": case "transmit": case "trans": case "tx": case "xt":
				if (tokens.Length > 1) {
					if (tokens[1].StartsWith("3.") || tokens[1].StartsWith("3,")) tokens[1] = tokens[1].Substring(2);
					if (!int.TryParse(tokens[1], out n)) yield break;
					if (n <= 5) {
						if (n < 1) yield break;
						yield return null;
						foreach (var o in this.TwitchTuneTo(n)) yield return o;
						this.Connector.TwitchSubmit();
					} else {
						var index = Array.IndexOf(this.frequencies, n);
						if (index < 0)
							yield return string.Format("sendtochaterror 3.{0} MHz is not an available channel.", n);
						else {
							yield return null;
							foreach (var o in this.TwitchTuneTo(index + 1)) yield return o;
							this.Connector.TwitchSubmit();
						}
					}
				} else {
					yield return null;
					this.Connector.TwitchSubmit();
				}
				break;
		}
	}

	private IEnumerable<object> TwitchTuneTo(int userNumber) {
		--userNumber;
		if (this.channelIndex < userNumber) {
			do {
				this.Connector.TwitchMoveUp();
				yield return "trywaitcancel 0.1";
			} while (this.channelIndex < userNumber);
		} else if (this.channelIndex > userNumber) {
			do {
				this.Connector.TwitchMoveDown();
				yield return "trywaitcancel 0.1";
			} while (this.channelIndex > userNumber);
		}
	}

	public IEnumerator TwitchHandleForcedSolve() {
		while (!this.Solved) {
			foreach (var o in this.TwitchTuneTo(Array.IndexOf(this.words, this.correctChannels[this.WordsCorrectlySubmitted]) + 1)) yield return o;
			this.Connector.TwitchSubmit();
			yield return new WaitForSeconds(0.1f);
		}
	}

	private enum Symbol {
		Dot,
		Dash
	}
}
