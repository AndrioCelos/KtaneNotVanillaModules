using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NotVanillaModulesLib;
using UnityEngine;

public class NotPassword : NotVanillaModule<NotPasswordConnector> {
	[ReadOnlyWhenPlaying]
	public char MissingLetter;
	public PasswordSolutionAction SolutionAction { get; private set; }
	public TimerCondition PressCondition { get; private set; }
	public TimerCondition ReleaseCondition { get; private set; }
	public int SolutionCount { get; private set; }
	public int SolutionDelay { get; private set; }

	public bool Down { get; private set; }
	public bool Holding { get; private set; }
	public int MashCount { get; private set; }
	public bool WasPressed { get; private set; }
	public float InteractionTime { get; private set; }
	public int InteractionTickCount { get; private set; }
	public bool PressedIncorrectly { get; private set; }

	private int lastBombTime;
	private bool twitchStruck;

	public override void Start() {
		base.Start();
		this.Connector.KMBombModule.OnActivate = this.KMBombModule_OnActivate;
		this.Connector.SubmitPressed += this.Connector_SubmitPressed;
		this.Connector.SubmitReleased += this.Connector_SubmitReleased;

		var letters = new char[26];
		for (int i = 0; i < 26; ++i) letters[i] = (char) ('A' + i);
		letters.Shuffle();

		var forced = false;
		if (Application.isEditor) {
			// If MissingLetter is already set to a valid letter in the test harness, force the missing letter to be that letter.
			var i = Array.IndexOf(letters, char.ToUpper(this.MissingLetter));
			if (i >= 0) {
				forced = true;
				this.Log("Forcing the missing letter to be {0}.", this.MissingLetter = letters[i]);
				letters[i] = letters[25];
			}
		}

		var choices = new char[5];
		for (int i = 0; i < 5; ++i) {
			Array.Copy(letters, i * 5, choices, 0, 5);
			this.Connector.SetSpinnerChoices(i, choices);
		}

		if (!forced) this.Log("The missing letter is {0}.", this.MissingLetter = letters[25]);

		switch (this.MissingLetter) {
			case 'A':
				this.SolutionAction = PasswordSolutionAction.Press;
				this.PressCondition = TimerCondition.Contains('5');
				this.SolutionCount = 1;
				break;
			case 'B':
				this.SolutionAction = PasswordSolutionAction.Press;
				this.PressCondition = TimerCondition.SecondsDigitIsNotPrime();
				this.SolutionCount = 1;
				break;
			case 'C':
				this.SolutionAction = PasswordSolutionAction.HoldCondition;
				this.PressCondition = TimerCondition.Contains('7');
				this.ReleaseCondition = TimerCondition.Contains('2');
				break;
			case 'D':
				this.SolutionAction = PasswordSolutionAction.Press;
				this.PressCondition = TimerCondition.SecondsDigitIsPrimeOrZero();
				this.SolutionCount = 1;
				break;
			case 'E':
				this.SolutionAction = PasswordSolutionAction.HoldTime;
				this.SolutionDelay = 4;
				break;
			case 'F':
				this.SolutionAction = PasswordSolutionAction.Press;
				this.SolutionCount = 2;
				break;
			case 'G':
				this.SolutionAction = PasswordSolutionAction.HoldIndefinitely;
				this.PressCondition = TimerCondition.Contains('6');
				this.SolutionDelay = UnityEngine.Random.Range(4, 13);
				break;
			case 'H':
				this.SolutionAction = PasswordSolutionAction.PressTwice;
				this.SolutionDelay = 4;
				break;
			case 'I':
				this.SolutionAction = PasswordSolutionAction.Press;
				this.PressCondition = TimerCondition.Contains('1');
				this.SolutionCount = 1;
				break;
			case 'J':
				this.SolutionAction = PasswordSolutionAction.Press;
				this.SolutionCount = 3;
				break;
			case 'K':
				this.SolutionAction = PasswordSolutionAction.Press;
				this.SolutionCount = 1;
				break;
			case 'L':
				this.SolutionAction = PasswordSolutionAction.Press;
				this.PressCondition = TimerCondition.MinutesIsEven();
				this.SolutionCount = 1;
				break;
			case 'M':
				this.SolutionAction = PasswordSolutionAction.HoldCondition;
				this.ReleaseCondition = TimerCondition.Contains('3');
				break;
			case 'N':
				this.SolutionAction = PasswordSolutionAction.HoldTime;
				this.SolutionDelay = 8;
				break;
			case 'O':
				this.SolutionAction = PasswordSolutionAction.HoldIndefinitely;
				this.SolutionDelay = UnityEngine.Random.Range(4, 13);
				break;
			case 'P':
				this.SolutionAction = PasswordSolutionAction.Press;
				this.PressCondition = TimerCondition.Contains('7');
				this.SolutionCount = 1;
				break;
			case 'Q':
				this.SolutionAction = PasswordSolutionAction.Press;
				this.SolutionCount = 2;
				break;
			case 'R':
				this.SolutionAction = PasswordSolutionAction.HoldTime;
				this.SolutionDelay = 3;
				break;
			case 'S':
				this.SolutionAction = PasswordSolutionAction.HoldCondition;
				this.PressCondition = TimerCondition.Contains('4');
				this.ReleaseCondition = TimerCondition.Contains('3');
				break;
			case 'T':
				this.SolutionAction = PasswordSolutionAction.HoldIndefinitely;
				this.PressCondition = TimerCondition.Contains('3');
				this.SolutionDelay = UnityEngine.Random.Range(4, 13);
				break;
			case 'U':
				this.SolutionAction = PasswordSolutionAction.PressTwice;
				this.SolutionDelay = 2;
				break;
			case 'V':
				this.SolutionAction = PasswordSolutionAction.Press;
				this.SolutionCount = 5;
				break;
			case 'W':
				this.SolutionAction = PasswordSolutionAction.Press;
				this.PressCondition = TimerCondition.Contains('2');
				this.SolutionCount = 1;
				break;
			case 'X':
				this.SolutionAction = PasswordSolutionAction.PressIndefinitely;
				this.SolutionCount = UnityEngine.Random.Range(10, 25);
				break;
			case 'Y':
				this.SolutionAction = PasswordSolutionAction.Press;
				this.SolutionCount = 1;
				break;
			case 'Z':
				this.SolutionAction = PasswordSolutionAction.HoldTime;
				this.SolutionDelay = 10;
				break;
		}
	}

	private void KMBombModule_OnActivate() {
		this.Connector.Activate();
	}

	public void Update() {
		if (this.Solved) return;

		var time = this.GetComponent<KMBombInfo>().GetTime();
		if ((int) time != this.lastBombTime) {
			++this.InteractionTickCount;
			this.lastBombTime = (int) time;
		}

		if (this.Down) {
			if (this.Holding) {
				if (!this.PressedIncorrectly) {
					if (this.SolutionAction == PasswordSolutionAction.HoldIndefinitely) {
						if (this.InteractionTickCount >= this.SolutionDelay) {
							this.Disarm();
						}
					} else {
						if (this.InteractionTickCount >= 30) {
							this.Log("It seems like you are trying to hold the button until the module is disarmed. That was incorrect.");
							this.Strike();
							this.PressedIncorrectly = true;
						}
					}
				}
			} else if (this.MashCount == 0) {
				this.InteractionTime += Time.deltaTime;
				if (this.InteractionTime >= 0.7f) {
					this.StartedHolding();
				}
			}
		} else if (this.MashCount > 0) {
			this.InteractionTime += Time.deltaTime;
			if (this.InteractionTime >= 0.7f) {
				this.StoppedMashing();
			}
		}
	}

	private void Strike() {
		this.twitchStruck = true;
		this.Connector.KMBombModule.HandleStrike();
	}

	private void StoppedMashing() {
		if (!this.PressedIncorrectly) {
			switch (this.SolutionAction) {
				case PasswordSolutionAction.Press:
					if (this.SolutionCount == this.MashCount) {
						this.Log(string.Format("The button was pressed {0}. That was correct.", this.MashCount == 1 ? "once" : this.MashCount + " times"));
						this.Disarm();
						break;
					}
					goto default;
				case PasswordSolutionAction.PressTwice:
					// Do nothing here; wait for them to press it the second time.
					break;
				default:
					this.Log(string.Format("The button was pressed {0}. That was incorrect.", this.MashCount == 1 ? "once" : this.MashCount + " times"));
					this.Strike();
					break;
			}
		}
		this.PressedIncorrectly = false;
		this.MashCount = 0;
	}

	private void StartedHolding() {
		this.Holding = true;
	}

	private void Connector_SubmitPressed(object sender, EventArgs e) {
		this.Down = true;
		if (this.Solved) return;
		this.InteractionTime = 0;
		if (!this.WasPressed) this.InteractionTickCount = 0;

		// If we've already registered a strike recently, ignore further presses until you leave it alone for 1 second.
		// This is in case they try mashing the button when the press condition is not met.
		// We only want one strike in that situation.
		if (this.PressedIncorrectly) return;

		var bombInfo = this.GetComponent<KMBombInfo>();
		switch (this.SolutionAction) {
			case PasswordSolutionAction.Press:
				if (this.PressCondition != null) {
					if (this.PressCondition.Invoke(bombInfo.GetTime(), bombInfo.GetFormattedTime()))
						this.Log("The button was pressed at {0}. That was correct.", bombInfo.GetFormattedTime());
					else {
						this.Log("The button was pressed at {0}. That was incorrect: it should have been pressed {1}.", bombInfo.GetFormattedTime(), this.PressCondition);
						this.Strike();
						this.PressedIncorrectly = true;
					}
				}
				break;
			case PasswordSolutionAction.PressTwice:
				break;
			case PasswordSolutionAction.PressIndefinitely:
				if (this.MashCount + 1 >= this.SolutionCount) {
					this.Log("The button is still being mashed. That was correct.");
					this.Disarm();
				}
				break;
			case PasswordSolutionAction.HoldCondition:
			case PasswordSolutionAction.HoldIndefinitely:
			case PasswordSolutionAction.HoldTime:
				if (this.PressCondition != null && !this.PressCondition.Invoke(bombInfo.GetTime(), bombInfo.GetFormattedTime())) {
					this.Log("The button was pressed at {0}. That was incorrect.", bombInfo.GetFormattedTime());
					this.Strike();
					this.PressedIncorrectly = true;
				}
				break;
		}
	}

	private void Connector_SubmitReleased(object sender, EventArgs e) {
		this.Down = false;
		if (this.Solved) return;
		this.InteractionTime = 0;
		var bombInfo = this.GetComponent<KMBombInfo>();

		if (this.Holding) {
			this.Holding = false;
			this.WasPressed = false;
			if (this.PressedIncorrectly) {
				this.PressedIncorrectly = false;
				return;
			}

			switch (this.SolutionAction) {
				case PasswordSolutionAction.HoldCondition:
					if (this.ReleaseCondition == null || this.ReleaseCondition.Invoke(bombInfo.GetTime(), bombInfo.GetFormattedTime())) {
						this.Log("The button was held and released at {0}. That was correct.", bombInfo.GetFormattedTime());
						this.Disarm();
						break;
					}
					goto default;
				case PasswordSolutionAction.HoldTime:
					if (this.InteractionTickCount == this.SolutionDelay) {
						this.Log("The button was held for {0} ticks. That was correct.", this.InteractionTickCount);
						this.Disarm();
					} else {
						this.Log("The button was held for {0} ticks. That was incorrect.", this.InteractionTickCount);
						this.Strike();
					}
					break;
				default:
					this.Log("The button was held and released at {0}. That was incorrect.", bombInfo.GetFormattedTime());
					this.Strike();
					break;
			}
		} else {
			if (this.SolutionAction == PasswordSolutionAction.PressTwice) {
				if (this.WasPressed) {
					if (this.InteractionTickCount == this.SolutionDelay) {
						this.Log(string.Format("The button was pressed twice with a delay of {0} ticks. That was correct.", this.InteractionTickCount));
						this.Disarm();
					} else {
						this.Log(string.Format("The button was pressed twice with a delay of {0} ticks. That was incorrect: it should have been {1} ticks.",
							this.InteractionTickCount, this.SolutionDelay));
						this.Strike();
					}
					this.WasPressed = false;
				} else {
					this.WasPressed = true;
					this.InteractionTickCount = 0;
				}
			}
			++this.MashCount;
			if (!this.PressedIncorrectly && this.MashCount >= 30) {
				this.Log("It seems like you are trying to mash the button until the module is disarmed. That was incorrect.");
				this.Strike();
				this.PressedIncorrectly = true;
			}
		}
	}

	// Twitch Plays support
	public static readonly string TwitchHelpMessage
		= "!{0} cycle 1 3 5 - cycle the letters in columns 1, 3 and 5 | !{0} toggle - move all columns down one letter | " +
		"!{0} tap - tap once | !{0} tap on 5 - tap when the timer contains a 5 | !{0} tap 5 - tap 5 times | !{0} tap 5:59 | !{0} tap 5:59 then 5:54 | " +
		"!{0} mash - tap until something happens | !{0} hold | !{0} hold on 5 | !{0} hold for 5 | !{0} release on 2";
	// `!{0} cycle` is deliberately excluded because it takes too long and its use is generally frowned on.
	[NonSerialized]
	public bool ZenModeActive;
	public IEnumerator ProcessTwitchCommand(string command) {
		var tokens = command.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
		if (tokens.Length == 0) yield break;

		this.twitchStruck = false;

		var bombInfo = this.GetComponent<KMBombInfo>();

		if (tokens[0].EqualsIgnoreCase("release")) {
			if (!this.Down)
				yield return "sendtochaterror You must start holding the button first.";
			else if (tokens.Length == 3 && tokens[1].EqualsIgnoreCase("on") && tokens[2].Length == 1 && char.IsDigit(tokens[2][0])) {
				yield return "strikemessage releasing submit incorrectly";
				while (!bombInfo.GetFormattedTime().Contains(tokens[2][0]))
					yield return "trycancel The button was not released due to a request to cancel.";
				this.Connector.TwitchReleaseSubmit();
			}
		} else {
			if (this.Down)
				yield return "sendtochaterror The button is already being held.";
			else if (tokens[0].EqualsIgnoreCase("cycle")) {
				if (tokens.Length == 1)
					yield return "sendtochaterror You must specify one or more columns to cycle.";
				else {
					var indices = new List<int>();
					foreach (var token in tokens.Skip(1)) {
						int i;
						if (int.TryParse(token, out i) && i > 0 && i <= 5)
							indices.Add(i);
						else
							yield break;
					}
					yield return null;
					if (indices.Count >= 3) yield return "waiting music";
					foreach (var i in indices) {
						for (int j = 0; j < 5; ++j) {
							this.Connector.TwitchMoveDown(i - 1);
							yield return "trywaitcancel 1";
						}
					}
				}
			} else if (tokens[0].EqualsIgnoreCase("toggle")) {
				if (tokens.Length == 1) {
					yield return null;
					for (int i = 0; i < 5; ++i) {
						this.Connector.TwitchMoveDown(i);
						yield return new WaitForSeconds(0.1f);
					}
				}
			} else if (tokens[0].EqualsIgnoreCase("tap")) {
				int count = 1; int i; float time;
				switch (tokens.Length) {
					case 1:
						for (; count > 0; --count) {
							yield return "strikemessage pressing submit incorrectly";
							this.Connector.TwitchPressSubmit();
							//yield return new WaitForSeconds(0.1f);
							if (!this.twitchStruck) yield return "strikemessage releasing submit incorrectly";
							this.Connector.TwitchReleaseSubmit();
							yield return new WaitForSeconds(0.2f);
							yield return "trycancel";
						}
						yield return new WaitForSeconds(1);
						break;
					case 2:
						if (int.TryParse(tokens[1], out count) && count > 0 && count < 100)
							goto case 1;
						else if (tokens[1].Contains(':') && GeneralExtensions.TryParseTime(tokens[1], out time)) {
							count = 1;
							if (this.ZenModeActive ? time <= bombInfo.GetTime() : time >= bombInfo.GetTime())
								yield return "sendtochaterror The specified time has already passed.";
							else {
								yield return null;
								if (Math.Abs(time - bombInfo.GetTime()) >= 15) yield return "waiting music";
								i = (int) time;
								while ((int) bombInfo.GetTime() != i)
									yield return "trycancel The button was not pressed due to a request to cancel.";
								goto case 1;
							}
						}
						break;
					case 3:
						if (tokens[1].EqualsIgnoreCase("on") && tokens[2].Length == 1 && char.IsDigit(tokens[2][0])) {
							yield return null;
							while (!bombInfo.GetFormattedTime().Contains(tokens[2][0]))
								yield return "trycancel The button was not pressed due to a request to cancel.";
							goto case 1;
						}
						break;
					case 4:
						float time2;
						if (tokens[2].EqualsIgnoreCase("then") && GeneralExtensions.TryParseTime(tokens[1], out time) && GeneralExtensions.TryParseTime(tokens[3], out time2)) {
							int j;
							i = (int) time;
							j = (int) time2;
							if (this.ZenModeActive ? (j <= i) : (j >= i)) yield break;
							if (this.ZenModeActive ? i <= (int) bombInfo.GetTime() : i >= (int) bombInfo.GetTime())
								yield return "sendtochaterror The specified time has already passed.";

							yield return null;
							while ((int) bombInfo.GetTime() != i)
								yield return "trycancel The button was not pressed due to a request to cancel.";
							yield return "strikemessage pressing submit incorrectly";
							this.Connector.TwitchPressSubmit();
							//yield return new WaitForSeconds(0.1f);
							if (!this.twitchStruck) yield return "strikemessage releasing submit incorrectly";
							this.Connector.TwitchReleaseSubmit();
							yield return new WaitForSeconds(0.2f);

							while ((int) bombInfo.GetTime() != j)
								yield return "trycancel The button was not pressed due to a request to cancel.";
							goto case 1;
						}
						break;
				}
			} else if (tokens[0].EqualsIgnoreCase("mash")) {
				for (int i = 0; i < 50; ++i) {
					yield return "strikemessage pressing submit incorrectly";
					this.Connector.TwitchPressSubmit();
					//yield return new WaitForSeconds(0.1f);
					if (!this.twitchStruck) yield return "strikemessage releasing submit incorrectly";
					this.Connector.TwitchReleaseSubmit();
					yield return new WaitForSeconds(0.2f);
					yield return "trycancel";
				}
			} else if (tokens[0].EqualsIgnoreCase("hold")) {
				switch (tokens.Length) {
					case 1:
						yield return "strikemessage pressing submit incorrectly";
						this.Connector.TwitchPressSubmit();
						if (this.twitchStruck) {
							this.Connector.TwitchReleaseSubmit();
							yield return "sendtochat The button was not held due to a strike.";
						} else
							yield return new WaitForSeconds(1f);
						break;
					case 3:
						if (tokens[1].EqualsIgnoreCase("on")) {
							if (tokens[2].Length == 1 && char.IsDigit(tokens[2][0])) {
								yield return null;
								while (!bombInfo.GetFormattedTime().Contains(tokens[2][0]))
									yield return "trycancel The button was not pressed due to a request to cancel.";
								yield return "strikemessage pressing submit incorrectly";
								this.Connector.TwitchPressSubmit();
								if (this.twitchStruck) {
									this.Connector.TwitchReleaseSubmit();
									yield return "sendtochat The button was not held due to a strike.";
								} else
									yield return new WaitForSeconds(1f);
							}
						} else if (tokens[1].EqualsIgnoreCase("for")) {
							int i;
							if (int.TryParse(tokens[2], out i) && i >= 0 && i < 100) {
								yield return null;
								if (i >= 15) yield return "waiting music";

								yield return "strikemessage pressing submit incorrectly";
								this.Connector.TwitchPressSubmit();
								if (this.twitchStruck) {
									this.Connector.TwitchReleaseSubmit();
									yield return "sendtochat The button was not held due to a strike.";
								} else {
									while (this.InteractionTickCount < i)
										yield return "trycancel The button was not released due to a request to cancel.";
									yield return "strikemessage releasing submit incorrectly";
									this.Connector.TwitchReleaseSubmit();
								}
							}
						}
						break;
				}
			}
		}
	}

	public enum PasswordSolutionAction {
		/// <summary>Press the button a specific number of times. There may be a condition on when it may be pressed.</summary>
		// Strike when you press it at the wrong time or when you stop pressing it at wrong number.
		Press,
		/// <summary>Hold the button when a condition is met, and release it when another condition is met. Both conditions need not exist.</summary>
		// Strike when you hold it at the wrong time, or after holding at the correct time, release it too early or at the wrong time.
		HoldCondition,
		/// <summary>Hold the button for a specific number of timer ticks.</summary>
		// Strike when you release the button at the wrong time.
		HoldTime,
		/// <summary>Hold the button until the module disarms itself. There may be a condition on when it may be pressed.</summary>
		// Strike when you hold it at the wrong time, or after holding at the correct time, release it too early.
		HoldIndefinitely,
		/// <summary>Press the button twice with a specific delay.</summary>
		// Strike when you release the button after holding too long, or press it again at the wrong time.
		PressTwice,
		/// <summary>Mash the button until the module disarms itself.</summary>
		// Strike when you release the button after holding it too long or if you stop mashing too early.
		PressIndefinitely
	}
}
