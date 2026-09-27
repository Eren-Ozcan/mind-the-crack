# Mind the Crack — Project Notes

Hypercasual timing/arcade game. Walk a character along the sidewalk to the
beat of the music and get as far as possible without stepping on the lines
between the slabs. Unity 6, Android (no iOS in v1.0).

## Documents — read these first

| File | Contents |
| --- | --- |
| `docs/game-design.md` | Game design, all mechanic decisions |
| `docs/economy.md` | **Single source of truth for economy numbers** |
| `docs/onboarding.md` | The first 60 seconds, second by second |
| `docs/test-plan.md` | 5-layer test plan, device matrix, gates |
| `docs/roadmap.md` | Phases, schedule, decisions made |
| `docs/market-research.md` | Market data, competitors, benchmarks |
| `tools/economy_sim.py` | Economy simulation — run it when a number changes |

## Invariant rules

### 1. Upgrades never touch skill

No upgrade, purchase or reward may change: the timing window (±60 / ±120
ms), the slab centre tolerance for Perfect (40%), the BPM ramp, step
distances (1.0 / 1.5 / 2.5), the generation difficulty curve.

Upgrades only touch: coin multiplier, magnet radius, head start, extra
stumble, offline income, clover.

This rule is a table in `docs/game-design.md` §8.1 and an automated test in
`docs/test-plan.md` §2. The document is not enough — the test must be kept
too.

### 2. Economy numbers are never hardcoded

All economy values come from `docs/economy.md` and are exported to Unity as
a ScriptableObject. Before changing a number, run
`python tools/economy_sim.py` first; a value is not accepted until all four
target checks report OK.

### 3. The time source is dspTime

`Time.time` / `Time.deltaTime` are never used anywhere rhythm-related. Use
`AudioSettings.dspTime` + `AudioSource.PlayScheduled`. Unity UI Button is
never used for rhythm input (its callback fires on release, not on press).

### 4. Store assets never enter this repo

Local: `docs/store-assets-originals/` (gitignored). Permanent: private
`Eren-Ozcan/pictures` repo under `pictures/mind-the-crack/`.

### 5. Account verification

Before opening the Google/Play Console/AdMob/Firebase consoles, verify the
active account. Expected account: `yilkgamesstudio@gmail.com`. URLs and
pitfalls: `C:\Projects\pictures\STUDIO.md`. Read
`C:\Projects\pictures\ADS_POLICY.md` before adding an ad placement.

## Folder layout

```
Assets/Scripts/Rhythm/     Conductor, timing window
Assets/Scripts/Gameplay/   Steps, landing evaluation, sidewalk generation
Assets/Scripts/Config/     ScriptableObject settings
Assets/Scripts/Debug/      Development HUD
docs/                      Design and planning documents
tools/                     Python helpers (economy simulation)
```

## Phase status

Phase 0 (vertical slice) — in progress. Gate criterion: ≥ 6 out of 10
people do not put it down for 5 minutes. Phase 1 does not start until it is
passed.
