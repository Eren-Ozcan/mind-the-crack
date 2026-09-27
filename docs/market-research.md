# Mind the Crack — Market Research

Date: 22 September 2026. Sources at the end of the file.

## 1. Summary — one paragraph for the decision

The pure hypercasual model (single mechanic + 100% ad revenue) no longer
works in 2026: user acquisition cost grew faster than ad revenue, and
publishers that built the category, like Voodoo, have left this model
entirely. By contrast, "hybrid casual" (hypercasual hook +
progression/economy/meta) is the growing side. Mind the Crack's current
design already sits on the right side (coins, character unlocks, daily
quests, daily challenge) — but these were deferred to v1.1/v1.2. **The main
output of this research: part of the meta layer must be pulled into the
MVP**, otherwise v1.0 will not be a measurable product; it will only be a
prototype.

Second main output: the game should not be positioned as a "rhythm game".
Roughly 82.5% of the rhythm game market is in Japan, and the competitive
advantage of the winners in the West (Beatstar, Magic Tiles) is a licensed
music catalogue — a space a one-person studio cannot enter. In Mind the
Crack music is the **feel** layer, not a catalogue. Store positioning should
be "timing/reflex + humour", not "rhythm".

## 2. Market state (2026)

| Indicator | Value | Meaning |
| --- | --- | --- |
| Blended game CPI | 0.56 USD (+30% yearly), North America 1.68 USD | Scaling hypercasual with paid UA is now expensive |
| Casual Android rewarded ad eCPM | 3.60 (2023 H1) → 3.25 (2024 H1) → 3.02 USD (2025 H1), recovering since 2025 | Ad revenue flat; CPI rose faster |
| Hybrid casual IAP revenue | 4.2 billion USD in 2025, +20% | The only growing casual subsegment |
| Hypercasual install volume | Still the only growing segment according to Sensor Tower | Downloads yes, revenue no — careful |
| Rhythm games worldwide revenue | ~1 billion USD, ~825 million of it in Japan | In the West rhythm = small, licence-locked market |

In March 2026 Voodoo's CEO said "the model of offsetting the decline of old
games by releasing new ones is a thing of the past" and put the decline at
up to 40% a year. Azur Games' H1 2026 report points the same way: install
volume and revenue have decoupled, and two games in the same subgenre can
differ in revenue many times over depending on monetisation strategy.

**What it means for us:** the goal is not "buy 100k cheap installs, make
money from ads". The goal is to measure D1/D7 with organic + low-budget
test traffic and find out whether the game sticks; if it does, deepen the
meta.

## 3. Competitor analysis

### 3.1 Direct thematic competitor — Steppy Pants (Halfbrick / Super Entertainment)

The same superstition premise: walk the sidewalk, do not step on the lines.
Endless walking game, physics comedy, one-touch control, character
cosmetics, 6 modes, a weekly 7.99 USD VIP subscription. A long-lived,
well-known game.

**What our difference should be:** in Steppy Pants the feel is "drunk leg
physics" — control is intentionally slippery and funny. In Mind the Crack
the feel should be "metronome": crisp control, fair punishment, mistakes
are the player's reading mistakes. So the same premise, the opposite feel.
That is already enough differentiation, but it has to be readable in the
store visuals and in the first 5 seconds (on-screen beat indicator, foot
shadow on the slabs).

### 3.2 Rhythm side competitors

| Game | Owner | Model | Lesson for us |
| --- | --- | --- | --- |
| Magic Tiles 3 | Amanotes | Huge song catalogue, ~10M downloads a month, ~500M lifetime | Do not enter the catalogue war |
| Beatstar | Space Ape | Licensed hit songs, highest-earning rhythm game in the West, ~30M downloads | Revenue is in the licence, not the mechanic |
| Dancing Line | Cheetah/BoomBit | Level based, collect gems → unlock skins | Cosmetic economy is simple and works |
| Beat Blade: Dash Dance | — | One-time ~9.99 USD bundle: no ads + unlimited revives + all songs | One-time "everything" bundles are common in rhythm games |

Because of licensing costs **all music must be original/procedural**. This
is an advantage, not a constraint: changing BPM with gameplay and adding
layers on a Perfect streak is only possible with music we produce
ourselves. With a licensed song the music is fixed and the gameplay has to
fit it.

## 4. Numbers to target (benchmarks)

These are our definition of "success"; test results will be read against
them.

| Metric | Market median | Our passing grade | Note |
| --- | --- | --- | --- |
| D1 retention | ~22% (median across all games) | ≥ 35% | ~40% is expected if going to a publisher |
| D7 retention | hybrid casual 15–22% | ≥ 15% (v1.0), ≥ 20% (v1.1) | v1.0 now ships with the upgrade tree, so the expectation went up |
| Session length | — | ≥ 4 min (4–6 runs per session) | Consistent with the design's 30–90 s/run |
| CPI (test) | casual/puzzle US Android 1.50–3.50 USD | — | **No paid UA** (decision of 22 Sep 2026); CPI only matters if a budget is set aside later |
| Measurement cohort | — | ≥ 900 installs | Minimum for D1 ±3 points at 95% confidence. Will accumulate organically; see `test-plan.md` §6 |
| Crash-free sessions | — | ≥ 99.5% | Play Console vitals |

The 900 install cohort requirement matters: a D1 number from a 100–200
person test is noise, and the design is not changed based on it.

## 5. Monetisation plan (consistent with market data)

A pure ad model is not enough. Layers:

1. **Rewarded ad — continue.** On death, continue with clover, once per
   session. The highest eCPM format in casual, and a gift rather than a
   punishment for the player.
2. **Rewarded ad — coins/cosmetics.** "Double coins" at the end of the run.
   A second rewarded placement raises ARPDAU without IAP pressure.
3. **Interstitial.** Every 3–4 deaths, never in the first 3 runs. Showing
   ads in the first session is the most common D1 killer.
4. **One-time "No ads + starter pack"** ~4.99 USD. The little sibling of
   Beat Blade's 9.99 USD "everything" bundle. No subscription — Steppy
   Pants' weekly 7.99 VIP is not for us; it carries refund/cancellation
   overhead and store risk.
5. **Cosmetics + characters.** Unlocked with coins, sped up with IAP.

Mediation: the studio is already on AdMob, and AdMob is still the
recommended starting point for indies in 2026. If scale comes (above ~50k
impressions a day), an AppLovin MAX comparison is done; not now.

## 6. Main risks

| Risk | Impact | Mitigation |
| --- | --- | --- |
| Android audio latency (0.01–0.2 s by device) breaks the rhythm | The game feels "unfair", D1 collapses | `AudioSettings.dspTime` + `PlayScheduled`, DSP buffer = Best latency, calibration screen on first launch, manual offset in settings |
| Rhythm + placement together are hard, exceeding the hypercasual threshold | Steep learning curve, early quitting | Very wide timing window and no death (stumble) in the first 3 runs, tightening afterwards |
| Steppy Pants' shadow | Perceived as a "copy" | The difference in feel (crisp control, rhythm, punishment animations) must read in the first screenshot |
| Procedural generation produces unsolvable sequences | Unfair death | Generation runs in reverse: pick a valid step sequence first, lay the slabs out around it (see design document) |
| The superstition theme is culture-bound | The premise is not understood outside the West | The premise should lean on visuals, not text: step on a line and a flowerpot falls — no explanation needed |
| Revenue expectation without ads | The revenue target is missed | The v1.0 target is retention, not revenue; revenue is discussed after v1.1 |

## Sources

- [Hypercasual and hybrid casual in 2026 full report — Azur Games](https://azurgames.com/blog/hypercasual-and-hybrid-casual-in-2026-full-report/)
- [State of Mobile 2026: 9 Key Trends — Deconstructor of Fun](https://www.deconstructoroffun.com/blog/2026/2/2/state-of-mobile-2026)
- [Mobile Gaming's Shift from Hyper to Hybrid-Casual — Unity](https://unity.com/blog/mobile-gaming-shift-hyper-hybrid-casual)
- [What happened to hypercasual? — PocketGamer.biz](https://www.pocketgamer.biz/what-happened-to-hypercasual-the-markets-evolution-over-the-past-year/)
- [Mobile Game KPIs 2026: 20 Benchmarks — Game Growth Advisor](https://gamegrowthadvisor.com/blog/2026-03-17-mobile-game-kpis-benchmarks-2026/)
- [Mobile Game Retention Guide 2026 — Game Growth Advisor](https://gamegrowthadvisor.com/blog/2026-03-17-mobile-game-retention-strategies-2026/)
- [Hybrid-Casual Monetization — Playio](https://blog.playio.co/hybrid-casual-games-monetization)
- [Beatstar: The Western World Finds its Rhythm — Naavik](https://naavik.co/deep-dives/beatstar-west-finds-rhythm/)
- [Steppy Pants — App Store](https://apps.apple.com/us/app/steppy-pants/id1094138419)
- [Beat Blade: Dash Dance guide — Pocket Tactics](https://www.pockettactics.com/beat-blade-dash-dance/guide)
- [Rhythm Game Crash Course: syncing with dspTime — Native Audio](https://exceed7.com/native-audio/rhythm-game-crash-course/dsp-sync.html)
- [Coding to the Beat — Game Developer](https://www.gamedeveloper.com/audio/coding-to-the-beat---under-the-hood-of-a-rhythm-game-in-unity)
- [ironSource vs AppLovin MAX vs AdMob 2026 — UndrAds](https://undrads.com/blogs/ironsource-vs-applovin-max-vs-admob)

---

# Second round of research (22 September 2026)

## 7. Economy and ARPDAU

| Segment | ARPDAU range |
| --- | --- |
| Ads only, casual | 0.01–0.05 USD |
| Hypercasual | 0.03–0.08 USD |
| Hybrid casual | 0.15–0.50 USD |

Hybrid casual produces roughly **5×** the ARPDAU of hypercasual; the reason
is opening two revenue paths (ads + purchases) from a single player. The IAP
+ ads + offerwall trio brings 15–25% more ARPDAU than a single channel.

**Target for Mind the Crack:** 0.12–0.20 USD in v1.0. Because a Hooked Inc
style upgrade tree + prestige structure was pulled into v1.0, the target
moved from the hypercasual band to the low end of the hybrid casual band
(see design document §7–8). 0.20–0.30 USD after v1.1 (Istanbul move,
clover, daily challenge).

### Currency economy design

The 2026 standard is dual currency: a **soft currency** earned through play,
and a **hard currency** bought or given as a reward. For Mind the Crack:

- **Soft currency (coins):** collected on the sidewalk, end-of-run reward,
  daily quest reward, offline income. Spend (sink): **upgrade tree (4
  branches × 10 levels — main sink)**, shoe/outfit cosmetics, character
  unlocks (v1.1).
- **Hard currency (clover):** rare, forgives a death. Source: rewarded ads,
  daily quests, IAP packs. This is v1.1 — but the economy must be designed
  for it now; adding a second currency later breaks the price balance.

**Rule:** every faucet (source) must have a sink (spend). When there is
nothing left to unlock, coins become meaningless and the end-of-run reward
stops feeling rewarding. v1.0 has 8 cosmetics + 40 upgrade levels; thanks
to the exponential cost curve the sink refills with prestige (moving) —
Hooked Inc's ocean region logic.

## 8. User acquisition and creatives

This section will be used directly in and after v1.0 testing.

- **TikTok** is the fastest-growing UA channel in 2026; 25–35% of top
  studios' UA spend. The most efficient channel for under-35 targeting and
  visually readable casual games. Mind the Crack fits both descriptions.
- **Creatives that work:** content that feels native to TikTok — raw
  gameplay, satisfying mechanics, UGC-style hooks, creator commentary. Not
  a polished ad film.
- **The hook is decisive:** a strong hook can cut CPI by 40–60% within the
  same campaign. Hook = the combination of the ad creative and the first
  session; what the ad shows must happen exactly in the game's first 10
  seconds.
- **Creative lifespan has shortened:** video creative lifespan on Meta fell
  from 14 days in 2024 to 9.2 days in 2026; in mature markets the practical
  lifespan is under 5 days. Plan: 10–20 new variants every 2 weeks.
- The **playable ad** format nearly doubled in a year.

### Creative hypotheses for Mind the Crack (to be tested)

1. **"Don't step!"** — a single line on screen, the foot approaching it, a
   last-moment long step. Tension + relief, 3 seconds.
2. **Death compilation** — the 4 superstition deaths back to back, fast
   cuts. A humour hook, the most shareable part of the product.
3. **Streak/music layers** — the music adding layers as the Perfect streak
   grows, the multiplier climbing on screen. The "satisfying mechanic"
   category.
4. **UGC style** — hand on the phone, sound on, close-up, a "one more"
   reaction.

The superstition premise gets people commenting on TikTok ("I still don't
step on them") — an advantage for Spark Ads, because the comments carry
over to the ad.

## 9. Music sourcing

- A vertical layering system needs a library with **stems**; many
  royalty-free libraries only provide a stereo mixdown. Libraries where
  stems are included in the licence are preferred, not ones that sell them
  as an expensive add-on.
- The licence must cover mobile store distribution and be Content ID clean
  (it will be used in TikTok/YouTube creatives).
- Budget: for projects under 5,000 USD a royalty-free pack makes sense; if
  adaptive stems are needed, either a library that includes stems or a
  custom composer.
- **Tooling:** FMOD or Wwise — both have a free indie tier and Unity
  integration. But Mind the Crack's layer system is simple (4 stems,
  on/off). It can be done with Unity's own `AudioSource`s, all started at
  once with `PlayScheduled` and controlled by volume. **Decision: try Unity
  native first, FMOD if that is not enough.** An extra SDK means build size
  and sync complexity.

## 10. Unity technical notes

- **IL2CPP** produces 47% smaller builds than Mono (at twice the build
  time). For Android: IL2CPP + ARM64, code stripping on.
- **Known conflict:** Android build errors have been reported with the
  Firebase Analytics + LevelPlay(AdMob) combination. Google Mobile Ads Unity
  Plugin v5.0.0 fixed Android IL2CPP support. Plugin versions will be noted
  during setup; if problems arise, continue with plain AdMob without
  mediation.
- Removing unused packages from the project also shrinks the generated
  code — clean up Unity's default package set in Phase 0.

## Additional sources

- [ARPDAU Benchmarks 2026 — Perkox](https://blog.perkox.com/2026/08/arpdau-benchmarks-2026/)
- [Revenue = DAU × ARPDAU — Playio](https://blog.playio.co/arpdau-benchmarks-mobile-games)
- [Game Economy Design: IAP, Hybrid, and D2C — Unity](https://unity.com/resources/game-economy-design-guide)
- [TikTok Ads for Mobile Games: 2026 UA Playbook — Game Growth Advisor](https://gamegrowthadvisor.com/blog/2026-05-07-tiktok-ads-mobile-games-ua-playbook-2026/)
- [Mobile Game Ad Creative Strategy 2026 — Game Growth Advisor](https://gamegrowthadvisor.com/blog/2026-05-12-mobile-game-ad-creative-strategy-2026/)
- [Royalty-Free Music for Indie Game Devs: Stems, Loops, Adaptive Audio — Layerhouse](https://www.layerhouse.io/blog/royalty-free-music-indie-game-developers)
- [Music Licensing For Video Games 2026 — Foxi](https://www.foximusic.com/blog/music-licensing-for-video-games-guide/)
- [IL2CPP build size optimizations — Unity Support](https://support.unity.com/hc/en-us/articles/208412186-IL2CPP-build-size-optimizations)
- [Firebase Unity SDK — AdMob/LevelPlay conflict (issue #1127)](https://github.com/firebase/firebase-unity-sdk/issues/1127)
