# Not Wiresword

Not Wiresword Dictionary Tester is a script that can evaluate a list of words, determining whether a combination of wires that can spell that word exists.

To use the script, first fill in the Table fields. Each should contain six strings: one for each wire on the module. Each string contains all letters that may be associated with the corresponding wire. Then, provide a list of words in either the Words or Dictionary field. Dictionary should point to a text file which contains words separated by any whitespace.

The test scene contains an instance that is pre-filled with the default module rules.

To run the test, either enter play mode with the script active, or right click the heading in the Inspector and select `Run test` or `Write results to files`. When writing to files, the results will be written to the Unity project directory.

The words provided will be separated into four categories:

* Words that can appear whether or not the bomb serial number has a vowel
* Words that can appear only when the serial number has a vowel
* Words that can appear only when the serial number does not have a vowel
* Words that can never appear
