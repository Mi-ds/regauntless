using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// ───────────────────────────────────────────────────────────────────────────
// Regauntless — headless crawl simulator (first cut).
// SRD 3.5-accurate stats. Pluggable dice. Weapon-die sweep. Instrumented.
// Pure C#, no engine. "Run the numbers, read the report."
//   Run it:  cd Tools/CrawlSim && dotnet run -c Release
// Streams live progress so you can watch it chunk through the runs.
// ───────────────────────────────────────────────────────────────────────────

enum DiceMode { Take10, Min, Max, RNG }

static class Dice {
    public static Random rng = new Random(12345);
    public static int Roll(int sides, DiceMode m) => m switch {
        DiceMode.Min => 1, DiceMode.Max => sides,
        DiceMode.Take10 => (int)Math.Floor((sides + 1) / 2.0),   // per-die avg, floored (combat.md)
        DiceMode.RNG => rng.Next(1, sides + 1), _ => 1 };
    public static int D20(DiceMode m) => m switch {
        DiceMode.Min => 1, DiceMode.Max => 20, DiceMode.Take10 => 10, DiceMode.RNG => rng.Next(1,21), _ => 10 };
}

class Combatant {
    public string Name; public int MaxHp, Hp, AC, Bab, StrMod, WeaponDie;
    public int CritRange = 20, CritMult = 2;
    public bool Alive => Hp > 0;
}

static class Sim {
    public static int Attack(Combatant a, Combatant d, DiceMode m, List<string> log) {
        int nat = Dice.D20(m);
        int atk = nat + a.Bab + a.StrMod;

        // nat-1 auto-miss, nat-20 auto-hit, and crit THREAT are ROLLING phenomena — they exist
        // only when you actually roll a d20. The deterministic modes (Take10/Min/Max) do NOT
        // roll, so they cannot crit or crit-fail; they are a plain (value + mods) vs AC compare.
        bool isRoll = (m == DiceMode.RNG);
        bool autoMiss = isRoll && nat == 1;
        bool autoHit  = isRoll && nat == 20;
        bool threat   = isRoll && nat >= a.CritRange;   // crit threat only when rolling

        bool hit = !autoMiss && (autoHit || atk >= d.AC);
        if (hit) {
            int dmg = Dice.Roll(a.WeaponDie, m) + a.StrMod; if (dmg < 1) dmg = 1;
            if (threat) for (int i = 1; i < a.CritMult; i++) dmg += Math.Max(1, Dice.Roll(a.WeaponDie, m) + a.StrMod);
            d.Hp -= dmg;
            log?.Add($"  {a.Name} hits {d.Name} (atk {atk} vs AC {d.AC}) {dmg}{(threat ? " CRIT" : "")} -> {Math.Max(0, d.Hp)}/{d.MaxHp}");
            return dmg;
        }
        log?.Add($"  {a.Name} MISS vs {d.Name} (atk {atk} vs AC {d.AC})");
        return 0;
    }
    public static bool Fight(Combatant hero, List<Combatant> enemies, DiceMode heroMode, List<string> log) {
        // DIFFICULTY is player-favoring (combat.md): the mode tilts the HERO's dice
        // (Easy=Max, Normal=Take10, Hard=Min). ENEMIES are the fixed yardstick — they always
        // resolve at the Normal/Take-10 baseline. Difficulty tilts the player, NOT the monsters.
        // (Gremlin/RNG is the exception: when the hero rolls live, enemies roll live too.)
        DiceMode enemyMode = (heroMode == DiceMode.RNG) ? DiceMode.RNG : DiceMode.Take10;

        int round = 0;
        const int RoundCap = 100;   // stalemate safety net (a true both-can't-hit case shouldn't hang)
        while (hero.Alive && enemies.Any(e => e.Alive) && round < RoundCap) {
            round++; log?.Add($" -- round {round} --");
            var t = enemies.First(e => e.Alive); Attack(hero, t, heroMode, log);
            foreach (var e in enemies.Where(e => e.Alive)) if (hero.Alive) Attack(e, hero, enemyMode, log);
        }
        if (round >= RoundCap) log?.Add("  [STALEMATE — hit round cap]");
        return hero.Alive && !enemies.Any(e => e.Alive);
    }
}

class Program {
    // SRD-accurate-ish stat factories (first cut; refine as needed)
    static Combatant Fighter(int wd, int crR = 20, int crM = 2) => new Combatant {
        Name = $"Fighter(d{wd})", MaxHp = 12, Hp = 12, AC = 16, Bab = 1, StrMod = 4,   // 18 Str -> +4
        WeaponDie = wd, CritRange = crR, CritMult = crM };
    static Combatant Goblin() => new Combatant {
        Name = "Goblin", MaxHp = 5, Hp = 5, AC = 15, Bab = 1, StrMod = 0, WeaponDie = 6 };  // morningstar 1d6
    static Combatant Commoner() => new Combatant {
        Name = "Villager", MaxHp = 2, Hp = 2, AC = 10, Bab = 0, StrMod = 0, WeaponDie = 4 };

    static void P(string s) { Console.WriteLine(s); Console.Out.Flush(); _file?.Add(s); }
    static List<string> _file;

    static void Main() {
        _file = new List<string>();
        P("=== REGAUNTLESS CRAWL SIM — first cut (SRD 3.5) ===");
        P("[boot] stats: Fighter(18 Str, d10 HD, AC16), Goblin(5hp AC15), Commoner(2hp)\n");

        // ── MODE 0: murder hobos (sanity + a readable play-by-play) ──
        P("### MODE 0: Murder Hobos — Fighter d8 longsword vs 8 villagers (take-10) ###");
        {
            var f = Fighter(8, 19); var v = Enumerable.Range(0, 8).Select(_ => Commoner()).ToList();
            var log = new List<string>(); Sim.Fight(f, v, DiceMode.Take10, log);
            foreach (var l in log.Take(24)) P(l);
            P($"  => fighter hp {f.Hp}/{f.MaxHp}, 8 villagers down.\n");
        }

        int[] dice = { 4, 6, 8, 10, 12 }; string[] nm = { "dagger", "shortsword", "longsword", "greatclub", "greataxe" };
        int[] crR = { 19, 19, 19, 20, 20 }; int[] crM = { 2, 2, 2, 2, 3 }; int gang = 3; int N = 2000;

        P($"### MODE 1: L1 Fighter (18 Str) vs {gang} goblins — weapon sweep, {N} RNG runs/case ###\n");

        var results = new (string nm, bool t10, bool mn, bool mx, double winpct, double rounds)[dice.Length];
        var swRNG = Stopwatch.StartNew();
        for (int w = 0; w < dice.Length; w++) {
            bool t10 = Sim.Fight(Fighter(dice[w], crR[w], crM[w]), Enumerable.Range(0, gang).Select(_ => Goblin()).ToList(), DiceMode.Take10, null);
            bool mn  = Sim.Fight(Fighter(dice[w], crR[w], crM[w]), Enumerable.Range(0, gang).Select(_ => Goblin()).ToList(), DiceMode.Min, null);
            bool mx  = Sim.Fight(Fighter(dice[w], crR[w], crM[w]), Enumerable.Range(0, gang).Select(_ => Goblin()).ToList(), DiceMode.Max, null);
            int wins = 0; long rounds = 0;
            for (int i = 0; i < N; i++) {
                var f = Fighter(dice[w], crR[w], crM[w]); var g = Enumerable.Range(0, gang).Select(_ => Goblin()).ToList();
                var log = new List<string>(); if (Sim.Fight(f, g, DiceMode.RNG, log)) wins++;
                rounds += log.Count(l => l.StartsWith(" -- round"));
            }
            results[w] = (nm[w], t10, mn, mx, 100.0 * wins / N, (double)rounds / N);
            P($"  [{nm[w]} d{dice[w]}] done — RNG win {100.0 * wins / N:F1}%");   // one clean line per weapon case
        }
        swRNG.Stop();

        P("\n  RESULTS — difficulty = player-favoring (Easy=player maxes, Normal=take-10, Hard=player mins;");
        P("            enemies fixed at Normal). Gremlin = both roll live (" + N + " runs).");
        P($"  {"weapon",-14}{"Easy",-7}{"Normal",-8}{"Hard",-7}{"Gremlin%",-10}{"avgRounds",-10}");
        foreach (var r in results)
            P($"  {r.nm,-14}{(r.mx ? "WIN" : "lose"),-7}{(r.t10 ? "WIN" : "lose"),-8}{(r.mn ? "WIN" : "lose"),-7}{r.winpct,-10:F1}{r.rounds,-10:F1}");

        P("\n  [timing] deterministic pass...");
        var swDet = Stopwatch.StartNew();
        for (int w = 0; w < dice.Length; w++) for (int i = 0; i < N; i++) {
            var f = Fighter(dice[w], crR[w], crM[w]); var g = Enumerable.Range(0, gang).Select(_ => Goblin()).ToList();
            Sim.Fight(f, g, DiceMode.Take10, null);
        }
        swDet.Stop();

        P($"\n### COMPUTE COST ({dice.Length * N} fights/pass) ###");
        P($"  take-10 (deterministic): {swDet.Elapsed.TotalMilliseconds:F1} ms");
        P($"  RNG (live rolls+logs):   {swRNG.Elapsed.TotalMilliseconds:F1} ms");
        double perUs = swDet.Elapsed.TotalMilliseconds / (dice.Length * N) * 1000.0;
        P($"  per-fight (deterministic): {perUs:F2} us  =>  a 1000-fight crawl ~ {perUs:F1} ms combat compute");
        P("  => combat math is NOT the perf bottleneck even on a budget phone.\n");
        P("[done] — change N at top of Program.cs to scale.");

        // always write a report file so results survive terminal weirdness
        try { System.IO.File.WriteAllLines("/tmp/sim_report.txt", _file); P("[report written to /tmp/sim_report.txt]"); }
        catch { }
    }
}
