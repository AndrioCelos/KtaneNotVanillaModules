using System;
using System.Collections;
using System.Linq;
using NotVanillaModulesLib;
using KModkit;
using UnityEngine;
using Random = UnityEngine.Random;

public class NotButton : NotVanillaModule<NotButtonConnector> {
	public delegate int MashCountFormula(int a, int b, int c, int d, int e, int f, int g);

	public TextMesh DisplayText;
	public Light[] Lights;
	public bool OpenCoverOnSelection;

	public ButtonColour Colour { get; set; }
	public ButtonLabel Label { get; set; }
	public ButtonLightColour LightColour { get; private set; }

	public bool Down { get; private set; }
	public bool Holding { get; private set; }
	public bool Mashing { get; private set; }
	public int MashCount { get; private set; }
	public float InteractionTime { get; private set; }

	public ButtonAction SolutionAction { get; private set; }
	public TimerCondition SolutionTimerCondition { get; private set; }
	public int SolutionMashCount { get; private set; }

	const ButtonAction Press = ButtonAction.Press;
	const ButtonAction Hold = ButtonAction.Hold;
	const ButtonAction Mash = ButtonAction.Mash;
	private static readonly ButtonAction[,] defaultActionTable = new[,] {
		{ Press, Mash , Hold , Press, Hold , Hold , Press, Mash , Press },
		{ Mash , Press, Press, Hold , Mash , Mash , Mash , Mash , Mash  },
		{ Hold , Press, Mash , Mash , Press, Hold , Press, Press, Hold  },
		{ Press, Hold , Press, Mash , Mash , Hold , Press, Press, Press },
		{ Hold , Mash , Mash , Press, Hold , Press, Hold , Press, Mash  },
		{ Press, Hold , Press, Mash , Press, Hold , Mash , Hold , Press },
		{ Mash , Hold , Hold , Press, Mash , Mash , Hold , Mash , Hold  },
		{ Mash , Press, Hold , Press, Press, Press, Mash , Hold , Mash  },
		{ Press, Mash , Press, Hold , Mash , Press, Press, Press, Hold  },
		{ Hold , Hold , Mash , Mash , Press, Mash , Hold , Mash , Mash  }
	};
	private TimerCondition[] defaultTimerConditions;
	private static readonly MashCountFormula[] defaultMashCountFormulas = new MashCountFormula[] {
		(a, b, c, d, e, f, g) => a + 2 * b - d,
		(a, b, c, d, e, f, g) => 2 * b + 1 - g,
		(a, b, c, d, e, f, g) => 2 * a + d - c,
		(a, b, c, d, e, f, g) => d + 2 * f - b,
		(a, b, c, d, e, f, g) => e + f + g - b,
		(a, b, c, d, e, f, g) => 2 * c + d - 1,
		(a, b, c, d, e, f, g) => 2 * (f - a) + d,
		(a, b, c, d, e, f, g) => 3 * g - a - 3,
		(a, b, c, d, e, f, g) => (f + a * c) * (e + d),
		(a, b, c, d, e, f, g) => a * b + c * d - g * (e - f)
	};

	private KMSelectable button;
	private KMBombInfo bombInfo;

	private static string SolutionActionPastTense(ButtonAction action) {
		switch (action) {
			case ButtonAction.Press: return "pressed";
			case ButtonAction.Hold: return "held";
			case ButtonAction.Mash: return "mashed";
			default: throw new ArgumentException();
		}
	}

	public override void Start () {
		base.Start();
		this.bombInfo = this.GetComponent<KMBombInfo>();

		// Fix lights.
		var lightScale = this.transform.lossyScale.x;
		foreach (var light in this.Lights) light.range *= lightScale;

		this.defaultTimerConditions = new[] {
			/* W  */ new TimerCondition((t, s) => true, "any time"),
			/* R  */ TimerCondition.SecondsDigitIs(1),
			/* Y  */ TimerCondition.Contains('4'),
			/* G  */ TimerCondition.SecondsDigitsAddTo(7),
			/* B  */ TimerCondition.TensDigitIsPrimeOrZero(),
			/* WR */ TimerCondition.Contains('9'),
			/* WY */ TimerCondition.Contains((char) ('0' + Math.Min(9, this.bombInfo.GetBatteryCount()))),
			/* WG */ TimerCondition.SecondsDigitsMatch(),
			/* WB */ TimerCondition.SecondsDigitIsNot(7),
			/* RY */ TimerCondition.Contains((char) ('0' + this.bombInfo.GetSerialNumberNumbers().Last())),
			/* RG */ TimerCondition.SecondsDigitIsPrimeOrZero(),
			/* RB */ TimerCondition.SecondsDigitMatchesLeftDigit(),
			/* YG */ TimerCondition.SecondsDigitsDifferBy(4),
			/* YB */ TimerCondition.Contains('6'),
			/* GB */ TimerCondition.TensDigitIsNot(2)
		};

		this.Connector.SetColour(this.Colour = (ButtonColour) Random.Range(0, 10));
		this.Log("Colour is " + this.Colour);
		this.Connector.SetLabel(this.Label = (ButtonLabel) Random.Range(0, 9));
		this.Log("Label is " + this.Label);
		this.SolutionAction = defaultActionTable[(int) this.Colour, (int) this.Label];
		this.Log("The button should be " + SolutionActionPastTense(this.SolutionAction) + ".");

		if (this.SolutionAction == ButtonAction.Mash) {
			var a = this.bombInfo.GetBatteryCount();
			var b = this.bombInfo.GetPorts().Distinct().Count();
			var c = this.bombInfo.GetSolvableModuleNames().Count;
			var d = this.bombInfo.GetIndicators().Count();
			var e = this.bombInfo.GetSerialNumberNumbers().Last();
			var f = this.bombInfo.GetSerialNumberLetters().ElementAt(1) - 'A';
			var g = this.Label.ToString().Length;
			this.SolutionMashCount = defaultMashCountFormulas[(int) this.Colour](a, b, c, d, e, f, g);
			// If the result is outside the range 10~99, subtract or add 7 until it's within the range.
			if (this.SolutionMashCount < 10) {
				var rem = (this.SolutionMashCount % 7 + 7) % 7;
				this.SolutionMashCount = rem + (rem >= 3 ? 7 : 14);
			} else if (this.SolutionMashCount > 99) {
				var rem = this.SolutionMashCount % 7;
				this.SolutionMashCount = rem + (rem >= 2 ? 91 : 98);
			}
			this.Log(string.Format("The button should be pressed {0} times.", this.SolutionMashCount));
		}

		this.Connector.Held += this.Button_Held;
		this.Connector.Released += this.Button_Released;

		var moduleSelectable = this.GetComponent<KMSelectable>();
		this.button = moduleSelectable.Children[0];
		moduleSelectable.OnHighlight = () => { if (this.OpenCoverOnSelection) this.Connector.OpenCover(); };
		this.button.OnHighlight = () => { if (this.OpenCoverOnSelection) this.Connector.OpenCover(); };
		moduleSelectable.OnHighlightEnded = () => { if (this.OpenCoverOnSelection) this.Connector.CloseCover(); };
		this.button.OnHighlightEnded = () => { if (this.OpenCoverOnSelection) this.Connector.CloseCover(); };
		moduleSelectable.OnCancel = () => { this.Connector.CloseCover(); return true; };  // Twitch Plays
		if (Application.isEditor) {
			// Things work a bit differently in the test harness from the actual game.
			// TODO: There is currently an issue whereby going from one module to another does not call any event.
			moduleSelectable.OnInteract = () => { if (!this.OpenCoverOnSelection) this.Connector.OpenCover(); return true; };
		} else {
			this.OpenCoverOnSelection = this.Connector.ShouldOpenCoverOnSelection;
			moduleSelectable.OnFocus = () => { if (!this.OpenCoverOnSelection) this.Connector.OpenCover(); };
			moduleSelectable.OnDefocus = () => this.Connector.CloseCover();
		}

		this.DisplayText.gameObject.SetActive(false);
	}

	public void OpenCover() { this.Connector.OpenCover(); }

	public void Update() {
		if (this.Solved) return;
		if (this.Down) {
			if (!this.Holding && this.MashCount == 0) {  // Can't change your mind and start holding the button when you're already mashing...
				this.InteractionTime += Time.deltaTime;
				if (this.InteractionTime >= 0.7f) {
					this.StartedHolding();
				}
			}
		} else if (this.MashCount > 0) {
			this.InteractionTime += Time.deltaTime;
			if (this.InteractionTime >= (this.MashCount > 1 ? 3 : 0.7f)) {
				if (this.MashCount == 1) {
					if (this.SolutionAction == ButtonAction.Press) {
						this.Log("The button was pressed. That was correct.");
						this.Disarm();
					} else {
						this.Log(string.Format("The button was pressed. That was incorrect: it should have been {0}.", SolutionActionPastTense(this.SolutionAction)));
						this.Connector.KMBombModule.HandleStrike();
					}
				} else {
					if (this.SolutionAction == ButtonAction.Mash) {
						if (this.MashCount == this.SolutionMashCount) {
							this.Log(string.Format("The button was mashed {0} times. That was correct.", this.MashCount));
							this.Disarm();
						} else {
							this.Log(string.Format("The button was mashed {0} times. That was incorrect: it should have been mashed {1} times.", this.MashCount, this.SolutionMashCount));
							this.Connector.KMBombModule.HandleStrike();
						}
					} else {
						this.Log(string.Format("The button was mashed {0} times. That was incorrect: it should have been {1}.", this.MashCount, SolutionActionPastTense(this.SolutionAction)));
						this.Connector.KMBombModule.HandleStrike();
					}
				}
				this.MashCount = 0;
				this.DisplayText.gameObject.SetActive(false);
			}
		}
	}

	private void Button_Held(object sender, EventArgs e) {
		this.Down = true;
		if (this.Solved) return;
		this.InteractionTime = 0;
		this.GetComponent<KMSelectable>().AddInteractionPunch(0.5f);
	}

	private void Button_Released(object sender, EventArgs e) {
		// There's an issue with the test harness whereby pressing Enter to select the module raises this event.
		if (!this.Down) return;

		this.Down = false;
		if (this.Solved) return;
		this.InteractionTime = 0;
		this.GetComponent<KMSelectable>().AddInteractionPunch(-0.35f);

		if (this.Holding) {
			this.Holding = false;
			this.SetLightColour(ButtonLightColour.Off);

			var timeString = this.GetComponent<KMBombInfo>().GetFormattedTime();
			if (this.SolutionAction == ButtonAction.Hold) {
				var time = this.GetComponent<KMBombInfo>().GetTime();
				if (this.SolutionTimerCondition.Invoke(time, timeString)) {
					this.Log(string.Format("The button was held and released at {0}. That was correct.", timeString));
					this.Disarm();
				} else {
					this.Log(string.Format("The button was held and released at {0}. That was incorrect: it should have been released {1}.", timeString, this.SolutionTimerCondition.Description));
					this.Connector.KMBombModule.HandleStrike();
				}
			} else {
				this.Log(string.Format("The button was held and released at {0}. That was incorrect: it should have been {1}.", timeString, SolutionActionPastTense(this.SolutionAction)));
				this.Connector.KMBombModule.HandleStrike();
			}
		} else {
			++this.MashCount;
			if (this.MashCount > 1) {
				this.DisplayText.text = (this.MashCount % 100).ToString();
				this.DisplayText.gameObject.SetActive(true);
			}
		}
	}

	private void StartedHolding() {
		this.Holding = true;
		this.SetLightColour((ButtonLightColour) (Random.Range(0, 15) + 1));
		this.SolutionTimerCondition = this.defaultTimerConditions[(int) this.LightColour - 1];
		this.Log(string.Format("The button is being held. The light is {0}. The button should be released {1}.", this.LightColour, this.SolutionTimerCondition.Description));
	}

	private void SetLightColour(ButtonLightColour colour) {
		this.Connector.SetLightColour(this.LightColour = colour);
		if (colour == ButtonLightColour.Off) {
			foreach (var light in this.Lights)
				light.gameObject.SetActive(false);
		} else {
			foreach (var light in this.Lights)
				light.gameObject.SetActive(true);
			for (int i = 0; i < this.Lights.Length; i += 2) {
				switch (colour) {
					case ButtonLightColour.White:
					case ButtonLightColour.WhiteRed:
					case ButtonLightColour.WhiteYellow:
					case ButtonLightColour.WhiteGreen:
					case ButtonLightColour.WhiteBlue:
						this.Lights[i].color = Color.white;
						break;
					case ButtonLightColour.Red:
					case ButtonLightColour.RedYellow:
					case ButtonLightColour.RedGreen:
					case ButtonLightColour.RedBlue:
						this.Lights[i].color = Color.red;
						break;
					case ButtonLightColour.Yellow:
					case ButtonLightColour.YellowGreen:
					case ButtonLightColour.YellowBlue:
						this.Lights[i].color = Color.yellow;
						break;
					case ButtonLightColour.Green:
					case ButtonLightColour.GreenBlue:
						this.Lights[i].color = Color.green;
						break;
					case ButtonLightColour.Blue:
						this.Lights[i].color = Color.blue;
						break;
				}
			}
			for (int i = 1; i < this.Lights.Length; i += 2) {
				switch (colour) {
					case ButtonLightColour.White:
						this.Lights[i].color = Color.white;
						break;
					case ButtonLightColour.Red:
					case ButtonLightColour.WhiteRed:
						this.Lights[i].color = Color.red;
						break;
					case ButtonLightColour.Yellow:
					case ButtonLightColour.WhiteYellow:
					case ButtonLightColour.RedYellow:
						this.Lights[i].color = Color.yellow;
						break;
					case ButtonLightColour.Green:
					case ButtonLightColour.WhiteGreen:
					case ButtonLightColour.RedGreen:
					case ButtonLightColour.YellowGreen:
						this.Lights[i].color = Color.green;
						break;
					case ButtonLightColour.Blue:
					case ButtonLightColour.WhiteBlue:
					case ButtonLightColour.RedBlue:
					case ButtonLightColour.YellowBlue:
					case ButtonLightColour.GreenBlue:
						this.Lights[i].color = Color.blue;
						break;
				}
			}
		}
	}

	// Twitch Plays support
	public static readonly string TwitchHelpMessage = "!{0} tap | !{0} hold | !{0} release 1:13 1:23 | !{0} mash 50";
	[NonSerialized]
	public bool ZenModeActive;

	public IEnumerator ProcessTwitchCommand(string command) {
		var tokens = command.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
		if (tokens.Length == 0) yield break;

		if (tokens[0].Equals("release", StringComparison.InvariantCultureIgnoreCase)) {
			if (!this.Down)
				yield return "sendtochaterror You must start holding the button first.";
			else if (tokens.Length > 1) {
				var time = this.ZenModeActive ? float.PositiveInfinity : float.NegativeInfinity;
				var bombTime = this.bombInfo.GetTime();
				float time2;
				foreach (var timeString in tokens.Skip(1)) {
					if (!GeneralExtensions.TryParseTime(timeString, out time2)) yield break;
					if (this.ZenModeActive ? (time2 < time && time2 > bombTime) : (time2 > time && time2 < bombTime))
						time = time2;
				}
				if (float.IsInfinity(time))
					yield return tokens.Length == 2 ? "sendtochaterror The specified time has already passed."
						: "sendtochaterror All of the specified times have already passed.";
				else {
					var timeInt = (int) time;
					this.Log(string.Format("Releasing the button at {0}", GeneralExtensions.FormatTime(time)));
					yield return null;
					if (Math.Abs(time - this.bombInfo.GetTime()) > 15) yield return "waiting music";

					while (timeInt != (int) this.bombInfo.GetTime()) {
						yield return "trycancel The button was not released due to a request to cancel.";
					}

					var wasHolding = this.Holding;
					this.Connector.TwitchRelease();
					if (!wasHolding) {
						// The button was held so briefly it didn't count as such... Not sure this is actually possible.
						yield return this.SolutionAction == ButtonAction.Press ? "solve" : "strike";
					}
				}
			}
		} else if (this.Down)
			yield return "sendtochaterror The button is already being held.";
		else {
			if (tokens[0].Equals("press", StringComparison.InvariantCultureIgnoreCase) || tokens[0].Equals("mash", StringComparison.InvariantCultureIgnoreCase) ||
				tokens[0].Equals("tap", StringComparison.InvariantCultureIgnoreCase)) {
				int count;
				if (tokens.Length == 2) int.TryParse(tokens[1], out count);  // Sets count to 0 on error.
				else count = tokens.Length == 1 && !tokens[0].Equals("mash", StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;
				if (count > 0 && count < 100) {
					yield return null;
					if (count > 60) yield return "waiting music";
					for (; count > 0; --count) {
						this.Connector.TwitchPress();
						yield return new WaitForSeconds(1 / 12f);
						this.Connector.TwitchRelease();
						yield return new WaitForSeconds(1 / 12f);
					}
					yield return new WaitForSeconds(1);
					yield return (this.MashCount == 1 ? (this.SolutionAction == ButtonAction.Press) : (this.SolutionAction == ButtonAction.Mash && this.SolutionMashCount == this.MashCount))
						? "solve" : "strike";
				}
			} else if (tokens[0].Equals("hold", StringComparison.InvariantCultureIgnoreCase)) {
				yield return null;
				this.Connector.TwitchPress();
				yield return new WaitForSeconds(1);
			}
		}
	}

	public IEnumerator TwitchHandleForcedSolve() {
		this.Connector.OpenCover();
		switch (this.SolutionAction) {
			case ButtonAction.Press:
				this.Connector.TwitchPress();
				yield return new WaitForSeconds(1 / 12f);
				this.Connector.TwitchRelease();
				break;
			case ButtonAction.Mash:
				for (int i = 0; i < this.SolutionMashCount; ++i) {
					this.Connector.TwitchPress();
					yield return new WaitForSeconds(1 / 12f);
					this.Connector.TwitchRelease();
					yield return new WaitForSeconds(1 / 12f);
				}
				break;
			case ButtonAction.Hold:
				this.Connector.TwitchPress();
				yield return new WaitForSeconds(1);
				yield return new WaitUntil(() => this.SolutionTimerCondition.Invoke(this.bombInfo.GetTime(), this.bombInfo.GetFormattedTime()));
				this.Connector.TwitchRelease();
				break;
			default:
				this.Disarm();
				break;
		}
		this.Connector.CloseCover();
	}
}
