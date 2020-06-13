using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NotVanillaModulesLib;
using KModkit;
using UnityEngine;

public class NotWiresword : NotVanillaModule<NotWiresConnector> {
	private static readonly Dictionary<char, WireState[]> defaultWireTableNoVowel = new Dictionary<char, WireState[]> {
		{ 'A', new[] { new WireState(2, WireColour.Red), new WireState(3, WireColour.Yellow), new WireState(5, WireColour.Purple) } },
		{ 'B', new[] { new WireState(0, WireColour.Purple), new WireState(5, WireColour.Orange) } },
		{ 'C', new[] { new WireState(0, WireColour.Yellow), new WireState(5, WireColour.White) } },
		{ 'D', new[] { new WireState(2, WireColour.Purple), new WireState(3, WireColour.Orange) } },
		{ 'E', new[] { new WireState(1, WireColour.Grey), new WireState(2, WireColour.Grey), new WireState(3, WireColour.Purple) } },
		{ 'F', new[] { new WireState(0, WireColour.Grey), new WireState(4, WireColour.Grey) } },
		{ 'G', new[] { new WireState(1, WireColour.Red), new WireState(1, WireColour.Yellow) } },
		{ 'H', new[] { new WireState(0, WireColour.Black), new WireState(3, WireColour.Green) } },
		{ 'I', new[] { new WireState(3, WireColour.Blue), new WireState(3, WireColour.Black), new WireState(4, WireColour.Purple) } },
		{ 'J', new[] { new WireState(4, WireColour.Red), new WireState(4, WireColour.White) } },
		{ 'K', new[] { new WireState(0, WireColour.Orange), new WireState(4, WireColour.Orange) } },
		{ 'L', new[] { new WireState(0, WireColour.Blue), new WireState(4, WireColour.Yellow) } },
		{ 'M', new[] { new WireState(1, WireColour.Green), new WireState(4, WireColour.Green) } },
		{ 'N', new[] { new WireState(1, WireColour.Blue), new WireState(2, WireColour.Yellow) } },
		{ 'O', new[] { new WireState(0, WireColour.White), new WireState(3, WireColour.Grey), new WireState(4, WireColour.Black) } },
		{ 'P', new[] { new WireState(1, WireColour.Orange), new WireState(5, WireColour.Black) } },
		{ 'Q', new[] { new WireState(0, WireColour.Red), new WireState(5, WireColour.Grey) } },
		{ 'R', new[] { new WireState(2, WireColour.Green), new WireState(4, WireColour.Blue) } },
		{ 'S', new[] { new WireState(1, WireColour.Black), new WireState(3, WireColour.Red) } },
		{ 'T', new[] { new WireState(1, WireColour.Purple), new WireState(2, WireColour.Blue) } },
		{ 'U', new[] { new WireState(2, WireColour.Orange), new WireState(5, WireColour.Red) } },
		{ 'V', new[] { new WireState(2, WireColour.White), new WireState(5, WireColour.Yellow) } },
		{ 'W', new[] { new WireState(1, WireColour.White), new WireState(5, WireColour.Green) } },
		{ 'X', new[] { new WireState(0, WireColour.Green), new WireState(3, WireColour.White) } },
		{ 'Y', new[] { new WireState(2, WireColour.Black), new WireState(5, WireColour.Blue) } }
	};
	private static readonly Dictionary<char, WireState[]> defaultWireTableVowel = new Dictionary<char, WireState[]> {
		{ 'A', new[] { new WireState(0, WireColour.Red), new WireState(2, WireColour.Purple), new WireState(5, WireColour.Blue) } },
		{ 'B', new[] { new WireState(2, WireColour.Yellow), new WireState(4, WireColour.Purple) } },
		{ 'C', new[] { new WireState(1, WireColour.Green), new WireState(4, WireColour.Green) } },
		{ 'D', new[] { new WireState(2, WireColour.Green), new WireState(3, WireColour.Grey) } },
		{ 'E', new[] { new WireState(2, WireColour.Red), new WireState(3, WireColour.Orange), new WireState(5, WireColour.Yellow) } },
		{ 'F', new[] { new WireState(5, WireColour.Grey), new WireState(5, WireColour.Purple) } },
		{ 'G', new[] { new WireState(1, WireColour.Grey), new WireState(1, WireColour.Black) } },
		{ 'H', new[] { new WireState(0, WireColour.White), new WireState(3, WireColour.Blue) } },
		{ 'I', new[] { new WireState(0, WireColour.Yellow), new WireState(3, WireColour.Red), new WireState(3, WireColour.White) } },
		{ 'J', new[] { new WireState(0, WireColour.Purple), new WireState(4, WireColour.Blue) } },
		{ 'K', new[] { new WireState(0, WireColour.Orange), new WireState(2, WireColour.Black) } },
		{ 'L', new[] { new WireState(1, WireColour.Red), new WireState(4, WireColour.White) } },
		{ 'M', new[] { new WireState(4, WireColour.Black), new WireState(4, WireColour.Yellow) } },
		{ 'N', new[] { new WireState(1, WireColour.Blue), new WireState(2, WireColour.White) } },
		{ 'O', new[] { new WireState(0, WireColour.Green), new WireState(2, WireColour.Orange), new WireState(3, WireColour.Purple) } },
		{ 'P', new[] { new WireState(1, WireColour.Orange), new WireState(4, WireColour.Orange) } },
		{ 'Q', new[] { new WireState(2, WireColour.Blue), new WireState(5, WireColour.Red) } },
		{ 'R', new[] { new WireState(3, WireColour.Yellow), new WireState(5, WireColour.White) } },
		{ 'S', new[] { new WireState(4, WireColour.Red), new WireState(5, WireColour.Green) } },
		{ 'T', new[] { new WireState(0, WireColour.Blue), new WireState(1, WireColour.Purple) } },
		{ 'U', new[] { new WireState(1, WireColour.White), new WireState(4, WireColour.Grey) } },
		{ 'V', new[] { new WireState(0, WireColour.Grey), new WireState(1, WireColour.Yellow) } },
		{ 'W', new[] { new WireState(3, WireColour.Black), new WireState(5, WireColour.Black) } },
		{ 'X', new[] { new WireState(0, WireColour.Black), new WireState(3, WireColour.Green) } },
		{ 'Y', new[] { new WireState(2, WireColour.Grey), new WireState(5, WireColour.Orange) } }
	};
	private static readonly string[] defaultWords = new[] {
		"almost", "answer", "around", "assert", "bother", "bundle", "cancel",
		"choose", "course", "demand", "easily", "expert", "facade", "family",
		"faulty", "health", "hollow", "inform", "inject", "insert", "inside",
		"jacket", "jockey", "kindly", "ladder", "latent", "magnet", "manual",
		"market", "nickel", "notice", "number", "occult", "octave", "paddle",
		"parent", "parsec", "patent", "person", "policy", "public", "racket",
		"random", "search", "second", "should", "tackle", "tangle", "topple",
		"tricky", "undone", "unisex", "verbal", "victor", "within", "worded",
	};

	private int[] orderToCut;

	public override void Start() {
		base.Start();
		this.Connector.WireCut += this.Connector_WireCut;

		var table = this.GetComponent<KMBombInfo>().GetSerialNumber().Any(c => c == 'A' || c == 'E' || c == 'I' || c == 'O' || c == 'U') ? defaultWireTableVowel : defaultWireTableNoVowel;
		string word;
		var letters = new char[6];
		int attempt = 0;
		while (true) {
			++attempt;
			if (attempt >= 1000) {
				// Hopefully this doesn't happen.
				this.Log("Couldn't generate a puzzle after 1000 attempts. Cut wires in any order.");
				this.orderToCut = new int[0];
				return;
			}
			word = defaultWords.PickRandom().ToUpperInvariant();

			int i;
			for (i = 0; i < word.Length; ++i) {
				WireState[] allStates;
				if (!table.TryGetValue(word[i], out allStates)) break;
				var states = allStates.Where(s => letters[s.Position] == '\0').ToList();
				WireState state;
				if (states.Count == 0) {
					// All wires that can represent this letter already have an associated letter.
					// Swap one of those associations to a different wire if possible.
					state = default(WireState);
					bool swapFound = false;
					foreach (var state2 in allStates) {
						states = table[letters[state2.Position]].Where(s => letters[s.Position] == '\0').ToList();
						if (states.Count > 0) {
							letters[states.First().Position] = letters[state2.Position];
							letters[state2.Position] = '\0';
							state = state2;
							swapFound = true;
							break;
						}
					}
					if (!swapFound) break;  // Otherwise, start over generating the puzzle.
				} else
					state = states.PickRandom();
				letters[state.Position] = word[i];
			}
			if (i >= word.Length) break;  // A valid puzzle has been generated.
			this.Log("Attempt {0}: failed after {1} letter(s) '{2}' - {3}.", attempt, i, word.ToLowerInvariant(), letters.Select(c => c == '\0' ? '_' : c).Join(""));
			for (i = 0; i < 6; ++i) letters[i] = '\0';
		}
		this.orderToCut = new int[word.Length];
		for (int i = 0; i < word.Length; ++i) {
			var wireIndex = this.orderToCut[i] = Array.IndexOf(letters, word[i]);
			letters[wireIndex] = (char) 1;
			this.Connector.Wires[wireIndex].Colour = table[word[i]].Where(s => s.Position == wireIndex).First().Colour;
		}
		this.Log(this.Connector.Wires.Select(w => w.Colour).Join(" "));
		this.Log("The password is '{0}'. The wires should be cut in this order: {1} (generated in {2} attempts).", word.ToLowerInvariant(), this.orderToCut.Select(i => i + 1).Join(", "), attempt);
	}

	private void Connector_WireCut(object sender, WireCutEventArgs e) {
		for (int i = 0; i < this.orderToCut.Length; ++i) {
			if (this.orderToCut[i] == e.WireIndex)
				break;
			else {
				// Make sure all wires that come before this one in the correct order are cut.
				if (!this.Connector.Wires[this.orderToCut[i]].Cut) {
					this.Log("Wire {0} was cut when the next wire should have been wire {1}.", e.WireIndex + 1, this.orderToCut[i] + 1);
					this.Connector.KMBombModule.HandleStrike();
					break;
				}
			}
		}
		if (this.Connector.Wires.All(w => w.Cut)) this.Disarm();
	}

	// Twitch Plays support
	public static readonly string TwitchHelpMessage
		= "!{0} cut 1 2 3 4 5 6 - wires are numbered from top to bottom.";
	public IEnumerator ProcessTwitchCommand(string command) {
		if (this.TwitchColourblindModeCommand(command)) { yield return null; yield break; }

		var tokens = command.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
		if (tokens.Length == 0) yield break;

		var indices = new List<int>();
		foreach (var token in tokens[0].EqualsIgnoreCase("cut") ? tokens.Skip(1) : tokens) {
			int i;
			if (!int.TryParse(token, out i) || i < 1 || i > 6) yield break;
			if (!indices.Contains(i)) indices.Add(i);
		}

		foreach (var index in indices) {
			yield return string.Format("strikemessage cutting wire {0}", index);
			this.Connector.TwitchCut(index - 1);
			yield return new WaitForSeconds(0.1f);
		}
	}

	public IEnumerator TwitchHandleForcedSolve() {
		for (int i = 0; i < this.orderToCut.Length; ++i) {
			var result = this.Connector.TwitchCut(this.orderToCut[i]);
			if (result) yield return new WaitForSeconds(0.1f);
		}
	}

	private struct WireState {
		public int Position;
		public WireColour Colour;

		public WireState(int position, WireColour colour) {
			this.Position = position;
			this.Colour = colour;
		}
	}
}
