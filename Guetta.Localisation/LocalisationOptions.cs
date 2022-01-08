using System;
using System.Globalization;

namespace Guetta.Localisation
{
    public class LocalisationOptions
    {
        public string Language { get; set; } = CultureInfo.CurrentCulture.Name;
    }
}