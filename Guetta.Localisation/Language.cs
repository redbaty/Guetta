using System.Collections.Generic;

namespace Guetta.Localisation
{
    public class Language
    {
        public int Id { get; set; }

        public string ShortName { get; set; }

        public string LongName { get; set; }
        
        public ICollection<LanguageItemEntry> LanguageItems { get; set; }
    }
}