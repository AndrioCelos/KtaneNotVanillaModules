using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class NotWireswordDictionaryTester : MonoBehaviour {
	public string[] NoVowelTable;
	public string[] VowelTable;
	public string[] Words;
	public TextAsset Dictionary;

	private readonly Dictionary<char, List<int>> noVowelTable = new Dictionary<char, List<int>>();
	private readonly Dictionary<char, List<int>> vowelTable = new Dictionary<char, List<int>>();

	private List<string> badWordsNoVowel;
	private List<string> badWordsVowel;
	private List<string> badWords;
	private List<string> goodWords;

	[ContextMenu("Run test")]
	public void Start() {
		BuildDictionary(this.NoVowelTable, this.noVowelTable);
		BuildDictionary(this.VowelTable, this.vowelTable);

		var words = this.Words.Length > 0 ? this.Words : this.Dictionary != null ? this.Dictionary.text.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries) : null;
		if (words == null) {
			Debug.LogError("NotWireswordDictionaryTester has no words to test.");
			return;
		}

		var badWords = new[] { new HashSet<string>(), new HashSet<string>(), new HashSet<string>() };
		var goodWords = new[] { new HashSet<string>(), new HashSet<string>(), new HashSet<string>() };
		var letters2 = new char[6];
		for (int vowel = 0; vowel < 2; ++vowel) {
			var table2 = vowel == 0 ? noVowelTable : vowelTable;
			foreach (var word2 in words) {
				for (int i = 0; i < 6; ++i) letters2[i] = '\0';
				if (Try(table2, word2.ToUpperInvariant()))
					goodWords[vowel].Add(word2);
				else
					badWords[vowel].Add(word2);
			}
		}
		badWords[2].UnionWith(badWords[0]);
		badWords[2].IntersectWith(badWords[1]);
		badWords[0].ExceptWith(badWords[2]);
		badWords[1].ExceptWith(badWords[2]);
		goodWords[2].UnionWith(goodWords[0]);
		goodWords[2].IntersectWith(goodWords[1]);

		this.badWordsNoVowel = badWords[0].ToList();
		this.badWordsNoVowel.Sort();
		this.badWordsVowel = badWords[1].ToList();
		this.badWordsVowel.Sort();
		this.badWords = badWords[2].ToList();
		this.badWords.Sort();
		this.goodWords = goodWords[2].ToList();
		this.goodWords.Sort();

		Debug.LogFormat("Words which can appear only with no vowel: {0}", this.badWordsVowel.Join(", "));
		Debug.LogFormat("Words which can appear only with a vowel: {0}", this.badWordsNoVowel.Join(", "));
		Debug.LogFormat("Words which can't appear ever: {0}", this.badWords.Join(", "));
		Debug.LogFormat("Words which can appear always: {0}", this.goodWords.Join(", "));
	}

	[ContextMenu("Write results to files")]
	public void WriteResultsToFile() {
		if (!Application.isPlaying) this.Start();
		WriteSetToFile(this.badWordsNoVowel, "BadWordsNoVowel.txt");
		WriteSetToFile(this.badWordsVowel, "BadWordsVowel.txt");
		WriteSetToFile(this.badWords, "BadWords.txt");
		WriteSetToFile(this.goodWords, "GoodWords.txt");
		Debug.LogFormat("Results written to {0}", Environment.CurrentDirectory);
	}
	private static void WriteSetToFile(IEnumerable<string> words, string file) {
		using (var writer = new StreamWriter(file)) {
			foreach (var word in words) writer.WriteLine(word);
		}
	}

	private static void BuildDictionary(IList<string> inTable, IDictionary<char, List<int>> outTable) {
		for (int i = 0; i < inTable.Count; ++i) {
			foreach (var c in inTable[i].Where(char.IsLetter).Select(char.ToUpperInvariant)) {
				List<int> list;
				if (!outTable.TryGetValue(c, out list)) outTable[c] = list = new List<int>();
				if (!list.Contains(i)) list.Add(i);
			}
		}
	}

	private static bool Try(IDictionary<char, List<int>> table, string word) {
		word = word.ToUpperInvariant();
		if (!word.All(table.ContainsKey)) return false;
		return Try(table, word, 0, new char[6]);
	}
	private static bool Try(IDictionary<char, List<int>> table, string word, int index, char[] letters) {
		if (index >= word.Length) return true;
		foreach (var pos in table[word[index]]) {
			if (letters[pos] == '\0') {
				letters[pos] = word[index];
				if (Try(table, word, index + 1, letters)) return true;
				letters[pos] = '\0';
			}
		}
		return false;
	}

}
