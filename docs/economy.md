# Mind the Crack — Economy Model

This file is the **single source of truth** for economy numbers. On the Unity
side these values are copied into a ScriptableObject; no numbers are
hardcoded.

The model is runnable: `python tools/economy_sim.py`. Before changing a
number, run the simulation first; a value is not accepted until all four
target checks report OK.

## 1. Target curve and validation status

| Target | Status |
| --- | --- |
| 3–4 upgrades affordable on day 1 | ✅ 3 |
| First move possible around day 7 | ✅ day 8 |
| No branch at level 10 on day 7 | ✅ none |
| Sink must not dry up (at 40/40 levels with no move, coins pile up) | ✅ none |

Move days (30-day simulation): **8, 13, 17, 22, 28** — gaps of 5, 4, 5, 6
days. The gap widening over time is intended: each city takes a little
longer than the previous one, so the game does not run dry in the long term.

## 2. Cost formula

```
cost(branch, level, city) = base[branch] × 1.90^city × 1.60^level
```

- `level` is 0-based (the first purchase costs the level 0 price).
- `city` is the move count (0 = starting city).
- **1.90 > 1.50** (prestige multiplier) is deliberate: cost grows faster
  than income, otherwise every move takes less time than the one before and
  the game becomes meaningless within 3 weeks.

### Base costs and first-city table

| Branch | Base | lv1 | lv2 | lv3 | lv4 | lv5 | lv6 | lv7 | lv8 | lv9 | lv10 | Total |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Shoes | 50 | 50 | 80 | 128 | 205 | 328 | 524 | 839 | 1342 | 2147 | 3436 | 9,079 |
| Endurance | 70 | 70 | 112 | 179 | 287 | 459 | 734 | 1174 | 1879 | 3006 | 4810 | 12,710 |
| Neighborhood | 90 | 90 | 144 | 230 | 369 | 590 | 944 | 1510 | 2416 | 3865 | 6185 | 16,343 |
| Luck | 120 | 120 | 192 | 307 | 492 | 786 | 1258 | 2013 | 3221 | 5154 | 8246 | 21,789 |

All 40 levels in the first city: **59,921 coins**.

The base ordering is not arbitrary: Shoes is cheapest because the coin
multiplier is the main economy engine — it should be the first thing the
player buys. Luck is most expensive because it is the weakest branch in
v1.0 (clover unlocks in v1.1).

## 3. Branch effects

| Branch | Effect per level | At level 10 |
| --- | --- | --- |
| **Shoes** | Coin multiplier +10% | ×2.0 |
| **Endurance** | Head start +12 m | +120 m; extra stumble at lv5 and lv10 |
| **Neighborhood** | Offline +6 coins/hour, cap +0.4 hours | 60 coins/hour, 6 hour cap |
| **Luck** | Magnet → pickup efficiency +4% | +40%; clover drop chance in v1.1 |

Multipliers combine multiplicatively: `coin multiplier = (1 + 0.10×Shoes) ×
(1 + 0.04×Luck) × prestige multiplier`.

**None of them touch the timing window, Perfect tolerance, BPM ramp or
step distances** — the invariant rule in the design document §8.1.

## 4. Run economy

| Variable | Value | Note |
| --- | --- | --- |
| Collected on the sidewalk | 0.22 coins/metre | Roughly one coin every 4–5 metres |
| End-of-run bonus | 0.08 coins/metre | 2× with a rewarded ad |
| Rewarded 2× watch rate | 35% | Assumption — to be measured and updated in Phase 4 |
| Offline claims | 2 per day | Cap depends on Neighborhood level |

A new player earns roughly **23 coins** from a 70 metre run. Day 1
(2 sessions × 5 runs) ~230 coins → 3 upgrades.

## 5. Prestige (moving)

- **Condition:** 28/40 total levels. **No** distance requirement (rationale in §7).
- **Reward:** permanent coin multiplier ×1.5 (compounding: 1.5 → 2.25 → 3.38 → …).
- **Reset:** all upgrade levels, current coins.
- **Kept:** cosmetics, best distance, daily quest progress, prestige
  multiplier.
- **Unlocked:** new city — sidewalk pattern, music set, local obstacle.

On the first day after a move, 9–12 upgrades are bought at once. This is
intentional: in prestige games the first hour after a move is the most
satisfying moment. The ×1.90 growth of the cost base brings the pace back
to normal from the second day on.

## 6. Currencies and sink balance

| Currency | Source (faucet) | Spend (sink) |
| --- | --- | --- |
| **Coins** | In-run pickups, end-of-run bonus, daily quests, offline income, rewarded 2× | Upgrade tree (main sink), cosmetics |
| **Clover** (v1.1) | Rewarded ads, daily quests, rare run drops, IAP | Death forgiveness, upgrade speed-up |

### Cosmetic prices (v1.0, 8 items — secondary sink)

| Item | Price |
| --- | --- |
| Shoes × 3 | 250 / 900 / 2,500 |
| Outfit × 3 | 400 / 1,400 / 3,500 |
| Hat × 2 | 1,800 / 6,000 |

Total 16,750 coins. Deliberately small next to the upgrade tree: cosmetics
are not the main sink, just a side goal that does not delay upgrades.

## 7. Rejected variants (do not retry)

**Distance-gated prestige** — "walk 12,000 m total in that city + 20
levels" was tried. Result: the player reaches 40/40 levels and waits on the
distance requirement with nothing left to spend on; on days 19–22 the
balance piled up to 145,000 coins with every upgrade maxed. **The sink
dried up.** The prestige condition was tied to levels only.

**City cost multiplier 1.45** — being smaller than the prestige multiplier
(1.5), every move took less time than the one before (gaps 5 → 4 → 3 → 3
days) and the multiplier shot up to 11.4× by day 30. Raised to 1.90.

**Prestige threshold 30/40 levels** — the first move slipped to day 9,
outside the 7-day target. Lowered to 28.

## 8. Known limit of v1.0

v1.0 has a single city, so **no moving**. The simulation (prestige off)
shows 40/40 levels reached on **day 15**. From that point the upgrade sink
is exhausted, leaving cosmetics and score chasing.

This is acceptable for v1.0: v1.0 is a measurement release and the window
to measure is D1–D7. But **v1.1 (Istanbul + moving) must ship within two
weeks**, otherwise early players are left in an exhausted game.

## 9. Assumptions — to be measured and updated in Phase 4

These numbers were set before the game existed; all of them are hypotheses:

| Assumption | Value | How it is measured |
| --- | --- | --- |
| Average day 1 run distance | 70 m | `run_end.distance` histogram |
| Skill growth | +18% per day, capped at 650 m | Average distance per day |
| Sessions/day | 2 on day 1, then 3 | Firebase session count |
| Runs/session | 5 | `run_start` / session |
| Rewarded 2× watch rate | 35% | `ad_rewarded` / `run_end` |
| Offline claims/day | 2 | `offline_income_claimed` |

When real data arrives, the constants at the top of `tools/economy_sim.py`
are updated, the four target checks are rerun, and cost bases are retuned
if needed.
