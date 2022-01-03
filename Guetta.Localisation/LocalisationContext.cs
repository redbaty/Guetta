using System.Collections.Generic;
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
                e.HasKey(i => i.Code);

                e.HasData(new LanguageItem
                    {
                        Code = "InvalidArgument"
                    },
                    new LanguageItem
                    {
                        Code = "SongQueued"
                    },
                    new LanguageItem
                    {
                        Code = "SongPlaying"
                    },
                    new LanguageItem
                    {
                        Code = "SongSkipped"
                    },
                    new LanguageItem
                    {
                        Code = "NotInChannel"
                    },
                    new LanguageItem
                    {
                        Code = "NoSongsInQueue"
                    },
                    new LanguageItem
                    {
                        Code = "CantSkip"
                    },
                    new LanguageItem
                    {
                        Code = "SongDownloading"
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
                        LongName = "English",
                        LanguageItems = new List<LanguageItemEntry>
                        {
                            new()
                            {
                                ItemId = "InvalidArgument",
                                Value = "Invalid arguments"
                            },
                            new()
                            {
                                ItemId = "SongQueued",
                                Value = "The song has been queued"
                            },
                            new()
                            {
                                ItemId = "SongPlaying",
                                Value = "Now playing {0}, queued by {1}"
                            },
                            new()
                            {
                                ItemId = "SongSkipped",
                                Value = "Skipping song {0}"
                            },
                            new()
                            {
                                ItemId = "NotInChannel",
                                Value = "{0} you're not in a voice channel"
                            },
                            new()
                            {
                                ItemId = "NoSongsInQueue",
                                Value = "No songs in queue"
                            },
                            new()
                            {
                                ItemId = "CantSkip",
                                Value = "Can't skip songs"
                            },
                            new()
                            {
                                ItemId = "SongDownloading",
                                Value = "Downloading song..."
                            }
                        }
                    },
                    new Language
                    {
                        Id = 2,
                        ShortName = "ptBR",
                        LongName = "Português Brasileiro"
                    });

                e.HasMany(i => i.LanguageItems)
                    .WithOne(i => i.Language);
            });

            modelBuilder.Entity<LanguageItemEntry>(e => { e.HasKey(i => new { i.LanguageId, i.ItemId }); });
        }
    }
}