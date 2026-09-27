"""Mind the Crack — economy simulation.

Goal: validate the upgrade tree's progression curve before writing code.
Target curve:
  - 3-4 upgrades affordable on day 1
  - first move (prestige) possible on day 7
  - level 10 unreachable without prestige

Run:     python tools/economy_sim.py
Output:  per-day table + target checks.

The numbers here are the single source of truth. On the Unity side these
values are copied into a ScriptableObject; no numbers are hardcoded.
"""

from dataclasses import dataclass, field

# --------------------------------------------------------------------------
# TUNABLE NUMBERS  (balancing happens here)
# --------------------------------------------------------------------------

MAX_LEVEL = 10

# Per-branch cost bases. cost(n) = base * 1.6^n,  n = 0..9
BRANCH_BASE = {
    "shoes": 50,         # coin multiplier — main economy branch, cheapest
    "endurance": 70,     # head start + extra stumble
    "neighborhood": 90,  # offline income
    "luck": 120,         # magnet + clover chance (fully unlocked in v1.1)
}
COST_GROWTH = 1.6

# Branch effects (per level)
SHOES_MULT_PER_LEVEL = 0.10        # coin multiplier +10%/lv  (lv10 = 2.0x)
HEADSTART_PER_LEVEL = 12           # metres skipped at run start
MAGNET_COIN_BONUS_PER_LEVEL = 0.04  # pickup efficiency +4%/lv
OFFLINE_COINS_PER_HOUR_PER_LEVEL = 6
OFFLINE_CAP_HOURS_BASE = 2.0
OFFLINE_CAP_HOURS_PER_LEVEL = 0.4   # lv10 = 6 hour cap

# Run economy
COINS_PER_METER = 0.22          # collected on the sidewalk
END_BONUS_PER_METER = 0.08      # end-of-run bonus
REWARDED_DOUBLE_RATE = 0.35     # share of players watching the end-of-run 2x ad

# Player behaviour
RUNS_PER_SESSION = 5
SESSIONS_PER_DAY = {1: 2}       # 2 sessions on day 1, 3 on later days
DEFAULT_SESSIONS = 3
OFFLINE_CLAIMS_PER_DAY = 2      # returns twice a day to claim offline income

# Skill curve — average run distance (metres)
BASE_DISTANCE = 70
SKILL_GROWTH = 0.18             # +18% per day
DISTANCE_CAP = 650

# Prestige (moving)
# The condition is levels ONLY — not tied to a separate distance grind. With
# a distance requirement the player reaches 40/40 levels and waits with
# nothing left to spend on (the sink dries up, coins pile up). See
# economy.md "Rejected variants".
PRESTIGE_TOTAL_LEVELS = 28      # out of 40 levels
PRESTIGE_MULT = 1.5             # permanent coin multiplier, compounded on every move
CITY_COST_MULT = 1.90           # cost bases multiplied by this in every new city

SIM_DAYS = 30


# --------------------------------------------------------------------------

def cost(branch: str, level: int, city: int = 0) -> int:
    """level = current level (0-based). Returns the next level's price.
    city = move count (0 = first city)."""
    base = BRANCH_BASE[branch] * (CITY_COST_MULT ** city)
    return round(base * (COST_GROWTH ** level))


@dataclass
class State:
    levels: dict = field(default_factory=lambda: {b: 0 for b in BRANCH_BASE})
    coins: float = 0.0
    total_distance: float = 0.0
    prestige_mult: float = 1.0
    prestige_count: int = 0
    spent: float = 0.0

    @property
    def total_levels(self) -> int:
        return sum(self.levels.values())

    @property
    def coin_mult(self) -> float:
        shoes = 1.0 + SHOES_MULT_PER_LEVEL * self.levels["shoes"]
        magnet = 1.0 + MAGNET_COIN_BONUS_PER_LEVEL * self.levels["luck"]
        return shoes * magnet * self.prestige_mult


def avg_distance(day: int, st: State) -> float:
    skill = BASE_DISTANCE * ((1 + SKILL_GROWTH) ** (day - 1))
    skill = min(skill, DISTANCE_CAP)
    return skill + HEADSTART_PER_LEVEL * st.levels["endurance"]


def run_income(dist: float, st: State) -> float:
    pickup = dist * COINS_PER_METER
    end_bonus = dist * END_BONUS_PER_METER
    end_bonus *= (1 + REWARDED_DOUBLE_RATE)   # averaged over players watching the 2x ad
    return (pickup + end_bonus) * st.coin_mult


def offline_income(st: State) -> float:
    lvl = st.levels["neighborhood"]
    if lvl == 0:
        return 0.0
    cap = OFFLINE_CAP_HOURS_BASE + OFFLINE_CAP_HOURS_PER_LEVEL * lvl
    per_hour = OFFLINE_COINS_PER_HOUR_PER_LEVEL * lvl
    return cap * per_hour * st.coin_mult * OFFLINE_CLAIMS_PER_DAY


def buy_greedy(st: State) -> list:
    """Buy the cheapest affordable upgrade until coins run out."""
    bought = []
    while True:
        options = [
            (cost(b, lv, st.prestige_count), b)
            for b, lv in st.levels.items() if lv < MAX_LEVEL
        ]
        if not options:
            break
        price, branch = min(options)
        if price > st.coins:
            break
        st.coins -= price
        st.spent += price
        st.levels[branch] += 1
        bought.append(f"{branch}{st.levels[branch]}")
    return bought


def simulate():
    st = State()
    prestige_day = None
    prestige_days = []
    city_distance = 0.0
    rows = []

    for day in range(1, SIM_DAYS + 1):
        sessions = SESSIONS_PER_DAY.get(day, DEFAULT_SESSIONS)
        runs = sessions * RUNS_PER_SESSION
        dist = avg_distance(day, st)

        day_income = runs * run_income(dist, st) + offline_income(st)
        st.coins += day_income
        st.total_distance += runs * dist
        city_distance += runs * dist

        bought = buy_greedy(st)

        ready = st.total_levels >= PRESTIGE_TOTAL_LEVELS
        prestiged = False
        if ready:
            if prestige_day is None:
                prestige_day = day
            # The player moves as soon as they are ready.
            st.levels = {b: 0 for b in BRANCH_BASE}
            st.coins = 0.0
            st.prestige_mult *= PRESTIGE_MULT
            st.prestige_count += 1
            city_distance = 0.0
            prestiged = True
            prestige_days.append(day)

        rows.append({
            "day": day, "runs": runs, "dist": dist, "income": day_income,
            "bought": bought, "levels": dict(st.levels),
            "total_levels": st.total_levels, "coins": st.coins,
            "total_distance": st.total_distance, "ready": ready,
            "prestiged": prestiged, "pcount": st.prestige_count,
            "pmult": st.prestige_mult,
        })

    return rows, prestige_day, prestige_days, st


def main():
    rows, prestige_day, prestige_days, st = simulate()

    print("Mind the Crack — economy simulation\n")
    print(f"{'Day':>3} {'Avg.m':>6} {'Income':>9} {'Balance':>8} "
          f"{'Lv':>3} {'x':>5}  Bought / event")
    print("-" * 88)
    for r in rows:
        tag = (', '.join(r['bought']) or '-')
        if r['prestiged']:
            tag += f"  >>> MOVE #{r['pcount']} (multiplier {r['pmult']:.2f}x)"
        print(f"{r['day']:>3} {r['dist']:>6.0f} "
              f"{r['income']:>9.0f} {r['coins']:>8.0f} "
              f"{r['total_levels']:>3} {r['pmult']:>5.2f}  {tag}")

    print("\nCost table (branch / level):")
    header = "       " + "".join(f"{i+1:>7}" for i in range(MAX_LEVEL))
    print(header)
    for b in BRANCH_BASE:
        line = "".join(f"{cost(b, i):>7}" for i in range(MAX_LEVEL))
        total = sum(cost(b, i) for i in range(MAX_LEVEL))
        print(f"{b[:6]:<6}{line}   total {total}")

    grand = sum(sum(cost(b, i) for i in range(MAX_LEVEL)) for b in BRANCH_BASE)
    print(f"\nAll 40 levels: {grand} coins")

    print("\nTarget checks:")
    d1 = len(rows[0]["bought"])
    ok1 = 3 <= d1 <= 4
    print(f"  [{'OK ' if ok1 else 'NO '}] day 1: 3-4 upgrades  -> {d1}")

    ok7 = prestige_day is not None and 6 <= prestige_day <= 9
    print(f"  [{'OK ' if ok7 else 'NO '}] prestige ready ~day 7 -> day "
          f"{prestige_day if prestige_day else 'never'}")

    maxed = [b for b, lv in rows[6]["levels"].items() if lv >= MAX_LEVEL] \
        if len(rows) > 6 else []
    ok10 = not maxed
    print(f"  [{'OK ' if ok10 else 'NO '}] no branch at 10 on day 7 -> "
          f"{maxed or 'none'}")

    # Sink dry-up: at 40/40 levels without a move, coins pile up.
    dry = [r["day"] for r in rows
           if r["total_levels"] == MAX_LEVEL * len(BRANCH_BASE)
           and not r["prestiged"]]
    print(f"  [{'OK ' if not dry else 'NO '}] sink dry-up (40/40 with "
          f"no move) -> {dry or 'none'}")

    gaps = [b - a for a, b in zip(prestige_days, prestige_days[1:])]
    print(f"\nMove days: {prestige_days}  (gaps: {gaps})")
    print(f"Day {SIM_DAYS}: multiplier {rows[-1]['pmult']:.2f}x, "
          f"levels {rows[-1]['levels']}")


if __name__ == "__main__":
    main()
