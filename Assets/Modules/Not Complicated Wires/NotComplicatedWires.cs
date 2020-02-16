using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NotVanillaModulesLib;
using KModkit;
using UnityEngine;

public class NotComplicatedWires : NotVanillaModule<NotComplicatedWiresConnector> {
	private KMBombInfo kmBombInfo;
	private readonly bool[] shouldCut = new bool[6];
	private List<int> activeSpaceIndices;

	private static readonly Dictionary<char, Operation[]> defaultOperationTable = new Dictionary<char, Operation[]>();

	static NotComplicatedWires() {
		defaultOperationTable['1'] = defaultOperationTable['6'] = defaultOperationTable['E'] = defaultOperationTable['J'] =
			new[] { Operation.AND, Operation.XOR, Operation.OR, Operation.XNOR, Operation.IMPLIES, Operation.NAND };
		defaultOperationTable['2'] = defaultOperationTable['7'] = defaultOperationTable['D'] = defaultOperationTable['I'] =
			new[] { Operation.OR, Operation.IMPLIES, Operation.NAND, Operation.AND, Operation.XOR, Operation.XNOR };
		defaultOperationTable['3'] = defaultOperationTable['8'] = defaultOperationTable['C'] = defaultOperationTable['H'] =
			new[] { Operation.XNOR, Operation.AND, Operation.XOR, Operation.IMPLIES, Operation.NAND, Operation.OR };
		defaultOperationTable['4'] = defaultOperationTable['9'] = defaultOperationTable['B'] = defaultOperationTable['G'] =
			new[] { Operation.IMPLIES, Operation.NAND, Operation.XNOR, Operation.XOR, Operation.OR, Operation.AND };
		defaultOperationTable['5'] = defaultOperationTable['0'] = defaultOperationTable['A'] = defaultOperationTable['F'] =
			new[] { Operation.XOR, Operation.OR, Operation.AND, Operation.NAND, Operation.XNOR, Operation.IMPLIES };
		defaultOperationTable['\0'] =
			new[] { Operation.NAND, Operation.XNOR, Operation.IMPLIES, Operation.OR, Operation.AND, Operation.XOR };
	}

	public override void Start () {
		base.Start();
		this.kmBombInfo = this.GetComponent<KMBombInfo>();
		this.Connector.KMBombModule.OnActivate = this.KMBombModule_OnActivate;

		var indices = Enumerable.Range(0, 6).ToList();

		// Maybe remove some wires.
		var wireCount = UnityEngine.Random.Range(4, 7);
		for (int i = wireCount; i < this.Connector.WireSpaces.Count; ++i) {
			var j = UnityEngine.Random.Range(0, indices.Count);
			this.Connector.WireSpaces[indices[j]].Empty = true;
			indices.RemoveAt(j);
		}
		this.activeSpaceIndices = indices;

		// Guarantee that at least one wire should be cut.
		var guaranteedIndex = indices.PickRandom();
		for (int i = 0; i < indices.Count; ++i) {
			var index = indices[i];
			var colours = (ComplicatedWireColours) UnityEngine.Random.Range(1, 7);
			var operation = this.GetOperation(colours);
			var space = this.Connector.WireSpaces[index];
			space.Colours = colours;
			if (index == guaranteedIndex) {
				int bits;
				switch (operation) {
					case Operation.AND: space.HasSymbol = true; space.LightOn = true; break;
					case Operation.OR: bits = UnityEngine.Random.Range(1, 4); space.HasSymbol = bits % 2 != 0; space.LightOn = bits / 2 != 0; break;
					case Operation.XOR: space.HasSymbol = UnityEngine.Random.Range(0, 2) != 0; space.LightOn = !space.HasSymbol; break;
					case Operation.NAND: bits = UnityEngine.Random.Range(0, 3); space.HasSymbol = bits % 2 != 0; space.LightOn = bits / 2 != 0; break;
					case Operation.NOR: space.HasSymbol = false; space.LightOn = false; break;
					case Operation.XNOR: space.HasSymbol = space.LightOn = UnityEngine.Random.Range(0, 2) != 0; break;
					case Operation.IMPLIES:
						bits = UnityEngine.Random.Range(0, 3);
						if (bits == 1) bits = 3;
						space.HasSymbol = bits % 2 != 0; space.LightOn = bits / 2 != 0;
						break;
				}
				this.shouldCut[index] = true;
			} else {
				var a = space.HasSymbol = UnityEngine.Random.Range(0, 2) != 0;
				var b = space.LightOn = UnityEngine.Random.Range(0, 2) != 0;
				switch (operation) {
					case Operation.AND: this.shouldCut[index] = a && b; break;
					case Operation.OR: this.shouldCut[index] = a || b; break;
					case Operation.XOR: this.shouldCut[index] = a ^ b; break;
					case Operation.NAND: this.shouldCut[index] = !(a && b); break;
					case Operation.NOR: this.shouldCut[index] = !(a || b); break;
					case Operation.XNOR: this.shouldCut[index] = a == b; break;
					case Operation.IMPLIES: this.shouldCut[index] = b || !a; break;
				}
			}
			this.Connector.Log("Wire {0}: {1} (symbol) {2} {3} (light) is {4}.",
				i + 1, space.HasSymbol, this.GetOperation(space.Colours), space.LightOn, this.shouldCut[index]);
		}

		this.Connector.UpdateSelectable();
		this.Connector.WireCut += this.Connector_WireCut;
	}

	private Operation GetOperation(ComplicatedWireColours colours) {
		Operation[] array;
		if (!defaultOperationTable.TryGetValue(this.kmBombInfo.GetSerialNumber()[1], out array))
			array = defaultOperationTable['\0'];
		return array[(int) colours - 1];
	}

	private void KMBombModule_OnActivate() {
		foreach (var space in this.Connector.WireSpaces) space.Activate();
	}

	private void Connector_WireCut(object sender, WireCutEventArgs e) {
		var space = this.Connector.WireSpaces[e.WireIndex];
		var wireIndex = this.activeSpaceIndices.IndexOf(e.WireIndex) + 1;

		if (this.shouldCut[e.WireIndex]) {
			this.Log("Wire {0} was cut. That was correct.", wireIndex);
			if (Enumerable.Range(0, 6).All(i => !this.shouldCut[i] || this.Connector.WireSpaces[i].Cut))
				this.Connector.KMBombModule.HandlePass();
		} else {
			this.Log("Wire {0} was cut. That was incorrect: {1} (symbol) {2} {3} (light) is false",
				wireIndex, space.HasSymbol, this.GetOperation(space.Colours), space.LightOn);
			this.Connector.KMBombModule.HandleStrike();
			// Yes, you can still strike after disarming the module. Complicated Wires also does this.
		}
	}

	// Twitch Plays support
	public static readonly string TwitchHelpMessage
		= "!{0} cut 2 3 6 - wires are numbered from left to right; empty spaces are not counted.";
	public IEnumerator ProcessTwitchCommand(string command) {
		var tokens = command.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
		if (tokens.Length == 0) yield break;

		var indices = new List<int>();
		foreach (var token in tokens[0].EqualsIgnoreCase("cut") ? tokens.Skip(1) : tokens) {
			int i;
			if (!int.TryParse(token, out i) || i < 1 || i > this.activeSpaceIndices.Count) yield break;
			if (!indices.Contains(i)) indices.Add(i);
		}

		foreach (var index in indices) {
			yield return string.Format("strikemessage cutting wire {0}", index);
			this.Connector.TwitchCut(this.activeSpaceIndices[index - 1]);
			yield return new WaitForSeconds(0.1f);
		}
	}

	public IEnumerator TwitchHandleForcedSolve() {
		for (int i = 0; i < this.shouldCut.Length; ++i) {
			if (this.shouldCut[i]) {
				var result = this.Connector.TwitchCut(i);
				if (result) yield return new WaitForSeconds(0.1f);
			}
		}
	}

	private enum Operation {
		AND,
		OR,
		XOR,
		NAND,
		NOR,
		XNOR,
		IMPLIES
	}
}
