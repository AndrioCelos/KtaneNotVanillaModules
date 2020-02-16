using UnityEngine;

namespace NotVanillaModulesLib {
	public class Nudger : MonoBehaviour {
		public static Nudger Instance { get; private set; }

		public Nudger() {
			Instance = this;
		}

		public static void SetTarget(int index, Transform target) {
			if (Instance != null) Instance.Targets[index] = target;
		}

		public Transform Target;
		public Transform[] Targets;
		public float Speed = 0.01f;
		private int RepeatDelay = 30;
		public void Update() {
			bool changed = false;
			bool active = false;
			if (Input.GetKey(KeyCode.Keypad2) || Input.GetKey(KeyCode.Keypad4) || Input.GetKey(KeyCode.Keypad6) || Input.GetKey(KeyCode.Keypad8) || Input.GetKey(KeyCode.Keypad3) || Input.GetKey(KeyCode.Keypad9) || Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.Minus)) {
				if (RepeatDelay == 30) active = true;
				if (RepeatDelay == 0) active = true;
				else --RepeatDelay;
			} else {
				RepeatDelay = 30;
			}

			if (Input.GetKey(KeyCode.Alpha0) && Targets.Length > 9) {
				Target = Targets[9];
				Debug.LogFormat("[Nudger] Selected '{0}'", Target != null ? Target.name : "null");
			}
			for (int i = 0; i < 9 && i < Targets.Length; ++i) {
				if (Input.GetKey(KeyCode.Alpha1 + i)) {
					Target = Targets[i];
					Debug.LogFormat("[Nudger] Selected '{0}'", Target != null ? Target.name : "null");
				}
			}

			if (!active || Target == null) return;
			if (Input.GetKey(KeyCode.X)) {
				if (Input.GetKey(KeyCode.Keypad8)) {
					Target.localPosition = new Vector3(Target.localPosition.x + Speed, Target.localPosition.y, Target.localPosition.z);
					changed = true;
				}
				if (Input.GetKey(KeyCode.Keypad2)) {
					Target.localPosition = new Vector3(Target.localPosition.x - Speed, Target.localPosition.y, Target.localPosition.z);
					changed = true;
				}
				if (Input.GetKey(KeyCode.Keypad4)) {
					Target.localRotation = Quaternion.Euler(Target.localRotation.eulerAngles.x - 360 * Speed, Target.localRotation.eulerAngles.y, Target.localRotation.eulerAngles.z);
					changed = true;
				}
				if (Input.GetKey(KeyCode.Keypad6)) {
					Target.localRotation = Quaternion.Euler(Target.localRotation.eulerAngles.x + 360 * Speed, Target.localRotation.eulerAngles.y, Target.localRotation.eulerAngles.z);
					changed = true;
				}
				if (Input.GetKey(KeyCode.KeypadPlus)) {
					Target.localScale = new Vector3(Target.localScale.x + Speed, Target.localScale.y, Target.localScale.z);
					changed = true;
				}
				if (Input.GetKey(KeyCode.KeypadMinus)) {
					Target.localScale = new Vector3(Target.localScale.x - Speed, Target.localScale.y, Target.localScale.z);
					changed = true;
				}
			}
			if (Input.GetKey(KeyCode.Y)) {
				if (Input.GetKey(KeyCode.Keypad8)) {
					Target.localPosition = new Vector3(Target.localPosition.x, Target.localPosition.y + Speed, Target.localPosition.z);
					changed = true;
				}
				if (Input.GetKey(KeyCode.Keypad2)) {
					Target.localPosition = new Vector3(Target.localPosition.x, Target.localPosition.y - Speed, Target.localPosition.z);
					changed = true;
				}
				if (Input.GetKey(KeyCode.Keypad4)) {
					Target.localRotation = Quaternion.Euler(Target.localRotation.eulerAngles.x, Target.localRotation.eulerAngles.y - 360 * Speed, Target.localRotation.eulerAngles.z);
					changed = true;
				}
				if (Input.GetKey(KeyCode.Keypad6)) {
					Target.localRotation = Quaternion.Euler(Target.localRotation.eulerAngles.x, Target.localRotation.eulerAngles.y + 360 * Speed, Target.localRotation.eulerAngles.z);
					changed = true;
				}
				if (Input.GetKey(KeyCode.KeypadPlus)) {
					Target.localScale = new Vector3(Target.localScale.x, Target.localScale.y + Speed, Target.localScale.z);
					changed = true;
				}
				if (Input.GetKey(KeyCode.KeypadMinus)) {
					Target.localScale = new Vector3(Target.localScale.x, Target.localScale.y - Speed, Target.localScale.z);
					changed = true;
				}
			}
			if (Input.GetKey(KeyCode.Z)) {
				if (Input.GetKey(KeyCode.Keypad8)) {
					Target.localPosition = new Vector3(Target.localPosition.x, Target.localPosition.y, Target.localPosition.z + Speed);
					changed = true;
				}
				if (Input.GetKey(KeyCode.Keypad2)) {
					Target.localPosition = new Vector3(Target.localPosition.x, Target.localPosition.y, Target.localPosition.z - Speed);
					changed = true;
				}
				if (Input.GetKey(KeyCode.Keypad4)) {
					Target.localRotation = Quaternion.Euler(Target.localRotation.eulerAngles.x, Target.localRotation.eulerAngles.y, Target.localRotation.eulerAngles.z - 360 * Speed);
					changed = true;
				}
				if (Input.GetKey(KeyCode.Keypad6)) {
					Target.localRotation = Quaternion.Euler(Target.localRotation.eulerAngles.x, Target.localRotation.eulerAngles.y, Target.localRotation.eulerAngles.z + 360 * Speed);
					changed = true;
				}
				if (Input.GetKey(KeyCode.KeypadPlus)) {
					Target.localScale = new Vector3(Target.localScale.x, Target.localScale.y, Target.localScale.z + Speed);
					changed = true;
				}
				if (Input.GetKey(KeyCode.KeypadMinus)) {
					Target.localScale = new Vector3(Target.localScale.x, Target.localScale.y, Target.localScale.z - Speed);
					changed = true;
				}
			}

			if (Input.GetKey(KeyCode.P)) {
				if (Input.GetKey(KeyCode.Keypad2)) { Target.localPosition += Vector3.back * Speed; changed = true; }
				if (Input.GetKey(KeyCode.Keypad8)) { Target.localPosition += Vector3.forward * Speed; changed = true; }
				if (Input.GetKey(KeyCode.Keypad4)) { Target.localPosition += Vector3.left * Speed; changed = true; }
				if (Input.GetKey(KeyCode.Keypad6)) { Target.localPosition += Vector3.right * Speed; changed = true; }
				if (Input.GetKey(KeyCode.Keypad3)) { Target.localPosition += Vector3.down * Speed; changed = true; }
				if (Input.GetKey(KeyCode.Keypad9)) { Target.localPosition += Vector3.up * Speed; changed = true; }
			}
			if (Input.GetKey(KeyCode.S)) {
				if (Input.GetKey(KeyCode.Keypad2)) { Target.localScale += Vector3.back * Speed; changed = true; }
				if (Input.GetKey(KeyCode.Keypad8)) { Target.localScale += Vector3.forward * Speed; changed = true; }
				if (Input.GetKey(KeyCode.Keypad4)) { Target.localScale += Vector3.left * Speed; changed = true; }
				if (Input.GetKey(KeyCode.Keypad6)) { Target.localScale += Vector3.right * Speed; changed = true; }
				if (Input.GetKey(KeyCode.Keypad3)) { Target.localScale += Vector3.down * Speed; changed = true; }
				if (Input.GetKey(KeyCode.Keypad9)) { Target.localScale += Vector3.up * Speed; changed = true; }
			}
			if (Input.GetKey(KeyCode.R)) {
				if (Input.GetKey(KeyCode.Keypad2)) { Target.localEulerAngles += Vector3.back * Speed; changed = true; }
				if (Input.GetKey(KeyCode.Keypad8)) { Target.localEulerAngles += Vector3.forward * Speed; changed = true; }
				if (Input.GetKey(KeyCode.Keypad4)) { Target.localEulerAngles += Vector3.left * Speed; changed = true; }
				if (Input.GetKey(KeyCode.Keypad6)) { Target.localEulerAngles += Vector3.right * Speed; changed = true; }
				if (Input.GetKey(KeyCode.Keypad3)) { Target.localEulerAngles += Vector3.down * Speed; changed = true; }
				if (Input.GetKey(KeyCode.Keypad9)) { Target.localEulerAngles += Vector3.up * Speed; changed = true; }
			}

			if (Input.GetKey(KeyCode.Equals)) {
				Speed += 0.001f;
				Debug.Log("[Nudger] Speed is now " + Speed);
			}
			if (Input.GetKey(KeyCode.Minus)) {
				Speed -= 0.001f;
				Debug.Log("[Nudger] Speed is now " + Speed);
			}

			if (changed) {
				Debug.LogFormat("[Nudger] Transform of '{0}': {1} × {2} < {3}",
					Target.name, Target.localPosition.ToString("n4"), Target.localEulerAngles.ToString("n4"), Target.localScale.ToString("n4"));
			}
		}
	}
}
