using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NotVanillaModulesLib;
using Random = UnityEngine.Random;

public class NotSimaze : NotVanillaModule<NotSimonConnector> {
	private int mazeIndex;
	// In this module, the x axis points east and the y axis points north.
	private int x;
	private int y;
	private int goalX;
	private int goalY;

	private bool firstMoveMade;
	private Coroutine coroutine;
	private bool tone = false;

	// Musical notes
	private const float Cs5 = 554.365f;
	private const float E5 = 659.255f;
	private const float G5 = 783.991f;
	private const float B5 = 987.767f;

	private const MazeCell z = 0;
	private const MazeCell E = MazeCell.WallEast;
	private const MazeCell N = MazeCell.WallNorth;
	private const MazeCell EN = MazeCell.WallEast | MazeCell.WallNorth;
	private static readonly MazeCell[,,] mazes = new[, ,] {
		{
			{ z , N , N , E , z , E  },
			{ E , z , E , N , EN, E  },
			{ N , E , N , z , EN, E  },
			{ z , EN, N , z , E , EN },
			{ EN, z , E , EN, z , E  },
			{ N , EN, N , N , EN, EN }
		},
		{
			{ N , N , E , N , E , E  },
			{ N , E , N , z , EN, E  },
			{ z , EN, z , N , z , EN },
			{ E , E , N , EN, N , E  },
			{ E , N , E , z , E , E  },
			{ N , N , N , EN, N , EN }
		},
		{
			{ z , z , N , N , N , EN },
			{ E , EN, z , N , N , EN },
			{ E , z , EN, N , z , E  },
			{ E , N , N , N , EN, E  },
			{ z , z , EN, z , E , E  },
			{ EN, N , N , EN, N , EN }
		},
		{
			{ z , N , E , z , N , E  },
			{ z , EN, z , EN, E , E  },
			{ E , N , N , E , E , EN },
			{ EN, z , E , N , z , E  },
			{ E , EN, z , N , EN, E  },
			{ N , N , EN, N , N , EN }
		},
		{
			{ E , N , z , EN, N , E  },
			{ E , z , N , N , E , E  },
			{ E , E , z , EN, EN, E  },
			{ E , E , E , z , N , E  },
			{ N , N , N , N , EN, E  },
			{ N , N , N , N , N , EN }
		},
		{
			{ N , N , z , N , E , E  },
			{ z , E , z , E , EN, E  },
			{ EN, E , E , N , N , E  },
			{ z , N , N , E , z , EN },
			{ E , z , N , EN, z , E  },
			{ EN, N , N , EN, EN, EN }
		}
	};

	public override void Start() {
		base.Start();
		this.Connector.KMBombModule.OnActivate = this.KMBombModule_OnActivate;
		this.Connector.ButtonPressed += this.Connector_ButtonPressed;

		this.mazeIndex = Random.Range(0, 6);
		this.x = Random.Range(0, 6);
		this.y = Random.Range(0, 6);
		do {
			this.goalX = Random.Range(0, 6);
			this.goalY = Random.Range(0, 6);
		} while (this.goalX == this.x && this.goalY == this.y);

		this.Log("Using the {0} maze. The starting position is ({1}, {2}). The goal is ({3}, {4}).",
			(SimazeColour) this.mazeIndex, (SimazeColour) this.x, (SimazeColour) this.y, (SimazeColour) this.goalX, (SimazeColour) this.goalY);
	}

	private void Connector_ButtonPressed(object sender, SimonButtonEventArgs e) {
		if (this.Solved) return;
		this.tone = true;
		if (this.coroutine != null) this.StopCoroutine(this.coroutine);

		SimazeColour colour;
		switch (e.Colour) {
			case SimonButtons.Red: colour = SimazeColour.Red; break;
			case SimonButtons.Yellow: colour = SimazeColour.Yellow; break;
			case SimonButtons.Green: colour = SimazeColour.Green; break;
			case SimonButtons.Blue: colour = SimazeColour.Blue; break;
			default: throw new ArgumentOutOfRangeException("e");
		}
		this.Flash(colour);

		bool wall; string direction;
		switch (e.Colour) {
			case SimonButtons.Yellow: direction = "south"; wall = this.y == 0 || (mazes[this.mazeIndex, this.y - 1, this.x] & MazeCell.WallNorth) != 0; break;
			case SimonButtons.Red: direction = "north"; wall = (mazes[this.mazeIndex, this.y, this.x] & MazeCell.WallNorth) != 0; break;
			case SimonButtons.Blue: direction = "west"; wall = this.x == 0 || (mazes[this.mazeIndex, this.y, this.x - 1] & MazeCell.WallEast) != 0; break;
			case SimonButtons.Green: direction = "east"; wall = (mazes[this.mazeIndex, this.y, this.x] & MazeCell.WallEast) != 0; break;
			default: throw new ArgumentOutOfRangeException("e");
		}
		if (wall) {
			this.Log("You hit a wall in the {0} maze from ({1}, {2}) moving {3}.", (SimazeColour) this.mazeIndex, (SimazeColour) this.x, (SimazeColour) this.y, direction);
			this.Connector.KMBombModule.HandleStrike();
			this.coroutine = this.StartCoroutine(this.FlashCoroutine(2));
		} else {
			switch (e.Colour) {
				case SimonButtons.Yellow: --this.y; break;
				case SimonButtons.Red: ++this.y; break;
				case SimonButtons.Blue: --this.x; break;
				case SimonButtons.Green: ++this.x; break;
			}
			this.firstMoveMade = true;
			if (this.x == this.goalX && this.y == this.goalY) {
				this.Disarm();
				this.coroutine = null;
			} else
				this.coroutine = this.StartCoroutine(this.FlashCoroutine(2));
		}
	}

	private void KMBombModule_OnActivate() {
		if (this.coroutine == null) this.coroutine = this.StartCoroutine(this.FlashCoroutine(0));
	}

	private IEnumerator FlashCoroutine(float delay) {
		if (delay > 0) yield return new WaitForSeconds(delay);
		while (true) {
			if (this.firstMoveMade) {
				this.Flash((SimazeColour) this.x);
				yield return new WaitForSeconds(0.7f);
				this.Flash((SimazeColour) this.y);
			} else {
				this.Flash((SimazeColour) this.mazeIndex);
				yield return new WaitForSeconds(0.7f);
				this.Flash((SimazeColour) this.x);
				yield return new WaitForSeconds(0.7f);
				this.Flash((SimazeColour) this.y);
				yield return new WaitForSeconds(0.7f);
				this.Flash((SimazeColour) this.goalX);
				yield return new WaitForSeconds(0.7f);
				this.Flash((SimazeColour) this.goalY);
			}
			yield return new WaitForSeconds(2);
		}
	}

	private void Flash(SimazeColour colour) {
		SimonButtons colour1; SimonButtons? colour2;
		switch (colour) {
			case SimazeColour.Red: colour1 = SimonButtons.Red; colour2 = null; break;
			case SimazeColour.Orange: colour1 = SimonButtons.Red; colour2 = SimonButtons.Yellow; break;
			case SimazeColour.Yellow: colour1 = SimonButtons.Yellow; colour2 = null; break;
			case SimazeColour.Green: colour1 = SimonButtons.Green; colour2 = null; break;
			case SimazeColour.Blue: colour1 = SimonButtons.Blue; colour2 = null; break;
			case SimazeColour.Purple: colour1 = SimonButtons.Red; colour2 = SimonButtons.Blue; break;
			default: throw new ArgumentOutOfRangeException("colour");
		}

		if (colour2 == null) {
			this.Connector.FlashLight(colour1);
			if (this.tone) this.Connector.PlayTones(0.5f, GetTone(colour1));
		} else {
			this.Connector.FlashLight(colour1);
			this.Connector.FlashLight(colour2.Value);
			if (this.tone) this.Connector.PlayTones(0.2f, GetTone(colour1), GetTone(colour2.Value));
		}
	}

	private static float GetTone(SimonButtons colour) {
		switch (colour) {
			case SimonButtons.Red: return Cs5;
			case SimonButtons.Blue: return E5;
			case SimonButtons.Green: return G5;
			case SimonButtons.Yellow: return B5;
			default: throw new ArgumentOutOfRangeException("colour");
		}
	}

	// Twitch Plays support
	public static readonly string TwitchHelpMessage
		= "!{0} press red green blue yellow | !{0} rgby | !{0} press up right left down | !{0} press urld | !{0} press news";
	public IEnumerator ProcessTwitchCommand(string command) {
		var tokens = command.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
		if (tokens.Length == 0) yield break;

		var enumerable = tokens[0].EqualsIgnoreCase("press") || tokens[0].EqualsIgnoreCase("move") ? tokens.Skip(1) : tokens;
		var buttons = new List<SimonButtons>();
		if (!TryParseCharDirections(enumerable, buttons)) {
			buttons.Clear();
			if (!TryParseWordDirections(enumerable, buttons)) yield break;
		}
		if (buttons.Count == 0) yield break;

		yield return null;
		for (int i = 0; i < buttons.Count; ++i) {
			var direction = buttons[i];
			yield return string.Format("strikemessage pressing {0} (#{1} in command)", direction, i + 1);
			this.Connector.TwitchPress(direction);
			yield return new WaitForSeconds(0.1f);
		}
	}

	private static bool TryParseCharDirections(IEnumerable<string> tokens, IList<SimonButtons> list) {
		int mode = 0;
		// Disambiguate 'r'.
		foreach (var c in tokens.SelectMany(t => t)) {
			switch (c) {
				case 'y': case 'Y': case 'g': case 'G': case 'b': case 'B': mode = 1; break;
				case 'u': case 'U': case 'd': case 'D': case 'l': case 'L': mode = 2; break;
				case 'n': case 'N': case 's': case 'S': case 'e': case 'E': case 'w': case 'W': mode = 3; break;
			}
		}
		if (mode == 0) return false;  // `move r` is ambiguous and thus not allowed.

		foreach (var token in tokens) {
			foreach (var c in token) {
				switch (mode) {
					case 1:
						switch (c) {
							case 'r': case 'R': list.Add(SimonButtons.Red); break;
							case 'y': case 'Y': list.Add(SimonButtons.Yellow); break;
							case 'g': case 'G': list.Add(SimonButtons.Green); break;
							case 'b': case 'B': list.Add(SimonButtons.Blue); break;
							default: return false;
						}
						break;
					case 2:
						switch (c) {
							case 'u': case 'U': list.Add(SimonButtons.Red); break;
							case 'd': case 'D': list.Add(SimonButtons.Yellow); break;
							case 'r': case 'R': list.Add(SimonButtons.Green); break;
							case 'l': case 'L': list.Add(SimonButtons.Blue); break;
							default: return false;
						}
						break;
					case 3:
						switch (c) {
							case 'n': case 'N': list.Add(SimonButtons.Red); break;
							case 's': case 'S': list.Add(SimonButtons.Yellow); break;
							case 'e': case 'E': list.Add(SimonButtons.Green); break;
							case 'w': case 'W': list.Add(SimonButtons.Blue); break;
							default: return false;
						}
						break;
				}
			}
		}
		return true;
	}
	private static bool TryParseWordDirections(IEnumerable<string> tokens, IList<SimonButtons> list) {
		foreach (var token in tokens) {
			switch (token.ToLowerInvariant()) {
				case "up": case "north": case "red": list.Add(SimonButtons.Red); break;
				case "down": case "south": case "yellow": list.Add(SimonButtons.Yellow); break;
				case "right": case "east": case "green": list.Add(SimonButtons.Green); break;
				case "left": case "west": case "blue": list.Add(SimonButtons.Blue); break;
				default: return false;
			}
		}
		return true;
	}

	public IEnumerator TwitchHandleForcedSolve() {
		var directions = new SimonButtons[6, 6];
		var x = this.x; var y = this.y;
		directions[x, y] = (SimonButtons) (-1);

		var queue = new Queue<Vector2Int>();
		queue.Enqueue(new Vector2Int(x, y));

		int count = 0;
		while (queue.Count > 0) {
			++count;
			if (count > 2048) throw new InvalidOperationException("Infinite loop in maze solver");
			var point = queue.Dequeue();

			if (point.x == this.goalX && point.y == this.goalY) {
				var list = new List<SimonButtons>();
				while (true) {
					var direction2 = directions[point.x, point.y];
					if (direction2 < 0) break;
					list.Add(direction2);
					switch (direction2) {
						case SimonButtons.Green: --point.x; break;
						case SimonButtons.Red: --point.y; break;
						case SimonButtons.Blue: ++point.x; break;
						case SimonButtons.Yellow: ++point.y; break;
					}
				}
				for (int i = list.Count - 1; i >= 0; --i) {
					this.Connector.TwitchPress(list[i]);
					yield return new WaitForSeconds(0.1f);
				}
				yield break;
			}

			var direction = directions[point.x, point.y];
			if (direction != SimonButtons.Blue && (mazes[this.mazeIndex, point.y, point.x] & MazeCell.WallEast) == 0) {
				queue.Enqueue(new Vector2Int(point.x + 1, point.y));
				directions[point.x + 1, point.y] = SimonButtons.Green;
			}
			if (direction != SimonButtons.Yellow && (mazes[this.mazeIndex, point.y, point.x] & MazeCell.WallNorth) == 0) {
				queue.Enqueue(new Vector2Int(point.x, point.y + 1));
				directions[point.x, point.y + 1] = SimonButtons.Red;
			}
			if (direction != SimonButtons.Green && point.x > 0 && (mazes[this.mazeIndex, point.y, point.x - 1] & MazeCell.WallEast) == 0) {
				queue.Enqueue(new Vector2Int(point.x - 1, point.y));
				directions[point.x - 1, point.y] = SimonButtons.Blue;
			}
			if (direction != SimonButtons.Red && point.y > 0 && (mazes[this.mazeIndex, point.y - 1, point.x] & MazeCell.WallNorth) == 0) {
				queue.Enqueue(new Vector2Int(point.x, point.y - 1));
				directions[point.x, point.y - 1] = SimonButtons.Yellow;
			}
		}
		throw new InvalidOperationException("Couldn't solve the maze?!");
	}

	public enum SimazeColour {
		Red,
		Orange,
		Yellow,
		Green,
		Blue,
		Purple
	}

	[Flags]
	private enum MazeCell {
		WallNorth = 1,
		WallEast = 2
	}
}
