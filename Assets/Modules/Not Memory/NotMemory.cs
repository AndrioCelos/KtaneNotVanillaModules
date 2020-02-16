using System.Collections;
using NotVanillaModulesLib;
using System;
using System.Collections.ObjectModel;
using KModkit;
using System.Linq;
using Random = UnityEngine.Random;
using UnityEngine;

public class NotMemory : NotVanillaModule<NotMemoryConnector> {
	public int Display { get; private set; }
	public int LightCount { get; private set; }
	public ReadOnlyCollection<int> Labels { get; private set; }

	private static readonly Rule[][] defaultRules = new[] {
		/* 0 */ new[] {
			new Rule(Conditions.DisplayIs(1), Actions.PressPositionFromLabel(1)),
			new Rule(Conditions.ButtonLabelIs(0, 4), Actions.PressLabelMatchingDisplay()),
			new Rule(Conditions.SerialNumberIsOdd(), Actions.PressLabel(1)),
			new Rule(null, Actions.PressPositionFromDisplay())
		},
		/* 1 */ new[] {
			new Rule(Conditions.NoBatteries(), Actions.PressLabel(4)),
			new Rule(Conditions.ButtonLabelIs(2, 1), Actions.PressPosition(0)),
			new Rule(Conditions.DisplayIs(3), Actions.PressPositionFromLabel(2)),
			new Rule(Conditions.ButtonLabelMatchesDisplay(1), Actions.PressPosition(2)),
			new Rule(null, Actions.PressLabel(1))
		},
		/* 2 */ new[] {
			new Rule(Conditions.DisplayIs(4), Actions.PressLabelMatchingPositionOfLabel(4)),
			new Rule(Conditions.ButtonLabelIs(1, 3), Actions.PressPosition(0)),
			new Rule(Conditions.ButtonLabelMatchesDisplay(3), Actions.PressPositionFromDisplay()),
			new Rule(null, Actions.PressPosition(1))
		},
		/* 3 */ new[] {
			new Rule(Conditions.PortPresent(Port.Parallel), Actions.PressPosition(0)),
			new Rule(Conditions.ButtonLabelIs(3, 1), Actions.PressPosition(1)),
			new Rule(Conditions.DisplayIs(2), Actions.PressLabelMatchingPositionOfLabel(3)),
			new Rule(Conditions.ButtonLabelMatchesDisplay(0), Actions.PressPositionFromDisplay()),
			new Rule(null, Actions.PressLabel(2))
		},
		/* 4 */ new[] {
			new Rule(Conditions.DisplayIs(3), Actions.PressPositionFromLabel(1)),
			new Rule(Conditions.ButtonLabelIs(1, 1), Actions.PressPosition(0)),
			new Rule(Conditions.ButtonLabelMatchesDisplay(0), Actions.PressPosition(1)),
			new Rule(Conditions.AtLeastNIndicators(4), Actions.PressLabel(3)),
			new Rule(Conditions.ButtonLabelIs(3, 4), Actions.PressPosition(3)),
			new Rule(null, Actions.PressLabel(2))
		},
		/* 5 */ new[] {
			new Rule(Conditions.NoPorts(), Actions.PressLabel(2)),
			new Rule(Conditions.DisplayIs(4), Actions.PressPositionFromLabel(3)),
			new Rule(Conditions.ButtonLabelIs(0, 3), Actions.PressPosition(2)),
			new Rule(Conditions.ButtonLabelMatchesDisplay(2), Actions.PressPositionFromDisplay()),
			new Rule(Conditions.ButtonLabelIsNot(1, 2), Actions.PressLabel(2)),
			new Rule(null, Actions.PressPosition(2))
		}
	};

	private KMBombInfo bombInfo;
	private int correctButton;

	public override void Start() {
		base.Start();
		this.bombInfo = this.GetComponent<KMBombInfo>();
		this.Connector.KMBombModule.OnActivate = this.KMBombModule_OnActivate;
		this.Connector.ButtonPressed += this.Connector_ButtonPressed;
		this.Connector.ButtonsSunk += this.Connector_ButtonsSunk;
		this.Connector.AnimationFinished += this.Connector_AnimationFinished;
		this.GeneratePuzzle();
	}

	private void GeneratePuzzle() {
		this.LightCount = Random.Range(0, 6);
		this.Display = Random.Range(1, 5);

		var labels = new[] { 1, 2, 3, 4 };
		this.Labels = Array.AsReadOnly(labels);
		labels.Shuffle();
		for (int i = 0; i < 4; ++i) this.Connector.SetButtonLabel(i, labels[i].ToString());

		var list = defaultRules[this.LightCount];
		foreach (var rule in list) {
			if (rule.Condition == null || rule.Condition(this, this.bombInfo)) {
				this.correctButton = rule.Action(this);
				break;
			}
		}
		this.Log("The display is {0}. {1} {2} lit. The button labels are '{3}'. The correct button is {4}.",
			this.Display, this.LightCount, this.LightCount == 1 ? "light is" : "lights are", labels.Join("', '"), this.DescribeButton(this.correctButton));
	}

	private void KMBombModule_OnActivate() {
		this.Connector.DisplayText = this.Display.ToString();
		this.Connector.Stage = this.LightCount;
		this.Connector.Activate();
	}

	private void Connector_AnimationFinished(object sender, EventArgs e) {
		this.Connector.Stage = this.LightCount;
	}

	private void Connector_ButtonPressed(object sender, KeypadButtonEventArgs e) {
		if (this.Solved || !this.Connector.InputValid) return;
		if (e.ButtonIndex == this.correctButton) {
			this.Log("You pressed {0}. That was correct.", this.DescribeButton(e.ButtonIndex));
			this.Disarm();
		} else {
			this.Log("You pressed {0}. That was incorrect: the correct button is {1}.", this.DescribeButton(e.ButtonIndex), this.DescribeButton(this.correctButton));
			this.Connector.KMBombModule.HandleStrike();
			this.Connector.AnimateButtons();
		}
	}

	private void Connector_ButtonsSunk(object sender, EventArgs e) {
		this.GeneratePuzzle();
	}

	private string DescribeButton(int index) {
		string ordinal;
		switch (index) {
			case 0: ordinal = "first"; break;
			case 1: ordinal = "second"; break;
			case 2: ordinal = "third"; break;
			case 3: ordinal = "fourth"; break;
			default: ordinal = index.ToString(); break;
		}
		return string.Format("the {0} button (labelled '{1}')", ordinal, this.Labels[index]);
	}

	// Twitch Plays support
	public static readonly string TwitchHelpMessage
		= "!{0} position 2 | !{0} pos 2 | !{0} p 2 - presses the button in the 2nd position | !{0} label 3 | !{0} lab 3 | !{0} l 3 - presses the button labelled 3";
	public IEnumerator ProcessTwitchCommand(string command) {
		var tokens = command.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
		if (tokens.Length != 2) yield break;

		int n;
		if (!int.TryParse(tokens[1], out n) || n < 1 || n > 4) yield break;

		if ("position".StartsWith(tokens[0], StringComparison.InvariantCultureIgnoreCase)) {
			yield return null;
			this.Connector.TwitchPress(n - 1);
		} else if ("label".StartsWith(tokens[0], StringComparison.InvariantCultureIgnoreCase)) {
			yield return null;
			this.Connector.TwitchPress(this.Labels.IndexOf(n));
		}
	}

	public IEnumerator TwitchHandleForcedSolve() {
		yield return new WaitWhile(() => this.Connector.Animating);
		this.Connector.TwitchPress(this.correctButton);
		yield break;
	}

	private delegate bool Condition(NotMemory module, KMBombInfo bombInfo);
	private delegate int ActionCheck(NotMemory module);

	private class Rule {
		public Condition Condition { get; private set; }
		public ActionCheck Action { get; private set; }

		public Rule(Condition condition, ActionCheck action) {
			this.Condition = condition;
			this.Action = action;
		}
	}

	private static class Conditions {
		public static Condition DisplayIs(int n) { return (m, i) => m.Display == n; }
		public static Condition ButtonLabelIs(int pos, int label) { return (m, i) => m.Labels[pos] == label; }
		public static Condition ButtonLabelIsNot(int pos, int label) { return (m, i) => m.Labels[pos] != label; }
		public static Condition SerialNumberIsOdd() { return (m, i) => i.GetSerialNumberNumbers().Last() % 2 != 0; }
		public static Condition NoBatteries() { return (m, i) => i.GetBatteryCount() == 0; }
		public static Condition NoPorts() { return (m, i) => i.GetPortCount() == 0; }
		public static Condition ButtonLabelMatchesDisplay(int pos) { return (m, i) => m.Labels[pos] == m.Display; }
		public static Condition PortPresent(Port portType) { return (m, i) => i.IsPortPresent(portType); }
		public static Condition AtLeastNIndicators(int n) { return (m, i) => i.GetIndicators().Count() >= n; }
	}

	private static class Actions {
		public static ActionCheck PressPositionFromLabel(int pos) { return m => m.Labels[pos] - 1; }
		public static ActionCheck PressLabelMatchingDisplay() { return m => m.Labels.IndexOf(m.Display); }
		public static ActionCheck PressPosition(int pos) { return m => pos; }
		public static ActionCheck PressLabel(int label) { return m => m.Labels.IndexOf(label); }
		public static ActionCheck PressPositionFromDisplay() { return m => m.Display - 1; }
		public static ActionCheck PressLabelMatchingPositionOfLabel(int label) { return m => m.Labels.IndexOf(m.Labels.IndexOf(label) + 1); }
	}
}
