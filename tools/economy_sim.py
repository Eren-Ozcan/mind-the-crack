"""Mind the Crack — ekonomi simulasyonu.

Amac: yukseltme agacinin ilerleme egrisini koddan once dogrulamak.
Hedef egri:
  - 1. gun 3-4 yukseltme alinabilmeli
  - 7. gun ilk tasinma (prestij) mumkun olmali
  - 10. seviyeler prestij olmadan alinamamali

Calistir:  python tools/economy_sim.py
Cikti: gun bazli tablo + hedef kontrolleri.

Buradaki sayilar tek dogru kaynak. Unity tarafinda ScriptableObject'e bu
degerler aktarilir; kodda sabit sayi yazilmaz.
"""

from dataclasses import dataclass, field

# --------------------------------------------------------------------------
# AYARLANABILIR SAYILAR  (dengeleme burada yapilir)
# --------------------------------------------------------------------------

MAX_LEVEL = 10

# Dal bazli maliyet tabanlari. maliyet(n) = taban * 1.6^n,  n = 0..9
BRANCH_BASE = {
    "ayakkabi": 50,      # para carpani  — ana ekonomi dali, en ucuz
    "dayaniklilik": 70,  # baslangic avansi + ekstra tokezleme
    "mahalle": 90,       # cevrimdisi gelir
    "sans": 120,         # miknatis + yonca sansi (v1.1'de tam acilir)
}
COST_GROWTH = 1.6

# Dal etkileri (seviye basina)
SHOES_MULT_PER_LEVEL = 0.10        # para carpani +%10/sv  (sv10 = 2.0x)
HEADSTART_PER_LEVEL = 12           # metre, run basinda atlanan mesafe
MAGNET_COIN_BONUS_PER_LEVEL = 0.04  # toplama verimi +%4/sv
OFFLINE_COINS_PER_HOUR_PER_LEVEL = 6
OFFLINE_CAP_HOURS_BASE = 2.0
OFFLINE_CAP_HOURS_PER_LEVEL = 0.4   # sv10 = 6 saat tavan

# Run ekonomisi
COINS_PER_METER = 0.22          # kaldirimdan toplanan
END_BONUS_PER_METER = 0.08      # run sonu bonusu
REWARDED_DOUBLE_RATE = 0.35     # oyuncularin run sonu 2x reklamini izleme orani

# Oyuncu davranisi
RUNS_PER_SESSION = 5
SESSIONS_PER_DAY = {1: 2}       # 1. gun 2 oturum, sonraki gunler 3
DEFAULT_SESSIONS = 3
OFFLINE_CLAIMS_PER_DAY = 2      # gunde 2 kez donup cevrimdisi geliri alir

# Beceri egrisi — ortalama run mesafesi (metre)
BASE_DISTANCE = 70
SKILL_GROWTH = 0.18             # gun basina +%18
DISTANCE_CAP = 650

# Prestij (tasinma)
# Kosul SADECE seviye — ayri bir mesafe grindine baglanmaz. Mesafe sarti
# konursa oyuncu 40/40 seviyeye ulasip harcayacak yer kalmadan bekler
# (sink kurur, para yigilir). Bkz. economy.md "Reddedilen varyantlar".
PRESTIGE_TOTAL_LEVELS = 28      # 40 seviyenin 30'u
PRESTIGE_MULT = 1.5             # kalici para carpani, her tasinmada carpilir
CITY_COST_MULT = 1.90           # her yeni sehirde maliyet tabanlari x1.45

SIM_DAYS = 30


# --------------------------------------------------------------------------

def cost(branch: str, level: int, city: int = 0) -> int:
    """level = su anki seviye (0 tabanli). Bir sonraki seviyenin fiyati.
    city = kacinci tasinma (0 = ilk sehir)."""
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
        shoes = 1.0 + SHOES_MULT_PER_LEVEL * self.levels["ayakkabi"]
        magnet = 1.0 + MAGNET_COIN_BONUS_PER_LEVEL * self.levels["sans"]
        return shoes * magnet * self.prestige_mult


def avg_distance(day: int, st: State) -> float:
    skill = BASE_DISTANCE * ((1 + SKILL_GROWTH) ** (day - 1))
    skill = min(skill, DISTANCE_CAP)
    return skill + HEADSTART_PER_LEVEL * st.levels["dayaniklilik"]


def run_income(dist: float, st: State) -> float:
    pickup = dist * COINS_PER_METER
    end_bonus = dist * END_BONUS_PER_METER
    end_bonus *= (1 + REWARDED_DOUBLE_RATE)   # 2x reklam izleyenlerin ortalamasi
    return (pickup + end_bonus) * st.coin_mult


def offline_income(st: State) -> float:
    lvl = st.levels["mahalle"]
    if lvl == 0:
        return 0.0
    cap = OFFLINE_CAP_HOURS_BASE + OFFLINE_CAP_HOURS_PER_LEVEL * lvl
    per_hour = OFFLINE_COINS_PER_HOUR_PER_LEVEL * lvl
    return cap * per_hour * st.coin_mult * OFFLINE_CLAIMS_PER_DAY


def buy_greedy(st: State) -> list:
    """En ucuz alinabilir yukseltmeyi al, para yetmeyene kadar."""
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
            # Oyuncu hazir olur olmaz tasinir.
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

    print("Mind the Crack — ekonomi simulasyonu\n")
    print(f"{'Gun':>3} {'Ort.m':>6} {'Gelir':>9} {'Bakiye':>8} "
          f"{'Sv':>3} {'x':>5}  Alinan / olay")
    print("-" * 88)
    for r in rows:
        tag = (', '.join(r['bought']) or '-')
        if r['prestiged']:
            tag += f"  >>> TASINMA #{r['pcount']} (carpan {r['pmult']:.2f}x)"
        print(f"{r['day']:>3} {r['dist']:>6.0f} "
              f"{r['income']:>9.0f} {r['coins']:>8.0f} "
              f"{r['total_levels']:>3} {r['pmult']:>5.2f}  {tag}")

    print("\nMaliyet tablosu (dal / seviye):")
    header = "       " + "".join(f"{i+1:>7}" for i in range(MAX_LEVEL))
    print(header)
    for b in BRANCH_BASE:
        line = "".join(f"{cost(b, i):>7}" for i in range(MAX_LEVEL))
        total = sum(cost(b, i) for i in range(MAX_LEVEL))
        print(f"{b[:6]:<6}{line}   toplam {total}")

    grand = sum(sum(cost(b, i) for i in range(MAX_LEVEL)) for b in BRANCH_BASE)
    print(f"\n40 seviyenin tamami: {grand} para")

    print("\nHedef kontrolleri:")
    d1 = len(rows[0]["bought"])
    ok1 = 3 <= d1 <= 4
    print(f"  [{'OK ' if ok1 else 'HAYIR'}] 1. gun 3-4 yukseltme  -> {d1}")

    ok7 = prestige_day is not None and 6 <= prestige_day <= 9
    print(f"  [{'OK ' if ok7 else 'HAYIR'}] ~7. gun prestij hazir -> "
          f"{prestige_day if prestige_day else 'ulasilamadi'}. gun")

    maxed = [b for b, lv in rows[6]["levels"].items() if lv >= MAX_LEVEL] \
        if len(rows) > 6 else []
    ok10 = not maxed
    print(f"  [{'OK ' if ok10 else 'HAYIR'}] 7. gunde hicbir dal 10 degil -> "
          f"{maxed or 'hicbiri'}")

    # Sink kurumasi: 40/40 seviyedeyken tasinma yoksa para yigilir.
    dry = [r["day"] for r in rows
           if r["total_levels"] == MAX_LEVEL * len(BRANCH_BASE)
           and not r["prestiged"]]
    print(f"  [{'OK ' if not dry else 'HAYIR'}] sink kurumasi (40/40 iken "
          f"tasinma yok) -> {dry or 'yok'}")

    gaps = [b - a for a, b in zip(prestige_days, prestige_days[1:])]
    print(f"\nTasinma gunleri: {prestige_days}  (aralik: {gaps})")
    print(f"{SIM_DAYS}. gun: carpan {rows[-1]['pmult']:.2f}x, "
          f"seviye {rows[-1]['levels']}")


if __name__ == "__main__":
    main()
