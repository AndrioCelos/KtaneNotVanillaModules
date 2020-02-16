using System;
using System.Linq;

public class TimerCondition {
	public delegate bool ConditionDelegate(float seconds, string timerString);

	public ConditionDelegate Condition { get; private set; }
	public string Description { get; private set; }

	public TimerCondition(ConditionDelegate condition, string description) {
		this.Condition = condition;
		this.Description = description;
	}

	public bool Invoke(float seconds, string timerString) { return this.Condition.Invoke(seconds, timerString); }
	public bool Invoke(KMBombInfo bombInfo) { return this.Condition.Invoke(bombInfo.GetTime(), bombInfo.GetFormattedTime()); }

	private static bool IsPrime(int digit) {
		return digit == 2 || digit == 3 || digit == 5 || digit == 7;
	}
	private static bool IsPrimeOrZero(int digit) {
		return digit == 0 || digit == 2 || digit == 3 || digit == 5 || digit == 7;
	}

	public static TimerCondition AnyTime() {
		return new TimerCondition((t, s) => true, "at any time");
	}
	public static TimerCondition SecondsDigitIs(int digit) {
		return new TimerCondition((t, s) => (int) t % 10 == digit, "when the right-most seconds digit is " + digit);
	}
	public static TimerCondition SecondsDigitIsNot(int digit) {
		return new TimerCondition((t, s) => (int) t % 10 != digit, "when the right-most seconds digit is not " + digit);
	}
	public static TimerCondition TensDigitIs(int digit) {
		return new TimerCondition((t, s) => (int) t / 10 % 6 == digit, "when the 10s of seconds digit is " + digit);
	}
	public static TimerCondition TensDigitIsNot(int digit) {
		return new TimerCondition((t, s) => (int) t / 10 % 6 != digit, "when the 10s of seconds digit is not " + digit);
	}
	public static TimerCondition Contains(char digit) {
		return new TimerCondition((t, s) => s.Contains(digit), "when any digit on when the timer is " + digit);
	}
	public static TimerCondition SecondsDigitIsEven() {
		return new TimerCondition((t, s) => (int) t % 2 == 0, "when the right-most seconds digit is even");
	}
	public static TimerCondition SecondsDigitIsOdd() {
		return new TimerCondition((t, s) => (int) t % 2 == 1, "when the right-most seconds digit is odd");
	}
	public static TimerCondition SecondsDigitsAddTo(int sum) {
		return new TimerCondition((t, s) => (int) t % 10 + (int) t / 10 % 6 == sum, "when the two seconds digits add to " + sum);
	}

	internal static TimerCondition MinutesIsEven() {
		return new TimerCondition((t, s) => (int) t % 120 < 60, "when the number of whole minutes on the timer is even");
	}
	internal static TimerCondition MinutesIsOdd() {
		return new TimerCondition((t, s) => (int) t % 120 >= 60, "when the number of whole minutes on the timer is odd");
	}

	public static TimerCondition SecondsDigitIsPrimeOrZero() {
		return new TimerCondition((t, s) => IsPrimeOrZero((int) t % 10), "when the right-most seconds digit is prime or zero");
	}
	public static TimerCondition SecondsDigitIsNotPrime() {
		return new TimerCondition((t, s) => !IsPrime((int) t % 10), "when the right-most seconds digit is not prime");
	}
	public static TimerCondition TensDigitIsPrimeOrZero() {
		return new TimerCondition((t, s) => IsPrimeOrZero((int) t / 10 % 6), "when the 10s of seconds digit is prime or zero");
	}
	public static TimerCondition SecondsDigitMatchesLeftDigit() {
		return new TimerCondition((t, s) => (int) t % 10 == s[0] - '0', "when the right-most seconds digit matches the left-most timer digit");
	}
	public static TimerCondition SecondsDigitsMatch() {
		return new TimerCondition((t, s) => (int) t % 10 == (int) t / 10 % 6, "when the two seconds digits match");
	}
	public static TimerCondition SecondsDigitsDifferBy(int difference) {
		return new TimerCondition((t, s) => Math.Abs((int) t % 10 - (int) t / 10 % 6) == difference, "when the two seconds digits differ by " + difference);
	}

	public override string ToString() {
		return this.Description;
	}
}
