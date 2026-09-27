# Mind the Crack — Production Plan and Roadmap

Assumes one-person development (Eren + Claude). Durations are calendar
weeks, not full-time.

## 0. Engine decision

**Recommendation: Unity.**

Rationale: the game's single critical technical requirement is audio–input
synchronisation. Unity's standard solution for this is mature and
documented (`AudioSettings.dspTime`, `AudioSource.PlayScheduled`, DSP
buffer setting). The studio's other games are Capacitor/web (Cengel
Bulmaca, Reefy) and Godot (Little Grand Hotel) — both are wrong for this
game:

- **Capacitor/web:** WebAudio latency in Android WebView varies widely by
  device and cannot be controlled. Ruled out for a rhythm game.
- **Godot:** audio latency control on mobile is not as mature as Unity's,
  and there are few rhythm game examples and little community knowledge.

The cost: the studio has no Unity pipeline — AdMob, Firebase and Play
Console integrations will be set up on the Unity side for the first time.
That means roughly 1 extra week in Phase 3. It is worth it, because a sync
problem cannot be solved with the wrong engine.

## 1. Phases

### Phase 0 — Vertical slice (1–2 weeks) · **gate: is it fun?**

Goal: no coins, art, menus or ads. Only the feeling of "landing on a slab
by pressing on the beat".

- [x] Unity 6.1 (6000.1.17f1) project, `dspTime`-based `Conductor`
      (beat counter, commit delay, offset calculation).
- [x] Greybox sidewalk — but instead of fixed slabs, **the reverse
      generation algorithm was written up front** (`SidewalkGenerator`).
      Pulled forward from Phase 1 because it is the biggest algorithmic
      risk.
- [x] Character = capsule; step = lerp from beat to beat, arc on jumps.
- [x] Input: tap = long step, swipe = jump, timing window tunable in the
      `StepConfig` ScriptableObject.
- [x] Landing evaluation: Perfect / Good / Stumble / Death.
- [x] Debug HUD: offset ms, distance, score, multiplier, streak, step kind.
- [x] Procedural metronome (`ClickTrack`) — no waiting on audio assets.
- [x] Preview markers showing the landing point of all 3 options.
- [x] Generation validation test (10,000 seeds) — `Mind the Crack >
      Validate Generator` menu or `-executeMethod`.
- [ ] URP setup (the project opened via CLI came with the Built-in
      pipeline; materials fall back to the Standard shader for now).
- [ ] On-phone test (APK) and a 10 person playability session.

**Gate criterion:** Eren + at least 3 people play on a phone and do not
put it down for 5 minutes. If they do, the core mechanic is fixed; Phase 1
does not start.

### Phase 1 — Core systems (2–3 weeks)

- [ ] Reverse generation algorithm (design document §3.2) + unit tests:
      every generated segment must be solvable, automated test with 10,000
      seeds.
- [ ] 3 ground types, difficulty curve, BPM ramp and switching between the
      5 BPM loop sets.
- [ ] Music layer system (streak → stem on/off, fade on bar boundaries).
- [ ] Calibration screen + offset storage.
- [ ] 2 obstacles (puddle, dog poop).
- [ ] Death flow + 4 superstition animations (start with placeholder
      animations).
- [ ] Score, multiplier, end-of-run screen.

### Phase 2 — Meta, economy and shell (4–5 weeks) **[EXPANDED]**

A Hooked Inc style upgrade + prestige structure was added to this phase.
Design document §7 and §8.

**2a — Economy model · ✅ DONE (22 Sep 2026)**
- [x] Simulation: `tools/economy_sim.py`, documentation: `docs/economy.md`.
- [x] All four target checks OK: 3 upgrades on day 1 · first move on day 8 ·
      no branch at 10 on day 7 · the sink does not dry up.
- [x] Cost formula `base × 1.90^city × 1.60^level`, branch bases
      50/70/90/120.
- [ ] Export the numbers to a Unity ScriptableObject. **No numbers are
      hardcoded** — `economy.md` is the single source.

**2b — Systems · 2 weeks**
- [ ] Persistent save (local JSON, not `PlayerPrefs`) — the schema includes
      dual currency + prestige multiplier fields from the start.
- [ ] Upgrade tree: 4 branches × 10 levels (Shoes, Luck, Endurance,
      Neighborhood).
- [ ] Applying upgrades to the run — **only the allowed fields** (design
      §8.1 table). No code is written that touches the timing window.
- [ ] Offline income: 2–4 hour cap, extended by the Neighborhood branch.
      No income if the device clock is moved back.
- [ ] Prestige infrastructure: city ID, permanent multiplier, move
      condition check (single city in v1.0, moving UI in v1.1).
- [ ] Cosmetics system: 8 shoes/outfits, unlocked with coins.
- [ ] 3 daily quests, midnight reset (against device clock manipulation:
      no reset if the last seen date goes backwards).
- [ ] Distance milestones and their rewards.

**2c — Shell · 1–2 weeks**
- [ ] Upgrade screen (4 branches, level indicator, cost, "not enough"
      state), offline income welcome screen.
- [ ] Main menu, settings (sound, haptics, offset), privacy links.
- [ ] Localisation infrastructure: EN + TR.

### Phase 3 — Release infrastructure (1–2 weeks)

- [ ] AdMob (Unity plugin) + UMP consent flow. Placements per
      `ADS_POLICY.md`.
- [ ] Firebase Analytics: run_start, run_end (distance, cause of death, max
      multiplier), ad_shown, ad_rewarded, iap_purchase, calibration_offset,
      **upgrade_purchased (branch, level, total coins so far),
      offline_income_claimed, currency_balance (daily snapshot)**.
- [ ] IAP: no ads + starter pack (Google Play Billing).
- [ ] Play Console listing (studio account `yilkgamesstudio@gmail.com`),
      Data Safety form, privacy/account deletion URLs (studio-wide URLs —
      no game-specific page).
- [ ] Firebase API key restriction — as soon as the project is created, not
      postponed.
- [ ] Visual assets in `docs/store-assets-originals/` (gitignored) +
      `pictures/mind-the-crack/` in the `Eren-Ozcan/pictures` repo.

**Note:** the 12 testers / 14 days requirement **does not apply** to this
game — the studio account already has production access (confirmed through
Cengel Bulmaca and Reefy). A closed test will still be run, because that is
the only way to see audio latency across device variety.

### Phase 4 — Testing and measurement (3–4 weeks)

- [ ] Closed test: at least 10 different Android devices, focus on audio
      latency (device matrix and protocol: `test-plan.md` §3).
- [ ] Measurement cohort: ≥ 900 installs, **organic** — no paid campaign.
      The gate depends on sample size, not a date; no retention decision
      until 900 is reached (`test-plan.md` §6).
- [ ] Metrics to read: D1, D7, session length, runs/session, death distance
      histogram, calibration offset distribution, rewarded watch rate,
      **level where players get stuck in the upgrade tree, coin balance
      distribution (piling coins = insufficient sink), offline income
      return rate**.
- [ ] Gate: D1 ≥ 35% **and** D7 ≥ 15%. Below that, no move to v1.1 — if D1
      is low the core is revised, if D7 is low the economy curve is.

### Phase 5 — v1.1 (if the gate is passed, 4–5 weeks)

Istanbul + the first real move (prestige UI), clover, daily challenge
(upgrades normalised), share clip, 2 characters, 2 grounds, 2 obstacles,
coin pack IAP.

## 2. Overall schedule

| Phase | Duration | Cumulative |
| --- | --- | --- |
| 0 — vertical slice | 1–2 weeks | 2 |
| 1 — core | 2–3 weeks | 5 |
| 2 — meta, economy, shell | 4–5 weeks | 10 |
| 3 — release infrastructure | 1–2 weeks | 12 |
| 4 — testing | 3–4 weeks | 16 |

Realistic range for the v1.0 release: **4.5–5 months.**

The upgrade + prestige structure added ~3 weeks to the schedule. Expected in
return: D7 12% → 15%+, ARPDAU 0.05–0.08 → 0.12–0.20 USD. According to the
market data, hybrid casual produces ~5× the ARPDAU of hypercasual — these 3
weeks are the difference between the game being a measurable product or
not.

## 3. Asset list (v1.0)

| Category | Count | Note |
| --- | --- | --- |
| Character model | 1 + 8 cosmetic variants | Low-poly, single rig — **made in-house in Blender** |
| Animation | walk, long step, jump, stumble, 4 deaths | 8 clips |
| Ground | 3 types × 3 variations | Tile based, repeating |
| Obstacle | 2 | Puddle, dog poop |
| Music | 5 BPM × 4 stems | 20 loops, original. Sourcing decision deferred (free/CC0 research, AI if that fails); placeholder metronome in Phases 0–2 |
| SFX | ~15 | Step, ding, streak break, deaths, UI |
| UI | menu, HUD, end of run, store, **upgrade tree**, **offline income**, settings, calibration | 8 screens |
| Upgrade icon | 4 branches × 10 level states | 1 icon per branch + a level frame is enough |
| Store visuals | icon, feature graphic, 6 screenshots, promo video | To the pictures repo. **The ASO/store plan will be done once the game is finished**; only mandatory fields in Phase 3 |

## 4. Decisions made (22 September 2026)

| Topic | Decision | Consequence |
| --- | --- | --- |
| Visual production | **Blender** — character, animation, ground, obstacles in-house | No asset store/freelance budget; the low-poly style suits this production anyway |
| Music | **Left for last.** When its turn comes, free/CC0 stem research first, AI generation if that fails | Phases 0–2 proceed with a placeholder metronome |
| Test budget | **None** — no paid UA campaign | The measurement strategy changed, see `test-plan.md` §6; no A/B testing in v1.0 |
| iOS | **Later** | v1.0 is Android only |
| Store/ASO plan | **After the game is finished** | Only the mandatory listing fields are filled in Phase 3 |
| Economy model | **Built and validated** | `docs/economy.md` + `tools/economy_sim.py`, 4/4 target checks OK |
| First session flow | **Defined** | `docs/onboarding.md` — calibration removed from first launch |
| Test plan | **Written** | `docs/test-plan.md` — 5 layers, device matrix, gates |

The only thing left open: **the music source**, and that was deliberately
deferred to the end of Phase 2.

## 5. Rules for this repository

- Store/marketing visuals are **never committed to this repo**. Local:
  `docs/store-assets-originals/` (gitignored). Permanent: private
  `Eren-Ozcan/pictures` repo under `pictures/mind-the-crack/`.
- Before opening the Google/Play/AdMob/Firebase consoles, the active account
  is verified; the expected account is `yilkgamesstudio@gmail.com`. URLs
  and pitfalls: `C:\Projects\pictures\STUDIO.md`.
- Before adding an ad placement, `C:\Projects\pictures\ADS_POLICY.md` is
  read.
