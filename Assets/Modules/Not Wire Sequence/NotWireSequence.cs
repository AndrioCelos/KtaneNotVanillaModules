using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NotVanillaModulesLib;
using UnityEngine;
using Random = UnityEngine.Random;

public class NotWireSequence : NotVanillaModule<NotWireSequenceConnector> {
	private static readonly string[][] paragraphs = new[] {
		new[] { "The", "colour", "of", "each", "wire", "represents", "a", "paragraph",
			"within", "this", "manual", "and", "each", "number", "on", "the", "right",
			"corresponds", "to", "the", "word", "at", "that", "position", "into", "the",
			"paragraph", "indexing", "from", "zero" },
		new[] { "There", "will", "always", "be", "three", "wires", "on", "each", "panel", "For", "each", "pair", "of", "letter",
			"and", "number", "connected", "by", "a", "wire", "index", "that", "many", "words", "into", "the",
			"paragraph", "indicated", "by", "the", "colour", "of", "the", "wire", "and", "if", "the", "word", "located",
			"contains", "the", "letter", "connected", "to", "the", "wire", "cut", "it", "Otherwise", "leave", "it",
			"alone", "Repeat", "the", "procedure", "mentioned", "for", "all", "twelve", "wires" },
		new[] { "After", "all", "wires", "necessary", "have", "been", "cut", "press", "the", "down", "button", "to", "move",
			"on", "to", "the", "next", "panel", "The", "module", "will", "be", "disarmed", "after", "all", "four", "panels",
			"have", "had", "their", "required", "wires", "cut", "Cutting", "an", "incorrect", "wire", "will",
			"register", "a", "strike" },
		new[] { "The", "first", "word", "of", "each", "paragraph", "will", "be", "bolded", "and", "the", "last", "word", "of",
			"each", "paragraph", "will", "be", "underlined", "This", "is", "to", "ensure", "that", "the",
			"boundaries", "between", "each", "body", "of", "text", "are", "very", "clearly", "established" },
		new[] { "When", "locating", "a", "word", "from", "a", "paragraph", "starts", "from", "the", "first", "word", "at",
			"zero", "and", "count", "along", "until", "you", "have", "reached", "the", "desired", "number", "If", "the",
			"number", "given", "is", "larger", "than", "the", "amount", "of", "words", "in", "the", "paragraph", "move",
			"back", "to", "the", "first", "word", "of", "the", "paragraph", "after", "you", "have", "reached", "the", "end" }
	};

	private readonly string[] words = new string[12];
	private readonly bool[] shouldCut = new bool[12];

	public override void Start() {
		base.Start();
		this.Connector.KMBombModule.OnActivate = this.KMBombModule_OnActivate;
		this.Connector.WireCut += this.Connector_WireCut;
		this.Connector.UpPressed += this.Connector_UpPressed;
		this.Connector.DownPressed += this.Connector_DownPressed;
		this.RandomiseWires();
		this.Connector.InitialisePages();
	}

	private void KMBombModule_OnActivate() {
		this.Connector.MoveToPage(0);
	}

	private void RandomiseWires() {
		for (int i = 0; i < this.Connector.Pages.Count; ++i) {
			var page = this.Connector.Pages[i];
			var toIndices = new[] { 0, 1, 2 };
			toIndices.Shuffle();
			for (int j = 0; j < page.Wires.Count; ++j) {
				var wire = page.Wires[j];
				wire.To = toIndices[j];
				wire.Colour = (WireSequenceColour) Random.Range(0, 5);
				int index = Random.Range(0, 50);
				page.Wires[wire.To].Number = index.ToString();

				var paragraph = paragraphs[(int) wire.Colour];
				var word = paragraph[index % paragraph.Length];
				this.words[i * 3 + j] = word.ToLowerInvariant();
				// 50% chance to guarantee that the wire should be cut.
				if (Random.Range(0, 2) == 0) {
					wire.Letter = char.ToUpper(word.PickRandom()).ToString();
					this.shouldCut[i * 3 + j] = true;
				} else {
					wire.Letter = ((char) ('A' + Random.Range(0, 26))).ToString();
					this.shouldCut[i * 3 + j] = word.ContainsIgnoreCase(wire.Letter);
				}

				this.Log("Panel {0} wire {1}: '{2}' in the {3} paragraph, word {4} ('{5}'): {6}.",
					i + 1, j + 1, wire.Letter, wire.Colour.ToString().ToLowerInvariant(), index, this.words[i * 3 + j], this.shouldCut[i * 3 + j] ? "cut" : "do not cut");
			}
		}
	}

	private void Connector_UpPressed(object sender, EventArgs e) {
		if (this.Solved || this.Connector.Animating || this.Connector.CurrentPage == 0) return;
		this.Connector.MoveToPage(this.Connector.CurrentPage - 1);
	}

	private void Connector_DownPressed(object sender, EventArgs e) {
		if (this.Solved || this.Connector.Animating) return;

		if (this.Connector.CurrentPage >= this.Connector.Stage) {
			for (int i = 0; i < 3; ++i) {
				if (this.shouldCut[this.Connector.CurrentPage * 3 + i] && !this.Connector.Pages[this.Connector.CurrentPage].Wires[i].Cut) {
					this.Log("Attempted to move past panel {0} when wire {1} still needs to be cut.", this.Connector.CurrentPage + 1, i + 1);
					this.Connector.KMBombModule.HandleStrike();
					return;
				}
			}
			++this.Connector.Stage;
			if (this.Connector.Stage >= 4) this.Disarm();
		}
		this.Connector.MoveToPage(this.Connector.CurrentPage + 1);
	}

	private void Connector_WireCut(object sender, WireCutEventArgs e) {
		if (!this.shouldCut[e.WireIndex]) {
			this.Log("Wire {0} on panel {1} was incorrectly cut.", e.WireIndex % 3 + 1, e.WireIndex / 3 + 1);
			this.Connector.KMBombModule.HandleStrike();
		}
	}
		// Twitch Plays support
	public static readonly string TwitchHelpMessage
		= "!{0} cut 1 - cuts the wire at the first letter on the current panel | !{0} cut E - cuts the wire with letter E | !{0} down | !{0} up | | !{0} d | !{0} u | !{0} cut 1 2 3 d";
	public IEnumerator ProcessTwitchCommand(string command) {
		var tokens = command.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
		if (tokens.Length == 0) yield break;

		switch (tokens[0].ToLowerInvariant()) {
			case "down": case "d":
				yield return "strikemessage pressing down";
				this.Connector.TwitchMoveDown();
				yield return new WaitForSeconds(1.5f);
				break;
			case "up": case "u":
				yield return null;
				this.Connector.TwitchMoveUp();
				yield return new WaitForSeconds(1.5f);
				break;
			case "cut": case "c":
				bool down = false; var wireIndices = new List<int>();
				for (int i = 1; i < tokens.Length; ++i) {
					var token = tokens[i];
					if (token.Length == 1) {
						if (char.IsDigit(token[0])) {
							if (token[0] < '1' || token[0] > '3') yield break;
							wireIndices.Add(token[0] - '1');
						} else if ((token[0] == 'd' || token[0] == 'D') && i == tokens.Length - 1 && i > 1) {
							down = true;
						} else {
							var index = this.Connector.Pages[this.Connector.CurrentPage].Wires.IndexOf(w => w.Letter[0] == char.ToUpperInvariant(token[0]));
							if (index < 0) {
								yield return string.Format("sendtochaterror Letter {0} was not found on this panel.", char.ToUpperInvariant(token[0]));
								yield break;
							}
							if (this.Connector.Pages[this.Connector.CurrentPage].Wires.Skip(index + 1).Any(w => w.Letter[0] == char.ToUpperInvariant(token[0]))) {
								yield return string.Format("sendtochaterror Letter {0} appears multiple times on this panel.", char.ToUpperInvariant(token[0]));
								yield break;
							}
							wireIndices.Add(index);
						}
					} else if (token.EqualsIgnoreCase("down") && i == tokens.Length - 1 && i > 1)
						down = true;
					else
						yield break;
				}
				if (wireIndices.Count == 0) yield break;
				foreach (var index in wireIndices) {
					yield return string.Format("strikemessage cutting wire {0}", index + 1);
					this.Connector.TwitchCut(index);
					yield return new WaitForSeconds(0.1f);
				}
				if (down) {
					yield return "strikemessage pressing down";
					this.Connector.TwitchMoveDown();
					yield return new WaitForSeconds(1.5f);
				}
				break;
		}

	}

	public IEnumerator TwitchHandleForcedSolve() {
		while (!this.Solved) {
			for (int i = 0; i < 3; ++i) {
				if (this.shouldCut[this.Connector.CurrentPage * 3 + i] && !this.Connector.Pages[this.Connector.CurrentPage].Wires[i].Cut) {
					this.Connector.TwitchCut(i);
					yield return new WaitForSeconds(0.1f);
				}
			}
			this.Connector.TwitchMoveDown();
			yield return new WaitWhile(() => this.Connector.Animating);
		}
	}
}
