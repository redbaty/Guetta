using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Guetta.Localisation
{
    public static class AppExtensions
    {
        public static void AddGuettaLocalisation(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<LocalisationService>();
            serviceCollection.AddDbContext<LocalisationContext>(e => { e.UseInMemoryDatabase("default"); });
            serviceCollection.AddLocalization();
        }
    }
}