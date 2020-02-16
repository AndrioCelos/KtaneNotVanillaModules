using UnityEngine;

namespace NotVanillaModulesLib {
	internal static class TwitchExtensions {
		public static void Press(KMSelectable selectable) => selectable.OnInteract?.Invoke();
		public static void Release(KMSelectable selectable) => selectable.OnInteractEnded?.Invoke();
		public static void Click(KMSelectable selectable) {
			selectable.OnInteract?.Invoke();
			selectable.OnInteractEnded?.Invoke();
		}

#if (DEBUG)
		public static void Press(Component component) => Press(component.GetComponent<KMSelectable>());
		public static void Release(Component component) => Release(component.GetComponent<KMSelectable>());
		public static void Click(Component component) => Click(component.GetComponent<KMSelectable>());
#else
		public static void Press(Selectable selectable) {
			selectable.HandleInteract();
		}
		public static void Press(Component component) => Press(component.GetComponent<Selectable>());

		public static void Release(Selectable selectable) {
			selectable.OnInteractEnded();
			selectable.SetHighlight(false);
		}
		public static void Release(Component component) => Release(component.GetComponent<Selectable>());

		public static void Click(Selectable selectable) {
			Press(selectable);
			Release(selectable);
		}
		public static void Click(Component component) => Click(component.GetComponent<Selectable>());
#endif
	}
}
