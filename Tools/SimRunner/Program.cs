using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

// Regauntless — SimRunner (the sheet -> sim BRIDGE)
// Closes the loop between the two existing tools:
//   SheetResolver proved a .sheet computes into real stats.
//   CrawlSim has a combat engine (but ran on hardcoded magic-number combatants).
// SimRunner resolves a REAL character sheet + a REAL bestiary mob into Combatants and runs the
// (ported) fight loop on them. "Instantiate the character you built and actually fight something."
// Standalone dotnet console tool (mirrors CrawlSim / SheetResolver). Unity-independent.
//   run:  cd Tools/SimRunner && dotnet run -c Release  [sheetPath] [mobName] [mobSize]

enum DiceMode { Take10, Min, Max, RNG }
static class Dice {
    public static Random rng = new Random(12345);
    public static int RollDie(int sides, DiceMode m) => m switch {
        DiceMode.Min => 1, DiceMode.Max => sides,
        DiceMode.Take10 => (int)Math.Floor((sides + 1) / 2.0), // per-die avg floored (combat.md)
        DiceMode.RNG => rng.Next(1, sides + 1), _ => 1 };
    public static int D20(DiceMode m) => m switch {
        DiceMode.Min => 1, DiceMode.Max => 20, DiceMode.Take10 => 10, DiceMode.RNG => rng.Next(1,21), _ => 10 };
}

// a parsed dice expression like "1d8", "1d4-3", "2d4", flat "1"
struct DamageExpr {
    public int n, sides, flat;
    public static DamageExpr Parse(string s) {
        s = s.Trim();
        var m = Regex.Match(s, @"^(\d+)d(\d+)\s*([+-]\s*\d+)?$");
        if (m.Success) {
            int flat = 0;
            if (m.Groups[3].Success) flat = int.Parse(m.Groups[3].Value.Replace(" ",""));
            return new DamageExpr { n = int.Parse(m.Groups[1].Value), sides = int.Parse(m.Groups[2].Value), flat = flat };
        }
        // flat number (e.g. viper tiny "1")
        if (int.TryParse(s, out int f)) return new DamageExpr { n = 0, sides = 0, flat = f };
        return new DamageExpr { n = 1, sides = 4, flat = 0 }; // fallback
    }
    public int Roll(DiceMode mode, int extraFlat = 0) {
        int total = flat + extraFlat;
        for (int i = 0; i < n; i++) total += Dice.RollDie(sides, mode);
        return Math.Max(1, total); // min 1 on a hit (combat.md)
    }
    public override string ToString() => n > 0 ? $"{n}d{sides}{(flat!=0?(flat>0?"+"+flat:flat.ToString()):"")}" : flat.ToString();
}

class Attack { public string Name; public int ToHit; public DamageExpr Dmg; public int CritMult = 2; }

class Combatant {
    public string Name; public int MaxHp, Hp, AC;
    public List<Attack> Attacks = new();
    public bool Alive => Hp > 0;
}

static class Sim {
    public static void DoAttack(Combatant a, Combatant d, Attack atk, DiceMode m, List<string> log) {
        int nat = Dice.D20(m);
        int roll = nat + atk.ToHit;
        bool isRoll = (m == DiceMode.RNG);
        bool autoMiss = isRoll && nat == 1;
        bool autoHit  = isRoll && nat == 20;
        bool hit = !autoMiss && (autoHit || roll >= d.AC);
        if (hit) {
            int dmg = atk.Dmg.Roll(m);
            d.Hp -= dmg;
            log?.Add($"  {a.Name} {atk.Name} HITS {d.Name}  (atk {roll} vs AC {d.AC})  {dmg} dmg -> {Math.Max(0,d.Hp)}/{d.MaxHp}");
        } else {
            log?.Add($"  {a.Name} {atk.Name} misses {d.Name}  (atk {roll} vs AC {d.AC})");
        }
    }
    public static bool Fight(Combatant hero, Combatant mob, DiceMode heroMode, List<string> log) {
        DiceMode mobMode = (heroMode == DiceMode.RNG) ? DiceMode.RNG : DiceMode.Take10; // enemies fixed at Normal
        int round = 0; const int cap = 100;
        while (hero.Alive && mob.Alive && round < cap) {
            round++; log?.Add($" -- round {round} --");
            // hero: first attack (full-attack / multi-attack is a later enrichment)
            if (hero.Attacks.Count > 0) DoAttack(hero, mob, hero.Attacks[0], heroMode, log);
            // mob: all its attacks (badger-style multi-attack supported)
            if (hero.Alive) foreach (var ma in mob.Attacks) { if (!mob.Alive) break; DoAttack(mob, hero, ma, mobMode, log); }
        }
        return hero.Alive && !mob.Alive;
    }
}

class SimRunner {
    static string DataDir;
    static void Main(string[] args) {
        DataDir = FindData();
        if (DataDir == null) { Console.WriteLine("!! can't find data/ dir"); Environment.Exit(2); }
        string sheetPath = args.Length > 0 ? args[0] : Path.Combine(DataDir, "sheets", "Grukk.sheet");
        string mobName   = args.Length > 1 ? args[1] : "Goblin";
        string mobSize   = args.Length > 2 ? args[2] : "Small";

        // ── GAUNTLET MODE: one hero vs the entire fightable CR<1 bestiary ──
        if (sheetPath == "--all" || mobName == "--all" || (args.Length>0 && args[0]=="--all")) {
            string sp = (args.Length > 1 && args[1] != "--all") ? args[1] : Path.Combine(DataDir,"sheets","Grukk.sheet");
            RunGauntlet(sp);
            return;
        }

        var hero = BuildHeroFromSheet(sheetPath, out string heroDesc);
        var mob  = BuildMobFromBestiary(mobName, mobSize, out string mobDesc);

        Console.WriteLine("=== REGAUNTLESS — SimRunner (sheet -> sim) ===\n");
        Console.WriteLine($"HERO: {heroDesc}");
        Console.WriteLine($"MOB : {mobDesc}\n");

        // one readable deterministic play-by-play (Normal / take-10)
        Console.WriteLine($"### FIGHT (Normal / take-10 play-by-play): {hero.Name} vs {mob.Name} ###");
        var log = new List<string>();
        var h = Clone(hero); var mb = Clone(mob);
        bool win = Sim.Fight(h, mb, DiceMode.Take10, log);
        foreach (var l in log) Console.WriteLine(l);
        Console.WriteLine($"  => {(win ? hero.Name + " WINS" : mob.Name + " wins")}  (hero {Math.Max(0,h.Hp)}/{h.MaxHp}, mob {Math.Max(0,mb.Hp)}/{mb.MaxHp})\n");

        // difficulty sweep + RNG win-rate
        Console.WriteLine("### DIFFICULTY SWEEP (hero dice tilt; mob fixed at Normal) ###");
        foreach (var (label, mode) in new[]{("Easy(max)",DiceMode.Max),("Normal(10)",DiceMode.Take10),("Hard(min)",DiceMode.Min)}) {
            var hh = Clone(hero); var mm = Clone(mob);
            bool w = Sim.Fight(hh, mm, mode, null);
            Console.WriteLine($"  {label,-12}: {(w ? "WIN" : "lose"),-5}  (hero ends {Math.Max(0,hh.Hp)}/{hh.MaxHp})");
        }
        int N = 2000, wins = 0; long rounds = 0;
        for (int i=0;i<N;i++){ var hh=Clone(hero); var mm=Clone(mob); var lg=new List<string>(); if(Sim.Fight(hh,mm,DiceMode.RNG,lg)) wins++; rounds += lg.Count(x=>x.StartsWith(" -- round")); }
        Console.WriteLine($"  Gremlin(RNG) : {100.0*wins/N:F1}% win over {N} runs, avg {(double)rounds/N:F1} rounds\n");

        Console.WriteLine("[done] usage: dotnet run -c Release [sheetPath] [mobName] [mobSize]");
    }

    // ── GAUNTLET: hero vs the entire fightable CR<1 bestiary, as a matrix ──
    static void RunGauntlet(string sheetPath) {
        var heroGeared = BuildHeroFromSheet(sheetPath, out string heroDesc);
        var heroNaked  = BuildHeroNaked(sheetPath, out string nakedDesc);
        var mobs = EnumerateFightableMobs();

        Console.WriteLine("=== REGAUNTLESS — Gauntlet: one hero vs the whole CR<1 bestiary ===\n");
        Console.WriteLine($"GEARED: {heroDesc}");
        Console.WriteLine($"NAKED : {nakedDesc}");
        Console.WriteLine($"\nfightable stat-blocks: {mobs.Count}  (noncombat stubs + CR>=1 w/o interpreted attacks auto-skip)\n");

        int N = 3000;
        Console.WriteLine($"{"MOB",-30}{"hp/ac",-8}{"det(N)",-8}{"GEARED",-18}{"NAKED",-18}");
        Console.WriteLine($"{"",-30}{"",-8}{"",-8}{"win% / rnds",-18}{"win% / rnds",-18}");
        Console.WriteLine(new string('-', 82));
        foreach (var (label, make) in mobs) {
            var m0 = make();
            string hpac = $"{m0.MaxHp}/{m0.AC}";
            var (gw, gr, gdet) = Sweep(heroGeared, make, N);
            var (nw, nr, ndet) = Sweep(heroNaked,  make, N);
            Console.WriteLine($"{label,-30}{hpac,-8}{(gdet?"det:W":"det:L"),-8}{gw.ToString("F0")+"% / "+gr.ToString("F1"),-18}{nw.ToString("F0")+"% / "+nr.ToString("F1"),-18}");
        }
        Console.WriteLine(new string('-', 82));
        Console.WriteLine("det = Normal/take-10 deterministic result (W/L). win% = Gremlin/RNG win rate over N="+N+". rnds = avg rounds.");
        Console.WriteLine("NOTE: 'det:L' at take-10 often = the to-hit WALL (can't-hit stalemate), not a death — see run-loss-model.md.");
    }

    static (double winPct, double avgRounds, bool detWin) Sweep(Combatant hero, Func<Combatant> makeMob, int N) {
        var hd = Clone(hero); var md = makeMob();
        bool detWin = Sim.Fight(hd, md, DiceMode.Take10, null);
        int wins = 0; long rounds = 0;
        for (int i=0;i<N;i++){ var hh=Clone(hero); var mm=makeMob(); var lg=new List<string>(); if(Sim.Fight(hh,mm,DiceMode.RNG,lg)) wins++; rounds+=lg.Count(x=>x.StartsWith(" -- round")); }
        return (100.0*wins/N, (double)rounds/N, detWin);
    }

    // every monster/size that HAS attacks (noncombat stubs + uninterpreted CR>=1 skip naturally)
    static List<(string label, Func<Combatant> make)> EnumerateFightableMobs() {
        var root = JsonDocument.Parse(File.ReadAllText(Path.Combine(DataDir,"bestiary.json"))).RootElement;
        var list = new List<(string,Func<Combatant>)>();
        foreach (var m in root.GetProperty("monsters").EnumerateArray()) {
            string name = m.GetProperty("name").GetString();
            if (m.TryGetProperty("sizes", out var sizes)) {
                foreach (var sp in sizes.EnumerateObject()) {
                    if (sp.Value.TryGetProperty("attacks", out var a) && a.GetArrayLength() > 0) {
                        string sz = sp.Name; string nm = name;
                        list.Add(($"{name} ({sz})", () => BuildMobFromBestiary(nm, sz, out _)));
                    }
                }
            } else if (m.TryGetProperty("attacks", out var a) && a.GetArrayLength() > 0) {
                string nm = name;
                list.Add((name, () => BuildMobFromBestiary(nm, null, out _)));
            }
        }
        return list;
    }

    // naked variant: innate block, fists only (no looted gear) — the "start of the tutorial" hero
    static Combatant BuildHeroNaked(string path, out string desc) {
        var sheet = JsonDocument.Parse(File.ReadAllText(path)).RootElement;
        var build = sheet.GetProperty("build");
        string name = sheet.GetProperty("identity").GetProperty("name").GetString();
        var dc = build.GetProperty("derivedCache");
        int strMod = build.GetProperty("abilities").GetProperty("mods").GetProperty("str").GetInt32();
        int bab = dc.GetProperty("bab").GetInt32();
        int hp = dc.GetProperty("poolContribution").GetInt32();
        int ac = dc.GetProperty("acNaked").GetInt32();
        var hero = new Combatant { Name = name+"(naked)", MaxHp = hp, Hp = hp, AC = ac };
        hero.Attacks.Add(new Attack { Name = "Fists", ToHit = bab + strMod, Dmg = Combine(DamageExpr.Parse("1d3"), strMod) });
        desc = $"{name} NAKED — HP {hp}, AC {ac}, Fists +{bab+strMod} (1d3+Str{strMod:+0;-0;0})";
        return hero;
    }

    // ── build a Combatant from a resolved sheet: use the INNATE naked block + the looted weapon ──
    static Combatant BuildHeroFromSheet(string path, out string desc) {
        var sheet = JsonDocument.Parse(File.ReadAllText(path)).RootElement;
        var build = sheet.GetProperty("build");
        string name = sheet.GetProperty("identity").GetProperty("name").GetString();
        var dc = build.GetProperty("derivedCache");
        int strMod = build.GetProperty("abilities").GetProperty("mods").GetProperty("str").GetInt32();
        int bab    = dc.GetProperty("bab").GetInt32();
        int hp     = dc.GetProperty("poolContribution").GetInt32();

        // AC + weapon: prefer looted gear (state.lootedLoadout) if present, else naked + fists
        int ac = dc.GetProperty("acNaked").GetInt32();
        string weaponName = "Fists"; DamageExpr wdmg = DamageExpr.Parse("1d3"); int critMult = 2; string gearNote = "naked";
        var state = sheet.GetProperty("state");
        if (state.TryGetProperty("lootedLoadout", out var loot)) {
            var equip = LoadEquip();
            if (loot.TryGetProperty("armor", out var ar) && ar.ValueKind==JsonValueKind.String && equip.TryGetValue(ar.GetString(), out var armor)) {
                int acb = armor.GetProperty("srd").GetProperty("acBonus").GetInt32();
                int maxDex = armor.GetProperty("srd").TryGetProperty("maxDex", out var md)&&md.ValueKind==JsonValueKind.Number ? md.GetInt32() : 99;
                int dexMod = build.GetProperty("abilities").GetProperty("mods").GetProperty("dex").GetInt32();
                ac = 10 + acb + Math.Min(dexMod, maxDex); gearNote = ar.GetString();
            }
            if (loot.TryGetProperty("weapon", out var we) && we.ValueKind==JsonValueKind.String && equip.TryGetValue(we.GetString(), out var wpn)) {
                weaponName = we.GetString();
                string dmgS = wpn.GetProperty("srd").GetProperty("damageMedium").GetString();
                wdmg = DamageExpr.Parse(dmgS);
                string crit = wpn.GetProperty("srd").GetProperty("critical").GetString(); // e.g. "x3" or "19-20/x2"
                var cm = Regex.Match(crit, @"[x×](\d)"); if (cm.Success) critMult = int.Parse(cm.Groups[1].Value);
                gearNote += $" + {weaponName}";
            }
        }
        var hero = new Combatant { Name = name, MaxHp = hp, Hp = hp, AC = ac };
        hero.Attacks.Add(new Attack { Name = weaponName, ToHit = bab + strMod, Dmg = Combine(wdmg, strMod), CritMult = critMult });
        desc = $"{name} — HP {hp}, AC {ac} ({gearNote}), attack {weaponName} +{bab+strMod} ({wdmg}+Str{strMod:+0;-0;0} = {Combine(wdmg,strMod)})";
        return hero;
    }
    // fold the hero's Str mod into the weapon damage expr's flat
    static DamageExpr Combine(DamageExpr d, int strMod) => new DamageExpr { n = d.n, sides = d.sides, flat = d.flat + strMod };

    static Combatant BuildMobFromBestiary(string name, string size, out string desc) {
        var root = JsonDocument.Parse(File.ReadAllText(Path.Combine(DataDir,"bestiary.json"))).RootElement;
        var mob = root.GetProperty("monsters").EnumerateArray().FirstOrDefault(x => x.GetProperty("name").GetString()==name);
        if (mob.ValueKind != JsonValueKind.Object) { desc = $"(mob '{name}' not found)"; return new Combatant{Name=name,MaxHp=1,Hp=1,AC=10}; }
        JsonElement statBlock = mob;
        string label = name;
        if (mob.TryGetProperty("sizes", out var sizes)) { statBlock = sizes.GetProperty(size); label = $"{size} {name}"; }
        int hp = statBlock.GetProperty("hp").GetInt32();
        int ac = statBlock.GetProperty("ac").GetInt32();
        var c = new Combatant { Name = label, MaxHp = hp, Hp = hp, AC = ac };
        var atkDescs = new List<string>();
        foreach (var a in statBlock.GetProperty("attacks").EnumerateArray()) {
            string an = a.GetProperty("name").GetString();
            int th = a.GetProperty("toHit").GetInt32();
            var dmg = DamageExpr.Parse(a.GetProperty("damage").GetString());
            c.Attacks.Add(new Attack { Name = an, ToHit = th, Dmg = dmg });
            atkDescs.Add($"{an} +{th} ({dmg})");
        }
        desc = $"{label} — HP {hp}, AC {ac}, attacks: {string.Join(", ", atkDescs)}";
        return c;
    }

    static Dictionary<string,JsonElement> LoadEquip() {
        var root = JsonDocument.Parse(File.ReadAllText(Path.Combine(DataDir,"equipment.json"))).RootElement;
        var d = new Dictionary<string,JsonElement>();
        foreach (var arr in new[]{"weapons","armor"}) if (root.TryGetProperty(arr, out var a)) foreach (var e in a.EnumerateArray()) d[e.GetProperty("name").GetString()] = e;
        return d;
    }
    static Combatant Clone(Combatant c) => new Combatant { Name=c.Name, MaxHp=c.MaxHp, Hp=c.MaxHp, AC=c.AC, Attacks=c.Attacks };
    static string FindData() {
        foreach (var p in new[]{ Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..","..","..","data")),
                                  Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),"..","..","data")) })
            if (Directory.Exists(p)) return p;
        return null;
    }
}
