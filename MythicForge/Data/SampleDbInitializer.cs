using System;
using System.Collections.Generic;
using System.Data.Entity;
using MythicForge.Models;
using MythicForge.Services;

namespace MythicForge.Data
{
    /// <summary>
    /// Drops, recreates and reseeds the database every time the application
    /// starts, giving a clean, predictable catalog (and a demo login) on each run.
    /// </summary>
    public class SampleDbInitializer : DropCreateDatabaseAlways<SampleDbContext>
    {
        protected override void Seed(SampleDbContext context)
        {
            SeedDemoUser(context);
            SeedColors(context);
            SeedCreatures(context);

            context.SaveChanges();
            base.Seed(context);
        }

        private static void SeedDemoUser(SampleDbContext context)
        {
            context.Users.Add(new User
            {
                Email = "demo@example.com",
                DisplayName = "Demo Customer",
                PasswordHash = PasswordHasher.Hash("Password123!"),
                CreatedOn = DateTime.UtcNow
            });
        }

        private static void SeedColors(SampleDbContext context)
        {
            var colors = new List<Color>
            {
                new Color { Name = "Emerald", HexValue = "#2ecc71", PriceModifier = 0m, DisplayOrder = 1 },
                new Color { Name = "Sapphire", HexValue = "#3498db", PriceModifier = 0m, DisplayOrder = 2 },
                new Color { Name = "Ruby", HexValue = "#e74c3c", PriceModifier = 5m, DisplayOrder = 3 },
                new Color { Name = "Amethyst", HexValue = "#9b59b6", PriceModifier = 5m, DisplayOrder = 4 },
                new Color { Name = "Gold", HexValue = "#f1c40f", PriceModifier = 15m, DisplayOrder = 5 },
                new Color { Name = "Obsidian", HexValue = "#2c3e50", PriceModifier = 10m, DisplayOrder = 6 },
                new Color { Name = "Iridescent", HexValue = "#1abc9c", PriceModifier = 25m, DisplayOrder = 7 }
            };

            context.Colors.AddRange(colors);
        }

        private static void SeedCreatures(SampleDbContext context)
        {
            var creatures = new List<CreatureType>
            {
                new CreatureType
                {
                    Name = "Dragon",
                    Emoji = "\U0001F409",
                    Description = "A mighty fire-breather. Fully customizable from snout to tail.",
                    BasePrice = 120m,
                    DisplayOrder = 1,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Wings", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "No Wings", Description = "A grounded wyrm.", PriceModifier = 0m },
                                new CreatureOption { Name = "Leathery Wings", Description = "Classic bat-like wings.", PriceModifier = 40m },
                                new CreatureOption { Name = "Feathered Wings", Description = "Majestic feathered span.", PriceModifier = 60m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Horns", SelectionType = SelectionType.Single, IsRequired = false, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "None", PriceModifier = 0m },
                                new CreatureOption { Name = "Curved Horns", PriceModifier = 20m },
                                new CreatureOption { Name = "Crown of Spikes", PriceModifier = 35m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Breath", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Fire Breath", PriceModifier = 0m },
                                new CreatureOption { Name = "Frost Breath", PriceModifier = 25m },
                                new CreatureOption { Name = "Lightning Breath", PriceModifier = 35m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 4,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Armored Scales", PriceModifier = 30m },
                                new CreatureOption { Name = "Glowing Eyes", PriceModifier = 15m },
                                new CreatureOption { Name = "Spiked Tail", PriceModifier = 20m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    Name = "Unicorn",
                    Emoji = "\U0001F984",
                    Description = "An elegant steed with a shimmering horn.",
                    BasePrice = 95m,
                    DisplayOrder = 2,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Horn", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Pearl Horn", PriceModifier = 0m },
                                new CreatureOption { Name = "Crystal Horn", PriceModifier = 30m },
                                new CreatureOption { Name = "Rainbow Horn", PriceModifier = 45m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Mane", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Silken Mane", PriceModifier = 0m },
                                new CreatureOption { Name = "Flowing Rainbow Mane", PriceModifier = 25m },
                                new CreatureOption { Name = "Starlight Mane", PriceModifier = 40m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Wings", SelectionType = SelectionType.Single, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "No Wings", PriceModifier = 0m },
                                new CreatureOption { Name = "Pegasus Wings", Description = "Become an Alicorn!", PriceModifier = 55m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 4,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Glitter Hooves", PriceModifier = 12m },
                                new CreatureOption { Name = "Sparkle Trail", PriceModifier = 18m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    Name = "Giant",
                    Emoji = "\U0001F9CC",
                    Description = "A towering colossus built for strength.",
                    BasePrice = 150m,
                    DisplayOrder = 3,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Size", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Large (12 ft)", PriceModifier = 0m },
                                new CreatureOption { Name = "Huge (20 ft)", PriceModifier = 50m },
                                new CreatureOption { Name = "Colossal (40 ft)", PriceModifier = 120m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Weapon", SelectionType = SelectionType.Single, IsRequired = false, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Bare Hands", PriceModifier = 0m },
                                new CreatureOption { Name = "Great Club", PriceModifier = 30m },
                                new CreatureOption { Name = "Boulder Sling", PriceModifier = 45m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Mossy Beard", PriceModifier = 15m },
                                new CreatureOption { Name = "Stone Skin", PriceModifier = 40m },
                                new CreatureOption { Name = "Thunderous Voice", PriceModifier = 20m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    Name = "Pixie",
                    Emoji = "\U0001F9DA",
                    Description = "A tiny winged sprite full of mischief.",
                    BasePrice = 60m,
                    DisplayOrder = 4,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Wings", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Dragonfly Wings", PriceModifier = 0m },
                                new CreatureOption { Name = "Butterfly Wings", PriceModifier = 20m },
                                new CreatureOption { Name = "Leaf Wings", PriceModifier = 15m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Magic Dust", SelectionType = SelectionType.Single, IsRequired = false, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "None", PriceModifier = 0m },
                                new CreatureOption { Name = "Sleepy Dust", PriceModifier = 18m },
                                new CreatureOption { Name = "Healing Dust", PriceModifier = 28m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Flower Crown", PriceModifier = 10m },
                                new CreatureOption { Name = "Tiny Lantern", PriceModifier = 14m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Egyptian / Greek myth: the firebird that is reborn from its own ashes.
                    Name = "Phoenix",
                    Emoji = "\U0001F426\u200D\U0001F525",
                    Description = "A radiant firebird from Egyptian and Greek legend, reborn from its own ashes.",
                    BasePrice = 130m,
                    DisplayOrder = 5,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Plumage", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Ember Feathers", PriceModifier = 0m },
                                new CreatureOption { Name = "Solar Gold", PriceModifier = 30m },
                                new CreatureOption { Name = "Molten Crimson", PriceModifier = 40m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Rebirth Cycle", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Centennial", Description = "Reborn once a century.", PriceModifier = 0m },
                                new CreatureOption { Name = "Millennial", Description = "Rises once every thousand years.", PriceModifier = 35m },
                                new CreatureOption { Name = "Eternal Flame", Description = "Never truly dies.", PriceModifier = 60m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Aura", SelectionType = SelectionType.Single, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "None", PriceModifier = 0m },
                                new CreatureOption { Name = "Warm Glow", PriceModifier = 18m },
                                new CreatureOption { Name = "Blazing Corona", PriceModifier = 32m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 4,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Healing Tears", Description = "Its tears mend any wound.", PriceModifier = 40m },
                                new CreatureOption { Name = "Ash Trail", PriceModifier = 16m },
                                new CreatureOption { Name = "Song of Dawn", PriceModifier = 22m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Norse / Scandinavian myth: the colossal sea monster that drags ships under.
                    Name = "Kraken",
                    Emoji = "\U0001F991",
                    Description = "A colossal sea monster of Norse legend, said to drag whole ships beneath the waves.",
                    BasePrice = 175m,
                    DisplayOrder = 6,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Tentacles", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Eight Arms", PriceModifier = 0m },
                                new CreatureOption { Name = "Twelve Arms", PriceModifier = 45m },
                                new CreatureOption { Name = "Twenty Arms", PriceModifier = 90m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Size", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Ship-sized", PriceModifier = 0m },
                                new CreatureOption { Name = "Island-sized", PriceModifier = 80m },
                                new CreatureOption { Name = "Leviathan", PriceModifier = 160m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Grip", SelectionType = SelectionType.Single, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Suckers", PriceModifier = 0m },
                                new CreatureOption { Name = "Barbed Suckers", PriceModifier = 28m },
                                new CreatureOption { Name = "Crushing Coils", PriceModifier = 40m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 4,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Ink Cloud", PriceModifier = 20m },
                                new CreatureOption { Name = "Bioluminescence", PriceModifier = 26m },
                                new CreatureOption { Name = "Storm Summoning", Description = "Whips the sea into a maelstrom.", PriceModifier = 55m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Greek / Persian myth: eagle-headed, lion-bodied guardian of gold.
                    Name = "Griffin",
                    Emoji = "\U0001F985",
                    Description = "A noble beast with an eagle's head and a lion's body, guardian of treasure in Greek and Persian lore.",
                    BasePrice = 140m,
                    DisplayOrder = 7,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Wings", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Eagle Wings", PriceModifier = 0m },
                                new CreatureOption { Name = "Storm Wings", PriceModifier = 35m },
                                new CreatureOption { Name = "Golden Wings", PriceModifier = 55m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Talons", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Hunting Talons", PriceModifier = 0m },
                                new CreatureOption { Name = "Iron Talons", PriceModifier = 25m },
                                new CreatureOption { Name = "Gilded Talons", PriceModifier = 45m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Hindquarters", SelectionType = SelectionType.Single, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Lion", PriceModifier = 0m },
                                new CreatureOption { Name = "Panther", PriceModifier = 20m },
                                new CreatureOption { Name = "Tufted Lion Tail", PriceModifier = 30m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 4,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Guardian Instinct", Description = "Fiercely protects its hoard.", PriceModifier = 30m },
                                new CreatureOption { Name = "Regal Crest", PriceModifier = 18m },
                                new CreatureOption { Name = "Piercing Cry", PriceModifier = 16m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Japanese myth: the intelligent fox spirit that gains a tail with age and power.
                    Name = "Kitsune",
                    Emoji = "\U0001F98A",
                    Description = "A cunning fox spirit from Japanese folklore that grows wiser and more powerful with every tail.",
                    BasePrice = 110m,
                    DisplayOrder = 8,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Tails", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "One Tail", PriceModifier = 0m },
                                new CreatureOption { Name = "Five Tails", PriceModifier = 40m },
                                new CreatureOption { Name = "Nine Tails", Description = "A celestial kitsune of the highest rank.", PriceModifier = 85m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Element", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Foxfire", PriceModifier = 0m },
                                new CreatureOption { Name = "Frost", PriceModifier = 25m },
                                new CreatureOption { Name = "Void", PriceModifier = 45m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Form", SelectionType = SelectionType.Single, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Fox", PriceModifier = 0m },
                                new CreatureOption { Name = "Shapeshifter", Description = "An upright anthropomorphic fox-human hybrid with fox ears, muzzle and tails", PriceModifier = 35m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 4,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Illusion Weaving", PriceModifier = 28m },
                                new CreatureOption { Name = "Star Ball", Description = "A hoshi no tama holding part of its soul.", PriceModifier = 34m },
                                new CreatureOption { Name = "Shrine Blessing", PriceModifier = 20m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Arabian myth: a spirit of smokeless fire, bound to grant wishes.
                    Name = "Djinn",
                    Emoji = "\U0001F9DE",
                    Description = "A powerful spirit of smokeless fire from Arabian mythology, bound to grant the wishes of whoever frees it.",
                    BasePrice = 160m,
                    DisplayOrder = 9,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Element", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Smokeless Fire", PriceModifier = 0m },
                                new CreatureOption { Name = "Desert Wind", PriceModifier = 30m },
                                new CreatureOption { Name = "Shadow", PriceModifier = 50m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Wishes Granted", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "One Wish", PriceModifier = 0m },
                                new CreatureOption { Name = "Three Wishes", PriceModifier = 60m },
                                new CreatureOption { Name = "Unlimited", Description = "Bound only by an ancient contract.", PriceModifier = 140m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Vessel", SelectionType = SelectionType.Single, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Brass Lamp", PriceModifier = 0m },
                                new CreatureOption { Name = "Ancient Ring", PriceModifier = 22m },
                                new CreatureOption { Name = "Sealed Bottle", PriceModifier = 18m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 4,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Shape-shifting", PriceModifier = 35m },
                                new CreatureOption { Name = "Invisibility", PriceModifier = 30m },
                                new CreatureOption { Name = "Flight", PriceModifier = 20m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Hindu / Buddhist myth: a divine, half-serpent being that guards treasure and water.
                    Name = "Naga",
                    Emoji = "\U0001F40D",
                    Description = "A divine serpent-being of Hindu and Buddhist mythology, guardian of hidden treasure and sacred waters.",
                    BasePrice = 125m,
                    DisplayOrder = 10,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Hood", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Single Hood", PriceModifier = 0m },
                                new CreatureOption { Name = "Five Hoods", PriceModifier = 40m },
                                new CreatureOption { Name = "Seven Hoods", Description = "A royal Nagaraja.", PriceModifier = 75m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Scales", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Jade", PriceModifier = 0m },
                                new CreatureOption { Name = "Golden", PriceModifier = 30m },
                                new CreatureOption { Name = "Jeweled", PriceModifier = 55m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Venom", SelectionType = SelectionType.Single, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "None", PriceModifier = 0m },
                                new CreatureOption { Name = "Paralytic", PriceModifier = 26m },
                                new CreatureOption { Name = "Divine Nectar", Description = "Said to grant immortality.", PriceModifier = 48m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 4,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Guardian of Treasure", PriceModifier = 30m },
                                new CreatureOption { Name = "Water Command", PriceModifier = 24m },
                                new CreatureOption { Name = "Gemstone Crown", PriceModifier = 35m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Jewish folklore: an animated guardian of clay or stone, brought to life by a sacred word.
                    Name = "Golem",
                    Emoji = "\U0001F5FF",
                    Description = "A tireless guardian of clay or stone from Jewish folklore, animated by a sacred word.",
                    BasePrice = 145m,
                    DisplayOrder = 11,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Material", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Clay", PriceModifier = 0m },
                                new CreatureOption { Name = "Stone", PriceModifier = 40m },
                                new CreatureOption { Name = "Iron", PriceModifier = 75m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Animating Word", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Emet", Description = "'Truth' inscribed on its brow.", PriceModifier = 0m },
                                new CreatureOption { Name = "Sacred Scroll", PriceModifier = 25m },
                                new CreatureOption { Name = "Star Seal", PriceModifier = 40m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Stature", SelectionType = SelectionType.Single, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Man-sized", PriceModifier = 0m },
                                new CreatureOption { Name = "Towering", PriceModifier = 45m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 4,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Tireless Guardian", PriceModifier = 28m },
                                new CreatureOption { Name = "Unbreakable", PriceModifier = 50m },
                                new CreatureOption { Name = "Silent Sentinel", PriceModifier = 15m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Scottish / Celtic myth: a shape-shifting water horse that lurks in lochs and rivers.
                    Name = "Kelpie",
                    Emoji = "\U0001F40E",
                    Description = "A shape-shifting water horse from Scottish and Celtic legend, said to haunt lochs and rivers.",
                    BasePrice = 105m,
                    DisplayOrder = 12,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Form", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Water Horse", PriceModifier = 0m },
                                new CreatureOption { Name = "Wild Stallion", PriceModifier = 25m },
                                new CreatureOption { Name = "Human Guise", Description = "A pale humanlike figure with damp hair and river weeds woven into it", PriceModifier = 40m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Mane", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Kelp Mane", PriceModifier = 0m },
                                new CreatureOption { Name = "River-weed Mane", PriceModifier = 18m },
                                new CreatureOption { Name = "Foam Mane", PriceModifier = 28m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Hooves", SelectionType = SelectionType.Single, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Reversed Hooves", PriceModifier = 0m },
                                new CreatureOption { Name = "Silver Shoes", PriceModifier = 22m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 4,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Adhesive Skin", Description = "Riders cannot pull themselves free.", PriceModifier = 30m },
                                new CreatureOption { Name = "Storm Call", PriceModifier = 24m },
                                new CreatureOption { Name = "Pearl Eyes", PriceModifier = 16m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Greek myth: an alluring half-human, half-fish dweller of the deep.
                    Name = "Mermaid",
                    Emoji = "\U0001F9DC",
                    Description = "An enchanting half-human, half-fish being of Greek legend, at home in the ocean depths.",
                    BasePrice = 100m,
                    DisplayOrder = 13,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Tail", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Silver Scales", PriceModifier = 0m },
                                new CreatureOption { Name = "Iridescent Fins", PriceModifier = 28m },
                                new CreatureOption { Name = "Pearl-studded Tail", PriceModifier = 48m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Song", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Gentle Lullaby", PriceModifier = 0m },
                                new CreatureOption { Name = "Enchanting Siren Call", Description = "Lures sailors from afar.", PriceModifier = 35m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Coral Crown", PriceModifier = 18m },
                                new CreatureOption { Name = "Pet Seahorse", PriceModifier = 14m },
                                new CreatureOption { Name = "Bioluminescent Glow", PriceModifier = 24m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // European myth: a human cursed to transform into a monstrous wolf.
                    Name = "Werewolf",
                    Emoji = "\U0001F43A",
                    Description = "A cursed shapeshifter of European folklore that turns into a monstrous wolf beneath the full moon.",
                    BasePrice = 115m,
                    DisplayOrder = 14,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Form", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Wolfman", PriceModifier = 0m },
                                new CreatureOption { Name = "Dire Wolf", PriceModifier = 30m },
                                new CreatureOption { Name = "Full Moon Frenzy", PriceModifier = 55m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Fur", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Grey", PriceModifier = 0m },
                                new CreatureOption { Name = "Midnight Black", PriceModifier = 20m },
                                new CreatureOption { Name = "Silver-touched", PriceModifier = 35m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Glowing Eyes", PriceModifier = 16m },
                                new CreatureOption { Name = "Iron Claws", PriceModifier = 26m },
                                new CreatureOption { Name = "Howl of the Pack", PriceModifier = 20m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Greek myth: a one-eyed giant and master smith.
                    Name = "Cyclops",
                    Emoji = "\U0001F441",
                    Description = "A towering one-eyed giant of Greek myth, renowned as a master smith.",
                    BasePrice = 135m,
                    DisplayOrder = 15,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Size", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Towering", PriceModifier = 0m },
                                new CreatureOption { Name = "Mountainous", PriceModifier = 60m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Craft", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Blacksmith's Hammer", PriceModifier = 0m },
                                new CreatureOption { Name = "Boulder Throwing", PriceModifier = 30m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Thunderbolt Forge", Description = "Forged lightning for the gods.", PriceModifier = 40m },
                                new CreatureOption { Name = "Shepherd's Staff", PriceModifier = 15m },
                                new CreatureOption { Name = "Cave Hoard", PriceModifier = 20m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // European myth: the serpent king whose gaze turns victims to stone.
                    Name = "Basilisk",
                    Emoji = "\U0001F98E",
                    Description = "The venomous serpent king of European legend, whose very gaze can turn the living to stone.",
                    BasePrice = 120m,
                    DisplayOrder = 16,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Gaze", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Petrifying Stare", PriceModifier = 0m },
                                new CreatureOption { Name = "Venomous Glare", PriceModifier = 30m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Crest", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Serpent Crown", PriceModifier = 0m },
                                new CreatureOption { Name = "Cockerel Comb", PriceModifier = 22m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Venom Fangs", PriceModifier = 28m },
                                new CreatureOption { Name = "Stone Trail", PriceModifier = 18m },
                                new CreatureOption { Name = "Regal Scales", PriceModifier = 24m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Egyptian / Greek myth: a lion-bodied guardian that poses deadly riddles.
                    Name = "Sphinx",
                    Emoji = "\U0001F981",
                    Description = "A lion-bodied guardian of Egyptian and Greek myth that guards its secrets behind deadly riddles.",
                    BasePrice = 150m,
                    DisplayOrder = 17,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Wings", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "No Wings", PriceModifier = 0m },
                                new CreatureOption { Name = "Feathered Wings", PriceModifier = 45m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Riddle", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Ancient Riddle", PriceModifier = 0m },
                                new CreatureOption { Name = "Impossible Enigma", PriceModifier = 35m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Pharaoh Headdress", PriceModifier = 26m },
                                new CreatureOption { Name = "Lion Mane", PriceModifier = 18m },
                                new CreatureOption { Name = "Guardian's Gaze", PriceModifier = 22m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Greek myth: half-human, half-horse wanderers and master archers.
                    Name = "Centaur",
                    Emoji = "\U0001F434",
                    Description = "A half-human, half-horse wanderer of Greek myth, famed as a master archer.",
                    BasePrice = 110m,
                    DisplayOrder = 18,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Coat", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Chestnut", PriceModifier = 0m },
                                new CreatureOption { Name = "Palomino", PriceModifier = 20m },
                                new CreatureOption { Name = "Dappled Grey", PriceModifier = 28m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Weapon", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Longbow", PriceModifier = 0m },
                                new CreatureOption { Name = "War Spear", PriceModifier = 25m },
                                new CreatureOption { Name = "Oaken Staff", PriceModifier = 18m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Battle Paint", PriceModifier = 14m },
                                new CreatureOption { Name = "Quiver of Arrows", PriceModifier = 16m },
                                new CreatureOption { Name = "Braided Tail", PriceModifier = 10m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Himalayan / Tibetan myth: the elusive ape-like giant of the snowy peaks.
                    Name = "Yeti",
                    Emoji = "\U0001F9A7",
                    Description = "The elusive ape-like giant of Himalayan legend, roaming the highest snowbound peaks.",
                    BasePrice = 125m,
                    DisplayOrder = 19,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Coat", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Snow White", PriceModifier = 0m },
                                new CreatureOption { Name = "Frost Blue", PriceModifier = 26m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Size", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Large", PriceModifier = 0m },
                                new CreatureOption { Name = "Colossal", PriceModifier = 55m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Ice Breath", PriceModifier = 30m },
                                new CreatureOption { Name = "Thick Hide", PriceModifier = 24m },
                                new CreatureOption { Name = "Mountain Roar", PriceModifier = 18m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Greek myth: a fire-breathing hybrid of lion, goat and serpent.
                    Name = "Chimera",
                    Emoji = "\U0001F410",
                    Description = "A monstrous fire-breathing hybrid of Greek myth, part lion, part goat and part serpent.",
                    BasePrice = 165m,
                    DisplayOrder = 20,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Heads", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Lion & Goat", PriceModifier = 0m },
                                new CreatureOption { Name = "Lion, Goat & Serpent", PriceModifier = 50m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Breath", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Fire", PriceModifier = 0m },
                                new CreatureOption { Name = "Toxic Fumes", PriceModifier = 30m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Serpent Tail", PriceModifier = 28m },
                                new CreatureOption { Name = "Molten Mane", PriceModifier = 26m },
                                new CreatureOption { Name = "Venomous Bite", PriceModifier = 24m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Hebrew myth: the primordial sea monster of the deep.
                    Name = "Leviathan",
                    Emoji = "\U0001F40B",
                    Description = "The colossal primordial sea monster of Hebrew scripture, master of the ocean deep.",
                    BasePrice = 180m,
                    DisplayOrder = 21,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Size", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Titanic", PriceModifier = 0m },
                                new CreatureOption { Name = "World-Ending", PriceModifier = 120m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Scales", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Impenetrable Plates", PriceModifier = 0m },
                                new CreatureOption { Name = "Abyssal Black", PriceModifier = 40m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Whirlpool Maw", PriceModifier = 45m },
                                new CreatureOption { Name = "Glowing Depths", PriceModifier = 26m },
                                new CreatureOption { Name = "Crushing Coils", PriceModifier = 38m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Persian / Arabian myth: a bird so vast it can carry off elephants.
                    Name = "Roc",
                    Emoji = "\U0001F426",
                    Description = "A bird of Persian and Arabian legend so enormous it can carry off elephants in its talons.",
                    BasePrice = 140m,
                    DisplayOrder = 22,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Wingspan", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Vast", PriceModifier = 0m },
                                new CreatureOption { Name = "Sky-Darkening", PriceModifier = 60m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Talons", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Elephant-Grasping", PriceModifier = 0m },
                                new CreatureOption { Name = "Diamond-Hard", PriceModifier = 35m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Storm Feathers", PriceModifier = 28m },
                                new CreatureOption { Name = "Piercing Screech", PriceModifier = 18m },
                                new CreatureOption { Name = "Mountain Nest", PriceModifier = 15m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Greek myth: the many-headed serpent that grows two heads for each one severed.
                    Name = "Hydra",
                    Emoji = "\U0001F432",
                    Description = "A many-headed serpent of Greek myth that grows two new heads for every one cut away.",
                    BasePrice = 170m,
                    DisplayOrder = 23,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Heads", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Three Heads", PriceModifier = 0m },
                                new CreatureOption { Name = "Seven Heads", PriceModifier = 60m },
                                new CreatureOption { Name = "Nine Heads", PriceModifier = 110m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Regeneration", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Standard", PriceModifier = 0m },
                                new CreatureOption { Name = "Two-For-One", Description = "Sever one head, grow two.", PriceModifier = 45m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Venomous Blood", PriceModifier = 32m },
                                new CreatureOption { Name = "Acidic Breath", PriceModifier = 28m },
                                new CreatureOption { Name = "Immortal Head", PriceModifier = 50m }
                            }
                        }
                    }
                },
                new CreatureType
                {
                    // Irish / Celtic myth: a wailing spirit whose cry foretells a death.
                    Name = "Banshee",
                    Emoji = "\U0001F47B",
                    Description = "A wailing spirit of Irish and Celtic folklore whose mournful cry foretells a coming death.",
                    BasePrice = 95m,
                    DisplayOrder = 24,
                    OptionCategories = new List<OptionCategory>
                    {
                        new OptionCategory
                        {
                            Name = "Wail", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 1,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Mournful Cry", PriceModifier = 0m },
                                new CreatureOption { Name = "Death Shriek", PriceModifier = 40m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Form", SelectionType = SelectionType.Single, IsRequired = true, DisplayOrder = 2,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Veiled Woman", PriceModifier = 0m },
                                new CreatureOption { Name = "Spectral Mist", PriceModifier = 25m },
                                new CreatureOption { Name = "Ancient Crone", PriceModifier = 20m }
                            }
                        },
                        new OptionCategory
                        {
                            Name = "Extras", SelectionType = SelectionType.Multiple, IsRequired = false, DisplayOrder = 3,
                            Options = new List<CreatureOption>
                            {
                                new CreatureOption { Name = "Ghostly Glow", PriceModifier = 18m },
                                new CreatureOption { Name = "Floating Shroud", PriceModifier = 16m },
                                new CreatureOption { Name = "Omen of Fate", PriceModifier = 28m }
                            }
                        }
                    }
                }
            };

            // Category tags used by the catalog filter. Habitat (Land/Sea/Air) plus
            // physical/nature traits (Furry/Scales/Feathered/Fire/Spirit).
            var creatureTags = new Dictionary<string, string>
            {
                ["Dragon"] = "Land, Air, Scales, Fire",
                ["Unicorn"] = "Land, Furry",
                ["Giant"] = "Land",
                ["Pixie"] = "Air, Spirit",
                ["Phoenix"] = "Air, Feathered, Fire",
                ["Kraken"] = "Sea",
                ["Griffin"] = "Land, Air, Feathered, Furry",
                ["Kitsune"] = "Land, Furry, Spirit",
                ["Djinn"] = "Air, Spirit, Fire",
                ["Naga"] = "Land, Sea, Scales",
                ["Golem"] = "Land",
                ["Kelpie"] = "Sea, Land, Furry",
                ["Mermaid"] = "Sea, Spirit",
                ["Werewolf"] = "Land, Furry",
                ["Cyclops"] = "Land",
                ["Basilisk"] = "Land, Scales",
                ["Sphinx"] = "Land, Air, Furry",
                ["Centaur"] = "Land, Furry",
                ["Yeti"] = "Land, Furry",
                ["Chimera"] = "Land, Fire, Furry, Scales",
                ["Leviathan"] = "Sea, Scales",
                ["Roc"] = "Air, Feathered",
                ["Hydra"] = "Sea, Land, Scales",
                ["Banshee"] = "Air, Spirit"
            };

            foreach (var creature in creatures)
            {
                if (creatureTags.TryGetValue(creature.Name, out var tagValue))
                {
                    creature.Tags = tagValue;
                }
            }

            context.CreatureTypes.AddRange(creatures);
        }
    }
}
