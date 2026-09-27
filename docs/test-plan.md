# Mind the Crack — Test Plan

Scope: v1.0 (Android). iOS was deferred. **There is no paid test traffic
budget** — this fundamentally changes the measurement strategy, see §6.

## 1. Test layers

| Layer | When | Who | What it answers |
| --- | --- | --- | --- |
| A — Automated tests | Every commit | No CI, manually via `python`/Unity Test Runner | Did generation break, did saving break |
| B — Audio latency matrix | End of Phase 1, end of Phase 3 | Eren | Is the game fair across devices |
| C — Playability session | Phase 0 gate, end of Phase 2 | 8–10 people, sitting next to them | Is it fun, is the tutorial understood |
| D — Closed test (Play Console) | End of Phase 3, 2–3 weeks | 12–30 acquaintances | Crashes, device compatibility, long-term bugs |
| E — Production measurement | Continuously after launch | Organic players | D1/D7, economy curve, ad performance |

## 2. Layer A — Automated tests

These cover more cases than can be tested by hand; if the tests are not
written, the bugs are found in production.

| Test | Method | Pass criterion |
| --- | --- | --- |
| Generation solvability | Generate 10,000 seeds × 3 ground types, verify every segment has at least one valid step sequence | 100% |
| Generation determinism | Same seed twice → byte-for-byte identical segment | 100% (the daily challenge depends on it) |
| Difficulty curve bounds | Are long step/jump ratios within the defined range between 0–2000 m | No deviation |
| Save load/store | Write-read 1,000 random states | Lossless |
| Save backward compatibility | A v1.0 save opens with the next schema | No crash, missing fields fall back to defaults |
| Economy model | `python tools/economy_sim.py` | All 4 target checks OK |
| Offline income exploit | Scenarios with the device clock moved forward/back | Moving it back grants no income, moving it forward does not exceed the cap |
| Upgrade limits | No upgrade can write to the timing window/Perfect tolerance | Code-level test: run parameters do not change |

The last item matters: the design's most critical rule (§8.1) must be a
**test**, not just a document. Otherwise one day someone says "let the Luck
branch widen the window by 10 ms" and the game's fairness quietly dies.

## 3. Layer B — Audio latency matrix

The one existential risk of a rhythm game. On Android device latency ranges
from 0.01 to 0.2 seconds.

### Device matrix (at least 8 devices)

| Class | Example | Why |
| --- | --- | --- |
| Low end, old Android (9–11) | Android Go, 2–3 GB RAM | Worst audio latency is here |
| Mid-range Samsung | A series | Most common in Turkey |
| Mid-range Xiaomi/Redmi | Different MIUI audio path | MIUI has its own audio processing layer |
| Flagship, 120 Hz | High refresh rate | Relation between frame rate and `dspTime` |
| Tablet | 60 Hz, large screen | Layout breakage |
| Bluetooth headphones connected | Any | BT latency 100–300 ms, the disaster scenario |
| Speaker vs wired headphones | Same device, two modes | Offset changes when the output path changes |
| Low battery / thermal throttling | After a long session | Does sync break when frames drop |

### Protocol

1. Test mode is enabled on the device (debug menu): the metronome plays,
   the player taps on the beat 20 times.
2. Recorded: the `dspTime` offset (ms) of every tap, the mean, the standard
   deviation.
3. The same test is repeated with and without headphones.
4. The result is written to a table: `docs/latency-results.md` (device,
   Android version, output path, mean offset, std deviation, date).

### Pass criteria

| Metric | Threshold |
| --- | --- |
| Mean offset after calibration | ≤ 25 ms |
| Standard deviation of the offset | ≤ 30 ms (if this does not improve, the device is inconsistent) |
| Difference between automatic estimate and manual calibration | ≤ 20 ms |
| Playing over Bluetooth | Must be playable after calibration; if not, the game must detect BT and show a warning |

**Gate:** Phase 4 does not start until the criteria are met on at least 7
of the devices in the matrix. If they cannot be met, the order of fixes is:
(1) DSP buffer setting, (2) a Native Audio-like plugin, (3) moving to FMOD.

## 4. Layer C — Playability session

Produces observations, not numbers. 8–10 people, 10 minutes each, sitting
next to them. **No help, no questions, just watching.**

### Phase 0 gate (vertical slice)

One question: is it fun? Criterion: the person does not put it down for 5
minutes and says "one more" unprompted at least once. If 6 out of 10 people
do not show this behaviour, the core mechanic changes and Phase 1 does not
start.

### End of Phase 2 (with the meta)

| To observe | Red flag |
| --- | --- |
| How many seconds until the first tap | > 15 s → the icon is not read |
| How many attempts to get past the tap icon | > 3 → the window is narrow or the icon appears late |
| Did they open the upgrade screen on their own | If never, the meta is not visible |
| Which branch did they buy first | If not Shoes, the price ordering is wrong |
| What did they say/do on death | An "unfair" reaction → generation or latency problem |
| Did they skip the death animation | If everyone skips, the animations are too long |

### 3 questions after the session (only these)

1. How did you figure out what you had to do?
2. When you died, did you understand why?
3. Would you play again? (Watch the hesitation, not the answer.)

## 5. Layer D — Closed test

Play Console closed testing track, 12–30 acquaintances, at least 2 weeks.

**Note:** the 12 testers / 14 days requirement **does not apply** to this
account — the studio has production access. The closed test is run anyway,
because device variety and crash data cannot be collected any other way.

| To track | Source | Threshold |
| --- | --- | --- |
| Crash-free sessions | Crashlytics | ≥ 99.5% |
| ANR rate | Play Console vitals | ≤ 0.47% (Play threshold) |
| Cold start time | Vitals | ≤ 3 s on a mid-range device |
| Excessive battery use | Vitals | No warning |
| Save corruption | Support feedback | 0 cases |
| Economy drift | Daily `currency_balance` snapshot | Within ±40% of the simulation |

The last row is critical: if real players' coin balances drift far from the
simulation (especially upward), the sink is insufficient — cost bases are
updated without waiting for v1.1.

## 6. Layer E — Measurement without a paid budget

**We have to start by accepting reality:** measuring D1 with ±3 point
precision takes a cohort of ~900 installs. A new organic game may not
collect that for weeks. So:

- A "D1 40%" number from a 20–30 person closed test **is not a statistic**,
  it is noise. It is not used as grounds for a decision.
- The retention gates (D1 ≥ 35%, D7 ≥ 15%) remain valid, but **it will take
  time before they become measurable**. The gate is tied to sample size,
  not to a date: no decision before the cohort reaches 900.

### No-budget strategy

1. **First fix the things that need no sample.** Tutorial completion rate,
   time to first tap, crashes, audio latency — these can be read even from
   a 30 person sample, because the effect being looked for is large (either
   90% complete it or 50%).
2. **Ship to production, accumulate.** v1.0 is released on the production
   track, organic installs accumulate. Play Console retention data is
   collected automatically.
3. **Organic accelerators (effort, not money):** the game's own clips on
   TikTok/Reels (death compilations, Perfect streaks), cross-promotion in
   the studio's existing games, a game card on yilkgames.com, Reddit/gaming
   communities.
4. **Read the gate once the cohort threshold is reached.** At 900 installs
   D1/D7 are evaluated and the v1.1 decision is made.
5. **If the budget stance changes:** a one-time 300–500 USD TikTok campaign
   cuts this wait from weeks to days. It is not in the plan right now, but
   the gate criteria stay the same.

### Sample size guide

| Measured | Expected effect | Required sample |
| --- | --- | --- |
| Tutorial completion (90% or 50%) | Very large | 30 |
| Average time to first tap | Large | 50 |
| Crash rate | — | 100+ sessions |
| D1 (±10 points) | Rough | ~100 |
| D1 (±3 points) | For decisions | ~900 |
| A/B difference between two versions (3 points) | Fine | 2,000+/arm — unreachable without budget, no A/B testing |

The last row is a decision: **no A/B testing in v1.0.** A/B without enough
traffic is a wrong decision dressed up as statistics.

## 7. Pre-release checklist

To be gone through manually before every production release:

- [ ] Layer A tests green
- [ ] Audio latency matrix repeated on at least 3 devices (regression)
- [ ] Fresh install flow played end to end (on a device with the save wiped)
- [ ] Launch with an old save tested (the previous version's save)
- [ ] Launch in airplane mode: no ads, game runs, no crash
- [ ] Cancelling a rewarded ad grants no reward, closing it does not freeze
      the game
- [ ] IAP purchase + refund scenario
- [ ] UMP consent screen appears for the EU/Turkey, the game runs after
      declining
- [ ] Playable in silent mode (visual beat ring + haptics)
- [ ] Music and sync recover correctly on an incoming call / when the app
      is sent to the background
- [ ] 10 minute session under low battery / thermal throttling: sync does
      not drift

The last two items are specific to rhythm games and are the most often
skipped.

## 8. Gates summary

| Gate | Criterion | If not passed |
| --- | --- | --- |
| Phase 0 | ≥ 6 out of 10 people do not put it down for 5 min | Core mechanic changes |
| Phase 1 | Generation tests 100%, first latency matrix passed | Engine/audio path revised |
| Phase 2 | Economy simulation 4/4 OK, no playability red flags | Economy or tutorial revised |
| Phase 3 | Pre-release checklist complete | No release |
| Phase 4 | Crash-free ≥ 99.5% · tutorial completion ≥ 90% · latency matrix 7/8 | No move to v1.1 |
| Phase 4 (late) | With cohort ≥ 900: D1 ≥ 35% **and** D7 ≥ 15% | Low D1 → core revised, low D7 → economy revised |
