using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Guetta.Localisation
{
    public static class LocalisationExtensions
    {
        public static async Task ValidateAndConfigureLocalisation(this IServiceProvider serviceProvider)
        {
            var stringLocalizer = serviceProvider.GetService<IStringLocalizer>();

            using var serviceScope = serviceProvider.CreateScope();
            var localisationContext = serviceProvider.GetService<LocalisationContext>();

            await localisationContext!.Database.EnsureCreatedAsync();
            await LoadEntries(localisationContext);
        }
        
        private static async Task LoadEntries(LocalisationContext context)
        {
            
            
            await foreach (var entry in context.LanguageItemEntries.AsNoTracking()
                .Include(i => i.Language)
                .Include(i => i.Item)
                .AsAsyncEnumerable())
            {
                if (!LocalisationService.Items.ContainsKey(entry.Language.ShortName))
                {
                    LocalisationService.Items.Add(entry.Language.ShortName, new Dictionary<string, string>());
                }

                var languageItem = LocalisationService.Items[entry.Language.ShortName];
                languageItem.Add(entry.Item.Code, entry.Value);
            }
        }
    }
}