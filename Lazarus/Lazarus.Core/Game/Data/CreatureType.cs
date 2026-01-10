using System.Collections.Generic;

namespace Lazarus.Core.Game.Data;

/// <summary>
/// All creature types in the game, organized by their category (Ordo).
/// 240+ distinct species across 12 categories.
/// </summary>
public enum CreatureType
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO MANIPULARIS - Operator / tool-user / "hands" fantasy (Primates)
    // ═══════════════════════════════════════════════════════════════════════════
    Chimpanzee,
    Bonobo,
    Gorilla,
    Orangutan,
    Gibbon,
    Baboon,
    Mandrill,
    RhesusMacaque,
    JapaneseMacaque,
    CapuchinMonkey,
    SquirrelMonkey,
    SpiderMonkey,
    HowlerMonkey,
    RingTailedLemur,
    AyeAye,
    Tarsier,
    Marmoset,
    Tamarin,
    ColobusMonkey,
    Gelada,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO PREDATORIA - Hunter / burst / pursuit (Carnivores)
    // ═══════════════════════════════════════════════════════════════════════════
    GrayWolf,
    AfricanWildDog,
    RedFox,
    Coyote,
    Lion,
    Tiger,
    Leopard,
    Jaguar,
    Cheetah,
    SnowLeopard,
    Cougar,
    Lynx,
    Serval,
    SpottedHyena,
    GrizzlyBear,
    PolarBear,
    Wolverine,
    HoneyBadger,
    RiverOtter,
    HarpSeal,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO MARSUPIALIS - Skirmisher / weird utility / "pouch tech"
    // ═══════════════════════════════════════════════════════════════════════════
    RedKangaroo,
    EasternGreyKangaroo,
    Wallaby,
    TreeKangaroo,
    Koala,
    CommonWombat,
    TasmanianDevil,
    Quoll,
    Numbat,
    Bilby,
    Bandicoot,
    Quokka,
    SugarGlider,
    BrushtailPossum,
    RingtailPossum,
    GreaterGlider,
    VirginiaOpossum,
    Cuscus,
    Bettong,
    Antechinus,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO COLOSSOMAMMALIA - Big chassis / bruiser / boss-frame
    // ═══════════════════════════════════════════════════════════════════════════
    AfricanBushElephant,
    AsianElephant,
    WhiteRhinoceros,
    BlackRhinoceros,
    IndianRhinoceros,
    SumatranRhinoceros,
    Hippopotamus,
    PygmyHippopotamus,
    Giraffe,
    CapeBuffalo,
    AmericanBison,
    Moose,
    Elk,
    Gaur,
    Yak,
    Dromedary,
    BactrianCamel,
    Zebra,
    FeralHorse,
    Tapir,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO MICROMAMMALIA - Small / fast / evasive / sabotage
    // ═══════════════════════════════════════════════════════════════════════════
    BrownRat,
    HouseMouse,
    Hamster,
    Gerbil,
    GuineaPig,
    Chinchilla,
    Squirrel,
    Chipmunk,
    FlyingSquirrel,
    Beaver,
    Capybara,
    PrairieDog,
    Marmot,
    Vole,
    Lemming,
    Rabbit,
    Hare,
    Pika,
    Shrew,
    Mole,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO ARMORMAMMALIA - Plated / spined / defensive gimmicks
    // ═══════════════════════════════════════════════════════════════════════════
    GiantPangolin,
    TemmincksPangolin,
    SundaPangolin,
    ChinesePangolin,
    IndianPangolin,
    PhilippinePangolin,
    LongTailedPangolin,
    TreePangolin,
    NineBandedArmadillo,
    GiantArmadillo,
    ThreeBandedArmadillo,
    PinkFairyArmadillo,
    NorthAmericanPorcupine,
    CrestedPorcupine,
    AfricanBrushTailedPorcupine,
    Hedgehog,
    SpinyTenrec,
    GreaterHedgehogTenrec,
    SpinyRat,
    ArmoredRat,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO EXOSKELETALIS - Traps / venom / swarm / mech-shell (Arthropods)
    // ═══════════════════════════════════════════════════════════════════════════
    PrayingMantis,
    OrchidMantis,
    AssassinBug,
    BombardierBeetle,
    StagBeetle,
    HerculesBeetle,
    RhinocerosBeetle,
    GoliathBeetle,
    PaperWasp,
    Hornet,
    BulletAnt,
    ArmyAnt,
    Tarantula,
    JumpingSpider,
    BlackWidowSpider,
    TrapdoorSpider,
    EmperorScorpion,
    GiantCentipede,
    CoconutCrab,
    MantisShrimp,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO OCTOMORPHA - Stealth / grapple / ink / "smart horror" (Octopuses)
    // ═══════════════════════════════════════════════════════════════════════════
    GiantPacificOctopus,
    CommonOctopus,
    BlueRingedOctopus,
    MimicOctopus,
    WunderpusOctopus,
    CoconutOctopus,
    DayOctopus,
    CaribbeanReefOctopus,
    AtlanticPygmyOctopus,
    DumboOctopus,
    BlanketOctopus,
    Argonaut,
    StarSuckerPygmyOctopus,
    VeinedOctopus,
    SouthernSandOctopus,
    CaliforniaTwoSpotOctopus,
    EastPacificRedOctopus,
    MaoriOctopus,
    FrilledPygmyOctopus,
    SevenArmOctopus,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO MOLLUSCA - Shell / blade / toxin / "biotech material"
    // ═══════════════════════════════════════════════════════════════════════════
    GiantSquid,
    ColossalSquid,
    HumboldtSquid,
    VampireSquid,
    CommonCuttlefish,
    FlamboyantCuttlefish,
    ChamberedNautilus,
    GiantClam,
    Geoduck,
    RazorClam,
    Scallop,
    Oyster,
    Mussel,
    Abalone,
    Chiton,
    ConeSnail,
    GiantAfricanLandSnail,
    SeaHare,
    Nudibranch,
    BlueDragonSlug,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO MEDUSALIA - CC / AoE / "bioelectric neon" (Jellyfish)
    // ═══════════════════════════════════════════════════════════════════════════
    MoonJelly,
    LionsManeJellyfish,
    FriedEggJellyfish,
    CompassJellyfish,
    CrystalJelly,
    SpottedJellyfish,
    UpsideDownJellyfish,
    AtlanticSeaNettle,
    PacificSeaNettle,
    BlackSeaNettle,
    MauveStinger,
    BlueJellyfish,
    CauliflowerJellyfish,
    HelmetJellyfish,
    ImmortalJellyfish,
    NomurasJellyfish,
    BoxJellyfish,
    IrukandjJellyfish,
    PortugueseManOWar,
    ByTheWindSailor,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO OBSCURA - Fun category: Weird-tech specialist (Platypus only)
    // ═══════════════════════════════════════════════════════════════════════════
    Platypus,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO TARDIGRADA - Fun category: Zen tank specialist (Sloth only)
    // ═══════════════════════════════════════════════════════════════════════════
    Sloth
}

/// <summary>
/// Static registry mapping creature types to their categories and providing metadata.
/// </summary>
public static class CreatureTypes
{
    private static readonly Dictionary<CreatureType, CreatureTypeInfo> _typeInfo = new();
    private static bool _initialized = false;

    /// <summary>
    /// Information about a creature type.
    /// </summary>
    public class CreatureTypeInfo
    {
        public CreatureType Type { get; init; }
        public CreatureCategory Category { get; init; }
        public string DisplayName { get; init; } = "";
        public string Description { get; init; } = "";
    }

    /// <summary>
    /// Gets the category for a creature type.
    /// </summary>
    public static CreatureCategory GetCategory(CreatureType type)
    {
        EnsureInitialized();
        return _typeInfo.TryGetValue(type, out var info) ? info.Category : CreatureCategory.Predatoria;
    }

    /// <summary>
    /// Gets the display name for a creature type.
    /// </summary>
    public static string GetDisplayName(CreatureType type)
    {
        EnsureInitialized();
        return _typeInfo.TryGetValue(type, out var info) ? info.DisplayName : type.ToString();
    }

    /// <summary>
    /// Gets all creature types in a category.
    /// </summary>
    public static IEnumerable<CreatureType> GetTypesInCategory(CreatureCategory category)
    {
        EnsureInitialized();
        foreach (var kvp in _typeInfo)
        {
            if (kvp.Value.Category == category)
                yield return kvp.Key;
        }
    }

    /// <summary>
    /// Gets the info for a creature type.
    /// </summary>
    public static CreatureTypeInfo? GetInfo(CreatureType type)
    {
        EnsureInitialized();
        return _typeInfo.TryGetValue(type, out var info) ? info : null;
    }

    private static void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;
        Initialize();
    }

    private static void Register(CreatureType type, CreatureCategory category, string displayName, string description = "")
    {
        _typeInfo[type] = new CreatureTypeInfo
        {
            Type = type,
            Category = category,
            DisplayName = displayName,
            Description = description
        };
    }

    private static void Initialize()
    {
        // ═══════════════════════════════════════════════════════════════════════
        // ORDO MANIPULARIS - Primates
        // ═══════════════════════════════════════════════════════════════════════
        Register(CreatureType.Chimpanzee, CreatureCategory.Manipularis, "Chimpanzee", "Highly intelligent tool user");
        Register(CreatureType.Bonobo, CreatureCategory.Manipularis, "Bonobo", "Peaceful, social primate");
        Register(CreatureType.Gorilla, CreatureCategory.Manipularis, "Gorilla", "Powerful silverback");
        Register(CreatureType.Orangutan, CreatureCategory.Manipularis, "Orangutan", "Solitary forest dweller");
        Register(CreatureType.Gibbon, CreatureCategory.Manipularis, "Gibbon", "Agile brachiator");
        Register(CreatureType.Baboon, CreatureCategory.Manipularis, "Baboon", "Aggressive troop member");
        Register(CreatureType.Mandrill, CreatureCategory.Manipularis, "Mandrill", "Colorful forest dweller");
        Register(CreatureType.RhesusMacaque, CreatureCategory.Manipularis, "Rhesus Macaque", "Adaptable urban survivor");
        Register(CreatureType.JapaneseMacaque, CreatureCategory.Manipularis, "Japanese Macaque", "Snow monkey");
        Register(CreatureType.CapuchinMonkey, CreatureCategory.Manipularis, "Capuchin Monkey", "Clever tool user");
        Register(CreatureType.SquirrelMonkey, CreatureCategory.Manipularis, "Squirrel Monkey", "Small and quick");
        Register(CreatureType.SpiderMonkey, CreatureCategory.Manipularis, "Spider Monkey", "Long-limbed acrobat");
        Register(CreatureType.HowlerMonkey, CreatureCategory.Manipularis, "Howler Monkey", "Loudest land animal");
        Register(CreatureType.RingTailedLemur, CreatureCategory.Manipularis, "Ring-tailed Lemur", "Iconic Madagascar native");
        Register(CreatureType.AyeAye, CreatureCategory.Manipularis, "Aye-aye", "Nocturnal percussive forager");
        Register(CreatureType.Tarsier, CreatureCategory.Manipularis, "Tarsier", "Huge-eyed nocturnal hunter");
        Register(CreatureType.Marmoset, CreatureCategory.Manipularis, "Marmoset", "Tiny and quick");
        Register(CreatureType.Tamarin, CreatureCategory.Manipularis, "Tamarin", "Colorful small primate");
        Register(CreatureType.ColobusMonkey, CreatureCategory.Manipularis, "Colobus Monkey", "Leaf-eating acrobat");
        Register(CreatureType.Gelada, CreatureCategory.Manipularis, "Gelada", "Grass-grazing primate");

        // ═══════════════════════════════════════════════════════════════════════
        // ORDO PREDATORIA - Carnivores
        // ═══════════════════════════════════════════════════════════════════════
        Register(CreatureType.GrayWolf, CreatureCategory.Predatoria, "Gray Wolf", "Pack hunter");
        Register(CreatureType.AfricanWildDog, CreatureCategory.Predatoria, "African Wild Dog", "Endurance pack hunter");
        Register(CreatureType.RedFox, CreatureCategory.Predatoria, "Red Fox", "Adaptable solo hunter");
        Register(CreatureType.Coyote, CreatureCategory.Predatoria, "Coyote", "Opportunistic survivor");
        Register(CreatureType.Lion, CreatureCategory.Predatoria, "Lion", "Pride leader");
        Register(CreatureType.Tiger, CreatureCategory.Predatoria, "Tiger", "Solitary ambush predator");
        Register(CreatureType.Leopard, CreatureCategory.Predatoria, "Leopard", "Stealthy tree climber");
        Register(CreatureType.Jaguar, CreatureCategory.Predatoria, "Jaguar", "Powerful bite force");
        Register(CreatureType.Cheetah, CreatureCategory.Predatoria, "Cheetah", "Fastest land animal");
        Register(CreatureType.SnowLeopard, CreatureCategory.Predatoria, "Snow Leopard", "Mountain ghost");
        Register(CreatureType.Cougar, CreatureCategory.Predatoria, "Cougar", "Mountain lion");
        Register(CreatureType.Lynx, CreatureCategory.Predatoria, "Lynx", "Forest stalker");
        Register(CreatureType.Serval, CreatureCategory.Predatoria, "Serval", "High-jumping hunter");
        Register(CreatureType.SpottedHyena, CreatureCategory.Predatoria, "Spotted Hyena", "Bone-crushing pack hunter");
        Register(CreatureType.GrizzlyBear, CreatureCategory.Predatoria, "Grizzly Bear", "Powerful omnivore");
        Register(CreatureType.PolarBear, CreatureCategory.Predatoria, "Polar Bear", "Arctic apex predator");
        Register(CreatureType.Wolverine, CreatureCategory.Predatoria, "Wolverine", "Fearless and ferocious");
        Register(CreatureType.HoneyBadger, CreatureCategory.Predatoria, "Honey Badger", "Fearless aggressor");
        Register(CreatureType.RiverOtter, CreatureCategory.Predatoria, "River Otter", "Aquatic hunter");
        Register(CreatureType.HarpSeal, CreatureCategory.Predatoria, "Harp Seal", "Arctic swimmer");

        // ═══════════════════════════════════════════════════════════════════════
        // ORDO MARSUPIALIS - Marsupials
        // ═══════════════════════════════════════════════════════════════════════
        Register(CreatureType.RedKangaroo, CreatureCategory.Marsupialis, "Red Kangaroo", "Powerful hopper");
        Register(CreatureType.EasternGreyKangaroo, CreatureCategory.Marsupialis, "Eastern Grey Kangaroo", "Common kangaroo");
        Register(CreatureType.Wallaby, CreatureCategory.Marsupialis, "Wallaby", "Smaller kangaroo cousin");
        Register(CreatureType.TreeKangaroo, CreatureCategory.Marsupialis, "Tree Kangaroo", "Arboreal hopper");
        Register(CreatureType.Koala, CreatureCategory.Marsupialis, "Koala", "Eucalyptus specialist");
        Register(CreatureType.CommonWombat, CreatureCategory.Marsupialis, "Common Wombat", "Burrowing tank");
        Register(CreatureType.TasmanianDevil, CreatureCategory.Marsupialis, "Tasmanian Devil", "Fierce scavenger");
        Register(CreatureType.Quoll, CreatureCategory.Marsupialis, "Quoll", "Spotted marsupial predator");
        Register(CreatureType.Numbat, CreatureCategory.Marsupialis, "Numbat", "Termite specialist");
        Register(CreatureType.Bilby, CreatureCategory.Marsupialis, "Bilby", "Desert burrower");
        Register(CreatureType.Bandicoot, CreatureCategory.Marsupialis, "Bandicoot", "Quick digger");
        Register(CreatureType.Quokka, CreatureCategory.Marsupialis, "Quokka", "Friendly hopper");
        Register(CreatureType.SugarGlider, CreatureCategory.Marsupialis, "Sugar Glider", "Gliding marsupial");
        Register(CreatureType.BrushtailPossum, CreatureCategory.Marsupialis, "Brushtail Possum", "Nocturnal climber");
        Register(CreatureType.RingtailPossum, CreatureCategory.Marsupialis, "Ringtail Possum", "Prehensile-tailed climber");
        Register(CreatureType.GreaterGlider, CreatureCategory.Marsupialis, "Greater Glider", "Large gliding marsupial");
        Register(CreatureType.VirginiaOpossum, CreatureCategory.Marsupialis, "Virginia Opossum", "Playing dead specialist");
        Register(CreatureType.Cuscus, CreatureCategory.Marsupialis, "Cuscus", "Slow-moving tree dweller");
        Register(CreatureType.Bettong, CreatureCategory.Marsupialis, "Bettong", "Rat-kangaroo");
        Register(CreatureType.Antechinus, CreatureCategory.Marsupialis, "Antechinus", "Mouse-like marsupial");

        // ═══════════════════════════════════════════════════════════════════════
        // ORDO COLOSSOMAMMALIA - Large mammals
        // ═══════════════════════════════════════════════════════════════════════
        Register(CreatureType.AfricanBushElephant, CreatureCategory.Colossomammalia, "African Bush Elephant", "Largest land animal");
        Register(CreatureType.AsianElephant, CreatureCategory.Colossomammalia, "Asian Elephant", "Intelligent giant");
        Register(CreatureType.WhiteRhinoceros, CreatureCategory.Colossomammalia, "White Rhinoceros", "Armored tank");
        Register(CreatureType.BlackRhinoceros, CreatureCategory.Colossomammalia, "Black Rhinoceros", "Aggressive charger");
        Register(CreatureType.IndianRhinoceros, CreatureCategory.Colossomammalia, "Indian Rhinoceros", "One-horned tank");
        Register(CreatureType.SumatranRhinoceros, CreatureCategory.Colossomammalia, "Sumatran Rhinoceros", "Hairy rhino");
        Register(CreatureType.Hippopotamus, CreatureCategory.Colossomammalia, "Hippopotamus", "River horse");
        Register(CreatureType.PygmyHippopotamus, CreatureCategory.Colossomammalia, "Pygmy Hippopotamus", "Forest hippo");
        Register(CreatureType.Giraffe, CreatureCategory.Colossomammalia, "Giraffe", "Tallest animal");
        Register(CreatureType.CapeBuffalo, CreatureCategory.Colossomammalia, "Cape Buffalo", "Dangerous bovine");
        Register(CreatureType.AmericanBison, CreatureCategory.Colossomammalia, "American Bison", "Plains giant");
        Register(CreatureType.Moose, CreatureCategory.Colossomammalia, "Moose", "Largest deer");
        Register(CreatureType.Elk, CreatureCategory.Colossomammalia, "Elk", "Wapiti");
        Register(CreatureType.Gaur, CreatureCategory.Colossomammalia, "Gaur", "Largest wild cattle");
        Register(CreatureType.Yak, CreatureCategory.Colossomammalia, "Yak", "Mountain ox");
        Register(CreatureType.Dromedary, CreatureCategory.Colossomammalia, "Dromedary", "One-humped camel");
        Register(CreatureType.BactrianCamel, CreatureCategory.Colossomammalia, "Bactrian Camel", "Two-humped camel");
        Register(CreatureType.Zebra, CreatureCategory.Colossomammalia, "Zebra", "Striped horse");
        Register(CreatureType.FeralHorse, CreatureCategory.Colossomammalia, "Feral Horse", "Wild warhorse");
        Register(CreatureType.Tapir, CreatureCategory.Colossomammalia, "Tapir", "Living fossil");

        // ═══════════════════════════════════════════════════════════════════════
        // ORDO MICROMAMMALIA - Small mammals
        // ═══════════════════════════════════════════════════════════════════════
        Register(CreatureType.BrownRat, CreatureCategory.Micromammalia, "Brown Rat", "Urban survivor");
        Register(CreatureType.HouseMouse, CreatureCategory.Micromammalia, "House Mouse", "Tiny infiltrator");
        Register(CreatureType.Hamster, CreatureCategory.Micromammalia, "Hamster", "Pouch-cheeked hoarder");
        Register(CreatureType.Gerbil, CreatureCategory.Micromammalia, "Gerbil", "Desert runner");
        Register(CreatureType.GuineaPig, CreatureCategory.Micromammalia, "Guinea Pig", "Social cavy");
        Register(CreatureType.Chinchilla, CreatureCategory.Micromammalia, "Chinchilla", "Soft-furred jumper");
        Register(CreatureType.Squirrel, CreatureCategory.Micromammalia, "Squirrel", "Agile tree runner");
        Register(CreatureType.Chipmunk, CreatureCategory.Micromammalia, "Chipmunk", "Striped hoarder");
        Register(CreatureType.FlyingSquirrel, CreatureCategory.Micromammalia, "Flying Squirrel", "Gliding rodent");
        Register(CreatureType.Beaver, CreatureCategory.Micromammalia, "Beaver", "Dam builder");
        Register(CreatureType.Capybara, CreatureCategory.Micromammalia, "Capybara", "Largest rodent");
        Register(CreatureType.PrairieDog, CreatureCategory.Micromammalia, "Prairie Dog", "Town builder");
        Register(CreatureType.Marmot, CreatureCategory.Micromammalia, "Marmot", "Mountain whistler");
        Register(CreatureType.Vole, CreatureCategory.Micromammalia, "Vole", "Grass tunnel runner");
        Register(CreatureType.Lemming, CreatureCategory.Micromammalia, "Lemming", "Arctic migrator");
        Register(CreatureType.Rabbit, CreatureCategory.Micromammalia, "Rabbit", "Burrowing hopper");
        Register(CreatureType.Hare, CreatureCategory.Micromammalia, "Hare", "Fast open-ground runner");
        Register(CreatureType.Pika, CreatureCategory.Micromammalia, "Pika", "Rock rabbit");
        Register(CreatureType.Shrew, CreatureCategory.Micromammalia, "Shrew", "Tiny predator");
        Register(CreatureType.Mole, CreatureCategory.Micromammalia, "Mole", "Subterranean digger");

        // ═══════════════════════════════════════════════════════════════════════
        // ORDO ARMORMAMMALIA - Armored mammals
        // ═══════════════════════════════════════════════════════════════════════
        Register(CreatureType.GiantPangolin, CreatureCategory.Armormammalia, "Giant Pangolin", "Largest pangolin");
        Register(CreatureType.TemmincksPangolin, CreatureCategory.Armormammalia, "Temminck's Pangolin", "Ground pangolin");
        Register(CreatureType.SundaPangolin, CreatureCategory.Armormammalia, "Sunda Pangolin", "Malayan pangolin");
        Register(CreatureType.ChinesePangolin, CreatureCategory.Armormammalia, "Chinese Pangolin", "Burrowing pangolin");
        Register(CreatureType.IndianPangolin, CreatureCategory.Armormammalia, "Indian Pangolin", "Thick-tailed pangolin");
        Register(CreatureType.PhilippinePangolin, CreatureCategory.Armormammalia, "Philippine Pangolin", "Island pangolin");
        Register(CreatureType.LongTailedPangolin, CreatureCategory.Armormammalia, "Long-tailed Pangolin", "Tree pangolin");
        Register(CreatureType.TreePangolin, CreatureCategory.Armormammalia, "Tree Pangolin", "African tree climber");
        Register(CreatureType.NineBandedArmadillo, CreatureCategory.Armormammalia, "Nine-banded Armadillo", "Common armadillo");
        Register(CreatureType.GiantArmadillo, CreatureCategory.Armormammalia, "Giant Armadillo", "Largest armadillo");
        Register(CreatureType.ThreeBandedArmadillo, CreatureCategory.Armormammalia, "Three-banded Armadillo", "Rolling armadillo");
        Register(CreatureType.PinkFairyArmadillo, CreatureCategory.Armormammalia, "Pink Fairy Armadillo", "Tiny burrower");
        Register(CreatureType.NorthAmericanPorcupine, CreatureCategory.Armormammalia, "N.A. Porcupine", "Quill-covered rodent");
        Register(CreatureType.CrestedPorcupine, CreatureCategory.Armormammalia, "Crested Porcupine", "African spiny");
        Register(CreatureType.AfricanBrushTailedPorcupine, CreatureCategory.Armormammalia, "Brush-tailed Porcupine", "Tree porcupine");
        Register(CreatureType.Hedgehog, CreatureCategory.Armormammalia, "Hedgehog", "Spiny ball");
        Register(CreatureType.SpinyTenrec, CreatureCategory.Armormammalia, "Spiny Tenrec", "Madagascar spiny");
        Register(CreatureType.GreaterHedgehogTenrec, CreatureCategory.Armormammalia, "Greater Hedgehog Tenrec", "Large tenrec");
        Register(CreatureType.SpinyRat, CreatureCategory.Armormammalia, "Spiny Rat", "Armored rodent");
        Register(CreatureType.ArmoredRat, CreatureCategory.Armormammalia, "Armored Rat", "Tank rodent");

        // ═══════════════════════════════════════════════════════════════════════
        // ORDO EXOSKELETALIS - Arthropods
        // ═══════════════════════════════════════════════════════════════════════
        Register(CreatureType.PrayingMantis, CreatureCategory.Exoskeletalis, "Praying Mantis", "Ambush predator");
        Register(CreatureType.OrchidMantis, CreatureCategory.Exoskeletalis, "Orchid Mantis", "Flower mimic");
        Register(CreatureType.AssassinBug, CreatureCategory.Exoskeletalis, "Assassin Bug", "Stabbing predator");
        Register(CreatureType.BombardierBeetle, CreatureCategory.Exoskeletalis, "Bombardier Beetle", "Chemical sprayer");
        Register(CreatureType.StagBeetle, CreatureCategory.Exoskeletalis, "Stag Beetle", "Antlered fighter");
        Register(CreatureType.HerculesBeetle, CreatureCategory.Exoskeletalis, "Hercules Beetle", "Strongest insect");
        Register(CreatureType.RhinocerosBeetle, CreatureCategory.Exoskeletalis, "Rhinoceros Beetle", "Horned lifter");
        Register(CreatureType.GoliathBeetle, CreatureCategory.Exoskeletalis, "Goliath Beetle", "Largest beetle");
        Register(CreatureType.PaperWasp, CreatureCategory.Exoskeletalis, "Paper Wasp", "Nest builder");
        Register(CreatureType.Hornet, CreatureCategory.Exoskeletalis, "Hornet", "Aggressive stinger");
        Register(CreatureType.BulletAnt, CreatureCategory.Exoskeletalis, "Bullet Ant", "Painful sting");
        Register(CreatureType.ArmyAnt, CreatureCategory.Exoskeletalis, "Army Ant", "Swarm hunter");
        Register(CreatureType.Tarantula, CreatureCategory.Exoskeletalis, "Tarantula", "Hairy ambusher");
        Register(CreatureType.JumpingSpider, CreatureCategory.Exoskeletalis, "Jumping Spider", "Pouncing hunter");
        Register(CreatureType.BlackWidowSpider, CreatureCategory.Exoskeletalis, "Black Widow Spider", "Venomous web builder");
        Register(CreatureType.TrapdoorSpider, CreatureCategory.Exoskeletalis, "Trapdoor Spider", "Burrow ambusher");
        Register(CreatureType.EmperorScorpion, CreatureCategory.Exoskeletalis, "Emperor Scorpion", "Armored stinger");
        Register(CreatureType.GiantCentipede, CreatureCategory.Exoskeletalis, "Giant Centipede", "Venomous predator");
        Register(CreatureType.CoconutCrab, CreatureCategory.Exoskeletalis, "Coconut Crab", "Largest land crab");
        Register(CreatureType.MantisShrimp, CreatureCategory.Exoskeletalis, "Mantis Shrimp", "Punching powerhouse");

        // ═══════════════════════════════════════════════════════════════════════
        // ORDO OCTOMORPHA - Octopuses
        // ═══════════════════════════════════════════════════════════════════════
        Register(CreatureType.GiantPacificOctopus, CreatureCategory.Octomorpha, "Giant Pacific Octopus", "Largest octopus");
        Register(CreatureType.CommonOctopus, CreatureCategory.Octomorpha, "Common Octopus", "Standard octopus");
        Register(CreatureType.BlueRingedOctopus, CreatureCategory.Octomorpha, "Blue-ringed Octopus", "Deadly venom");
        Register(CreatureType.MimicOctopus, CreatureCategory.Octomorpha, "Mimic Octopus", "Shape shifter");
        Register(CreatureType.WunderpusOctopus, CreatureCategory.Octomorpha, "Wunderpus Octopus", "Striped wonder");
        Register(CreatureType.CoconutOctopus, CreatureCategory.Octomorpha, "Coconut Octopus", "Tool user");
        Register(CreatureType.DayOctopus, CreatureCategory.Octomorpha, "Day Octopus", "Diurnal hunter");
        Register(CreatureType.CaribbeanReefOctopus, CreatureCategory.Octomorpha, "Caribbean Reef Octopus", "Reef dweller");
        Register(CreatureType.AtlanticPygmyOctopus, CreatureCategory.Octomorpha, "Atlantic Pygmy Octopus", "Tiny octopus");
        Register(CreatureType.DumboOctopus, CreatureCategory.Octomorpha, "Dumbo Octopus", "Deep-sea flapper");
        Register(CreatureType.BlanketOctopus, CreatureCategory.Octomorpha, "Blanket Octopus", "Cape swimmer");
        Register(CreatureType.Argonaut, CreatureCategory.Octomorpha, "Argonaut", "Paper nautilus");
        Register(CreatureType.StarSuckerPygmyOctopus, CreatureCategory.Octomorpha, "Star-sucker Pygmy Octopus", "Star-armed tiny");
        Register(CreatureType.VeinedOctopus, CreatureCategory.Octomorpha, "Veined Octopus", "Shell carrier");
        Register(CreatureType.SouthernSandOctopus, CreatureCategory.Octomorpha, "Southern Sand Octopus", "Sand burrower");
        Register(CreatureType.CaliforniaTwoSpotOctopus, CreatureCategory.Octomorpha, "California Two-spot Octopus", "Spotted defender");
        Register(CreatureType.EastPacificRedOctopus, CreatureCategory.Octomorpha, "E. Pacific Red Octopus", "Red hunter");
        Register(CreatureType.MaoriOctopus, CreatureCategory.Octomorpha, "Maori Octopus", "Large reef dweller");
        Register(CreatureType.FrilledPygmyOctopus, CreatureCategory.Octomorpha, "Frilled Pygmy Octopus", "Frilly small");
        Register(CreatureType.SevenArmOctopus, CreatureCategory.Octomorpha, "Seven-arm Octopus", "Hidden arm");

        // ═══════════════════════════════════════════════════════════════════════
        // ORDO MOLLUSCA - Mollusks (non-octopus)
        // ═══════════════════════════════════════════════════════════════════════
        Register(CreatureType.GiantSquid, CreatureCategory.Mollusca, "Giant Squid", "Deep-sea legend");
        Register(CreatureType.ColossalSquid, CreatureCategory.Mollusca, "Colossal Squid", "Largest invertebrate");
        Register(CreatureType.HumboldtSquid, CreatureCategory.Mollusca, "Humboldt Squid", "Aggressive pack hunter");
        Register(CreatureType.VampireSquid, CreatureCategory.Mollusca, "Vampire Squid", "Deep-sea drifter");
        Register(CreatureType.CommonCuttlefish, CreatureCategory.Mollusca, "Common Cuttlefish", "Color changer");
        Register(CreatureType.FlamboyantCuttlefish, CreatureCategory.Mollusca, "Flamboyant Cuttlefish", "Toxic walker");
        Register(CreatureType.ChamberedNautilus, CreatureCategory.Mollusca, "Chambered Nautilus", "Living fossil");
        Register(CreatureType.GiantClam, CreatureCategory.Mollusca, "Giant Clam", "Reef fortress");
        Register(CreatureType.Geoduck, CreatureCategory.Mollusca, "Geoduck", "Burrowing giant");
        Register(CreatureType.RazorClam, CreatureCategory.Mollusca, "Razor Clam", "Fast digger");
        Register(CreatureType.Scallop, CreatureCategory.Mollusca, "Scallop", "Jet swimmer");
        Register(CreatureType.Oyster, CreatureCategory.Mollusca, "Oyster", "Pearl maker");
        Register(CreatureType.Mussel, CreatureCategory.Mollusca, "Mussel", "Anchor specialist");
        Register(CreatureType.Abalone, CreatureCategory.Mollusca, "Abalone", "Shell fortress");
        Register(CreatureType.Chiton, CreatureCategory.Mollusca, "Chiton", "Armored grazer");
        Register(CreatureType.ConeSnail, CreatureCategory.Mollusca, "Cone Snail", "Venomous harpoon");
        Register(CreatureType.GiantAfricanLandSnail, CreatureCategory.Mollusca, "Giant African Land Snail", "Slow tank");
        Register(CreatureType.SeaHare, CreatureCategory.Mollusca, "Sea Hare", "Ink sprayer");
        Register(CreatureType.Nudibranch, CreatureCategory.Mollusca, "Nudibranch", "Colorful toxin");
        Register(CreatureType.BlueDragonSlug, CreatureCategory.Mollusca, "Blue Dragon", "Stolen stingers");

        // ═══════════════════════════════════════════════════════════════════════
        // ORDO MEDUSALIA - Jellyfish
        // ═══════════════════════════════════════════════════════════════════════
        Register(CreatureType.MoonJelly, CreatureCategory.Medusalia, "Moon Jelly", "Common drifter");
        Register(CreatureType.LionsManeJellyfish, CreatureCategory.Medusalia, "Lion's Mane Jellyfish", "Giant stinger");
        Register(CreatureType.FriedEggJellyfish, CreatureCategory.Medusalia, "Fried Egg Jellyfish", "Mild stinger");
        Register(CreatureType.CompassJellyfish, CreatureCategory.Medusalia, "Compass Jellyfish", "Patterned drifter");
        Register(CreatureType.CrystalJelly, CreatureCategory.Medusalia, "Crystal Jelly", "Bioluminescent");
        Register(CreatureType.SpottedJellyfish, CreatureCategory.Medusalia, "Spotted Jellyfish", "Symbiotic");
        Register(CreatureType.UpsideDownJellyfish, CreatureCategory.Medusalia, "Upside-down Jellyfish", "Bottom sitter");
        Register(CreatureType.AtlanticSeaNettle, CreatureCategory.Medusalia, "Atlantic Sea Nettle", "East coast stinger");
        Register(CreatureType.PacificSeaNettle, CreatureCategory.Medusalia, "Pacific Sea Nettle", "West coast stinger");
        Register(CreatureType.BlackSeaNettle, CreatureCategory.Medusalia, "Black Sea Nettle", "Dark drifter");
        Register(CreatureType.MauveStinger, CreatureCategory.Medusalia, "Mauve Stinger", "Purple pain");
        Register(CreatureType.BlueJellyfish, CreatureCategory.Medusalia, "Blue Jellyfish", "Atlantic blue");
        Register(CreatureType.CauliflowerJellyfish, CreatureCategory.Medusalia, "Cauliflower Jellyfish", "Lumpy drifter");
        Register(CreatureType.HelmetJellyfish, CreatureCategory.Medusalia, "Helmet Jellyfish", "Deep-sea red");
        Register(CreatureType.ImmortalJellyfish, CreatureCategory.Medusalia, "Immortal Jellyfish", "Age reverser");
        Register(CreatureType.NomurasJellyfish, CreatureCategory.Medusalia, "Nomura's Jellyfish", "Giant bloomer");
        Register(CreatureType.BoxJellyfish, CreatureCategory.Medusalia, "Box Jellyfish", "Deadly cube");
        Register(CreatureType.IrukandjJellyfish, CreatureCategory.Medusalia, "Irukandji Jellyfish", "Tiny terror");
        Register(CreatureType.PortugueseManOWar, CreatureCategory.Medusalia, "Portuguese Man o' War", "Colonial stinger");
        Register(CreatureType.ByTheWindSailor, CreatureCategory.Medusalia, "By-the-wind Sailor", "Surface drifter");

        // ═══════════════════════════════════════════════════════════════════════
        // ORDO OBSCURA - Platypus only
        // ═══════════════════════════════════════════════════════════════════════
        Register(CreatureType.Platypus, CreatureCategory.Obscura, "Platypus", "Electroreception master");

        // ═══════════════════════════════════════════════════════════════════════
        // ORDO TARDIGRADA - Sloth only
        // ═══════════════════════════════════════════════════════════════════════
        Register(CreatureType.Sloth, CreatureCategory.Tardigrada, "Sloth", "Slow but unstoppable");
    }
}
