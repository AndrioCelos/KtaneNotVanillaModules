using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using KModkit;

using NotVanillaModulesLib;

using UnityEngine;

using Random = UnityEngine.Random;

public class NotKnob : NotVanillaModule<NotKnobConnector> {
	private static readonly Dictionary<char, bool[]> braille = new Dictionary<char, bool[]>() {
		{ 'A', new[] { true , false, false, false, false, false } },
		{ 'B', new[] { true , true , false, false, false, false } },
		{ 'C', new[] { true , false, false, true , false, false } },
		{ 'D', new[] { true , false, false, true , true , false } },
		{ 'E', new[] { true , false, false, false, true , false } },
		{ 'F', new[] { true , true , false, true , false, false } },
		{ 'G', new[] { true , true , false, true , true , false } },
		{ 'H', new[] { true , true , false, false, true , false } },
		{ 'I', new[] { false, true , false, true , false, false } },
		{ 'J', new[] { false, true , false, true , true , false } },
		{ 'K', new[] { true , false, true , false, false, false } },
		{ 'L', new[] { true , true , true , false, false, false } },
		{ 'M', new[] { true , false, true , true , false, false } },
		{ 'N', new[] { true , false, true , true , true , false } },
		{ 'O', new[] { true , false, true , false, true , false } },
		{ 'P', new[] { true , true , true , true , false, false } },
		{ 'Q', new[] { true , true , true , true , true , false } },
		{ 'R', new[] { true , true , true , false, true , false } },
		{ 'S', new[] { false, true , true , true , false, false } },
		{ 'T', new[] { false, true , true , true , true , false } },
		{ 'U', new[] { true , false, true , false, false, true  } },
		{ 'V', new[] { true , true , true , false, false, true  } },
		{ 'W', new[] { false, true , false, true , true , true  } },
		{ 'X', new[] { true , false, true , true , false, true  } },
		{ 'Y', new[] { true , false, true , true , true , true  } },
		{ 'Z', new[] { true , false, true , false, true , true  } }
	};
	private static readonly int[] leftLEDs = new[] { 6, 7, 8, 0, 1, 2 };
	private static readonly int[] rightLEDs = new[] { 5, 4, 3, 11, 10, 9 };

	private bool needyActive;
	private readonly char[,] letters = new char[4, 2];
	private readonly bool[] ledStates = new bool[12];
	private KnobPosition correctPosition;
	private readonly HashSet<char> serialNumberLetters = new HashSet<char>();

	public override void Start() {
		base.Start();
		this.Connector.KMNeedyModule.OnNeedyActivation = this.KMNeedyModule_OnNeedyActivation;
		this.Connector.KMNeedyModule.OnNeedyDeactivation = this.DisarmNeedy;
		this.Connector.KMNeedyModule.OnTimerExpired = this.KMNeedyModule_OnTimerExpired;
		this.Connector.Turned += this.Connector_Turned;
		this.serialNumberLetters.UnionWith(this.GetComponent<KMBombInfo>().GetSerialNumberLetters());
	}

	public void Update() {
		if (this.needyActive && !this.Connector.Panicking && this.Connector.KMNeedyModule.GetNeedyTimeRemaining() < 5) this.Connector.PanicLEDs();
	}

	private void KMNeedyModule_OnNeedyActivation() {
		this.needyActive = true;
		if (this.serialNumberLetters.Count > 0) {
			// For each incorrect position, with 50% chance, two letters that both don't match the condition are chosen. This makes it quicker on average to identify it as incorrect.
			this.correctPosition = (KnobPosition) Random.Range(0, 4);
			if (this.correctPosition == KnobPosition.Up) {
				// Both letters present in the serial number
				this.letters[0, 0] = this.GetLetter(true);
				this.letters[0, 1] = this.GetLetter(true);
			} else {
				var roll = Random.Range(0, 4);
				this.letters[0, 0] = this.GetLetter(roll == 0);
				this.letters[0, 1] = this.GetLetter(roll == 1);
			}
			if (this.correctPosition == KnobPosition.Down) {
				// Neither letter present in the serial number
				this.letters[1, 0] = this.GetLetter(false);
				this.letters[1, 1] = this.GetLetter(false);
			} else {
				var roll = Random.Range(0, 4);
				this.letters[1, 0] = this.GetLetter(roll != 0);
				this.letters[1, 1] = this.GetLetter(roll != 1);
			}
			if (this.correctPosition == KnobPosition.Left) {
				// Left letter present in the serial number
				this.letters[2, 0] = this.GetLetter(true);
				this.letters[2, 1] = this.GetLetter(false);
			} else {
				var roll = Random.Range(0, 4);
				this.letters[2, 0] = this.GetLetter(roll == 0);
				this.letters[2, 1] = this.GetLetter(roll != 1);
			}
			if (this.correctPosition == KnobPosition.Right) {
				// Right letter present in the serial number
				this.letters[3, 0] = this.GetLetter(false);
				this.letters[3, 1] = this.GetLetter(true);
			} else {
				var roll = Random.Range(0, 4);
				this.letters[3, 0] = this.GetLetter(roll != 0);
				this.letters[3, 1] = this.GetLetter(roll == 1);
			}
		} else {
			this.correctPosition = KnobPosition.Down;
			for (int i = 3; i >= 0; --i) {
				this.letters[i, 0] = (char) ('A' + Random.Range(0, 26));
				this.letters[i, 1] = (char) ('A' + Random.Range(0, 26));
			}
		}
		this.Log("Module active. The letters are as follows: Up: {0} {1}; Down: {2} {3}; Left: {4} {5}; Right: {6} {7}. The correct position is {8}.",
			this.letters[0, 0], this.letters[0, 1], this.letters[1, 0], this.letters[1, 1], this.letters[2, 0], this.letters[2, 1], this.letters[3, 0], this.letters[3, 1], this.correctPosition);
		this.UpdateLEDs();
		this.Connector.SetRotation((KnobPosition) Random.Range(0, 4));
	}

	private void DisarmNeedy() {
		this.needyActive = false;
		this.Connector.ClearLEDs();
	}

	private void KMNeedyModule_OnTimerExpired() {
		if (this.Connector.Position == this.correctPosition) {
			this.Log("Time's up. The knob is in the {0} position. That is correct.", this.correctPosition);
		} else {
			this.Log("Time's up. The knob is in the {0} position. That is incorrect: the correct position was {1}.", this.Connector.Position, this.correctPosition);
			this.Connector.KMNeedyModule.HandleStrike();
		}
		this.DisarmNeedy();
	}

	private void Connector_Turned(object sender, EventArgs e) {
		if (this.needyActive) this.UpdateLEDs();
	}

	private char GetLetter(bool present) {
		if (present) return this.serialNumberLetters.ElementAt(Random.Range(0, this.serialNumberLetters.Count));
		while (true) {
			var c = (char) ('A' + Random.Range(0, 26));
			if (!this.serialNumberLetters.Contains(c)) return c;
		}
	}

	private void UpdateLEDs() {
		var leftPattern = braille[this.letters[(int) this.Connector.Position, 0]];
		var rightPattern = braille[this.letters[(int) this.Connector.Position, 1]];
		for (int i = 5; i >= 0; --i) {
			this.ledStates[leftLEDs[i]] = leftPattern[i];
			this.ledStates[rightLEDs[i]] = rightPattern[i];
		}
		this.Connector.SetLEDs(this.ledStates);
	}

	// Twitch Plays support
	public static readonly string TwitchHelpMessage = "!{0} turn 2 - turn clockwise 2 times | !{0} cycle";

	public IEnumerator ProcessTwitchCommand(string command) {
		var tokens = command.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		int n; bool cycle = false;
		switch (tokens.Length) {
			case 1:
				if (tokens[0].EqualsAny("turn", "rotate")) n = 1;
				else if (tokens[0].EqualsIgnoreCase("cycle")) { n = 4; cycle = true; }
				else if (!int.TryParse(tokens[0], out n) || n <= 0 || n >= 4) yield break;
				break;
			case 2:
				if (!tokens[0].EqualsAny("turn", "rotate") || !int.TryParse(tokens[1], out n) || n <= 0 || n >= 4) yield break;
				break;
			default: yield break;
		}
		yield return null;
		for (; n > 0; --n) {
			this.Connector.TwitchTurn();
			yield return "trywaitcancel " + (cycle ? "2.5" : "0.2");
		}
	}
}
