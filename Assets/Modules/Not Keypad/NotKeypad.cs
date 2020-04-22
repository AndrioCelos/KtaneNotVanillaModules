using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NotVanillaModulesLib;
using UnityEngine;
using Random = UnityEngine.Random;
using C = NotVanillaModulesLib.NotKeypadConnector.LightColour;

#pragma warning disable IDE0001, IDE0002  // Name can be simplified

public class NotKeypad : NotVanillaModule<NotKeypadConnector> {
	public Light[] Lights;

	private NotKeypadConnector.LightColour[] sequenceColours;
	private int[] sequenceButtons;
	private int[] correctButtons;
	private Coroutine coroutine;

	public int Stage { get; private set; }
	public int StageProgress { get; private set; }

	private readonly List<int> lightShowPresses = new List<int>();

	private static readonly NotKeypadConnector.LightColour[,,] defaultColourTable = new[,,] {
		{ /* Copyright    */
			{ C.White, C.Grey, C.Cyan },
			{ C.Magenta, C.Pink, C.Orange },
			{ C.Purple, C.Brown, C.Blue },
			{ C.Green, C.Red, C.Yellow } },
		{ /* FilledStar   */
			{ C.Cyan, C.Brown, C.Yellow },
			{ C.Pink, C.Purple, C.Blue },
			{ C.Red, C.Green, C.Magenta },
			{ C.Orange, C.Grey, C.White } },
		{ /* HollowStar   */
			{ C.Green, C.Purple, C.Red },
			{ C.Orange, C.Magenta, C.Brown },
			{ C.Grey, C.Blue, C.Yellow },
			{ C.White, C.Cyan, C.Pink } },
		{ /* SmileyFace   */
			{ C.Orange, C.Blue, C.Magenta },
			{ C.Yellow, C.Grey, C.Brown },
			{ C.Pink, C.Purple, C.Green },
			{ C.Red, C.White, C.Cyan } },
		{ /* DoubleK      */
			{ C.Brown, C.Grey, C.Purple },
			{ C.Yellow, C.Blue, C.Green },
			{ C.White, C.Pink, C.Orange },
			{ C.Red, C.Magenta, C.Cyan } },
		{ /* Omega        */
			{ C.White, C.Orange, C.Grey },
			{ C.Green, C.Magenta, C.Red },
			{ C.Blue, C.Pink, C.Purple },
			{ C.Yellow, C.Brown, C.Cyan } },
		{ /* SquidKnife   */
			{ C.Pink, C.Yellow, C.Cyan },
			{ C.Grey, C.Red, C.Orange },
			{ C.Magenta, C.Brown, C.Green },
			{ C.Blue, C.White, C.Purple } },
		{ /* Pumpkin      */
			{ C.Pink, C.Magenta, C.Grey },
			{ C.Cyan, C.White, C.Purple },
			{ C.Green, C.Orange, C.Yellow },
			{ C.Brown, C.Red, C.Blue } },
		{ /* HookN        */
			{ C.White, C.Magenta, C.Orange },
			{ C.Pink, C.Blue, C.Green },
			{ C.Brown, C.Grey, C.Purple },
			{ C.Yellow, C.Cyan, C.Red } },
		{ /* Teepee       */
			{ C.Grey, C.Yellow, C.Magenta },
			{ C.Blue, C.Orange, C.White },
			{ C.Cyan, C.Green, C.Pink },
			{ C.Red, C.Brown, C.Purple } },
		{ /* Six          */
			{ C.White, C.Magenta, C.Red },
			{ C.Green, C.Cyan, C.Brown },
			{ C.Grey, C.Orange, C.Yellow },
			{ C.Pink, C.Blue, C.Purple } },
		{ /* SquigglyN    */
			{ C.Blue, C.Grey, C.Yellow },
			{ C.White, C.Purple, C.Magenta },
			{ C.Pink, C.Green, C.Cyan },
			{ C.Orange, C.Brown, C.Red } },
		{ /* AT           */
			{ C.Green, C.Orange, C.Yellow },
			{ C.Cyan, C.Grey, C.Brown },
			{ C.Pink, C.Magenta, C.Purple },
			{ C.White, C.Red, C.Blue } },
		{ /* Ae           */
			{ C.Cyan, C.Yellow, C.Green },
			{ C.Grey, C.Purple, C.Orange },
			{ C.Pink, C.Red, C.Brown },
			{ C.White, C.Blue, C.Magenta } },
		{ /* MeltedThree  */
			{ C.White, C.Red, C.Purple },
			{ C.Blue, C.Brown, C.Magenta },
			{ C.Grey, C.Pink, C.Cyan },
			{ C.Yellow, C.Orange, C.Green } },
		{ /* Euro         */
			{ C.Orange, C.Cyan, C.Red },
			{ C.Grey, C.White, C.Green },
			{ C.Pink, C.Brown, C.Blue },
			{ C.Yellow, C.Magenta, C.Purple } },
		{ /* Circle       */
			{ C.Pink, C.Red, C.White },
			{ C.Green, C.Purple, C.Grey },
			{ C.Magenta, C.Cyan, C.Blue },
			{ C.Brown, C.Yellow, C.Orange } },
		{ /* NWithHat     */
			{ C.Green, C.Grey, C.Cyan },
			{ C.Yellow, C.Red, C.Magenta },
			{ C.Pink, C.Orange, C.White },
			{ C.Blue, C.Purple, C.Brown } },
		{ /* Dragon       */
			{ C.Pink, C.Brown, C.Yellow },
			{ C.Orange, C.Red, C.Purple },
			{ C.Green, C.Blue, C.White },
			{ C.Magenta, C.Grey, C.Cyan } },
		{ /* QuestionMark */
			{ C.Green, C.Yellow, C.Orange },
			{ C.Blue, C.White, C.Grey },
			{ C.Purple, C.Brown, C.Red },
			{ C.Magenta, C.Cyan, C.Pink } },
		{ /* Paragraph    */
			{ C.Orange, C.Cyan, C.Yellow },
			{ C.White, C.Pink, C.Green },
			{ C.Brown, C.Magenta, C.Grey },
			{ C.Blue, C.Purple, C.Red } },
		{ /* RightC       */
			{ C.Blue, C.Green, C.Grey },
			{ C.Red, C.Cyan, C.Magenta },
			{ C.Brown, C.Orange, C.White },
			{ C.Yellow, C.Purple, C.Pink } },
		{ /* LeftC        */
			{ C.Pink, C.Brown, C.White },
			{ C.Cyan, C.Purple, C.Magenta },
			{ C.Yellow, C.Blue, C.Red },
			{ C.Orange, C.Grey, C.Green } },
		{ /* Pitchfork    */
			{ C.Cyan, C.Purple, C.Red },
			{ C.Brown, C.White, C.Magenta },
			{ C.Pink, C.Orange, C.Yellow },
			{ C.Grey, C.Blue, C.Green } },
		{ /* Tripod - not used */
			{ C.Black, C.Black, C.Black },
			{ C.Black, C.Black, C.Black },
			{ C.Black, C.Black, C.Black },
			{ C.Black, C.Black, C.Black } },
		{ /* Cursive      */
			{ C.Grey, C.Purple, C.Brown },
			{ C.Orange, C.White, C.Red },
			{ C.Pink, C.Magenta, C.Cyan },
			{ C.Blue, C.Green, C.Yellow } },
		{ /* Tracks       */
			{ C.Pink, C.Yellow, C.Grey },
			{ C.Red, C.Orange, C.Blue },
			{ C.Green, C.Cyan, C.White },
			{ C.Purple, C.Magenta, C.Brown } },
		{ /* Balloon      */
			{ C.Red, C.Magenta, C.White },
			{ C.Yellow, C.Purple, C.Brown },
			{ C.Blue, C.Orange, C.Cyan },
			{ C.Green, C.Pink, C.Grey } },
		{ /* WeirdNose    */
			{ C.Purple, C.Cyan, C.Green },
			{ C.Blue, C.Brown, C.Pink },
			{ C.Magenta, C.Orange, C.White },
			{ C.Red, C.Yellow, C.Grey } },
		{ /* UpsideDownY  */
			{ C.Grey, C.Brown, C.Blue },
			{ C.Orange, C.Pink, C.White },
			{ C.Cyan, C.Yellow, C.Red },
			{ C.Green, C.Magenta, C.Purple } },
		{ /* BT           */
			{ C.Purple, C.White, C.Brown },
			{ C.Green, C.Red, C.Blue },
			{ C.Grey, C.Magenta, C.Orange },
			{ C.Yellow, C.Cyan, C.Pink } }
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
		if (this.coroutine != null) this.StopCoroutine(this.coroutine);

		if (this.Solved) {
			this.HandleLightShow(e.ButtonIndex);
			return;
		}

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

	private void HandleLightShow(int buttonIndex) {
		this.lightShowPresses.Remove(buttonIndex);
		this.lightShowPresses.Add(buttonIndex);
		switch (this.lightShowPresses.Count) {
			case 2:
				if ((buttonIndex ^ 3) == this.lightShowPresses[0])
					this.coroutine = this.StartCoroutine(this.LightShowCoroutine(buttonIndex, this.LightShow2B));
				else
					this.coroutine = this.StartCoroutine(this.LightShowCoroutine(buttonIndex, this.LightShow2A));
				break;
			case 3:
				if ((this.lightShowPresses[0] ^ 3) == this.lightShowPresses[1])
					this.coroutine = this.StartCoroutine(this.LightShowCoroutine(buttonIndex, this.LightShow3B));
				else if ((this.lightShowPresses[1] ^ 3) == this.lightShowPresses[2])
					this.coroutine = this.StartCoroutine(this.LightShowCoroutine(buttonIndex, this.LightShow3C));
				else
					this.coroutine = this.StartCoroutine(this.LightShowCoroutine(buttonIndex, this.LightShow3A));
				break;
			case 4:
				if ((this.lightShowPresses[0] ^ 3) == this.lightShowPresses[2] && (this.lightShowPresses[1] ^ 3) == this.lightShowPresses[3] &&
					this.lightShowPresses[0] != this.lightShowPresses[1] && this.lightShowPresses[0] != this.lightShowPresses[3])
					this.coroutine = this.StartCoroutine(this.LightShowCoroutine(buttonIndex, this.LightShow4A));
				else if ((this.lightShowPresses[0] ^ 2) == this.lightShowPresses[1] && (this.lightShowPresses[2] ^ 2) == this.lightShowPresses[3] &&
					(this.lightShowPresses[0] & 1) != (this.lightShowPresses[2] & 1))
					this.coroutine = this.StartCoroutine(this.LightShowCoroutine(buttonIndex, this.LightShow4B));
				else if ((this.lightShowPresses[0] ^ 1) == this.lightShowPresses[1] && (this.lightShowPresses[2] ^ 1) == this.lightShowPresses[3] &&
					(this.lightShowPresses[0] & 2) != (this.lightShowPresses[2] & 2))
					this.coroutine = this.StartCoroutine(this.LightShowCoroutine(buttonIndex, this.LightShow4C));
				else
					this.coroutine = this.StartCoroutine(this.LightShowCoroutine(buttonIndex, this.LightShow4D));
				break;
			default:
				this.coroutine = this.StartCoroutine(this.LightShowCoroutine(buttonIndex, this.LightShow1));
				break;
		}
	}

	private IEnumerator LightShowCoroutine(int index, Func<IEnumerator> coroutine) {
		this.ClearAllLights();
		this.Connector.SetLightColour(index, Color.white);
		yield return new WaitForSeconds(1);
		this.ClearAllLights();
		var enumerator = coroutine();
		enumerator.MoveNext();
		this.lightShowPresses.Clear();
		this.coroutine = this.StartCoroutine(enumerator);
	}

	private IEnumerator LightShow1() {
		var index0 = this.lightShowPresses[0];
		yield return null;
		float time = 0;
		do {
			this.Connector.SetLightColour(index0, LightShow1Color(time));
			this.Connector.SetLightColour(index0 ^ 1, LightShow1Color(time - 0.3f));
			this.Connector.SetLightColour(index0 ^ 2, LightShow1Color(time - 0.3f));
			this.Connector.SetLightColour(index0 ^ 3, LightShow1Color(time - 0.6f));
			yield return null;
			time += Time.deltaTime;
		} while (time < 8f);
		this.ClearAllLights();
	}
	private static Color LightShow1Color(float time) {
		if (time < 0) return Color.black;
		if (time < 1) return new Color(time, 0, 0);
		if (time < 7) return Color.HSVToRGB((time - 1) / 6, 1, 1);
		if (time < 8) return new Color(8 - time, 0, 0);
		return Color.black;
	}

	private static readonly Color[] lightShow2Colors = new[] { Color.red, Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta };
	private IEnumerator LightShow2A() {
		var index0 = this.lightShowPresses[0];
		var index1 = this.lightShowPresses[1];
		yield return null;
		float time = 0; int lastQ = 0;
		do {
			var q = (int) (time / 2);
			if (q != lastQ) {
				if (q % 2 == 0) index0 ^= 3;
				else index1 ^= 3;
				lastQ = q;
			}
			var colorA = lightShow2Colors[q];
			var colorB = lightShow2Colors[(q + 1) % 6];
			Color color1, color2;
			var r = time % 2;
			if (r > 0.5f && r < 1.5f) {
				color1 = Color.Lerp(colorA, colorB, r - 0.5f);
				color2 = Color.Lerp(colorA, colorB, 1.5f - r);
			} else if (r <= 0.5f) {
				color1 = colorA * (r * 2);
				color2 = colorB * (r * 2);
			} else {
				color1 = colorB * ((2 - r) * 2);
				color2 = colorA * ((2 - r) * 2);
			}
			this.Connector.SetLightColour(index0, color1);
			this.Connector.SetLightColour(index1, color1);
			this.Connector.SetLightColour(index0 ^ 3, color2);
			this.Connector.SetLightColour(index1 ^ 3, color2);
			yield return null;
			time += Time.deltaTime;
		} while (time < 12);
	}
	private IEnumerator LightShow2B() {
		var index0 = this.lightShowPresses[0];
		yield return null;
		foreach (var color in lightShow2Colors.Concat(new[] { Color.white })) {
			for (int i = 0; i < 8; ++i) {
				switch (i) {
					case 0: case 4:
						this.Connector.SetLightColour(index0 ^ 3, color);
						yield return new WaitForSeconds(0.25f);
						this.Connector.SetLightColour(index0 ^ 3, Color.black);
						break;
					case 2: case 6:
						this.Connector.SetLightColour(index0, color);
						yield return new WaitForSeconds(0.25f);
						this.Connector.SetLightColour(index0, Color.black);
						break;
					default:
						this.Connector.SetLightColour(index0 ^ 1, color);
						this.Connector.SetLightColour(index0 ^ 2, color);
						yield return new WaitForSeconds(0.25f);
						this.Connector.SetLightColour(index0 ^ 1, Color.black);
						this.Connector.SetLightColour(index0 ^ 2, Color.black);
						break;
				}
			}
		}
		this.Connector.SetLightColour(index0 ^ 3, Color.white);
		yield return new WaitForSeconds(0.25f);
		this.Connector.SetLightColour(index0 ^ 3, Color.black);
	}

	private static readonly Color[] lightShow3AColors = new[] { Color.red, Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta, Color.red, Color.white };
	private IEnumerator LightShow3A() {
		var index0 = this.lightShowPresses[0];
		var index1 = this.lightShowPresses[1];
		yield return null;
		foreach (var color in lightShow3AColors) {
			for (int i = 0; i < 8; ++i) {
				int pos;
				switch (i) {
					case 0: case 4: pos = index0; break;
					case 1: case 5: pos = index1; break;
					case 2: case 6: pos = index0 ^ 3; break;
					default: pos = index1 ^ 3; break;
				}
				this.Connector.SetLightColour(pos, color);
				yield return new WaitForSeconds(0.25f);
				this.Connector.SetLightColour(pos, Color.black);
			}
		}
	}

	private IEnumerator LightShow3B() {
		var index0 = this.lightShowPresses[0];
		yield return null;
		float time = 0;
		do {
			this.Connector.SetLightColour(index0, LightShow3Color(time));
			this.Connector.SetLightColour(index0 ^ 1, LightShow3Color(time - 0.25f));
			this.Connector.SetLightColour(index0 ^ 2, LightShow3Color(time - 0.25f));
			this.Connector.SetLightColour(index0 ^ 3, LightShow3Color(time - 0.5f));
			yield return null;
			time += Time.deltaTime;
		} while (time < 12f);
		this.ClearAllLights();
	}
	private static Color LightShow3Color(float time) {
		if (time < 0) return Color.black;
		time /= 0.5f;
		var v = 1 - Math.Abs(time % 3 - 1);
		var i = (int) time / 3;
		return i < lightShow3AColors.Length ? lightShow3AColors[i] * v : Color.black;
	}

	private IEnumerator LightShow3C() {
		var index0 = this.lightShowPresses[0];
		yield return null;
		foreach (var color in lightShow3AColors) {
			for (int i = 0; i < 8; ++i) {
				var pos = index0 ^ (i & 1);
				this.Connector.SetLightColour(pos, color);
				this.Connector.SetLightColour(pos ^ 3, color);
				this.Connector.SetLightColour(pos ^ 1, Color.white);
				this.Connector.SetLightColour(pos ^ 2, Color.white);
				yield return new WaitForSeconds(0.25f);
			}
		}
	}

	private static readonly Color[] lightShow4AColors =
		new[] { Color.red, new Color(1, 0.5f, 0), Color.yellow, new Color(0.5f, 1, 0), Color.green, new Color(0, 1, 0.5f), Color.cyan, new Color(0, 0.5f, 1), Color.blue, new Color(0.5f, 0, 1), Color.magenta, new Color(1, 0, 0.5f), Color.red, Color.white };
	private IEnumerator LightShow4A() {
		var index0 = this.lightShowPresses[0];
		yield return null;
		foreach (var color in lightShow4AColors) {
			this.Connector.SetLightColour(index0, color);
			yield return new WaitForSeconds(0.25f);
			this.Connector.SetLightColour(index0 ^ 1, color);
			yield return new WaitForSeconds(0.25f);
			this.Connector.SetLightColour(index0 ^ 3, color);
			yield return new WaitForSeconds(0.25f);
			this.Connector.SetLightColour(index0 ^ 2, color);
			yield return new WaitForSeconds(0.25f);
		}
		for (int i = 0; i < 4; ++i) {
			this.Connector.SetLightColour(index0, Color.black);
			yield return new WaitForSeconds(0.25f);
			this.Connector.SetLightColour(index0 ^ 1, Color.black);
			yield return new WaitForSeconds(0.25f);
			this.Connector.SetLightColour(index0 ^ 3, Color.black);
			yield return new WaitForSeconds(0.25f);
			this.Connector.SetLightColour(index0 ^ 2, Color.black);
			yield return new WaitForSeconds(0.25f);
		}
	}

	private IEnumerator LightShow4B() {
		var index0 = this.lightShowPresses[0];
		yield return null;
		float time = 0; var colors = new Color[4]; var values = new float[4];
		colors[index0] = Color.white;
		values[index0] = 1;
		do {
			for (int i = 0; i < 4; ++i) {
				if (values[i] > 0) {
					values[i] -= 1 / 64f;
					this.Connector.SetLightColour(i, values[i] > 0 ? colors[i] * values[i] : Color.black);
				}
			}
			if (time < 20) {
				if (Random.Range(0, 15) == 0) {
					var color = Random.Range(0, 4) == 0 ? Color.white : Color.HSVToRGB((time + Random.Range(0, 4f)) / 24, 1, 1);
					index0 = Random.Range(0, 4);
					colors[index0] = color;
					values[index0] = 1;
					this.Connector.SetLightColour(index0, color);
				}
			}
			yield return null;
			time += Time.deltaTime;
		} while (time < 22);
		this.ClearAllLights();
	}

	private IEnumerator LightShow4C() {
		var index0 = this.lightShowPresses[0];
		yield return null;
		float time = 0;
		do {
			this.Connector.SetLightColour(index0, LightShow4CColor(time));
			this.Connector.SetLightColour(index0 ^ 1, LightShow4CColor(time - 0.125f));
			this.Connector.SetLightColour(index0 ^ 2, LightShow4CColor(time - 0.25f));
			this.Connector.SetLightColour(index0 ^ 3, LightShow4CColor(time - 0.375f));
			yield return null;
			time += Time.deltaTime;
		} while (time < 10);
	}
	private static Color LightShow4CColor(float time) {
		if (time < 0) return Color.black;
		var index = (int) time;
		return index < lightShow3AColors.Length ? lightShow3AColors[index] * (1 - (time - index)) : Color.black;
	}

	private IEnumerator LightShow4D() {
		var index0 = this.lightShowPresses[0];
		yield return null;
		int index1 = index0 ^ 1;
		for (int i = 0; i < 8; ++i) {
			for (int j = 0; j < 4; ++j) {
				if (j != 0) {
					index0 ^= 1;
					index1 ^= 1;
				}
				this.Connector.SetLightColour(index0, lightShow4AColors[i]);
				this.Connector.SetLightColour(index0 ^ 3, lightShow4AColors[i]);
				this.Connector.SetLightColour(index1, lightShow4AColors[i + 1]);
				this.Connector.SetLightColour(index1 ^ 3, lightShow4AColors[i + 1]);
				yield return new WaitForSeconds(0.25f);
			}
		}
		this.ClearAllLights();
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
