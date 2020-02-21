using System;
using System.Collections;
using System.Collections.Generic;
using NotVanillaModulesLib;
using UnityEngine;
using KModkit;
using System.Linq;

public class NotCapacitorDischarge : NotVanillaModule<NotCapacitorConnector> {
	public int Number { get; private set; }
	public bool Down { get; private set; }
	public bool PressedIncorrectly { get; private set; }

	public TimerCondition PressCondition { get; private set; }
	public TimerCondition ReleaseCondition { get; private set; }

	private bool needyActive;
	private bool exploded;

	private static readonly byte[,] defaultEvenSerialPressDigits = new byte[,] {
		{ 1, 7, 3, 0, 9 },
		{ 4, 3, 5, 1, 6 },
		{ 5, 0, 8, 9, 3 },
		{ 8, 8, 2, 6, 7 },
		{ 9, 1, 6, 4, 2 },
		{ 0, 4, 2, 5, 7 }
	};
	private static readonly byte[,] defaultOddSerialPressDigits = new byte[,] {
		{ 1, 0, 8, 2, 6 },
		{ 3, 5, 4, 0, 7 },
		{ 9, 2, 9, 7, 4 },
		{ 6, 5, 1, 8, 3 },
		{ 7, 0, 6, 9, 5 },
		{ 3, 2, 8, 4, 1 }
	};
	private static readonly byte[,] defaultEvenSerialReleaseDigits = new byte[,] {
		{ 1, 7, 3, 5, 9 },
		{ 0, 4, 8, 6, 0 },
		{ 2, 5, 7, 2, 3 },
		{ 9, 2, 8, 4, 6 },
		{ 7, 4, 3, 1, 0 },
		{ 6, 8, 1, 5, 9 }
	};
	private static readonly byte[,] defaultOddSerialReleaseDigits = new byte[,] {
		{ 7, 9, 0, 5, 9 },
		{ 3, 6, 2, 0, 3 },
		{ 3, 4, 8, 6, 2 },
		{ 1, 7, 8, 4, 0 },
		{ 9, 5, 1, 8, 1 },
		{ 6, 5, 4, 2, 7 }
	};

	private const string BuzzSound = "CapacitorBuzz";

	public override void Start() {
		base.Start();
		this.Connector.KMNeedyModule.OnNeedyActivation = this.KMNeedyModule_OnNeedyActivation;
		this.Connector.KMNeedyModule.OnNeedyDeactivation = this.KMNeedyModule_OnNeedyDeactivation;
		this.Connector.KMNeedyModule.OnTimerExpired = this.KMNeedyModule_OnTimerExpired;

		this.Connector.LeverPressed += this.Connector_LeverPressed;
		this.Connector.LeverReleased += this.Connector_LeverReleased;
	}

	private void Connector_LeverPressed(object sender, EventArgs e) {
		this.Down = true;
		if (this.PressCondition == null) return;

		var formattedTime = this.GetComponent<KMBombInfo>().GetFormattedTime();
		if (this.PressCondition.Invoke(this.GetComponent<KMBombInfo>().GetTime(), formattedTime)) {
			this.Connector.SetLight(true);
			var bombInfo = this.GetComponent<KMBombInfo>();
			var j = (this.Number - 1) / 20;
			if (bombInfo.GetSerialNumberNumbers().Last() % 2 == 0) {
				var i = bombInfo.GetSolvedModuleNames().Count / 3;
				this.ReleaseCondition = i > 5 ? TimerCondition.SecondsDigitIs(9) : TimerCondition.SecondsDigitIs(defaultEvenSerialReleaseDigits[i, j]);
			} else {
				var serialNumber = bombInfo.GetSerialNumber();
				var i = ContainsAny(serialNumber, 'A', 'E', 'I', 'O', 'U') ? 0 :
					ContainsAny(serialNumber, '2', '3', '5', '7') ? 1 :
					ContainsAny(serialNumber, 'Q', 'R', 'Z') ? 2 :
					ContainsAny(serialNumber, 'X', 'Y', 'K') ? 3 :
					ContainsAny(serialNumber, 'O', 'A', 'T') ? 4 :
					ContainsAny(serialNumber, 'D', 'I', 'E') ? 5 : 6;
				this.ReleaseCondition = i > 5 ? TimerCondition.SecondsDigitIs(9) : TimerCondition.SecondsDigitIs(defaultOddSerialReleaseDigits[i, j]);
			}
			this.Log(string.Format("The lever was pressed at {0}. That was correct. Release the lever {1}.", formattedTime, this.ReleaseCondition));
		} else {
			this.Log(string.Format("The lever was pressed at {0}. That was incorrect.", formattedTime));
			this.MistakePenalty();
			this.PressedIncorrectly = true;
		}
	}

	private void Connector_LeverReleased(object sender, EventArgs e) {
		this.Down = false;
		if (this.needyActive) this.Connector.SetLight(false);
		if (this.PressedIncorrectly) {
			this.PressedIncorrectly = false;
			return;
		}
		if (this.ReleaseCondition == null) return;

		var formattedTime = this.GetComponent<KMBombInfo>().GetFormattedTime();
		if (this.ReleaseCondition.Invoke(this.GetComponent<KMBombInfo>().GetTime(), formattedTime)) {
			this.Log(string.Format("The lever was released at {0}. That was correct.", formattedTime));
			this.Connector.KMNeedyModule.HandlePass();
			// OnNeedyDeactivation doesn't seem to fire in the live game, though it does in the test harness... Oops.
			this.KMNeedyModule_OnNeedyDeactivation();
		} else {
			this.Log(string.Format("The lever was released at {0}. That was incorrect.", formattedTime));
			this.MistakePenalty();
			this.PressedIncorrectly = true;
		}
	}

	private void MistakePenalty() {
		this.Connector.KMNeedyModule.SetNeedyTimeRemaining(this.Connector.KMNeedyModule.GetNeedyTimeRemaining() * 0.75f);
		this.GetComponent<KMAudio>().PlaySoundAtTransform(BuzzSound, this.transform);
	}

	private static bool ContainsAny(string s, params char[] chars) { return chars.Any(s.Contains); }

	private void KMNeedyModule_OnNeedyActivation() {
		this.needyActive = true;
		this.Number = UnityEngine.Random.Range(0, 100);
		this.Connector.SetDisplay(this.Number);
		if (this.Down) {
			this.PressedIncorrectly = true;
			this.Connector.SetLight(true);
		}

		var bombInfo = this.GetComponent<KMBombInfo>();
		var j = (this.Number - 1) / 20;
		if (bombInfo.GetSerialNumberNumbers().Last() % 2 == 0) {
			var batteryCount = bombInfo.GetBatteryCount();
			if (batteryCount > 5) {
				switch (j) {
					case 0: this.PressCondition = TimerCondition.SecondsDigitIs(this.Number % 10); break;
					case 1: this.PressCondition = TimerCondition.SecondsDigitIs(GeneralExtensions.DigitalRoot(this.Number)); break;
					case 2: this.PressCondition = TimerCondition.SecondsDigitIsNotPrime(); break;
					case 3: this.PressCondition = TimerCondition.SecondsDigitsMatch(); break;
					case 4: this.PressCondition = TimerCondition.AnyTime(); break;
				}
			} else this.PressCondition = TimerCondition.SecondsDigitIs(defaultEvenSerialPressDigits[batteryCount, j]);
		} else {
			var i = !bombInfo.IsPortPresent(Port.Parallel) ? 0 :
				!bombInfo.IsPortPresent(Port.Serial) ? 1 :
				!bombInfo.IsPortPresent(Port.DVI) ? 2 :
				!bombInfo.IsPortPresent(Port.StereoRCA) ? 3 :
				!bombInfo.IsPortPresent(Port.PS2) ? 4 :
				!bombInfo.IsPortPresent(Port.RJ45) ? 5 : -1;
			if (i < 0) {
				switch (j) {
					case 0: this.PressCondition = TimerCondition.AnyTime(); break;
					case 1: this.PressCondition = TimerCondition.SecondsDigitIsPrimeOrZero(); break;
					case 2: this.PressCondition = TimerCondition.SecondsDigitIsEven(); break;
					case 3: this.PressCondition = TimerCondition.SecondsDigitIsOdd(); break;
					case 4: this.PressCondition = TimerCondition.SecondsDigitIs(bombInfo.GetSerialNumber().First(char.IsDigit) - '0'); break;
				}
			} else this.PressCondition = TimerCondition.SecondsDigitIs(defaultOddSerialPressDigits[i, j]);
		}

		this.Log(string.Format("Module active. The number is {0:D2}. Press the lever {1}.", this.Number, this.PressCondition.Description));
	}

	private void KMNeedyModule_OnNeedyDeactivation() {
		this.needyActive = false;
		this.PressCondition = null;
		this.ReleaseCondition = null;
		this.Connector.ClearDisplay();
		this.Connector.SetLight(false);
		if (!this.exploded) this.Connector.SetDeathBar(0);
	}

	public void Update() {
		if (this.needyActive) {
			this.Connector.SetDeathBar(1 - this.Connector.KMNeedyModule.GetNeedyTimeRemaining() / this.Connector.KMNeedyModule.CountdownTime);
		}
	}

	private void KMNeedyModule_OnTimerExpired() {
		// The capacitor has blown; the module can't reactivate any more...
		this.Connector.Explode();
		this.exploded = true;
		this.Connector.KMNeedyModule.HandleStrike();
		this.Connector.SetDeathBar(1);
		this.KMNeedyModule_OnNeedyDeactivation();
	}

	// Twitch Plays support
	public static readonly string TwitchHelpMessage
		= "!{0} hold 7 - hold when the seconds digit is 7 | !{0} release 4 - release when the seconds digit is 4 | !{0} toggle 7 4 - combine both commands";

	public IEnumerator ProcessTwitchCommand(string command) {
		var tokens = command.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
		if (tokens.Length < 2) yield break;

		var bombInfo = this.GetComponent<KMBombInfo>();

		if (tokens[0].EqualsIgnoreCase("toggle")) {
			if (this.Down) {
				yield return "sendtochaterror The lever is already being held.";
				yield break;
			}
			int holdTime, releaseTime;
			if (tokens.Length != 3 || !int.TryParse(tokens[1], out holdTime) || holdTime < 0 || holdTime > 9 ||
				!int.TryParse(tokens[2], out releaseTime) || releaseTime < 0 || releaseTime > 9) yield break;
			yield return null;
			while ((int) bombInfo.GetTime() % 10 != holdTime)
				yield return "trycancel The lever was not held due to a request to cancel.";
			yield return "strikemessage pressing the lever incorrectly";
			this.Connector.TwitchPress();
			if (this.PressedIncorrectly) {
				this.Connector.TwitchRelease();
				yield return "sendtochat The lever was not held due to a mistake.";
				yield break;
			}
			yield return new WaitForSeconds(0.3f);
			while ((int) bombInfo.GetTime() % 10 != releaseTime)
				yield return "trycancel The lever was not released due to a request to cancel.";
			yield return "strikemessage releasing the lever incorrectly";
			this.Connector.TwitchRelease();
			yield break;
		}

		if (tokens.Length != 2) yield break;

		bool press; string verb;
		if (tokens[0].EqualsIgnoreCase("hold")) {
			if (this.Down) {
				yield return "sendtochaterror The lever is already being held.";
				yield break;
			}
			press = true; verb = "held";
		} else if (tokens[0].EqualsIgnoreCase("release")) {
			if (!this.Down) {
				yield return "sendtochaterror The lever is not being held.";
				yield break;
			}
			press = false; verb = "released";
		} else
			yield break;

		int n;
		if (!int.TryParse(tokens[1], out n) || n < 0 || n > 9) yield break;
		yield return null;
		while ((int) bombInfo.GetTime() % 10 != n)
			yield return string.Format("trycancel The lever was not {0} due to a request to cancel.", verb);

		if (press) {
			this.Connector.TwitchPress();
			if (this.PressedIncorrectly) {
				this.Connector.TwitchRelease();
				yield return "sendtochat The lever was not held due to a mistake.";
			}
		} else
			this.Connector.TwitchRelease();
	}
}
