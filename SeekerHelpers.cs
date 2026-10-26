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
        public const int VIOLATIONS_MAX = 2;

        // PartialRatio score required to count as a match
        private const int FUZZY_THRESHOLD = 90;
        // phrases shorter than this use exact whole-word matching only
        private const int MIN_FUZZY_LENGTH = 15;  

        private static readonly Regex FirstPersonEn = new(@"(?<![\w.])i(?![\w.])|\bmy\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex FirstPersonFr = new(@"\b(je|j|mon|ma|mes)\b|\bm'", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // Does the phrase start with first person? Sentence must also be first person.
        private static readonly Regex PhraseStartsFirstPerson = new(@"^(i|my|je|j|m|mon)\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // Phrases that are too generic to match anywhere in a sentence
        private static readonly HashSet<string> SentenceStartOnlyPhrases = new(
            new[]
            {
                "I have a",
                "I was a",
                "J’ai un",
                "J’ai une",
                "J’ai été",
                "J’étais"
            }.Select(p => NormalizeText(p).ToLowerInvariant()),
            StringComparer.Ordinal);

        private static readonly List<PhraseEntry> PhraseEntries = BuildPhraseEntries();

        private sealed record PhraseEntry(string Phrase, bool RequiresFirstPerson, Regex? ExactRegex);

        public static int CountSeekerViolations(JobOpportunity opportunity, ILogger logger)
        {
            var violationCount = 0;

            try
            {
                var inputs = new[]
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

                    foreach (var sentence in SplitIntoSentences(normInput))
                    {
                        var sentenceHasFirstPerson =
                            FirstPersonEn.IsMatch(sentence) || FirstPersonFr.IsMatch(sentence);

                        foreach (var entry in PhraseEntries)
                        {
                            // First person phrases only count when the sentence is first person too
                            if (entry.RequiresFirstPerson && !sentenceHasFirstPerson)
                                continue;

                            if (!IsMatch(entry, sentence))
                                continue;

                            // Count each offending sentence once, no matter how many phrases hit it
                            violationCount++;
                            logger.LogWarning("Violation: \"{Sentence}\" Match: \"{Phrase}\"", sentence, entry.Phrase);
                            break;
                        }
                    }
                }

                if (violationCount > 0)
                    logger.LogWarning("Total seeker violations: {Count}", violationCount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Count seeker violations failed: {Message}", ex.Message);
            }

            return violationCount;
        }

        private static bool IsMatch(PhraseEntry entry, string sentence)
        {
            if (entry.ExactRegex != null)
                return entry.ExactRegex.IsMatch(sentence);

            return Fuzz.PartialRatio(entry.Phrase, sentence) >= FUZZY_THRESHOLD;
        }

        private static List<PhraseEntry> BuildPhraseEntries()
        {
            var entries = new List<PhraseEntry>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var raw in BuildKeyPhrases())
            {
                var phrase = NormalizeText(raw).ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(phrase) || !seen.Add(phrase))
                    continue;

                var requiresFirstPerson = PhraseStartsFirstPerson.IsMatch(phrase);
                var startOnly = SentenceStartOnlyPhrases.Contains(phrase);

                Regex? exact = null;

                if (startOnly)
                {
                    exact = new Regex(@"^[\W_]*" + Regex.Escape(phrase) + @"(?!\w)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
                }
                else if (phrase.Length < MIN_FUZZY_LENGTH)
                {
                    exact = new Regex(@"(?<!\w)" + Regex.Escape(phrase) + @"(?!\w)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
                }

                entries.Add(new PhraseEntry(phrase, requiresFirstPerson, exact));
            }

            return entries;
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
                //"Master's",
                //"Bachelor's",
                "graduated in",
                "class of",
                "I am open to",
                "My name is",
                "I'm currently",
                "I’m looking to",
                "I offer",
                "I excel",
                "I gained",
                "I have experience",
                "I bring",
                "Hiring me",
                "Job search",
                "Open to roles",
                "Open to any roles",
                "Looking for opportunities",
                "Seeking opportunities",
                "Seeking roles",
                "Broaden my horizons",
                "I have a",
                "I was a",
                "I'm an affected employee",
                "I'm an unaffected employee",
                //"Maîtrise",
                //"Baccalauréat",
                "Diplômé en",
                "Diplômée en",
                "Cohorte",
                "Je suis ouvert à",
                "Je suis ouverte à",
                "Je me nomme",
                "Mon nom est",
                "Je suis actuellement",
                "Je cherche à",
                "J’offre",
                "J’excelle",
                "J’ai acquis",
                "J’ai de l’expérience",
                "J’apporte",
                "J’amène",
                "M’engager",
                "M’embaucher",
                "Recherche d’emploi",
                "Ouvert à des postes",
                "Ouverte à des postes",
                "Ouvert à tout type de poste",
                "Ouverte à tout type de poste",
                "À la recherche d’occasions",
                "À la recherche d’opportunités",
                "À la recherche de postes",
                "Élargir mes horizons",
                "J’ai un",
                "J’ai une",
                "J’ai été",
                "J’étais",
                "Je suis un employé affecté",
                "Je suis une employée affectée",
                "Je suis un employé non affecté",
                "Je suis une employée non affectée",
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
                "J’ai travaillé par le passé chez",
                "À la recherche active d’offres",
                "Je suis prêt à",
                "Vous pouvez me joindre à"
            };

            var expanded = new List<string>();

            foreach (var original in phrases)
            {
                var phrase = original.Replace('\u2019', '\'').Replace('\u2018', '\'');

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

            // Protect abbreviations
            input = Regex.Replace(input, @"\b(?:mr|mrs|ms|dr|prof|sr|jr|st|mt|vs)\.|\b(?:i\.e|e\.g|etc)\.", m => m.Value.Replace(".", "**DOT**"));
            // Protect emails
            input = Regex.Replace(input, @"\b[\w\.-]+@[\w\.-]+\.\w+\b", m => m.Value.Replace(".", "**DOT**"));
            // Protect URLs
            input = Regex.Replace(input, @"https?://[^\s]+", m => m.Value.Replace(".", "**DOT**"));
            // Protect decimal numbers
            input = Regex.Replace(input, @"\b\d+\.\d+\b", m => m.Value.Replace(".", "**DOT**"));

            // Split on sentence boundaries and line breaks
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

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = WebUtility.HtmlDecode(text);

            // Turn <br> and closing block tags into new lines so paragraphs and list items become separate sentences
            text = Regex.Replace(text, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"</(p|li|div|h[1-6]|tr|ul|ol)>", "\n", RegexOptions.IgnoreCase);

            text = Regex.Replace(text, "<.*?>", " ");

            text = text.Replace('\u00A0', ' ')
                 .Replace('\u2019', '\'')
                 .Replace('\u2018', '\'')
                 .Replace('\u201C', '"')
                 .Replace('\u201D', '"')
                 .Replace('\u2013', '-')
                 .Replace('\u2014', '-');

            text = RemoveAccents(text);

            // Collapse spaces/tabs but keep new lines
            text = Regex.Replace(text, @"[^\S\n]+", " ");
            text = Regex.Replace(text, @" ?\n[\s]*", "\n");

            return text.Normalize(NormalizationForm.FormC).Trim();
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
    }
}