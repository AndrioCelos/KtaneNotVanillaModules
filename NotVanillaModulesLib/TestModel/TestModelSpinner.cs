using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace NotVanillaModulesLib.TestModel {
	public class TestModelSpinner : MonoBehaviour {
		public TextMesh Text;
		public TestModelButton UpButton;
		public TestModelButton DownButton;

		public ReadOnlyCollection<char> Choices { get; private set; }

		public int SelectedIndex { get; private set; }
		public char SelectedChar { get; private set; }

		public event EventHandler SelectedCharChanged;

		public void Start() {
			this.Text.gameObject.SetActive(false);
			this.UpButton.Pressed += this.UpButton_Pressed;
			this.DownButton.Pressed += this.DownButton_Pressed;
		}

		private void UpButton_Pressed(object sender, KeypadButtonEventArgs e) {
			--this.SelectedIndex;
			if (this.SelectedIndex < 0) this.SelectedIndex = this.Choices.Count - 1;
			this.UpdateSelectedChar();
		}

		private void DownButton_Pressed(object sender, KeypadButtonEventArgs e) {
			++this.SelectedIndex;
			if (this.SelectedIndex >= this.Choices.Count) this.SelectedIndex = 0;
			this.UpdateSelectedChar();
		}

		private void UpdateSelectedChar() {
			this.SelectedChar = this.Choices[this.SelectedIndex];
			this.Text.text = this.SelectedChar.ToString();
			if (this.SelectedCharChanged != null) this.SelectedCharChanged.Invoke(this, EventArgs.Empty);
		}

		public void SetChoices(IEnumerable<char> choices) {
			this.Choices = choices.ToList().AsReadOnly();
			this.SelectedIndex = 0;
			this.UpdateSelectedChar();
		}

		public void Activate() {
			this.Text.gameObject.SetActive(true);
		}
	}
}
