using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KModkit;
using NotVanillaModulesLib;
using UnityEngine;
using Random = UnityEngine.Random;

public class NotVentingGas : NotVanillaModule<NotVentingGasConnector> {
	public string DisplayText { get; private set; }

	private Coroutine coroutine;
	private int value;
	private VentingGasButton correctButton;

	private static readonly string[] prompts = new[] {
		"VENT GAS", "VENT", "DETONATE", "DEFUSE", "DISARM", "DISABLE", "DISASSEMBLE",
		"CONTINUE", "PROCEED", "ACCEPT", "YES", "NO", "ACTIVATE", "DEACTIVATE", "SUCCESS", "ERROR"
	};
	private static readonly string[] punctuation = new[] { "", ".", "?", "!" };
	private static readonly int[,] defaultValues = new[,] {
		{ 5, 8, 4, 9 },
		{ 1, 1, 4, 6 },
		{ 3, 5, 8, 7 },
		{ 9, 5, 6, 1 },
		{ 6, 2, 6, 6 },
		{ 8, 8, 0, 3 },
		{ 6, 7, 9, 7 },
		{ 1, 1, 3, 6 },
		{ 6, 5, 1, 8 },
		{ 7, 2, 7, 3 },
		{ 0, 8, 6, 8 },
		{ 4, 4, 2, 8 },
		{ 1, 5, 4, 9 },
		{ 0, 3, 0, 1 },
		{ 2, 0, 3, 1 },
		{ 8, 8, 5, 2 }
	};

	public override void Start() {
		base.Start();
		this.Connector.KMNeedyModule.OnNeedyActivation = this.KMNeedyModule_OnNeedyActivation;
		this.Connector.KMNeedyModule.OnNeedyDeactivation = this.DisarmNeedy;
		this.Connector.KMNeedyModule.OnTimerExpired = this.KMNeedyModule_OnTimerExpired;
		this.Connector.ButtonPressed += this.Connector_ButtonPressed;
		this.value = this.GetComponent<KMBombInfo>().GetSerialNumberNumbers().LastOrDefault();
		this.Connector.InputText = "";
	}

	public void DisarmNeedy() {
		this.Connector.InputText = "";
		this.Connector.DisplayActive = false;
		if (this.coroutine != null) {
			this.StopCoroutine(this.coroutine);
			this.coroutine = null;
		}
	}

	private void KMNeedyModule_OnNeedyActivation() {
		// Bias the selection towards 'VENT GAS' and 'DETONATE'.
		int i = Random.Range(0, 8);
		if (i < 3) i = 0;
		else if (i == 3) i = 2;
		else i = Random.Range(0, prompts.Length);
		int j = Random.Range(0, punctuation.Length);
		this.DisplayText = prompts[i] + punctuation[j];
		this.Connector.DisplayText = this.DisplayText;
		this.Connector.DisplayActive = true;

		var value = defaultValues[i, j];
		this.correctButton = value > this.value ? VentingGasButton.Y : VentingGasButton.N;
		this.Log("Module active. The display reads '{0}{1}'. The value is {2}; the last value was {3}. The correct button is {4}.",
			prompts[i], punctuation[j], value, this.value, this.correctButton);
		this.value = value;
	}

	private void KMNeedyModule_OnTimerExpired() {
		this.Log("You didn't press the button in time.");
		this.Connector.KMNeedyModule.HandleStrike();
		this.DisarmNeedy();
	}

	private void Connector_ButtonPressed(object sender, VentingGasButtonEventArgs e) {
		if (this.Connector.DisplayActive && this.coroutine == null) this.coroutine = this.StartCoroutine(this.ButtonPressedCoroutine(e.Button));
	}

	private IEnumerator ButtonPressedCoroutine(VentingGasButton button) {
		var label = button == VentingGasButton.N ? "NO" : "YES";
		foreach (var c in label) {
			this.Connector.InputText += c;
			yield return new WaitForSeconds(0.5f);
		}
		yield return new WaitForSeconds(0.5f);
		if (this.Connector.DisplayActive) {
			if (button == this.correctButton)
				this.Log("You pressed {0}. That was correct.", button);
			else {
				this.Log("You pressed {0}. That was incorrect.", button);
				this.Connector.KMNeedyModule.HandleStrike();
			}
			this.Connector.KMNeedyModule.HandlePass();
			this.DisarmNeedy();
		}
	}

	// Twitch Plays support
	public static readonly string TwitchHelpMessage
		= "!{0} N | !{0} Y";

	public IEnumerator ProcessTwitchCommand(string command) {
		var tokens = command.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		string buttonString; VentingGasButton button;
		switch (tokens.Length) {
			case 1: buttonString = tokens[0]; break;
			case 2:
				if (!tokens[0].EqualsIgnoreCase("press")) yield break;
				buttonString = tokens[1];
				break;
			default: yield break;
		}
		if (buttonString.Length == 0) yield break;
		switch (buttonString[0]) {
			case 'n': case 'N': button = VentingGasButton.N; break;
			case 'y': case 'Y': button = VentingGasButton.Y; break;
			default: yield break;
		}
		yield return null;
		this.Connector.TwitchPress(button);
	}
}
