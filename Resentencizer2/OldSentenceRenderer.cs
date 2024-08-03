using System.Text.RegularExpressions;

namespace Resentencizer2
{
	public class OldSentenceRenderer
	{
		private readonly Regex attachToPreviousWord = new Regex(@"(?: |^)([.\}\)\]]|\\[;:]|[?!,]+)(?: |$)", RegexOptions.Compiled);
		private readonly Regex attachToNextWord = new Regex(@"(?: |^)([#\[\{\(])(?: |$)", RegexOptions.Compiled);
		private readonly Regex attachQuotes = new Regex(@"(?: |^)(\\"")(?: |$)", RegexOptions.Compiled);
		private readonly Regex attachContractions = new Regex(@"(?: )(\\'\w+)", RegexOptions.Compiled);
		private readonly Regex attachPluralContractions = new Regex(@"(?: )(\\')(?: |$)", RegexOptions.Compiled);
		private readonly Regex attachDashes = new Regex(@"(\w+-)(?: )(\w+)", RegexOptions.Compiled);
		private readonly Regex attachMemeArrow = new Regex(@"(?:^)(\\>)(?: )", RegexOptions.Compiled);
		public string Render(string rawString)
		{
			var renderedString = attachToPreviousWord.Replace(rawString, m => m.Groups[1] + " ");
			renderedString = attachToNextWord.Replace(renderedString, m => " " + m.Groups[1]);
			var opening = false;
			renderedString = attachQuotes.Replace(renderedString, m =>
			{
				opening = !opening;
				if (opening) { return " " + m.Groups[1]; } else { return m.Groups[1] + " "; }
			});
			renderedString = attachContractions.Replace(renderedString, m => "" + m.Groups[1]);
			renderedString = attachPluralContractions.Replace(renderedString, m => m.Groups[1] + " ");
			renderedString = attachDashes.Replace(renderedString, m => m.Groups[1] + "" + m.Groups[2]);
			renderedString = attachMemeArrow.Replace(renderedString, m => m.Groups[1] + "");
			renderedString = renderedString.Trim();
			return renderedString;
		}
	}
}
