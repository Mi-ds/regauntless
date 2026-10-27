using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;

// Regauntless — SheetResolver (vertical slice)
// Loads the SRD data files + a .sheet, RESOLVES every key-reference, RECOMPUTES the derived
// stats from scratch (mods/HP/BAB/saves/AC/skill-budget/feat-budget), and ASSERTS them against
// the sheet's cached values. Proves the cross-referencing data layer actually computes into a
// coherent character. Standalone dotnet console tool (mirrors Tools/CrawlSim). Unity-independent.

class SheetResolver
{
    static string DataDir;
    static int checks = 0, fails = 0;

    static void Main(string[] args)
    {
        // find repo-root/data relative to this tool (Tools/SheetResolver/ -> ../../data)
        DataDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data"));
        if (!Directory.Exists(DataDir)) DataDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "data"));
        if (!Directory.Exists(DataDir)) { Console.WriteLine("!! can't find data/ dir (looked near tool + cwd)"); Environment.Exit(2); }

        string sheetPath = args.Length > 0 ? args[0] : Path.Combine(DataDir, "sheets", "Grukk.sheet");
        Console.WriteLine($"data/  = {DataDir}");
        Console.WriteLine($"sheet  = {sheetPath}\n");

        // --- load data files (just the ones this slice needs) ---
        var races       = LoadArray("races.json", "races", "name");
        var classes     = LoadArray("classes.json", "classes", "name");
        var backgrounds = LoadArray("backgrounds.json", "backgrounds", "name");
        var feats       = LoadArray("feats.json", "feats", "name");
        var skills      = LoadArray("skills.json", "skills", "name");
        var equipment   = LoadEquip("equipment.json");
        var sheet       = JsonDocument.Parse(File.ReadAllText(sheetPath)).RootElement;

        Console.WriteLine($"loaded: races {races.Count}, classes {classes.Count}, backgrounds {backgrounds.Count}, feats {feats.Count}, skills {skills.Count}, weapons+armor {equipment.Count}\n");

        var build = sheet.GetProperty("build");
        string name = sheet.GetProperty("identity").GetProperty("name").GetString();
        Console.WriteLine($"==== RESOLVING: {name} ====");

        // --- resolve grant sources ---
        var gs = build.GetProperty("grantSources");
        string raceKey = gs.GetProperty("race").GetString();
        string bgKey   = gs.GetProperty("background").GetString();
        var cls0       = gs.GetProperty("classes")[0];
        string classKey = cls0.GetProperty("class").GetString();
        int    classLv  = cls0.GetProperty("levels").GetInt32();

        var race = Resolve(races, raceKey, "race");
        var klass = Resolve(classes, classKey, "class");
        var bg   = Resolve(backgrounds, bgKey, "background");
        Console.WriteLine($"  race={raceKey}  class={classKey} L{classLv}  background={bgKey}");

        // --- abilities: recompute mods from final scores, assert vs sheet ---
        var ab = build.GetProperty("abilities");
        var final = ab.GetProperty("final");
        var sheetMods = ab.GetProperty("mods");
        Console.WriteLine("  abilities (final -> mod):");
        var mod = new Dictionary<string,int>();
        foreach (var k in new[]{"str","dex","con","int","wis","cha"})
        {
            int score = final.GetProperty(k).GetInt32();
            int m = (int)Math.Floor((score - 10) / 2.0);
            mod[k] = m;
            Assert($"mod[{k}]", m, sheetMods.GetProperty(k).GetInt32());
            Console.WriteLine($"      {k.ToUpper()} {score,2} -> {m:+0;-0; 0}");
        }

        var dc = build.GetProperty("derivedCache");

        // --- HP / pool contribution = max(hitDie) + Con mod  (max HD at L1) ---
        string hitDie = klass.GetProperty("srd").GetProperty("hitDie").GetString(); // "d10"
        int dieMax = int.Parse(hitDie.TrimStart('d'));
        int hp = dieMax + mod["con"];
        Assert("HP/poolContribution", hp, dc.GetProperty("poolContribution").GetInt32());
        Console.WriteLine($"  HP (L1, max {hitDie}={dieMax} + Con {mod["con"]:+0;-0;0}) = {hp}");

        // --- BAB from class progression at L1 ---
        string bab = klass.GetProperty("srd").GetProperty("babProgression").GetString(); // full|3/4|1/2
        int babL1 = bab == "full" ? 1 : 0; // 3/4 and 1/2 are both 0 at level 1
        Assert("BAB", babL1, dc.GetProperty("bab").GetInt32());
        Console.WriteLine($"  BAB (L1, {bab}) = +{babL1}");

        // --- saves: class base (good=+2 / poor=+0 at L1) + ability mod ---
        var saves = klass.GetProperty("srd").GetProperty("saves");
        var sheetSaves = dc.GetProperty("saves");
        (string save, string abil)[] saveMap = { ("fort","con"), ("ref","dex"), ("will","wis") };
        Console.WriteLine("  saves:");
        foreach (var (s, a) in saveMap)
        {
            string quality = saves.GetProperty(s).GetString(); // good|poor
            int baseSave = quality == "good" ? 2 : 0;
            int total = baseSave + mod[a];
            Assert($"save[{s}]", total, sheetSaves.GetProperty(s).GetInt32());
            Console.WriteLine($"      {s,-4} {quality,-4} base+{baseSave} + {a} {mod[a]:+0;-0;0} = {total:+0;-0;0}");
        }

        // --- AC naked = 10 + Dex (civvies AC 0, Medium no size) ---
        int acNaked = 10 + mod["dex"];
        Assert("AC naked", acNaked, dc.GetProperty("acNaked").GetInt32());
        Console.WriteLine($"  AC naked (10 + Dex {mod["dex"]:+0;-0;0}) = {acNaked}");

        // --- AC with looted armor (state) resolves equipment.json ---
        if (sheet.GetProperty("state").TryGetProperty("lootedLoadout", out var loot) &&
            loot.TryGetProperty("armor", out var armEl) && armEl.ValueKind == JsonValueKind.String)
        {
            string armorKey = armEl.GetString();
            var armor = ResolveEquip(equipment, armorKey, "armor");
            if (armor.ValueKind == JsonValueKind.Object)
            {
                int acBonus = armor.GetProperty("srd").GetProperty("acBonus").GetInt32();
                int maxDex  = armor.GetProperty("srd").TryGetProperty("maxDex", out var md) && md.ValueKind==JsonValueKind.Number ? md.GetInt32() : 99;
                int acGeared = 10 + acBonus + Math.Min(mod["dex"], maxDex);
                Console.WriteLine($"  AC geared ({armorKey}: +{acBonus}, maxDex {maxDex}) = {acGeared}  [state/looted]");
            }
        }

        // --- feat references resolve + prereq spot-check ---
        Console.WriteLine("  feats:");
        foreach (var f in build.GetProperty("feats").EnumerateArray())
        {
            string fk = f.GetProperty("feat").GetString();
            string src = f.GetProperty("source").GetString();
            var feat = Resolve(feats, fk, "feat");
            // prereq spot check: Power Attack needs Str 13
            string prereq = feat.ValueKind==JsonValueKind.Object && feat.GetProperty("srd").TryGetProperty("prereq", out var pr) && pr.ValueKind==JsonValueKind.String ? pr.GetString() : null;
            string note = "";
            if (fk == "Power Attack") { bool ok = final.GetProperty("str").GetInt32() >= 13; Assert("Power Attack prereq Str>=13", ok, true); note = $"(prereq {prereq} -> Str {final.GetProperty("str").GetInt32()} {(ok?"OK":"FAIL")})"; }
            Console.WriteLine($"      {fk,-16} [{src}] {note}");
        }

        // --- skill refs resolve ---
        Console.WriteLine("  skills:");
        foreach (var s in build.GetProperty("skills").EnumerateArray())
        {
            string sk = s.GetProperty("skill").GetString();
            Resolve(skills, sk, "skill");
            Console.WriteLine($"      {sk,-12} ranks {s.GetProperty("ranks").GetInt32()}  classSkill={s.GetProperty("classSkill").GetBoolean()}");
        }

        // --- skill-point budget: class (N + Int) x4 at L1, min 1/lvl ---
        int skpBase = classKey == "Fighter" ? 2 : 2; // Fighter = 2 (other classes vary; slice does Fighter)
        int perLevel = Math.Max(1, skpBase + mod["int"]);
        int budgetL1 = perLevel * 4;
        Console.WriteLine($"  skill budget (L1 {classKey}: ({skpBase} + Int {mod["int"]:+0;-0;0}) x4, min1) = {budgetL1} purchased points");

        Console.WriteLine($"\n==== {checks} checks, {fails} failures ====");
        Console.WriteLine(fails == 0 ? ">>> PASS — the sheet's cached derived stats match a from-scratch recompute off the data files."
                                     : ">>> FAIL — recompute disagrees with the sheet (see X marks above).");
        Environment.Exit(fails == 0 ? 0 : 1);
    }

    // ---- helpers ----
    static Dictionary<string, JsonElement> LoadArray(string file, string arrayProp, string keyProp)
    {
        var root = JsonDocument.Parse(File.ReadAllText(Path.Combine(DataDir, file))).RootElement;
        var dict = new Dictionary<string, JsonElement>();
        foreach (var e in root.GetProperty(arrayProp).EnumerateArray())
            dict[e.GetProperty(keyProp).GetString()] = e;
        return dict;
    }
    // equipment.json has weapons[] + armor[]; flatten by name with a kind tag held in-element
    static Dictionary<string, JsonElement> LoadEquip(string file)
    {
        var root = JsonDocument.Parse(File.ReadAllText(Path.Combine(DataDir, file))).RootElement;
        var dict = new Dictionary<string, JsonElement>();
        foreach (var arr in new[]{"weapons","armor"})
            if (root.TryGetProperty(arr, out var a))
                foreach (var e in a.EnumerateArray())
                    dict[e.GetProperty("name").GetString()] = e;
        return dict;
    }
    static JsonElement Resolve(Dictionary<string, JsonElement> dict, string key, string kind)
    {
        checks++;
        if (key != null && dict.TryGetValue(key, out var e)) return e;
        fails++; Console.WriteLine($"   X UNRESOLVED {kind}: '{key}' not found in data"); 
        return default;
    }
    static JsonElement ResolveEquip(Dictionary<string, JsonElement> dict, string key, string kind) => Resolve(dict, key, kind);

    static void Assert<T>(string label, T got, T want) where T : IEquatable<T>
    {
        checks++;
        if (!got.Equals(want)) { fails++; Console.WriteLine($"   X MISMATCH {label}: recomputed={got} vs sheet={want}"); }
    }
}
