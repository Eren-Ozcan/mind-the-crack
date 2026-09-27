# Mind the Crack — Game Design Document (v2)

This document builds on the first draft. Changed sections are marked
**[CHANGED]**; the rationale is in `market-research.md` and the sections
below.

**One sentence:** Walk a character along the sidewalk to the beat of the
music and get as far as possible without stepping on the lines between the
slabs.

**Genre tag (store):** arcade / timing. It will not be tagged as a "rhythm
game" — the rhythm game category belongs to licensed music catalogues, and
there is no visibility to be had there.

**Platform:** v1.0 is Android only. iOS was deferred (decision of
22 Sep 2026) — an Apple Developer account, ATT consent and a separate audio
latency profile are a work item of their own.

**Related documents:** `economy.md` (economy numbers — single source of
truth), `onboarding.md` (first 60 seconds), `test-plan.md`,
`market-research.md`, `roadmap.md`.

---

## 1. Core gameplay

### 1.1 Step model

The character walks automatically. Every music beat is one step. The player
chooses the **type** and **timing** of the step.

| Input | Movement | Distance |
| --- | --- | --- |
| Nothing | Normal step | 1.0 units |
| Tap | Long step | 1.5 units |
| Swipe up | Jump | 2.5 units |

### 1.2 Timing window **[CHANGED — critical]**

In the first draft input timing affected nothing; a player tapping at any
moment between beats got the same result. That makes it a game with music,
not a rhythm game. Fix: input is evaluated against the beat.

| Offset (from the beat) | Result |
| --- | --- |
| ≤ ±60 ms | Perfect timing |
| ≤ ±120 ms | Valid |
| > ±120 ms | Input ignored → a normal step is taken (usually death) |

Windows tighten along the difficulty curve: ±200 ms in the first 3 runs,
the values above afterwards. Values are set through a ScriptableObject on
the Unity side, never hardcoded — these are the numbers that will be tuned
most after testing.

### 1.3 Perfect definition **[CHANGED]**

Perfect = the foot lands in the middle 40% of the slab **and** the input is
within ±60 ms. If either is missing it counts as "Good" (points, no
multiplier).

| Result | Condition | Points |
| --- | --- | --- |
| Perfect | Middle 40% + ≤60 ms | 3 × multiplier |
| Good | On the slab | 1 |
| Stumble | Too close to the line (±5% of the line) | 0, one beat lost |
| Death | Foot on the line | Run ends |

Multiplier: 1× → 1.5× → 2× → 3× based on consecutive Perfects (at 5, 10,
20 Perfects). Resets to 1× when the streak breaks.

**Timing of a step without input** (decision made in Phase 0): when the
player does nothing, the step is taken by the beat itself, so its timing
counts as Perfect by definition. Only a step the player asks for
(tap/swipe) can be mistimed. The alternative — a step without input can
never be Perfect — left the first 6 seconds of the tutorial without reward
and punished the "do nothing" option. To be reopened if Phase 0 testing
brings the feedback "doing nothing feels too safe".

### 1.4 Preview

Where the next 2 steps will land is shown on screen with a shadow (1 step
in the first draft — 2 steps are needed so long step/jump combinations can
be planned). Shadow colour: safe = white, lands on a line = red.

### 1.5 First death softening **[CHANGED]**

Instant death drags D1 down in hypercasual. Every run has 1 "stumble": the
first line contact is not death but a stagger + multiplier reset. The second
one kills. Clover (v1.1) comes on top of this, it does not replace it.

---

## 2. Rhythm system

- Starts at 100 BPM. +6 BPM every 30 seconds, capped at 160 BPM.
- Walking speed is tied to BPM; one step is always exactly one beat.
- Music layers unlock with the Perfect streak: 0–4 streak = drums + bass,
  5–9 = + percussion, 10–19 = + melody, 20+ = + hook. When the streak breaks
  it drops to the lowest layer (not a hard cut, a fade within 1 bar).
- **All music is original.** No licensed songs (cost + store risk).
  Produced as layered stems, all on the same BPM grid.
- **Sourcing decision (22 Sep 2026): left for last.** When its turn comes,
  free/CC0 stem libraries are researched first; failing that, AI generation
  (Suno/Udio-like) is tried. The licence must cover mobile store
  distribution and use in ad creatives (Content ID clean). A placeholder
  metronome + a single loop is enough through Phases 0–2.
- The BPM increase is not done as a tempo change in the music but by
  switching between **loop sets prepared at separate BPMs** (pitch shifting
  degrades quality). 5 sets for 100/115/130/145/160 BPM.

### 2.1 Technical synchronisation (implementation requirement)

- The time source is `AudioSettings.dspTime`, never `Time.time`.
- Music is started with `AudioSource.PlayScheduled`.
- Audio Settings → DSP Buffer Size = **Best latency**; clips are
  **Decompress On Load**.
- Unity UI Button is not used (its callback fires on release, not on
  press); input is read at press time through `Input`/`InputSystem`.
- **Calibration has two stages** (decision changed — a mandatory screen on
  first launch drags D1 down, see `onboarding.md` §4): (1) a silent estimate
  from the first 8 valid inputs of the tutorial run is accepted as the
  default offset; (2) if the estimated offset exceeds 40 ms, calibration is
  offered at the end of the first run. It is always reachable from settings
  and can be changed manually. On Android device latency ranges from
  0.01–0.2 s; the offset is not optional, but the way it is asked for must
  live inside the game.

---

## 3. Ground generation

### 3.1 Ground types (3 of them for v1.0)

| Ground | Slab length | Note | Version |
| --- | --- | --- | --- |
| Square concrete | 1.0 units, fixed | Learning area | v1.0 |
| Paving stone | 0.5 units, fixed | Requires frequent long steps | v1.0 |
| Hexagonal tile | 0.8–1.4 units, variable | Requires reading | v1.0 |
| Broken concrete | variable + extra cracks | Hard | v1.1 |
| Manhole cover | 2.0 unit safe island | Special event | v1.1 |

### 3.2 Generation algorithm **[CHANGED — solvability guarantee]**

Instead of laying slabs out randomly and then checking "is it solvable",
**reverse generation** is used:

1. Pick a step sequence based on difficulty (e.g. `[1.0, 1.5, 1.0, 2.5, 1.0]`).
   Difficulty = long step/jump ratio and number of consecutive hard steps.
2. Take the cumulative sum of the step sequence → exact positions where the
   foot will land.
3. Place slab boundaries (lines) **between** these positions; align them so
   every landing point falls in the middle 40% of a slab.
4. Add filler slabs if the chosen ground type's slab length constraint
   requires it — but never touch the landing points.

This way every sequence has at least one valid solution by definition.

**Solvable is not enough — required is also a must [found in Phase 0].** In
the first implementation, filler slabs placed in wide gaps were laid out at
roughly 1.0 intervals; as a result a player doing nothing drifted along on
top of them. In device testing the character **walked 1000 metres and
scored 1600 points without a single touch**. The intended path was not the
only valid path — it was not even the easiest one.

Rule added: **wherever a step longer than normal is intended, there is a
line at every full 1.0 units from the previous landing point.** So the "do
nothing" option leads straight onto a line. Only on plain normal steps is a
safe line placed in the middle.

Validation (`Mind the Crack > Validate Generator`): a player doing nothing
dies on all 500 seeds, after 4.1 steps on average. At the same time the
intended path stays 100% safe and Perfect-eligible over 3.48 million steps.

**Safe opening:** the first 8 metres are 100% normal steps. With the
difficulty curve starting at 0 metres, a fifth of the first steps required a
long step and a player who had been taught nothing died within 4 steps —
the "the first seconds cannot be lost" requirement of `onboarding.md` §2
starts here.

Difficulty curve: `difficulty = clamp01(distance / 400)` by distance. This
value pulls the long step ratio from 20% → 55% and the jump ratio from
5% → 25%.

---

## 4. Obstacles

| Obstacle | Effect | Version |
| --- | --- | --- |
| Puddle | You slip when you step in it; the next step is a forced long step | v1.0 |
| Dog poop | Instant death (burns the stumble) | v1.0 |
| Dropped ice cream | You stick for one beat (forced wait) | v1.1 |
| Slow-walking auntie | You have to wait one beat | v1.1 |

A forced wait beat is a good idea rhythmically, but it means extra state in
the generation algorithm; moved to v1.1.

---

## 5. Superstition deaths

Stepping on a line triggers one at random, 1–2 s. 4 of them for v1.0:

1. A black cat crosses in front, the character freezes.
2. A flowerpot falls from above.
3. A ladder topples over onto them.
4. A mirror falls out of their pocket and shatters.

The same death is never shown twice in a row. The death animation is
**skippable** (tap the screen) — retry speed is worth more for D1 than the
animation's comedy. Animation length is capped at 1.2 s.

---

## 6. Characters

Characters change gameplay (they are not cosmetic).

| Character | Normal step | Note | Version |
| --- | --- | --- | --- |
| Standard | 1.0 | Starter | v1.0 |
| Long-legged | 1.3 | Comfortable on big slabs, hard on paving | v1.1 |
| Kid | 0.7 | Advantage on dense slabs | v1.1 |
| Dog | 4 legs, 2 landing points | Hard mode, separate generation rule | v1.2 |

v1.0 will only have the standard character + cosmetic shoes/outfits.
Different step lengths require the generation algorithm to be tested per
character — they do not unlock until the meta is ready.

---

## 7. Cities and prestige **[CHANGED — not cosmetic, a progression structure]**

Cities are no longer a theme pack deferred to v1.2; they are **the prestige
loop itself**. When you finish a city you "move": upgrades reset, a
permanent multiplier is earned, and a new sidewalk pattern + music set +
local obstacle unlock.

| City | Sidewalk | Local obstacle | Prestige multiplier | Version |
| --- | --- | --- | --- | --- |
| Starter (generic) | Square concrete, paving, hexagonal | Puddle, dog poop | 1.0× | v1.0 |
| Istanbul | Cobblestone | Street cat, simit vendor stall | 1.5× | v1.1 |
| London | Wet stone | Plenty of puddles, bus stop | 2.25× | v1.2 |
| Tokyo | Narrow regular tiles | Pedestrian flow | 3.4× | v1.2 |
| Paris | Hexagonal tile | Café table | 5.0× | v1.2 |

Move condition: X total metres in that city + a certain level in the
upgrade tree. The multiplier grows exponentially (×1.5), so every move is
noticeably faster than the previous one — the same logic as Hooked Inc's
ocean regions.

v1.0 has a single city, but **the prestige infrastructure is built in
v1.0** (save format, multiplier field, move screen skeleton). Added later,
it would break existing players' economies.

---

## 8. Progression, economy and retention **[CHANGED — Hooked Inc structure]**

The market data is clear: hypercasual without a meta is not a measurable
product in 2026. Hybrid casual ARPDAU is ~5× that of hypercasual. The
structure has three layers:

1. **Run (skill)** — untouchable. Upgrades do not affect core skill.
2. **Upgrade tree** — permanent progression with coins earned in runs.
3. **Prestige (moving)** — §7.

### 8.1 Invariant rule — what upgrades touch and what they do not

In a rhythm game, purchasable accuracy stops it being a skill game and makes
the daily challenge and leaderboard meaningless.

| Upgradable | Not upgradable |
| --- | --- |
| Coin multiplier | Timing window (±60 / ±120 ms) |
| Magnet radius | Slab centre tolerance for Perfect (40%) |
| Head start (first N metres skipped) | BPM ramp |
| Extra stumble (max 2) | Obstacle frequency, generation difficulty curve |
| Clover capacity, passive income | Step distances (1.0 / 1.5 / 2.5) |

In the daily challenge **all upgrades are normalised** — everyone plays
with the same base values. Otherwise the leaderboard measures play time,
not skill.

### 8.2 Upgrade tree — 4 branches × 10 levels

| Branch | Effect | Note |
| --- | --- | --- |
| **Shoes** | Coin multiplier +10%/level | Main economy branch |
| **Luck** | Magnet radius, clover drop chance | Fully unlocked with clover in v1.1 |
| **Endurance** | Head start, extra stumble (lv. 5 and 10) | Noticeable relief for new players |
| **Neighborhood** | Passive (offline) income, chest speed | Reason to come back |

The cost curve is exponential: `cost(n) = base × 1.6^n`. The base differs
per branch. Target curve: the first 3 levels are affordable in the first
session, level 10 is not reachable without prestige (it pushes toward
moving).

### 8.3 Dual currency

| Currency | Source (faucet) | Spend (sink) |
| --- | --- | --- |
| **Coins** (soft) | In-run pickups, end-of-run reward, daily quests, rewarded ad 2× | Upgrade tree, cosmetics |
| **Clover** (hard) | Rewarded ads, daily quests, rare run drops, IAP | Death forgiveness, upgrade speed-up |

**Rule:** every faucet must have a sink. When there is nothing left to
unlock, coins become meaningless and the end-of-run reward stops feeling
rewarding.

Clover arrives in v1.1, but **the economy is designed for dual currency in
v1.0**; adding a second currency later breaks the whole price balance.

### 8.4 Offline income (limited idle)

Setup: NPCs wearing the cosmetics you unlocked walk the sidewalk while you
are away and collect a small amount of coins. **Cap 2–4 hours** (extended
by the Neighborhood branch). The goal is not to turn the game into an idle
game but to create a reason to return. The game itself stays a skill game.

### 8.5 In v1.0

- Coins, upgrade tree (4 branches, 10 levels), offline income, cosmetics.
- 3 daily quests, midnight reset.
- Distance milestones (100/250/500/1000 m) — ≥2 should unlock in the first
  session.
- Prestige infrastructure (single city, moving UI in v1.1).
- Clover, daily challenge + leaderboard, share clip → v1.1.

### 8.6 Economy balancing — a separate work item

The most underestimated job in a one-person team. Numbers are set up in a
table (Google Sheets / CSV), not in code, and exported to Unity as a
ScriptableObject. To be modelled: average run length × coins per run ×
session count → which level is reached on which day. Target curve: 3–4
upgrades on day 1, first move on day 7.

### 8.7 Daily challenge (v1.1)

Same seed for everyone, one attempt, upgrades normalised. Seed = date; since
the generation algorithm is deterministic no server is needed, only a
leaderboard (Firebase).
---

## 9. Shareability (v1.1)

The last 5 seconds before death are recorded and shared with one tap. Funny
text on the score screen: "Protected my mother's back for 347 metres." Texts
are picked from a pool by distance, 20 variants.

Note: screen recording with Unity's `Recorder` package is heavy on mobile;
alternatively, replaying and recording the last 5 seconds from an **input
recording** is cheaper. Decided in v1.1 by measuring a prototype.

---

## 10. Monetisation

Detailed rationale in `market-research.md` §5.

| Placement | Rule | Version |
| --- | --- | --- |
| Rewarded — continue | On death, once per session | v1.0 |
| Rewarded — double coins | End-of-run screen | v1.0 |
| Interstitial | Every 3–4 deaths; **never in the first 3 runs**; 90 s cooldown | v1.0 |
| No ads + starter pack | One-time ~4.99 USD, no subscription | v1.0 |
| Rewarded — double offline income | On returning to the game, twice a day | v1.0 |
| Rewarded — upgrade discount | Next upgrade 30% cheaper, once a day | v1.0 |
| Coin pack IAP | Coin + clover packs | v1.1 |

The upgrade tree creates three new rewarded ad placements (offline income
2×, upgrade discount, end-of-run coins 2×). These are the real ARPDAU lever:
the player sees the ad not as a punishment but as a tool that speeds up
progression. The total number of rewarded placements must not exceed 5 —
beyond that the game turns into ad clicking and the `ADS_POLICY.md` limits
are strained.

The studio's binding ad rules in `C:\Projects\pictures\ADS_POLICY.md` must
be read — the table here cannot contradict it.

---

## 11. Visual and audio direction

- Camera: behind and above, ~35° tilt. Both the character and the 4–5
  slabs ahead of them must be readable.
- Style: low-poly, pastel palette. **Readability above all**: the lines
  between slabs are the darkest value in the palette, the slab surface the
  lightest. No colour-blind mode is needed because the distinction is
  contrast, not colour.
- Audio: a click on every step that sits on the beat, a "ding" on Perfect,
  a short drop sound when the streak breaks, a comic effect on death.
- Haptics: light on Perfect, strong on death. Can be turned off in settings.

---

## 12. Release plan **[CHANGED — meta brought forward]**

| Version | Content | Target |
| --- | --- | --- |
| **v0.1 vertical slice** | Grey boxes, single ground, metronome, no death, no meta | The answer to "is it fun?" |
| **v1.0 (MVP)** | 3 grounds, 2 obstacles, 4 deaths, standard character, calibration · **upgrade tree (4 branches × 10 lv.), dual currency, offline income, prestige infrastructure** · daily quests, milestones · all ad placements | D1 ≥ 35%, D7 ≥ 15% |
| **v1.1** | Istanbul (first real move) + moving UI, clover, daily challenge, share clip, 2 characters, 2 grounds, 2 obstacles | D7 ≥ 20%, ARPDAU ≥ 0.12 USD |
| **v1.2** | London/Tokyo/Paris, dog character, skin store, seasonal event | LiveOps |

**The D7 target went up** (12% → 15%) because v1.0 now ships with the
upgrade tree and offline income; with a meta, the D7 expectation rises too.
The hybrid casual band is 15–22%.

Before moving on to v1.1, v1.0 data is reviewed: D1, D7, session length,
runs/session, **at which level players get stuck in the upgrade tree**,
return rate for offline income, rewarded ad watch rate, a histogram of the
distance at which players die.
