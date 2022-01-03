namespace Guetta.Localisation
{
    public class LanguageItemEntry
    {
        public Language Language { get; set; }

        public int LanguageId { get; set; }

        public LanguageItem Item { get; set; }

        public string ItemId { get; set; }

        public string Value { get; set; }
    }
}