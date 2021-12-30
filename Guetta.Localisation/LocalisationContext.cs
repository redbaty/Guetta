using Microsoft.EntityFrameworkCore;

namespace Guetta.Localisation
{
    public class LocalisationContext : DbContext
    {
        public LocalisationContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Language> Languages { get; set; }

        public DbSet<LanguageItem> LanguageItems { get; set; }

        public DbSet<LanguageItemEntry> LanguageItemEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LanguageItem>(e =>
            {
                e.HasIndex(i => i.Code).IsUnique();

                e.HasData(new LanguageItem
                    {
                        Code = "InvalidArgument",
                        Id = 1
                    },
                    new LanguageItem
                    {
                        Code = "SongQueued",
                        Id = 2
                    },
                    new LanguageItem
                    {
                        Code = "SongPlaying",
                        Id = 3
                    },
                    new LanguageItem
                    {
                        Code = "SongSkipped",
                        Id = 4
                    },
                    new LanguageItem
                    {
                        Code = "NotInChannel",
                        Id = 5
                    },
                    new LanguageItem
                    {
                        Code = "NoSongsInQueue",
                        Id = 6
                    },
                    new LanguageItem
                    {
                        Code = "CantSkip",
                        Id = 7
                    },
                    new LanguageItem
                    {
                        Code = "SongDownloading",
                        Id = 8
                    },
                    new LanguageItem
                    {
                        Code = "PlaylistQueued",
                        Id = 9
                    }
                );
            });

            modelBuilder.Entity<Language>(e =>
            {
                e.HasIndex(i => i.ShortName).IsUnique();
                e.HasData(new Language
                    {
                        Id = 1,
                        ShortName = "en",
                        LongName = "English"
                    },
                    new Language
                    {
                        Id = 2,
                        ShortName = "ptBR",
                        LongName = "Português Brasileiro"
                    });
            });

            modelBuilder.Entity<LanguageItemEntry>(e =>
            {
                e.HasKey(i => new {i.LanguageId, i.ItemId});
                e.HasData(new LanguageItemEntry
                    {
                        LanguageId = 1,
                        ItemId = 1,
                        Value = "Invalid arguments"
                    },
                    new LanguageItemEntry
                    {
                        LanguageId = 1,
                        ItemId = 2,
                        Value = "The song has been queued"
                    },
                    new LanguageItemEntry
                    {
                        LanguageId = 1,
                        ItemId = 3,
                        Value = "Now playing {0}, queued by {1}"
                    },
                    new LanguageItemEntry
                    {
                        LanguageId = 1,
                        ItemId = 4,
                        Value = "Skipping song {0}"
                    },
                    new LanguageItemEntry
                    {
                        LanguageId = 1,
                        ItemId = 5,
                        Value = "{0} you're not in a voice channel"
                    },
                    new LanguageItemEntry
                    {
                        LanguageId = 1,
                        ItemId = 6,
                        Value = "No songs in queue"
                    },
                    new LanguageItemEntry
                    {
                        LanguageId = 1,
                        ItemId = 7,
                        Value = "Can't skip songs"
                    },
                    new LanguageItemEntry
                    {
                        LanguageId = 1,
                        ItemId = 8,
                        Value = "Downloading song..."
                    },
                    new LanguageItemEntry
                    {
                        LanguageId = 1,
                        ItemId = 9,
                        Value = "The playlist {1} has been enqueued by {1}"
                    }
                );
            });
        }
    }
}