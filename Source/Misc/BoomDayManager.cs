using HarmonyLib;
using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimVore2
{
    [StaticConstructorOnStartup]
    public static class BoomDayMaterialProvider
    {
        public static Material BoomMaterial = MaterialPool.MatFrom("0_UNUSED/BoomGods2");
    }

    /// <summary>
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// This class is terribly written in a few hours and does not represent "good" code. It's a joke on april fools for the community and serves no other function
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    public static class BoomDayManager
    {
        public static bool IsBoomDay = false;
        public static SoundDef SpawnSounds;
        public static SoundDef SuccessSounds;
        public static SoundDef FailureSounds;
        public static List<string> BeeMovieScriptLines;
        public static IEnumerable<PawnKindDef> BoomingAnimals;
        static QuirkDef acidResistance = DefDatabase<QuirkDef>.GetNamed("DigestionResistance_VeryGood");
        static HediffDef blessingHediff = DefDatabase<HediffDef>.GetNamed("BoomDay_NitroGodBlessing", false);
        static MentalBreakDef boomMentalState = DefDatabase<MentalBreakDef>.GetNamed("RV2_ForbiddenFruit");
        static DamageDef explosionDamage = DefDatabase<DamageDef>.GetNamed("BoomDay_NitroStomachExplosion", false);
        static PawnKindDef boomyKind = DefDatabase<PawnKindDef>.GetNamed("Boomalope");
        static int beeMovieLineIndex = 0;
        static bool areTipsReplaced = false;
        static bool isCurrentlyBeingPunished = false;
        static IntVec3 punishPosition;
        static Map punishMap;
        static Pawn punishPawn;
        static int punishmentsLeft = punishmentsCount;
        public static bool IsBoomDayDisabled = false;
        static List<string> DiscordMemberList;

        const int punishmentsCount = 25;
        const int beeMovieLineCycle = 500;
        const int spawnCycle = 1000;
        const int mentalBreakCycle = 10000;
        const int punishCycle = 50;
        const float boomSpawnChance = 0.05f;
        const float judgeSuccessChance = 0.33f;
        const float mentalBreakChance = 0.2f;

        public static void StartUp(Harmony harmony)
        {
            DateTime now = DateTime.Now;
            IsBoomDay = now.Day == 1 && now.Month == 4;
            if(!IsBoomDay)
                return;

            Log.Warning("Oh. It's BoomDay, alright.");
            string beeMovieScript = @"According to all known laws
of aviation,
there is no way a bee
should be able to fly.
I used to throw literally the entire bee movie script here. But you complained so so much, that I removed it. Enjoy BoomDay. XOXO, your Nabbsies";
            BeeMovieScriptLines = beeMovieScript.Split('\n').ToList();
            InitSounds();
            RunPatches(harmony);

            BoomingAnimals = DefDatabase<PawnKindDef>.AllDefsListForReading
                .Where(kind => GoesBoom(kind.RaceProps)
                && kind.RaceProps.Animal);
            DiscordMemberList = new List<string>()
            {
                "ADOVEGUILIORD","AER","APersonWhoLikesPie","Aaronthelemon","Aether_Toast","AgeNull","AggressiveFlamingo","Alaryk21","Alex Wing","Alias_Unknown","AllAboutPanda","Amaiki Monogiri","Ami","Amuse","Amvi","AmyStorm","Andos","Anlanar","AnotherSlugcat","Anton the Shadow","Anu","Arcy","Ardith Prime","Areon","AresOfThrace","Ariata","Arlor-Mortes","Arr","Arsenovanmeersscheite","ArvenFox","Aryxn","Astrux Star","Athyros","AtlanteanConquistador","Atm","Auraknight","Autumn Hill","Aventus","Azavit","Azimut","Azura Falconhein","Azure_Enigma","B0G","BOOBUFESTUU","BanditoBurritus","Beef.","Biohazard","Birdie","Bixin","BlackBr.","Blackfire24die","BlackheartCVX","BlipBlop","Bloodwolf81","BloodyLabCoats","Bluehaus","Blues","BoTies","Bob of Boblandia","BobmonkaSs","Bookman","Breeii","Brixxo","Broop798","Bubbles","Buck","Buffy","Burner","BurritoBandit1","Cacame","Call me Zak","Callen The Soulsnatcher","Calveric","CanaryNova","Candjix","Cappot","Captain Cheesecake","Captain Indeed","Carnivora","Carrier_Oriskany","Cass (she/her) ðŸ’™","Celeste Lofn","Celinda Spencer","Charlston","CheapCheat","Chelira","ChezCat","Chris1388","Chris_","Chrisganbow","Chronosphere","Cirdan Levidensis","CleanPoison","Clockwork_Golem","Cloverlea [47]","Code'inRats","Combine Assassin","Commander Callum","Constellar","Corvus489","Corvux","CosCos","CptIsaac","CrayRabbit","CrusaderCrunch","Cryptos","Curious_incubus","Cylek","DHA","DaZellon","DakkaDood","Dalcoma","Damion_Whinchester","Dante","Darek","Dark Portal","Dark74","DarkFireAurora","DarkFlameMage","DarkMoonOfDeath","DarkSlayerEX","Dark_Archivist","Darl123","DarmondÄ›j","Darrak","Datu","Deathtrackes","Deatn","Deb","Dedal","Defender_Strike","Delta","Deputy Whiskey Whiskers","Derpahkiin","DiVi8","Diamondragon101","DidNothing","DigitalSquirrel","Dima Auxillary","Dinn","Dio251","Dionysios","DirtyDan","DiscountViscount","Disposable","DizzyCloud","Doc","Dog Flower","Dolly Fail Fail","DonADFSFE","Donndubhan","Dotakin","Douglas","Dr Dooley","Dr. Chainsaw","Dr.Kasun","DracoMan671","Dracodith","Dracos","Drago","DragoWhooves","DragonFox","DragonGod51","Dragundude","Drake Razgriz","Draukel, average demon","DressedQuasar22","Drsunshine","Dtanotrew","DumbDuck","Dunbant","Dunno","Dusk","Dusk Holloway","Dust","DuttyBreakdown","Dynes","Dyno","East_","Eden (RadLad)","Edisni","ElectricityLass","Ember","Embershard","Emperor Darksider","Ender19","Erc14","Ethanw80","Ether","Etut Eagl","Ev","EwMark07","Exermaras","Exmortis","FF","FFFUU","FRENKI","Fafafooey","Fakynn","FartingOwl","Ferrilata_","Ferros","Fevix","Field","FindingClock4","FireSlime","Firedevon","Firemario25","FishWithLegs","Flame_Valxsarion","Florence","FlyQAQ","Flynn762","ForeSail","Fortalice","FoxDood_KMG","Fracno","Freakdemon","Fredthe4th","Freya","FryingPan","FryingTheWood","FugDup","Fyren","GHOST","Gaeg","Galen_skywalker","Galmar1313","Gambit_ivs-son","Gchvuvygucyhogytufyf","GeneralsAlert","Genius Of Done","Geta","Ghost333","Ghostie","Gia","GigglingRaven","Gilded Gryphon","Gilliph","Gishtrak","Glk","Gloo","Gluttony","GnomeEngi","Goat (real)","Goats","Gogaga","Golden_Pig","Gonegirl","GooInABox","Gral Stonefist","Grapefruit","Grb","Green","GreensLime","Grez","Grimmy","Grosche","Guitex","Gustavo Benzi","Guy Ruby","Gwenyldrynn","HALO_XVII","HEE HEE HEE HA!","Haladur, House Inari","Hamburger","Hann Vok","Harlo","Hauki","Hawkeye32","Hdman34","Heartless Machine","Heavy_TF2","Hedleshrsmn","Hg_shurtugal","Hiddendream","Hmmmm","Holdelta","Humdinger","Humon!","I am who I am; Cringe","Ice The Blue Dragon","Iceflame","Identity Crisis","Ignus","Igor","IilliI","Il Postino","Illya","ImboundCarp","ImpVali","Impero","Inferno","Infi","InfinityKage","InfinityMachine","Ingenious_Iron","InkAndSteel","Inkdrop","InsanityCat","Insumes","Irist0rn","IroRaze","Italnerd","Ivalera-Pixie","Izabell","J03KICKAS5","JP3885","Ja'Ree","Jack","Jacketgun","JahanRenor","Jaler","Jayne Ariantho","Jimlad","Jit","JoannEcureuil","Joe biden","Jondo","Justice is Blind","K.thomson","K_Saima","Kabloey","Kaiden Cox","KaliiSteele","Kalisynth","Kanye West Gaming","KaraKhal","Karraidin","Kassc","Kavvan Shrike","Kaykit","Keedo","Keys","Killerhitman48","Kinai","Kira","Kisik","KitsuneNoMeiji","KitÃ¦ryn","Klag","Kligor","Klintelle","Koda","Koepfer[DE]","Kono Dio Da","Konstantin","Koole","Kotarith","Kraeleth","Krautzy","KremÃ©","Krieg Guardsman","Kubonot","Kuldaryx","Kushi","KuvoDeer","Kwyn","Kyo","KyronFox","LANDJAWS","LEE","LGayJ2506","LOLspc","LUSEN","Lanszlo","Lati","Laurient","LawrenceofSpice","LazySnake7","Leonovers","Leora","Lestat","Lexaeria","Lieutenant","LieutenantSparkles","LightCanadian","Liho","Lillica","Linkolas","Linrys","Lith1313","Lizergin","Ljust","Lockete217","Loki1998","Loner Imortal","Lookspark","Lord Fartucus","Lostless","LotharEgli","Lovery_Corpce","Lovis07","LulaPlasmatail","Luna.","LunaLoutre","Lunatyr","Lybearion","LycanWolf","MANDRAKE109","MR DARKNUT","MR.Mute","Mab","MaceOfClubs","MadMusician","Madame Lesbianne IV","Man Of Questionable Origins","Mango","Marine25","Matthews","Maxvell_mega","Mchccjg12","Me3","Mederic","Meowerine","MetalNeverDies","Mewtwo","MianQ","Miaz","Michl","Milly","Mint","Mirao","Mirogami","MislFox","MissKoko","Mist","Miyabi","MjÃ¶lkhaj","MobFighterLVL999","Mockingbird","ModdingAnon","Moineau Fa","Momma Fuzzy","Monti","Monxta","Mooman","MoonyKnights","Morgante","Mr. Skeltal","Mr.Mango","MrFrait","Mr_Sir132","Myconid32","Mza22794","NNDO","NOOneAtAll","NagaDevGuy","Nanakra","Nap Time","Nari","Natalie6","Natje","Nealthedragon","Nekamoeâ¤","Nekuzar","Neo","Neo Bear","Neoned_One","NeverKnowWhyThis","Nexuz","Nicaea","Nicholas Kane [ð“‚º]","Nick Wazowski","Nightbunk","NinjaTaco","Nivalenya","NnnDdt","NoBodhi","NobodyImportant","Nobodyofanymatter","NorgTheFishðŸŸ","NormChel","Not What We Expected","Notabot","Nouvi","Nox","Nuclear Chicken","Nue","Nuke","Null-wolf","Nut","OblongTriangle","Oiver","Okiti","Omagakain139","Omegahack","Ominara","Onikage-056, Blast-Happy Bunny","OnstrideDate","Orand","Ordo Redactus","Orionzete","Orolin","Osekamiga_sama","Ostravalordrec","Osuenn","OzSkygarm","PK_Burn","Paciel â™¡","Pallidcups","Pancha","PansexualDiceGoblin","Panth","Pantry Guard","PappaVol","ParabolicSamuel","Parapanora","Parm","PeanutzButter","Pear","PedroPerranca","Pervy Twitch","Phant0m5","PieMan","Plantstrider","PolySoup","Pooka","Porkuslavia","PossibleTrashPanda","PotatoSight","Price","Pringle can","ProbeRush","Proposita","ProtoDragonSoul","Przyjaciel","Pure Fox","PuroPaws","Purple-Wanderer","PurpleCupcake","Quasai","Questionmark/?","R.A","RCX","RDD","RYUMI","Raccoon_found a home","Rachnus, Goddess of Creation","Rad Lezar","Rael","Raggedy_Andi","RakeVuril","Rakurai007","RandomnessInc","Rantroper","Raru","RashDolphin","Raven","Rawrling","Razzy","RealOne11111","Recreaf","Reddeyfish","Redmax5000","Regional-Commander Midwest","Rejected_Son","Reji8627","Relatus","Relo","Ren","Retroas","Rett","Rex!!","Rez3056","Rhodes","Richard333","Ridoog77","RiggsÎžclipse","Rik 'Phlosion","Rinnin","Rithmere","Rivethead","Rizmekin","Roaming_Guardian","RoanHorse","Roblox GamerXoX","Roc","Rocksoul12","RomanPunch","Roseilette (Tersha)","Rowendall","Rox","RoyalArchduke","RoyalAsh","Rubiont-47(chatbot)","Rubytooth","Ruined King","Ryner","Ryou","S1lvr","SSeth","Sai670","Saklex (*Â´â–½ï½€*)","Sally","Sans-Dents","Satile","ScarlettChan","Schpadoinkle","ScottHorselyWaters-Waters","Scringle","Sebas","Selicia","SeloFox","Senka","Seoulbee","Sephiose","SerSeagulls (Holgrimm)","Serelith","Sero Reiuji","Shadow Wolf","Shadowmane","Shadowpuma","ShamelessWriter","Shary","Shawell","Shazbot","Sheights","Shi(ro)","Shigu","Shirea","Shiro","Shooks","Shyranda","Sidelingknave5","Signeow the Cat","Silver Midnight","Silver Wolfington","Simon91","Sir Dongle","Sirentu","Siulkas","Skeletsoha","Skits","Skullsmasher","Skunk","Skunktail","SkySteak","SleeplessB","Sloth","Smart","Smashgunner","Smeetreaper","SnaksTheKobold","SnapBushrat","SnaximumOverdrive","Sneedman","SnowFert","SnowMoring","Snowcraft","SoftHuggableDerg aka FilipMach","SoftVoid","SomeRandomGuy","Somerandomsmuck","SoularKnight","Sp00ky","SpaceSoap","SpankTank","Spectator","Spooby","SpotTea","Squazzel","Squeakchan","SquishyJam","Starr-Man","Stimton","Strato","StrawberryLoki","StuCen","Sultry_PieGirl_Kelly","Sunlight Swift","SuperSaiyanDerek","SuperSpaceSenpaiSama","Swift_Assassin","Swuna","Syd","T1-K4","TAC0002","TAC005","TET","THEaje123","T_","Tangent","Tanis","Tankfire5820","Tasald","Taschenformat","Tax Collector","TecMacRun","Tech Priest Waffle","Techie_Murau","Teck Nickel","Telkar","Tenebris","Terazin","Tess","ThatScootsy","The Antagonist","The Chef","The Lone Medic","The LoreMaster","The Ninja","The Notorious E_A_T","The Rat King","The Stubby Tailed Destroyer","TheAbsoluteUnit","TheEnchantedWolf","TheFaker4448","TheHornman","TheLittleCp","TheRealJeffBezoz","TheRisingSun56","TheSaltiestSergal","TheThirdFentreux","Thief of Time","ThisPersonDoesNotExist","Thredbo","Tidurian","Tj","ToastBaron","TothAkos20","Trinart","Trinity Corp","Triske","Troian","True Mind","Trylington","Tyler, Mysticfenrir","Typhon","UnbirthLover777","Universal","Uranium","Ursa","Vain","Valinor","Vearos","Vector","Ven","Vera","Verdant Zephyr","Verdeth","VerduxXudrev","Vermono","VespyEspy","Viperkiller","Virus","Virussr","Visser","Vistha Kai","Voracious Cutie","VoreBBB","Vortex67","W014T4K0D4","Walker123","Waria","WarpedRealities","Water","Whiskey Dick","White","Wildfire","With All My Heart","Wowololo","WyrmOfWind","XRedd","Xdiegoxl","Xehra","Xembrojen","Xodious","Xybora","YashaClawtooth","Yeen","Yeez","Your Neighborhood Trap","Ytrof","Zebb","Zebranky","Zekuven","Zeldazackman","Zelo","Zeman","Zenth","Zergal","Zeril","Zerothx16","Zeta","Zeth","Ziren I Marcus","Zohfur","Zombie42399","Zucchero","[=AWOL=]Mity","[REDACTED]","a7","aaa4568","aaa56789","abcdefg123456","acerola","achoo01","acorn","adjet","adriano20037","ajves","alec31907","alek4ever","amw232","animegeek","anonymousacount","ansonh92","aqamrose","arendjvr2000","asdasdadasda","asdfadfasdfs3eb3tb","askl","aszx123456","auron126","avelor89","badblade3719","barrabobabo","bbuttons92","beebo","bees123","benzol74","bignickdigger69","bingbingwahoo123","bippitybop","blade99","bob69","bobbler","bobbler2","bobby12408","bobot","bonk","boxshark","bubbleztoo","cajetan","calldahotline","cheeriermoss4","cheese touich","cheese130","chemicalcrux","cin89898","clkljsbs","cloudOrion","cooltank","cotton berry","crest","cutecumber1972","daht","dani042","dany7420","darkdeathly","darkeven84","darksin","dat boye","ddur","dead23","delta quadrangle","destymon","diCanio10","didakkos","dnsmolej","doctorspangle","dokupe","dollashot","dragonlordcody","dstowbpseukrzvkpzj","dtyl","dufva","elijuh","endercreeper8060","epsilon208","epsilon275","epsilon287","epsilon750","epsilon765","epsilon859","epsilon9057","epsilon953","ethereal","etx14","evmeswil","fE113","falty","fdssdfsdf","ferrethours","ffbdba","fhdfhfdg","fjnglkbjgfgfcggbfgbgggnfxhht","fmrl1","furhead","futa0425","fuuuuuu","gemclips1300","genderneutralnoun","gniioo","gouj1","greenars","guesswho","halal moment","happa_roll","hatten","hauvega","hereforaflash","herm1t","heromc","hexcorsist","hj","hjhjj","hugg","hypensQrone","idenr","imma drink sum bleach","insectiile","jaaldabaoth","jaclea","jadeaudrey","jag092","jedi_knight-2187","jhjjooujoi","jjkkh","jorb","kINGoFcABBAGE","kaalell","kaylish","ke","keevvv","ken0a0","killer","killzone1123","kiratu11","kitsuneneo","kjh","kjnjkn","konsol","kr_95","kwakamungus","legobildr","lemon","leoinc","letifer89","lightspectr","ljk","lurkerrrr","mandara483","manti_Star","manutazo","mason27","maxwellccm","maxxballz","mayonnaise","mehran13842005","memento_mori4444","michejailll","minusthedrifter","mlo","monnbrun","monta59l","mrmd17","mysterious Stranger","nadona","negative vibes","newt","nicanor","nikome","nobodyshouldcallmeanythingtyvm","not safe for work","nota999","ocelotzx","one","oofbeen","ooooga booga","organic","p8492","pacman0701","peteian","pokepaul","praven","psx110","qweqewqwe","radiantAurora","random_randomtoo","randomsmuck","rh7io67","rimling","rjw","robohobo","rockyoliver","roththemage","rut","rya","samye","scarface2944","scc30","scotbot","secretivesamuel","sevensevenseven","sharknado","shubinisfat","sika","sikspid","silent30","sirlord4","skgdkdldldkdldkhd","slakevilkis","smiledog.jgp","smiley","snek","starfowl","starhunter","starvedsquare","superpower2020","tea","temo","testtt","tiger33116","tomberry12","tonjes","trashman","trix","typhoon","umarÅ‚ z krindzu","unholy","vagaprime","vfvggl","vhbnm","vore101zs","vorelikei","vorelover 69","walkingfarm","warbrand2","wasauchimmer","waynerd","wazawaz","whatif12","whyimmortal_reader","wolfdracion","x50413","xDinoPL","xero256","xoxol227","yalie","yeee","yfhjm,","ym733172","yoshi","yukaR","yumboi","z123457","zawzaw","zbxdv","zgf2022",""
            };
        }

        private static void InitSounds()
        {
            SpawnSounds = DefDatabase<SoundDef>.GetNamed("BoomDay_Spawn");
            SuccessSounds = DefDatabase<SoundDef>.GetNamed("BoomDay_JudgeSuccess");
            FailureSounds = DefDatabase<SoundDef>.GetNamed("BoomDay_JudgeFailure");
        }

        private static void RunPatches(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(TickManager), "DoSingleTick"), null, new HarmonyMethod(typeof(BoomDayManager), "DoFoolsTick"));
            harmony.Patch(AccessTools.Method(typeof(GameplayTipWindow), "DrawWindow"), new HarmonyMethod(typeof(BoomDayManager), "ForceBoomTips"));
            harmony.Patch(AccessTools.Method(typeof(PreVoreUtility), "PopulateRecord"), null, new HarmonyMethod(typeof(BoomDayManager), "AddBoomBlessing"));
            harmony.Patch(AccessTools.Method(typeof(PostVoreUtility), "ApplyPostVore"), null, new HarmonyMethod(typeof(BoomDayManager), "RemoveBoomBlessing"));
            harmony.Patch(AccessTools.Method(typeof(DigestionUtility), "KillPreyWithDamage"), null, new HarmonyMethod(typeof(BoomDayManager), "JudgeOnDigest"));
            harmony.Patch(AccessTools.Method(typeof(VoreValidator), "CanVore"), new HarmonyMethod(typeof(BoomDayManager), "BlockMalePredators"));
            harmony.Patch(AccessTools.Method(typeof(Thing), "Draw"), new HarmonyMethod(typeof(BoomDayManager), "OverlayBoomMaterial"));
            harmony.Patch(AccessTools.Method(typeof(SectionLayer_Things), "DrawLayer"), new HarmonyMethod(typeof(BoomDayManager), "InjectDrawLayer"));
            harmony.Patch(AccessTools.Method(typeof(Game), "FinalizeInit"), new HarmonyMethod(typeof(BoomDayManager), "WorldLoaded"));

        }

        private static void DoPurelyRandomBullshit()
        {
            if(GenTicks.TicksGame % spawnCycle == 0)
            {
                if(Rand.Chance(boomSpawnChance))
                {
                    SpawnRandomBullshit();
                }
            }
            if(GenTicks.TicksGame % beeMovieLineCycle == 0)
            {
                Messages.Message(BeeMovieScriptLines[beeMovieLineIndex++], MessageTypeDefOf.SilentInput);
                if(beeMovieLineIndex == BeeMovieScriptLines.Count - 1)
                    beeMovieLineIndex = 0;
            }
            if(GenTicks.TicksGame % mentalBreakCycle == 0)
            {
                if(Rand.Chance(mentalBreakChance))
                {
                    CauseBoomMentalBreak();
                }
            }
        }

        private static void SpawnRandomBullshit()
        {
            Map map = Find.CurrentMap;
            if(map == null)
                return;
            int loopPrevention = 10;
            IntVec3 position = map.AllCells.RandomElement();
            while(!position.Standable(map) && loopPrevention-- > 0)
            {
                position = map.AllCells.RandomElement();
            }
            SoundStarter.PlayOneShotOnCamera(SpawnSounds);
            for(int i = 0; i < Rand.RangeInclusive(1, 6); i++)
            {
                GenSpawn.Spawn(PawnGenerator.GeneratePawn(BoomingAnimals.RandomElement()), position, map);
            }
        }

        private static void CauseBoomMentalBreak()
        {
            Pawn victim = Find.CurrentMap?.mapPawns?.FreeColonistsSpawned?.RandomElement();
            if(victim == null)
                return;
            boomMentalState.Worker.TryStart(victim, "I could go for some taco bell", false);
        }

        private static void DoFoolsTick()
        {
            if(IsBoomDayDisabled)
                return;
            DoPurelyRandomBullshit();
            if(isCurrentlyBeingPunished && punishmentsLeft == 0)
                EndPunishment();
            if(isCurrentlyBeingPunished)
                PUNISH();
        }
        private static void ForceBoomTips(ref List<string> ___allTipsCached)
        {
            if(IsBoomDayDisabled && !areTipsReplaced)
                return;
            if(IsBoomDayDisabled && areTipsReplaced)
            {
                // undo the tip replacement and flag them as un-replaced so the early exit triggers
                ___allTipsCached.Clear();
                areTipsReplaced = false;
                return;
            }
            if(areTipsReplaced)
                return;

            ___allTipsCached = new List<string>()
            {
                "The BOOM GODS have awoken! Be wary of the Boom!",
                "The BOOM GODS have awoken! Duck and cover, boys",
                "The BOOM GODS have awoken! It smells burnt around here!",
                "The BOOM GODS have awoken! Those Boomalopes never looked tastier!",
                "The BOOM GODS have awoken! Do you dare to try the forbidden fruit?",
                "The BOOM GODS have awoken! Big bada boom!",
                "The BOOM GODS have awoken! Test the strength of your walls, chomp on those lopes!",
                "The BOOM GODS have awoken! I love the smell of fresh napalm in the morning",
                "The BOOM GODS have awoken! We do a lil boomin",
                "The BOOM GODS have awoken! Ok, boomer",
            }.InRandomOrder().ToList();

            areTipsReplaced = true;
        }
        private static void AddBoomBlessing(VoreTrackerRecord record)
        {
            if(IsBoomDayDisabled)
                return;
            if(!record.VoreGoal.IsLethal || !GoesBoom(record.Prey.RaceProps))
                return;
            Hediff x = HediffMaker.MakeHediff(blessingHediff, record.Predator);
            record.Predator.health.AddHediff(x);
            record.Prey.QuirkManager(false)?.TryPostInitAddQuirk(acidResistance, out _);
            NotificationUtility.DoNotification(NotificationType.LetterThreatBig, $"The BOOM GODS whisper dark secrets of glorious death and destruction to {record.Predator.LabelShortCap}.", "The BOOM GODS have taken notice of your colony");
        }
        private static void RemoveBoomBlessing(VoreTrackerRecord record)
        {
            Hediff blessing = record.Predator?.health?.hediffSet?.GetFirstHediffOfDef(blessingHediff);
            if(blessing == null)
                return;
            record.Predator.health.RemoveHediff(blessing);
        }
        private static void JudgeOnDigest(VoreTrackerRecord record)
        {
            if(IsBoomDayDisabled)
                return;
            if(!GoesBoom(record.Prey.RaceProps))
                return;
            if(!record.Predator.health.hediffSet.HasHediff(blessingHediff))
                return;
            if(Rand.Chance(judgeSuccessChance))
            {
                REWARD(record.Predator);
            }
            else
            {
                BeginPunishment(record.Predator);
            }
        }
        private static void BeginPunishment(Pawn pawn)
        {
            NotificationUtility.DoNotification(NotificationType.MessageThreatBig, $"The BOOM GODS have judged {pawn.LabelShortCap} and have deemed them UNWORTHY. PREPARE FOR DESTRUCTION.");
            SoundStarter.PlayOneShotOnCamera(FailureSounds);
            isCurrentlyBeingPunished = true;
            punishPosition = pawn.Position;
            punishMap = pawn.MapHeld;
            punishPawn = pawn;
        }
        private static void PUNISH()
        {
            if(GenTicks.TicksGame % punishCycle != 0)
                return;
            if(!CellFinder.TryFindRandomCellNear(punishPosition, punishMap, 20, (IntVec3 loc) => loc.Standable(punishMap), out IntVec3 explosionPosition))
                return;
            if(punishmentsLeft % 12 == 0)
                SoundStarter.PlayOneShotOnCamera(FailureSounds);
            GenExplosion.DoExplosion(explosionPosition, punishMap, 10, explosionDamage, punishPawn);
            punishmentsLeft--;
        }
        private static void EndPunishment()
        {
            punishMap = null;
            punishPosition = default(IntVec3);
            punishPawn = null;
            isCurrentlyBeingPunished = false;
            punishmentsLeft = punishmentsCount;
        }
        private static void REWARD(Pawn pawn)
        {
            NotificationUtility.DoNotification(NotificationType.Letter, $"The BOOM GODS have deemed {pawn.LabelShortCap} worthy and bestow their gifts upon your colony.\n\nTake good care of them :)", "The BOOM GODS are satisfied");
            SoundStarter.PlayOneShotOnCamera(SuccessSounds);
            Map map = pawn.MapHeld;
            IntVec3 startPos = new IntVec3(pawn.Position.x - 15, 0, pawn.Position.z - 15);
            TerrainDef black = TerrainDef.Named("MetalTile");
            TerrainDef gold = TerrainDef.Named("GoldTile");
            TerrainDef yellow = TerrainDef.Named("SilverTile");
            TerrainDef red = TerrainDef.Named("CarpetRed");
            TerrainDef white = TerrainDef.Named("CarpetCream");
            TerrainDef[,] drawMap = new TerrainDef[,]
            {
                {black,null,null,null,null,null,null,null,null,black,black,black,black,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null},
                {black,black,null,null,null,null,null,null,null,black,gold,gold,black,black,black,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null},
                {yellow,black,black,null,null,null,null,null,null,black,gold,gold,gold,gold,black,black,black,null,null,null,null,null,null,null,null,null,null,null,null,null,null},
                {yellow,yellow,black,null,null,null,null,null,null,black,gold,gold,gold,gold,gold,gold,black,black,null,null,null,null,null,null,null,null,null,null,null,null,null},
                {yellow,yellow,yellow,black,null,null,null,null,null,black,gold,gold,gold,gold,gold,gold,gold,black,black,null,null,null,null,null,null,null,null,null,null,null,null},
                {yellow,yellow,yellow,black,black,null,null,null,null,black,gold,gold,gold,gold,gold,gold,gold,gold,black,black,null,null,null,null,null,null,null,null,null,null,null},
                {yellow,yellow,yellow,yellow,black,null,null,null,null,black,gold,gold,gold,gold,gold,gold,gold,gold,gold,black,black,null,null,null,null,null,null,null,null,null,null},
                {yellow,yellow,yellow,yellow,black,null,null,null,null,black,gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,black,black,null,null,null,null,null,null,null,null,null},
                {yellow,yellow,yellow,yellow,yellow,black,null,null,null,black,gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,black,black,null,null,null,null,null,null,null,null},
                {yellow,yellow,yellow,yellow,yellow,black,null,null,null,black,gold,gold,gold,gold,gold,gold,black,gold,gold,gold,gold,gold,black,null,null,null,null,null,null,null,null},
                {yellow,yellow,yellow,yellow,yellow,black,black,null,null,black,gold,gold,gold,gold,gold,gold,gold,black,gold,gold,gold,gold,black,black,null,null,null,null,null,null,null},
                {yellow,yellow,yellow,yellow,yellow,yellow,black,black,black,black,gold,gold,gold,gold,gold,gold,gold,black,gold,gold,gold,gold,gold,black,null,null,null,null,null,null,null},
                {yellow,yellow,yellow,yellow,yellow,yellow,black,gold,gold,gold,gold,gold,gold,gold,gold,black,black,gold,black,gold,black,gold,gold,black,black,null,null,null,null,null,null},
                {yellow,yellow,yellow,yellow,yellow,yellow,black,gold,gold,gold,gold,gold,gold,gold,black,black,black,black,gold,black,black,gold,gold,gold,black,null,null,null,null,null,null},
                {yellow,yellow,yellow,yellow,yellow,yellow,black,gold,gold,gold,gold,gold,gold,gold,gold,black,black,gold,gold,gold,gold,gold,gold,gold,black,null,null,null,null,null,null},
                {yellow,yellow,yellow,yellow,yellow,black,black,gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,black,black,null,null,null,null,null},
                {yellow,yellow,yellow,yellow,black,black,gold,gold,gold,gold,gold,gold,gold,gold,gold,black,black,gold,gold,gold,gold,gold,gold,gold,gold,black,null,null,null,null,null},
                {yellow,yellow,yellow,black,black,gold,gold,gold,gold,gold,black,gold,gold,gold,black,white,red,black,gold,gold,gold,gold,gold,gold,gold,black,null,null,null,null,null},
                {yellow,black,black,black,gold,gold,gold,gold,gold,gold,black,black,gold,gold,black,red,red,white,black,gold,gold,gold,gold,gold,gold,black,black,null,null,null,null},
                {black,black,gold,gold,gold,gold,gold,gold,gold,gold,gold,black,black,gold,gold,black,red,red,black,gold,gold,gold,gold,gold,gold,gold,black,null,null,null,null},
                {gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,black,black,black,black,white,red,red,black,gold,gold,gold,gold,gold,gold,black,null,null,null,null},
                {gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,gold,black,black,black,black,red,red,white,black,gold,gold,gold,gold,gold,black,null,null,null,null},
                {gold,gold,black,black,black,gold,gold,gold,gold,gold,gold,gold,gold,black,null,null,black,black,red,red,black,gold,gold,gold,gold,black,black,null,null,null,null},
                {black,black,black,black,black,black,black,gold,gold,gold,gold,black,black,null,null,null,null,null,black,black,black,black,gold,gold,gold,black,null,null,null,null,null},
                {black,yellow,yellow,yellow,yellow,black,black,black,gold,gold,black,black,null,null,null,null,null,null,null,black,black,black,black,black,black,black,null,null,null,null,null},
                {yellow,yellow,yellow,yellow,yellow,yellow,black,black,gold,black,black,null,null,null,null,null,null,null,null,null,null,null,null,black,black,null,null,null,null,null,null},
                {yellow,yellow,yellow,yellow,yellow,yellow,yellow,black,black,black,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null},
                {yellow,yellow,yellow,yellow,yellow,yellow,yellow,black,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null},
                {yellow,yellow,yellow,yellow,yellow,yellow,yellow,black,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null},
                {yellow,yellow,yellow,yellow,yellow,yellow,yellow,black,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null},
                {yellow,yellow,yellow,yellow,yellow,yellow,yellow,black,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null},
            };
            for(int i = 0; i < drawMap.GetLength(0); i++)
            {
                for(int j = 0; j < drawMap.GetLength(1); j++)
                {
                    TerrainDef drawMat = drawMap[drawMap.GetLength(0) - 1 - j, i];
                    if(drawMat == null)
                        continue;
                    IntVec3 drawLoc = new IntVec3(startPos.x + i, startPos.y, startPos.z + j);

                    map.terrainGrid.SetTerrain(drawLoc, drawMat);
                }
            }

            for(int i = 0; i < 100; i++)
            {
                Pawn boomy = PawnGenerator.GeneratePawn(boomyKind, Faction.OfPlayer);
                boomy.Name = new NameSingle(DiscordMemberList.RandomElement());
                GenSpawn.Spawn(boomy, pawn.Position, pawn.MapHeld);
            }
            IsBoomDayDisabled = true;
        }

        private static bool GoesBoom(RaceProperties raceProps)
        {
            DeathActionWorker worker = raceProps.DeathActionWorker;
            return worker is DeathActionWorker_SmallExplosion || worker is DeathActionWorker_BigExplosion;
        }

        private static bool BlockMalePredators(Pawn predator, ref string reason)
        {
            if(IsBoomDayDisabled)
                return true;
            if(predator.gender == Gender.Male)
            {
                reason = "Males can't vore. Vegan says so.";
                return false;
            }
            return true;
        }

        private static void OverlayBoomMaterial(Thing __instance)
        {
            if(IsBoomDayDisabled)
                return;
            if(!isCurrentlyBeingPunished)
                return;
            Vector3 drawPos = __instance.DrawPos;
            Vector3 drawSize = new Vector3(1f, 1f, 1f);
            drawPos.y = AltitudeLayer.Weather.AltitudeFor();
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(drawPos, Quaternion.identity, drawSize);
            Graphics.DrawMesh(MeshPool.plane10, matrix, BoomDayMaterialProvider.BoomMaterial, 0);
        }

        private static void InjectDrawLayer(SectionLayer __instance)
        {
            if(IsBoomDayDisabled)
                return;
            if(!isCurrentlyBeingPunished)
                return;
            if(!(__instance is SectionLayer_ThingsGeneral))
                return;
            foreach(LayerSubMesh mesh in __instance.subMeshes)
            {
                if(mesh.finalized && !mesh.disabled)
                {
                    Graphics.DrawMesh(mesh.mesh, Matrix4x4.identity, BoomDayMaterialProvider.BoomMaterial, 0);
                }
            }
        }

        private static void WorldLoaded()
        {
            if(Find.GameInfo.permadeathMode)
            {
                NotificationUtility.DoNotification(NotificationType.Letter, "Okay, look, you are running a permadeath save here, so I disabled the BoomDay functionalities. I highly recommend that you try them out in a non-permadeath save though! If you have no idea what I mean, I absolutely urge you to just start a new colony for funsies, it'll be great! :)", "From Nabber, to you");
                IsBoomDayDisabled = true;
            }
            else
            {
                IsBoomDayDisabled = false;
            }
        }

        [DebugAction("RimVore-2", "Toggle BoomDay", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ToggleBoomDayFlag()
        {
            if(IsBoomDayDisabled)
            {
                NotificationUtility.DoNotification(NotificationType.Letter, "Back for more?", "The BOOM GODS are pogging");
            }
            else
            {
                NotificationUtility.DoNotification(NotificationType.Letter, "I don't blame you. Do note that if you reload the game you need to do this again.", "The BOOM GODS understand");
            }

            IsBoomDayDisabled = !IsBoomDayDisabled;
        }
    }
}
