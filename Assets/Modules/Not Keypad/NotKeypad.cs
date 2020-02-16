using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NotVanillaModulesLib;
using UnityEngine;
using Random = UnityEngine.Random;

public class NotKeypad : NotVanillaModule<NotKeypadConnector> {
	public Light[] Lights;

	private NotKeypadConnector.LightColour[] sequenceColours;
	private int[] sequenceButtons;
	private int[] correctButtons;
	private Coroutine coroutine;

	public int Stage { get; private set; }
	public int StageProgress { get; private set; }

	private static readonly NotKeypadConnector.LightColour[,,] defaultColourTable = new[, ,] {
		{ /* Copyright    */
			{ NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Cyan },
			{ NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Orange },
			{ NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Blue },
			{ NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.Yellow } },
		{ /* FilledStar   */
			{ NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Yellow },
			{ NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.Blue },
			{ NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Magenta },
			{ NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.White } },
		{ /* HollowStar   */
			{ NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.Red },
			{ NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Brown },
			{ NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Yellow },
			{ NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.Pink } },
		{ /* SmileyFace   */
			{ NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Magenta },
			{ NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Brown },
			{ NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.Green },
			{ NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Cyan } },
		{ /* DoubleK      */
			{ NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Purple },
			{ NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Green },
			{ NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Orange },
			{ NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Cyan } },
		{ /* Omega        */
			{ NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.Grey },
			{ NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Red },
			{ NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Purple },
			{ NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Cyan } },
		{ /* SquidKnife   */
			{ NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Cyan },
			{ NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.Orange },
			{ NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Green },
			{ NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Purple } },
		{ /* Pumpkin      */
			{ NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Grey },
			{ NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Purple },
			{ NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.Yellow },
			{ NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.Blue } },
		{ /* HookN        */
			{ NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Orange },
			{ NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Green },
			{ NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Purple },
			{ NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.Red } },
		{ /* Teepee       */
			{ NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Magenta },
			{ NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.White },
			{ NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Pink },
			{ NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Purple } },
		{ /* Six          */
			{ NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Red },
			{ NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.Brown },
			{ NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.Yellow },
			{ NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Purple } },
		{ /* SquigglyN    */
			{ NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.Yellow },
			{ NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.Magenta },
			{ NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Cyan },
			{ NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Red } },
		{ /* AT           */
			{ NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.Yellow },
			{ NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Brown },
			{ NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Purple },
			{ NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.Blue } },
		{ /* Ae           */
			{ NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Green },
			{ NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.Orange },
			{ NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.Blue },
			{ NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Magenta } },
		{ /* MeltedThree  */
			{ NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.Purple },
			{ NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Magenta },
			{ NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Cyan },
			{ NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.Green } },
		{ /* Euro         */
			{ NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.Red },
			{ NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Green },
			{ NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Blue },
			{ NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Purple } },
		{ /* Circle       */
			{ NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.White },
			{ NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.Grey },
			{ NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.Blue },
			{ NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Orange } },
		{ /* NWithHat     */
			{ NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Cyan },
			{ NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.Magenta },
			{ NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.White },
			{ NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.Brown } },
		{ /* Dragon       */
			{ NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Yellow },
			{ NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.Purple },
			{ NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.White },
			{ NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Cyan } },
		{ /* QuestionMark */
			{ NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Orange },
			{ NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Grey },
			{ NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Red },
			{ NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.Pink } },
		{ /* Paragraph    */
			{ NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.Yellow },
			{ NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Green },
			{ NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Grey },
			{ NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.Red } },
		{ /* RightC       */
			{ NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Grey },
			{ NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.Magenta },
			{ NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.White },
			{ NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.Pink } },
		{ /* LeftC        */
			{ NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.White },
			{ NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.Magenta },
			{ NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Red },
			{ NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Green } },
		{ /* Pitchfork    */
			{ NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.Red },
			{ NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Magenta },
			{ NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.Yellow },
			{ NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Green } },
		{ /* Tripod - not used */
			{ NotKeypadConnector.LightColour.Black, NotKeypadConnector.LightColour.Black, NotKeypadConnector.LightColour.Black },
			{ NotKeypadConnector.LightColour.Black, NotKeypadConnector.LightColour.Black, NotKeypadConnector.LightColour.Black },
			{ NotKeypadConnector.LightColour.Black, NotKeypadConnector.LightColour.Black, NotKeypadConnector.LightColour.Black },
			{ NotKeypadConnector.LightColour.Black, NotKeypadConnector.LightColour.Black, NotKeypadConnector.LightColour.Black } },
		{ /* Cursive      */
			{ NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.Brown },
			{ NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Red },
			{ NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Cyan },
			{ NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Yellow } },
		{ /* Tracks       */
			{ NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Grey },
			{ NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.Blue },
			{ NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.White },
			{ NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Blue } },
		{ /* Balloon      */
			{ NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.White },
			{ NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.Brown },
			{ NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.Cyan },
			{ NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.Grey } },
		{ /* WeirdNose    */
			{ NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.Green },
			{ NotKeypadConnector.LightColour.Blue, NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Pink },
			{ NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.White },
			{ NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Grey } },
		{ /* UpsideDownY  */
			{ NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Brown, NotKeypadConnector.LightColour.Blue },
			{ NotKeypadConnector.LightColour.Orange, NotKeypadConnector.LightColour.Pink, NotKeypadConnector.LightColour.White },
			{ NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Red },
			{ NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Purple } },
		{ /* BT           */
			{ NotKeypadConnector.LightColour.Purple, NotKeypadConnector.LightColour.White, NotKeypadConnector.LightColour.Brown },
			{ NotKeypadConnector.LightColour.Green, NotKeypadConnector.LightColour.Red, NotKeypadConnector.LightColour.Blue },
			{ NotKeypadConnector.LightColour.Grey, NotKeypadConnector.LightColour.Magenta, NotKeypadConnector.LightColour.Orange },
			{ NotKeypadConnector.LightColour.Yellow, NotKeypadConnector.LightColour.Cyan, NotKeypadConnector.LightColour.Pink } }
		};

	public override void Start() {
		base.Start();
		this.Connector.KMBombModule.OnActivate = this.KMBombModule_OnActivate;
		this.Connector.ButtonPressed += this.Connector_ButtonPressed;

		// Fix lights.
		var lightScale = this.transform.lossyScale.x;
		foreach (var light in this.Lights) light.range *= lightScale;

		var symbols = new NotKeypadConnector.Symbol[4];
		for (int i = 0; i < 4; ++i) {
			var symbol = (NotKeypadConnector.Symbol) Random.Range(0, 30);
			if (symbol == NotKeypadConnector.Symbol.Tripod) symbol = NotKeypadConnector.Symbol.BT;
			this.Connector.SetSymbol(i, symbols[i] = symbol);
		}

		this.correctButtons = new int[5];
		this.sequenceButtons = new int[5];
		this.sequenceColours = new NotKeypadConnector.LightColour[5];
		for (int i = 0; i < this.correctButtons.Length; ++i) {
			this.correctButtons[i] = Random.Range(0, 4);
			this.sequenceButtons[i] = Random.Range(0, 4);
			this.sequenceColours[i] = defaultColourTable[(int) symbols[this.sequenceButtons[i]], this.correctButtons[i], Random.Range(0, 3)];
		}

		for (int i = 0; i < this.correctButtons.Length; ++i) {
			string correctButtonString;
			switch (this.correctButtons[i]) {
				case 0: correctButtonString = "top left"; break;
				case 1: correctButtonString = "top right"; break;
				case 2: correctButtonString = "bottom left"; break;
				case 3: correctButtonString = "bottom right"; break;
				default: correctButtonString = "unknown"; break;
			}
			this.Log("Step {0}: {1} flashes {2}; press {3}.",
				i + 1, NotKeypadConnector.GetSymbolChar(symbols[this.sequenceButtons[i]]), this.sequenceColours[i].ToString().ToLowerInvariant(), correctButtonString);
		}

		this.Stage = 1;
	}

	private void KMBombModule_OnActivate() {
		this.coroutine = this.StartCoroutine(this.PlaySequenceCoroutine());
	}

	private IEnumerator PlaySequenceCoroutine() {
		while (true) {
			for (int i = 0; i < this.Stage; ++i) {
				this.Connector.SetLightColour(this.sequenceButtons[i], this.sequenceColours[i]);
				yield return new WaitForSeconds(0.4f);
				this.Connector.SetLightColour(this.sequenceButtons[i], NotKeypadConnector.LightColour.Black);
				yield return new WaitForSeconds(0.2f);
			}
			yield return new WaitForSeconds(2);
		}
	}

	private void ClearAllLights() {
		for (int i = 0; i < 4; ++i) this.Connector.SetLightColour(i, NotKeypadConnector.LightColour.Black);
	}

	private IEnumerator CorrectFlashCoroutine(int index) {
		this.ClearAllLights();
		this.Connector.SetLightColour(index, NotKeypadConnector.LightColour.Green);
		yield return new WaitForSeconds(5);
		this.Connector.SetLightColour(index, NotKeypadConnector.LightColour.Black);
		this.StageProgress = 0;
		this.coroutine = this.StartCoroutine(this.PlaySequenceCoroutine());
	}
	private IEnumerator StageCompleteFlashCoroutine(int index) {
		this.ClearAllLights();
		this.Connector.SetLightColour(index, NotKeypadConnector.LightColour.Green);
		yield return new WaitForSeconds(1);
		this.Connector.SetLightColour(index, NotKeypadConnector.LightColour.Black);
		if (!this.Solved) {
			yield return new WaitForSeconds(1);
			this.coroutine = this.StartCoroutine(this.PlaySequenceCoroutine());
		}
	}
	private IEnumerator WrongFlashCoroutine(int index) {
		this.ClearAllLights();
		this.Connector.SetLightColour(index, NotKeypadConnector.LightColour.Red);
		yield return new WaitForSeconds(1);
		this.Connector.SetLightColour(index, NotKeypadConnector.LightColour.Black);
		yield return new WaitForSeconds(1);
		this.coroutine = this.StartCoroutine(this.PlaySequenceCoroutine());
	}

	private void Connector_ButtonPressed(object sender, KeypadButtonEventArgs e) {
		if (this.Solved) return;
		if (this.coroutine != null)	this.StopCoroutine(this.coroutine);
		if (e.ButtonIndex == this.correctButtons[this.StageProgress]) {
			++this.StageProgress;
			if (this.StageProgress >= this.Stage) {
				this.Log("Stage {0} complete.", this.Stage);
				++this.Stage;
				this.StageProgress = 0;
				if (this.Stage > this.sequenceButtons.Length)
					this.Disarm();
				this.coroutine = this.StartCoroutine(this.StageCompleteFlashCoroutine(e.ButtonIndex));
			} else
				this.coroutine = this.StartCoroutine(this.CorrectFlashCoroutine(e.ButtonIndex));
		} else {
			this.Log("Button {0} was pressed. That was incorrect: the correct button was {1}.", e.ButtonIndex + 1, this.correctButtons[this.StageProgress] + 1);
			this.Connector.KMBombModule.HandleStrike();
			this.StageProgress = 0;
			this.coroutine = this.StartCoroutine(this.WrongFlashCoroutine(e.ButtonIndex));
		}
	}

	// Twitch Plays support
	public static readonly string TwitchHelpMessage
		= "!{0} press 1 3 2 4 - buttons are numbered in reading order | !{0} press TL BL TR BR";
	public IEnumerator ProcessTwitchCommand(string command) {
		var tokens = command.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
		if (tokens.Length == 0) yield break;

		var indices = new List<int>();
		foreach (var token in tokens[0].EqualsIgnoreCase("press") ? tokens.Skip(1) : tokens) {
			switch (token.ToUpperInvariant()) {
				case "1": case "TL": case "LT": indices.Add(0); break;
				case "2": case "TR": case "RT": indices.Add(1); break;
				case "3": case "BL": case "LB": indices.Add(2); break;
				case "4": case "BR": case "RB": indices.Add(3); break;
				default: yield break;
			}
		}

		for (int i = 0; i < indices.Count; ++i) {
			var index = indices[i];
			yield return string.Format("strikemessage pressing button {0} (#{1} in command)", index + 1, i + 1);
			this.Connector.TwitchPress(index);
			yield return new WaitForSeconds(0.1f);
		}
	}

	public IEnumerator TwitchHandleForcedSolve() {
		while (!this.Solved) {
			this.Connector.TwitchPress(this.correctButtons[this.StageProgress]);
			yield return new WaitForSeconds(0.1f);
		}
	}
}
