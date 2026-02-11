using FuzzySharp;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace appsvc_function_dev_cm_listmgmt_dotnet001
{
    public static class SeekerHelpers
    {
        private static readonly List<string> keyPhrases = BuildKeyPhrases();

        public  static int CountSeekerViolations(JobOpportunity opportunity, ILogger logger)
        {
            var violationCount = 0;

            try
            {
                var inputs = new string[]
                {
                opportunity.JobTitleEn,
                opportunity.JobTitleFr,
                opportunity.JobDescriptionEn,
                opportunity.JobDescriptionFr
                };

                foreach (var input in inputs)
                {
                    if (string.IsNullOrEmpty(input)) continue;

                    var normInput = NormalizeText(input).ToLowerInvariant();
                    var sentences = SplitIntoSentences(normInput);

                    foreach (var sentence in sentences)
                    {
                        foreach (var phrase in keyPhrases)
                        {
                            string normPhrase = NormalizeText(phrase).ToLowerInvariant();

                            if (PhraseIsFirstPerson(normPhrase))
                            {
                                var hasFirstPerson = ContainsFirstPersonEnglish(sentence) || ContainsFirstPersonFrench(sentence);

                                if (!hasFirstPerson)
                                    continue;
                            }

                            var matchScore = Fuzz.PartialRatio(normPhrase, sentence);
                            if (matchScore >= 85)
                            {
                                violationCount++;
                                logger.LogWarning($"Violation: \"{sentence}\"\nFuzzy Match: \"{normPhrase}\"");
                            }
                        }
                    }
                }

                if (violationCount > 0)
                    logger.LogWarning($"Total seeker violations: {violationCount}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Count seeker violations failed: {ex.Message} - {ex.StackTrace}");
            }

            return violationCount;
        }

        private static List<string> BuildKeyPhrases()
        {
            var phrases = new List<string>
            {
                "seeking a position",
                "I’m seeking a role in",
                "I'm looking for",
                "I'm qualified for",
                "I'm familiar with",
                "I'm based in",
                "I bring to the table",
                "I'm open to",
                "I previously worked at",
                "actively seeking opportunities",
                "I'm ready to",
                "I can be reached at",
                "À la recherche d’un emploi",
                "Je recherche un emploi en",
                "Je recherche un emploi dans",
                "Je recherche",
                "Je suis parfaitement qualifié",
                "Je connais bien",
                "Je me trouve à",
                "Je me trouve en",
                "Je me trouve au",
                "J’apporte à l’emploi",
                "Je suis ouvert à",
                "J’ai travaillé par le passé chez",
                "À la recherche active d’offres",
                "Je suis prêt à",
                "Vous pouvez me joindre à"
            };

            var expanded = new List<string>();

            foreach (var phrase in phrases)
            {
                expanded.Add(phrase);

                if (phrase.Contains("I'm", StringComparison.OrdinalIgnoreCase))
                    expanded.Add(phrase.Replace("I'm", "I am", StringComparison.OrdinalIgnoreCase));

                if (phrase.Contains("I'll", StringComparison.OrdinalIgnoreCase))
                    expanded.Add(phrase.Replace("I'll", "I will", StringComparison.OrdinalIgnoreCase));

                if (phrase.Contains("I've", StringComparison.OrdinalIgnoreCase))
                    expanded.Add(phrase.Replace("I've", "I have", StringComparison.OrdinalIgnoreCase));

                if (phrase.Contains("I'd", StringComparison.OrdinalIgnoreCase))
                    expanded.Add(phrase.Replace("I'd", "I would", StringComparison.OrdinalIgnoreCase));
            }

            return expanded.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static List<string> SplitIntoSentences(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<string>();

            input = input.Replace("...", "**ELLIPSIS**");

            string[] abbreviations =
            {
                "Mr.", "Mrs.", "Ms.", "Dr.", "Prof.", "Sr.", "Jr.",
                "St.", "Mt.", "etc.", "i.e.", "e.g.", "vs."
            };

            foreach (var abbr in abbreviations)
            {
                input = input.Replace(abbr, abbr.Replace(".", "**DOT**"));
            }

            // Protect emails
            input = Regex.Replace(input, @"\b[\w\.-]+@[\w\.-]+\.\w+\b", m => m.Value.Replace(".", "**DOT**"));
            // Protect URLs
            input = Regex.Replace(input, @"https?://[^\s]+", m => m.Value.Replace(".", "**DOT**"));
            // Protect decimal numbers
            input = Regex.Replace(input, @"\b\d+\.\d+\b", m => m.Value.Replace(".", "**DOT**"));

            // Split on sentence boundaries
            var sentences = Regex.Split(input, @"(?<=[.!?])\s+|\r?\n+")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToList();

            // Restore the ellipsis & dots
            for (int i = 0; i < sentences.Count; i++)
            {
                sentences[i] = sentences[i].Replace("**ELLIPSIS**", "...").Replace("**DOT**", ".");
            }

            return sentences;
        }

        private static string NormalizeText(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return string.Empty;

            s = WebUtility.HtmlDecode(s);

            s = Regex.Replace(s, "<.*?>", " ");

            s = s.Normalize(NormalizationForm.FormD);

            s = s.Replace('\u00A0', ' ')
                 .Replace('\u2019', '\'')
                 .Replace('\u2018', '\'')
                 .Replace('\u201C', '"')
                 .Replace('\u201D', '"')
                 .Replace('\u2013', '-')
                 .Replace('\u2014', '-');

            s = RemoveAccents(s);

            s = Regex.Replace(s, @"\s+", " ");

            return s.Normalize(NormalizationForm.FormC).Trim();
        }

        private static string RemoveAccents(string text)
        {
            var chars = text.Normalize(NormalizationForm.FormD).ToCharArray();
            var sb = new StringBuilder();

            foreach (char c in chars)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private static bool ContainsFirstPersonEnglish(string text)
        {
            return Regex.IsMatch(text, @"\b(i am|i'm|i have|i've|i would|i'd|i will|i'll)\b");
        }

        private static bool ContainsFirstPersonFrench(string text)
        {
            return Regex.IsMatch(text, @"\b(je suis|j'suis|j ai|j'ai|je me|j ai travaille|j'ai travaille)\b");
        }

        private static bool PhraseIsFirstPerson(string phrase)
        {
            return Regex.IsMatch(phrase, @"^(i am|i'm|i have|i've|i would|i'd|i will|i'll|je suis|j'suis)", RegexOptions.IgnoreCase);
        }
    }
}
