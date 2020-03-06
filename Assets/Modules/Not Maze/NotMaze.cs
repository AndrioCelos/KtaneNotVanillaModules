using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NotVanillaModulesLib;
using UnityEngine;
using Random = UnityEngine.Random;

public class NotMaze : NotVanillaModule<NotMazeConnector> {
	public Vector2Int Position { get; private set; }
	public Vector2Int GoalPosition { get; private set; }

	public int SolveProgress { get; private set; }

	private int mazeIndex;
	private int distance;
	private Vector2Int startPosition;

	private float resetTime;

	private static readonly Vector2Int[,] defaultMazes = new[,] {
		{ new Vector2Int(0, 1), new Vector2Int(3, 5) },
		{ new Vector2Int(5, 0), new Vector2Int(3, 2) },
		{ new Vector2Int(2, 1), new Vector2Int(4, 4) },
		{ new Vector2Int(2, 2), new Vector2Int(4, 4) },
		{ new Vector2Int(5, 1), new Vector2Int(1, 4) },
		{ new Vector2Int(5, 0), new Vector2Int(4, 4) },
		{ new Vector2Int(2, 1), new Vector2Int(1, 4) },
		{ new Vector2Int(1, 1), new Vector2Int(5, 5) },
		{ new Vector2Int(2, 4), new Vector2Int(5, 5) },
		{ new Vector2Int(3, 0), new Vector2Int(5, 5) },
		{ new Vector2Int(3, 0), new Vector2Int(1, 1) },
		{ new Vector2Int(1, 1), new Vector2Int(4, 3) },
		{ new Vector2Int(1, 0), new Vector2Int(5, 3) },
		{ new Vector2Int(4, 1), new Vector2Int(1, 5) },
		{ new Vector2Int(1, 1), new Vector2Int(0, 4) },
		{ new Vector2Int(1, 1), new Vector2Int(4, 4) }
	};

	private const MazeDirection U = MazeDirection.Up;
	private const MazeDirection D = MazeDirection.Down;
	private const MazeDirection L = MazeDirection.Left;
	private const MazeDirection R = MazeDirection.Right;
	private static readonly MazeDirection[,,] defaultSolutionTable = new[,,] {
		//           1                        2                        3                        4                        5
		//             6                        7                        8                        9
		{ {U,L,R,L,R,U,D,R,L,L,U}, {L,U,R,R,D,L,D,U,R,R,R}, {L,R,D,D,R,D,U,D,R,R,U}, {D,D,L,D,L,R,L,R,D,U,U}, {U,L,U,R,R,L,D,U,R,D,D},
			{R,D,L,D,U,D,D,L,R,L,D}, {D,D,U,D,R,D,U,R,D,L,U}, {U,L,U,U,U,R,D,U,D,D,R}, {U,L,U,U,U,R,D,U,D,D,R} },
		{ {U,D,D,U,U,R,L,L,U,D,L}, {D,L,D,D,U,D,D,R,R,U,L}, {L,R,L,L,R,L,R,R,U,L,R}, {L,U,D,L,D,D,R,U,D,U,L}, {L,R,L,L,D,L,U,D,U,R,D},
			{U,U,U,R,U,R,U,R,R,U,R}, {R,U,U,U,L,D,D,U,R,R,L}, {R,U,R,D,L,L,R,U,L,R,R}, {U,U,D,R,R,R,D,L,L,U,U} },
		{ {D,D,U,U,D,R,U,U,R,D,U}, {R,D,L,L,R,D,U,D,D,L,U}, {U,D,U,U,D,U,R,D,R,R,D}, {L,U,L,L,D,D,D,D,D,U,L}, {L,D,U,R,U,L,R,D,L,U,R},
			{R,L,R,D,D,U,L,U,D,L,L}, {D,U,D,L,L,L,D,L,D,D,U}, {R,L,R,D,D,R,L,D,D,L,U}, {R,L,L,D,L,D,L,U,L,U,R} },
		{ {L,L,D,D,D,R,R,R,R,L,D}, {R,D,U,R,L,U,R,R,D,D,R}, {D,D,U,L,D,U,R,R,U,D,U}, {R,D,U,L,R,R,D,D,U,L,R}, {U,R,U,R,U,R,D,L,U,L,L},
			{R,L,R,R,U,L,L,L,D,R,D}, {R,U,D,D,R,D,R,L,U,R,U}, {R,U,U,L,U,L,L,L,U,L,L}, {D,R,R,U,D,L,D,U,D,D,R} },
		{ {R,U,R,R,U,L,D,L,L,U,U}, {U,U,R,L,U,D,D,U,D,R,U}, {U,L,D,D,R,U,D,L,D,R,R}, {L,L,U,U,U,R,D,U,R,D,R}, {U,L,U,D,R,U,L,D,D,U,R},
			{L,L,R,L,L,U,U,D,L,L,D}, {D,R,D,U,L,R,L,U,R,U,L}, {U,L,D,R,D,R,U,L,R,U,R}, {R,R,D,U,R,U,D,U,U,L,L} },
		{ {D,D,R,L,L,R,L,D,D,R,U}, {U,D,D,R,R,L,U,U,R,D,L}, {R,D,D,L,D,D,U,D,R,D,U}, {L,U,D,L,D,R,L,R,D,L,L}, {U,D,U,R,U,R,R,L,D,R,L},
			{D,D,R,L,D,U,R,R,L,R,R}, {R,R,R,U,U,L,D,L,D,L,D}, {U,L,L,U,U,L,D,D,U,U,R}, {L,L,L,R,U,R,D,U,L,D,U} },
		{ {D,L,L,U,L,L,U,R,U,R,D}, {D,U,D,L,R,D,R,U,L,U,U}, {D,U,R,D,L,L,L,U,L,D,R}, {L,D,U,D,L,D,R,D,L,L,L}, {U,U,L,R,D,U,D,R,D,L,L},
			{D,L,R,R,U,L,R,R,R,U,L}, {U,D,U,U,R,D,D,R,L,L,R}, {L,U,L,U,U,R,U,R,U,U,D}, {D,D,L,D,U,L,U,R,L,D,D} },
		{ {U,D,L,D,L,L,U,R,D,L,L}, {U,U,L,D,L,R,L,R,R,U,U}, {D,L,U,D,D,U,R,R,L,D,R}, {L,R,D,R,U,U,U,D,L,L,U}, {L,U,R,L,U,U,R,D,D,U,D},
			{L,U,U,D,D,R,L,D,L,R,U}, {D,R,U,U,L,D,L,L,U,R,U}, {L,U,L,D,L,U,L,R,R,D,D}, {D,U,D,D,D,L,U,D,R,R,R} },
		{ {R,L,D,L,D,U,R,R,D,D,D}, {R,R,R,D,R,L,U,L,D,R,D}, {D,R,D,U,R,L,U,U,L,R,R}, {D,R,U,U,D,U,L,L,L,L,R}, {U,L,U,D,L,L,U,D,D,R,R},
			{R,D,D,U,L,U,R,L,U,U,D}, {R,L,U,L,D,U,D,U,L,U,R}, {L,D,U,U,U,U,D,L,R,L,D}, {U,L,D,U,D,D,L,U,D,R,L} },
		{ {D,U,U,U,D,D,D,R,U,U,U}, {L,U,U,D,R,R,R,D,R,L,D}, {U,L,R,L,R,U,R,U,R,L,L}, {R,L,D,D,D,L,U,U,U,D,L}, {D,R,U,R,D,R,L,R,R,D,L},
			{U,U,L,L,R,U,U,D,R,L,R}, {D,D,D,D,U,L,L,R,L,L,D}, {D,L,D,R,L,L,D,L,D,U,U}, {L,D,D,D,L,U,R,D,R,D,U} },
		{ {D,D,D,L,U,U,R,D,U,L,D}, {U,D,R,L,L,L,D,R,R,U,L}, {L,L,D,R,D,U,D,L,U,U,D}, {D,R,R,U,D,U,R,U,D,R,U}, {U,D,U,D,L,L,D,U,D,U,D},
			{U,D,D,L,L,U,D,R,R,D,R}, {D,R,L,R,D,U,L,R,D,D,R}, {R,R,U,L,L,L,U,U,D,D,R}, {L,U,L,D,D,R,U,L,D,D,L} },
		{ {R,U,U,R,L,U,R,R,U,R,D}, {L,U,D,L,L,L,D,L,R,R,U}, {R,D,D,D,L,L,U,U,L,L,U}, {D,D,R,L,R,U,D,L,R,R,D}, {L,L,R,D,D,D,R,R,R,D,U},
			{U,D,L,R,R,R,D,D,U,R,R}, {D,D,R,D,R,R,D,U,D,L,D}, {D,R,R,R,U,D,D,U,L,L,D}, {U,R,R,D,L,D,L,L,R,U,L} },
		{ {R,R,U,D,L,R,U,R,U,U,L}, {D,D,R,R,L,R,D,R,L,U,U}, {U,D,U,L,R,D,U,D,R,L,U}, {L,R,D,R,R,R,U,L,R,D,U}, {R,U,R,L,U,R,L,D,D,U,D},
			{D,D,D,L,D,R,L,R,U,R,L}, {L,L,R,D,U,L,L,U,U,R,D}, {U,U,D,R,L,R,D,D,U,R,R}, {U,U,L,R,L,D,D,L,R,U,R} },
		{ {R,L,D,R,U,D,D,U,U,R,D}, {U,D,D,D,U,D,L,D,U,R,U}, {U,L,D,L,L,D,U,U,R,R,D}, {U,R,D,U,U,R,U,D,D,D,D}, {L,D,D,U,L,L,U,D,L,D,D},
			{U,L,U,L,D,U,L,D,R,D,L}, {R,R,R,U,U,D,R,L,R,L,R}, {U,L,L,L,U,R,R,L,L,U,R}, {R,U,L,R,L,L,U,L,U,R,U} },
		{ {U,U,R,R,L,D,R,D,L,L,L}, {R,L,R,D,L,U,L,D,L,L,R}, {D,U,L,D,L,L,L,R,R,R,U}, {U,L,D,U,D,U,L,U,D,R,R}, {D,L,U,R,U,R,U,R,U,R,D},
			{D,R,U,D,R,D,R,L,D,L,D}, {D,U,U,U,R,U,L,D,R,L,D}, {L,D,D,U,R,U,R,R,L,L,D}, {R,D,R,U,U,R,D,U,R,D,R} },
		{ {U,L,L,D,U,D,U,L,D,U,R}, {R,D,R,R,U,U,L,R,R,L,R}, {R,D,U,D,U,D,L,L,U,D,D}, {L,L,U,U,R,D,R,L,U,D,L}, {L,L,U,L,R,R,R,L,U,U,U},
			{L,R,D,D,U,U,U,L,D,U,U}, {D,L,R,D,L,R,D,D,R,D,R}, {L,R,U,R,D,L,L,R,R,R,R}, {U,D,U,L,L,U,D,L,R,D,D} },
	};

	public ReadOnlyCollection<Vector2Int> Markers { get; private set; }

	public override void Start() {
		base.Start();
		this.Connector.KMBombModule.OnActivate = this.KMBombModule_OnActivate;
		this.Connector.SetGoal(this.GoalPosition = new Vector2Int(Random.Range(0, 6), Random.Range(0, 6)));
		do {
			this.startPosition = new Vector2Int(Random.Range(0, 6), Random.Range(0, 6));
			this.distance = Math.Abs(this.GoalPosition.x - this.startPosition.x) + Math.Abs(this.GoalPosition.y - this.startPosition.y);
		} while (this.distance == 0 || this.distance > 9);
		this.Connector.SetPosition(this.Position = this.startPosition);

		this.mazeIndex = Random.Range(0, defaultMazes.GetLength(0));
		var rotation = Random.Range(0, 4);
		var markers = new[] { RotatePoint(defaultMazes[this.mazeIndex, 0], rotation), RotatePoint(defaultMazes[this.mazeIndex, 1], rotation) };
		this.Markers = Array.AsReadOnly(markers);
		foreach (var point in markers) this.Connector.ShowMarker(point.x, point.y);

		this.Log("Marker positions: {0}. Dot position: {1}. Triangle position: {2}. Correct inputs: {3}.",
			this.Markers.Select(p => p.ToString()).Join(", "), this.startPosition, this.GoalPosition,
			Enumerable.Range(0, 11).Select(i => defaultSolutionTable[this.mazeIndex, this.distance - 1, i].ToString()[0]).Join(""));

		this.Connector.InitialiseMaze();

		this.Connector.ButtonPressed += (s, e) => this.Move(e.Direction);
	}

	private static Vector2Int RotatePoint(Vector2Int point, int clockwiseTurns) {
		switch (clockwiseTurns & 3) {
			case 0: return point;
			case 1: return new Vector2Int(5 - point.y, point.x);
			case 2: return new Vector2Int(5 - point.x, 5 - point.y);
			case 3: return new Vector2Int(point.y, 5 - point.x);
			default: throw new ArgumentException("Unsigned modulo 4 out of range. Not sure how this happened.");
		}
	}

	public void Update() {
		if (this.resetTime > 0) {
			this.resetTime -= Time.deltaTime;
			if (this.resetTime <= 0) {
				this.Log("Input timed out after 10 seconds. The sequence entered was {0}.",
					Enumerable.Range(0, this.SolveProgress).Select(i => this.GetCorrectDirection(i).ToString()[0]).Join(""));
				this.SolveProgress = 0;
				this.resetTime = 0;
				this.Connector.SetPosition(this.Position = this.startPosition);
			}
		}
	}

	private void KMBombModule_OnActivate() {
		this.Connector.Activate();
	}

	public override void Disarm() {
		base.Disarm();
		this.Connector.SetPosition(this.GoalPosition);
	}

	public void Move(MazeDirection direction) {
		if (this.Solved) return;

		int dx, dy;
		switch (direction) {
			case MazeDirection.Up: dx = 0; dy = -1; break;
			case MazeDirection.Down: dx = 0; dy = 1; break;
			case MazeDirection.Left: dx = -1; dy = 0; break;
			case MazeDirection.Right: dx = 1; dy = 0; break;
			default: throw new ArgumentOutOfRangeException("direction");
		}

		var correctMazeDirection = this.GetCorrectDirection();
		if (direction == correctMazeDirection) {
			if (this.SolveProgress == 0) this.resetTime = 10;
			++this.SolveProgress;
			if (this.SolveProgress >= defaultSolutionTable.GetLength(2)) {
				this.Log("The correct sequence was entered ({0}).",
					Enumerable.Range(0, this.SolveProgress).Select(i => this.GetCorrectDirection(i).ToString()[0]).Join(""));
				this.Disarm();
				this.resetTime = 0;
			} else {
				var position = this.Position + new Vector2Int(dx, dy);
				if (position.x < 0 || position.x >= 6 || position.y < 0 || position.y >= 6) return;
				this.Connector.SetPosition(this.Position = position);
			}
		} else {
			this.Log("An incorrect sequence was entered ({0}{1}); the correct button was {2}.",
				Enumerable.Range(0, this.SolveProgress).Select(i => this.GetCorrectDirection(i).ToString()[0]).Join(""),
				direction.ToString()[0], correctMazeDirection);
			this.Connector.KMBombModule.HandleStrike();
			this.Connector.ShowWall(this.Position, direction);
			this.SolveProgress = 0;
			this.resetTime = 0;
			this.Connector.SetPosition(this.Position = this.startPosition);
		}
	}

	private MazeDirection GetCorrectDirection() { return this.GetCorrectDirection(this.SolveProgress); }
	private MazeDirection GetCorrectDirection(int step) { return defaultSolutionTable[this.mazeIndex, this.distance - 1, step]; }

	// Twitch Plays support
	public static readonly string TwitchHelpMessage
		= "!{0} press up down left right | !{0} udlr";
	public IEnumerator ProcessTwitchCommand(string command) {
		var tokens = command.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
		if (tokens.Length == 0) yield break;

		var enumerable = tokens[0].EqualsIgnoreCase("press") || tokens[0].EqualsIgnoreCase("move") ? tokens.Skip(1) : tokens;
		var buttons = new List<MazeDirection>();
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

	private static bool TryParseCharDirections(IEnumerable<string> fields, IList<MazeDirection> list) {
		foreach (var token in fields) {
			foreach (var c in token) {
				switch (c) {
					case 'u': case 'U': case 'n': case 'N': list.Add(MazeDirection.Up); break;
					case 'd': case 'D': case 's': case 'S': list.Add(MazeDirection.Down); break;
					case 'l': case 'L': case 'w': case 'W': list.Add(MazeDirection.Left); break;
					case 'r': case 'R': case 'e': case 'E': list.Add(MazeDirection.Right); break;
					default: return false;
				}
			}
		}
		return true;
	}
	private static bool TryParseWordDirections(IEnumerable<string> fields, IList<MazeDirection> list) {
		foreach (var token in fields) {
			switch (token.ToLowerInvariant()) {
				case "up": case "north": list.Add(MazeDirection.Up); break;
				case "down": case "south": list.Add(MazeDirection.Down); break;
				case "left": case "west": list.Add(MazeDirection.Left); break;
				case "right": case "east": list.Add(MazeDirection.Right); break;
				default: return false;
			}
		}
		return true;
	}

	public IEnumerator TwitchHandleForcedSolve() {
		while (!this.Solved) {
			this.Connector.TwitchPress(this.GetCorrectDirection());
			yield return new WaitForSeconds(0.1f);
		}
	}
}
