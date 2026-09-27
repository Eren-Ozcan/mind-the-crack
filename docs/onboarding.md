# Mind the Crack — First Session Flow

In hypercasual, the thing that decides D1 the most is the first 60 seconds.
This file defines those 60 seconds second by second and records the
decisions made, with their rationale.

## 1. Core decisions

| Decision | Choice | Rationale |
| --- | --- | --- |
| Launch | No menu, no logo wait — the game starts as soon as the app opens | The menu screen is the highest drop-off point in the first session |
| Tutorial format | No text, diegetic icon + shadow | Text is not read; it is also a localisation burden |
| First run | Cannot be lost (stumbles only) | Death in the first 30 seconds = instant churn |
| Calibration | **Not** on first launch, after the first run | Decision changed, §4 |
| First ad | None in the first 3 runs | Ads in the first session are the most common D1 killer |
| First upgrade | Must happen in the first session | If the meta is not felt in the first session, D7 does not come |

## 2. First run, second by second

Music plays from the first frame, 100 BPM. The character is already
walking.

| Time | What happens | Expected from the player |
| --- | --- | --- |
| 0–6 s | Wide square concrete, all slabs 1.0 units. The foot shadow lands in the middle of the slab on every step. The Perfect sound plays, the multiplier text grows. | Nothing. They just hear the rhythm. |
| 6–14 s | A wide slab appears ahead; a normal step would land on the line. The shadow turns **red**. A TAP icon pulsing with the beat appears. Window ±200 ms. | First tap → long step |
| — | If they fail: stumble, speed drops, the same situation repeats on the next slab. No death, the icon grows. | Tries again |
| 14–22 s | Same setup for the jump: a very wide gap, an up-arrow icon, swipe. | First swipe |
| 22–30 s | The first coins appear — on safe slabs, easy to collect. No icon, the coins explain themselves. | Collects |
| 30–45 s | Icons disappear. A mixed pattern starts (tap + swipe together), but the single stumble has been given back. The ground switches to paving stone. | Now actually playing |
| 45 s+ | Normal game. The window narrows gradually from ±200 ms to ±120/±60 ms (over 3 runs). | — |

The first run **does not end on two stumbles** — death is only possible
after the 45th second. In every case it ends with a death between 60 and 90
seconds; if it ends early, the second run opens with the same rules.

### First run economy

A normal 70 metre run gives ~23 coins; Shoes lv1 costs 50. So with standard
generation the player **cannot buy any upgrade** in the first session. Fix:

- Coin density is 2× in the tutorial run (0.44 coins/metre).
- A one-time "first walk" reward at the end of the run: **40 coins**.

That makes ~100 coins at the end of the first run → the first upgrade is
bought. These numbers are kept outside `tools/economy_sim.py` as a one-time
bonus.

## 3. After the first run

| Order | Screen | Note |
| --- | --- | --- |
| 1 | Death animation (1.2 s cap, skipped on tap) | One of the superstition deaths |
| 2 | End of run: distance, coins, funny text | The rewarded "double coins" button is **not here yet** — no ads in the first 3 runs |
| 3 | **Calibration offer** | §4 |
| 4 | **Upgrade screen opens automatically** | A glowing arrow over Shoes lv1. Until the purchase is made the "Play again" button looks secondary but can still be pressed — no forced clicking |
| 5 | Play again | — |

From the second run on, the upgrade screen does not open by itself.

## 4. Calibration — decision changed

**Old plan:** a calibration screen on first launch (tap the metronome 8
times).
**Problem:** an exercise asked for before the player has seen the game; a
reason to quit in the first 20 seconds. Someone who does not play rhythm
games does not understand why they are tapping.

**New plan — two stages:**

1. **Silent estimate.** The average offset against the beat of the first 8
   valid inputs in the tutorial run is computed and accepted as the device
   offset **by default**. The player does nothing.
2. **Offer.** At the end of the first run, only if the estimated offset
   exceeds 40 ms: "Shall we tune the rhythm to your phone?" → Yes/Later. If
   they say yes, the classic 8-beat calibration. If they say later, it is
   never asked again; it is always reachable from settings.

The offset stays manually adjustable in the settings screen (rhythm players
look for it).

## 5. The whole first session (target)

| Time | Event |
| --- | --- |
| 0:00 | Game opens, music plays, character walks |
| 0:08 | First tap |
| 0:20 | First swipe |
| 0:28 | First coin |
| 1:00–1:30 | First death, first comic punishment animation |
| 1:40 | First upgrade purchased |
| 2:00–5:00 | 4–5 more runs |
| ~3:30 | First interstitial (after run 4, at the earliest) |
| 4:00 | First daily quest completed (an easy one like "get 20 Perfects today") |
| 5:00 | Session ends — target ≥ 4 minutes |

## 6. What to measure (Phase 4)

Whether this flow works is read from these events:

| Event | Question it answers |
| --- | --- |
| `tutorial_first_tap` (time) | Is the tap icon understood? Target ≤ 12 s |
| `tutorial_tap_attempts` | How many attempts did it take? Target average ≤ 2 |
| `tutorial_first_swipe` (time) | Is the swipe understood? |
| `tutorial_completed` | Tutorial completion rate. Target ≥ 90% |
| `first_upgrade_purchased` (time in session) | Does the first upgrade happen? Target ≥ 70% in the first session |
| `calibration_offered` / `calibration_completed` | How often the offer appears, how many accept it |
| `session_end` (run count, duration) | Is the session ≥ 4 min? |

If `tutorial_completed` is below 90%, the problem is in the tutorial; no
need to look elsewhere to fix D1.

## 7. Left open

- **Player with sound off.** If the phone is on silent, the rhythm is not
  heard. Solution: a thin ring on screen pulsing with the beat (always on,
  only becomes prominent when muted) + haptics. To be added in Phase 1, no
  separate design needed.
- **Very short first session (< 60 s).** If the player quits before
  finishing the tutorial, the tutorial restarts from the beginning on the
  second launch (it repeats until completed).
