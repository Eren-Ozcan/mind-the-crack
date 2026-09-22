# Mind the Crack — Pazar Araştırması

Tarih: 22 Eylül 2026. Kaynaklar dosyanın sonunda.

## 1. Özet — karar için tek paragraf

Saf hypercasual (tek mekanik + %100 reklam geliri) model 2026'da artık
çalışmıyor: kullanıcı edinme maliyeti reklam gelirinden hızlı arttı, Voodoo
gibi kategoriyi kuran yayıncılar bu modelden tamamen çıktı. Buna karşılık
"hybrid casual" (hypercasual hook + ilerleme/ekonomi/meta) büyüyen taraf.
Mind the Crack'in mevcut tasarımı zaten doğru tarafta duruyor (para, karakter
açma, günlük görev, günlük meydan okuma) — ama bunlar v1.1/v1.2'ye ertelenmiş
durumda. **Bu araştırmanın ana çıktısı: meta katmanının bir kısmı MVP'ye
çekilmeli**, yoksa v1.0 ölçülebilir bir ürün olmaz; sadece bir prototip olur.

İkinci ana çıktı: oyunun türü "ritim oyunu" olarak konumlandırılmamalı. Ritim
oyunu pazarının yaklaşık %82,5'i Japonya'da ve batıdaki kazananların (Beatstar,
Magic Tiles) rekabet avantajı lisanslı müzik kataloğu — tek kişilik bir
stüdyonun giremeyeceği bir alan. Mind the Crack'te müzik **his** katmanı,
katalog değil. Mağaza konumlandırması "ritim" değil, "zamanlama/refleks +
mizah" olmalı.

## 2. Pazar durumu (2026)

| Gösterge | Değer | Anlamı |
| --- | --- | --- |
| Harmanlanmış oyun CPI | 0,56 USD (yıllık +%30), Kuzey Amerika 1,68 USD | Ücretli UA ile hypercasual ölçeklemek artık pahalı |
| Casual Android ödüllü reklam eCPM | 3,60 (2023 H1) → 3,25 (2024 H1) → 3,02 USD (2025 H1), 2025'ten beri toparlanıyor | Reklam geliri yatay; CPI daha hızlı arttı |
| Hybrid casual IAP geliri | 2025'te 4,2 milyar USD, +%20 | Büyüyen tek casual alt segment |
| Hypercasual install hacmi | Sensor Tower'a göre hâlâ büyüyen tek segment | İndirme var, gelir yok — dikkat |
| Ritim oyunları dünya geliri | ~1 milyar USD, bunun ~825 milyonu Japonya | Batıda ritim = küçük ve lisans kilitli pazar |

Voodoo CEO'su Mart 2026'da "eski oyunların düşüşünü yeni oyun çıkararak
telafi etme modeli geçmişte kaldı" dedi ve düşüşü yılda %40'a kadar
gösterdi. Azur Games'in H1 2026 raporu da aynı yöne işaret ediyor: install
hacmi ile gelir birbirinden koptu, aynı alt türde iki oyunun geliri
monetizasyon stratejisine göre kat kat farklı olabiliyor.

**Bizim için sonuç:** hedef, "100 bin ucuz install al, reklamla para kazan"
değil. Hedef, organik + düşük bütçeli test trafiğiyle D1/D7'yi ölçüp oyunun
tutup tutmadığını anlamak; tutuyorsa meta derinleştirmek.

## 3. Rakip analizi

### 3.1 Doğrudan tematik rakip — Steppy Pants (Halfbrick / Super Entertainment)

Aynı batıl inanç premisi: kaldırımda yürü, çizgiye basma. Sonsuz yürüme
oyunu, fizik komedisi, tek dokunuş kontrol, karakter kozmetiği, 6 mod,
haftalık 7,99 USD VIP aboneliği. Uzun ömürlü ve bilinir bir oyun.

**Farkımız ne olmalı:** Steppy Pants'te his "sarhoş bacak fiziği" — kontrol
kasıtlı olarak kaygan ve komik. Mind the Crack'te his "metronom" olmalı:
kontrol net, ceza adil, hata oyuncunun okuma hatası. Yani aynı premis, zıt
his. Bu zaten yeterli ayrışma, ama mağaza görselinde ve ilk 5 saniyede bunun
okunması gerekiyor (ekranda vuruş göstergesi, taşların üstünde ayak gölgesi).

### 3.2 Ritim tarafı rakipleri

| Oyun | Sahip | Modeli | Bizim için ders |
| --- | --- | --- | --- |
| Magic Tiles 3 | Amanotes | Devasa şarkı kataloğu, ayda ~10M indirme, ömür boyu ~500M | Katalog savaşına girilmez |
| Beatstar | Space Ape | Lisanslı hit şarkılar, batıdaki en çok kazanan ritim oyunu, ~30M indirme | Gelir lisansta, mekanikte değil |
| Dancing Line | Cheetah/BoomBit | Seviye bazlı, gem topla → skin aç | Kozmetik ekonomisi basit ve işliyor |
| Beat Blade: Dash Dance | — | Tek seferlik ~9,99 USD paket: reklamsız + sınırsız revive + tüm şarkılar | Tek seferlik "her şey" paketi ritimde yaygın |

Lisans maliyeti nedeniyle **tüm müzik orijinal/prosedürel üretilmeli**. Bu bir
kısıt değil avantaj: BPM'i oynanışa göre değiştirebilmek ve Perfect serisinde
katman eklemek ancak kendi ürettiğimiz müzikle mümkün. Lisanslı şarkıda müzik
sabit, oynanış ona uymak zorunda.

## 4. Hedeflenecek sayılar (benchmark)

Bunlar "başarı" tanımımız; test sonuçları bunlara göre okunacak.

| Metrik | Pazar medyanı | Bizim geçme notumuz | Not |
| --- | --- | --- | --- |
| D1 retention | ~%22 (tüm oyunlar medyanı) | ≥ %35 | Yayıncıya gidilecekse ~%40 aranıyor |
| D7 retention | hybrid casual %15–22 | ≥ %15 (v1.0), ≥ %20 (v1.1) | v1.0 artık yükseltme ağacıyla geliyor, beklenti yükseldi |
| Oturum süresi | — | ≥ 4 dk (oturum başına 4–6 run) | Tasarımdaki 30–90 sn/run ile uyumlu |
| CPI (test) | casual/puzzle US Android 1,50–3,50 USD | — | **Ücretli UA yapılmayacak** (22 Eyl 2026 kararı); CPI sadece ileride bütçe ayrılırsa anlamlı |
| Ölçüm kohortu | — | ≥ 900 install | D1 ±3 puan, %95 güven için gerekli minimum. Organik birikecek; bkz. `test-plan.md` §6 |
| Crash-free oturum | — | ≥ %99,5 | Play Console vitals |

900 install'lık kohort şartı önemli: 100–200 kişilik bir testten çıkan D1
sayısı gürültüdür, ona bakarak tasarım değiştirilmez.

## 5. Monetizasyon planı (pazar verisiyle uyumlu)

Saf reklam modeli yeterli değil. Katmanlar:

1. **Ödüllü reklam — devam et.** Ölünce yonca ile devam, oturum başına 1 kez.
   Casual'da en yüksek eCPM'li format, ve oyuncu için ceza değil hediye.
2. **Ödüllü reklam — para/kozmetik.** Run sonunda "parayı 2×'le". İkinci
   ödüllü yerleşimi, IAP baskısı yapmadan ARPDAU'yu yükseltir.
3. **Geçiş reklamı (interstitial).** 3–4 ölümde bir, ilk 3 run'da asla.
   İlk oturumda reklam göstermek D1'in en yaygın katilidir.
4. **Tek seferlik "Reklamsız + başlangıç paketi"** ~4,99 USD. Beat Blade'in
   9,99 USD'lik "her şey" paketinin küçük kardeşi. Abonelik yok — Steppy
   Pants'in haftalık 7,99 VIP'i bize göre değil, iade/iptal yükü ve mağaza
   riski taşır.
5. **Kozmetik + karakter.** Para ile açılır, IAP ile hızlandırılır.

Mediation: stüdyo zaten AdMob üzerinde ve indie için 2026'da da önerilen
başlangıç AdMob. Ölçek gelirse (günlük ~50 bin impression üstü) AppLovin MAX
karşılaştırması yapılır; şimdi değil.

## 6. Ana riskler

| Risk | Etki | Azaltma |
| --- | --- | --- |
| Android ses gecikmesi (cihaza göre 0,01–0,2 sn) ritmi bozar | Oyun "adaletsiz" hissettirir, D1 çöker | `AudioSettings.dspTime` + `PlayScheduled`, DSP buffer = Best latency, ilk açılışta kalibrasyon ekranı, ayarlarda manuel offset |
| Ritim + yerleşim ikisi birden zor, hypercasual eşiğini aşar | Öğrenme eğrisi dik, erken bırakma | İlk 3 run'da zamanlama penceresi çok geniş, ölüm yok (tökezleme), sonra sıkılaşır |
| Steppy Pants gölgesi | "Kopya" algısı | His farkı (net kontrol, ritim, ceza animasyonları) ilk ekran görüntüsünde okunmalı |
| Prosedürel üretim çözülemez dizi üretir | Haksız ölüm | Üretim ters yönde: önce geçerli adım dizisi seç, taşları ona göre diz (bkz. tasarım dokümanı) |
| Batıl inanç teması kültüre bağlı | Batı dışında premis anlaşılmaz | Premis metne değil görsele yaslanmalı: çizgiye basınca saksı düşer — açıklama gerektirmiyor |
| Reklamsız gelir beklentisi | Gelir hedefi tutmaz | Hedef v1.0'da gelir değil, retention; gelir v1.1'den sonra konuşulur |

## Kaynaklar

- [Hypercasual ve hybrid casual 2026 tam rapor — Azur Games](https://azurgames.com/blog/hypercasual-and-hybrid-casual-in-2026-full-report/)
- [State of Mobile 2026: 9 Key Trends — Deconstructor of Fun](https://www.deconstructoroffun.com/blog/2026/2/2/state-of-mobile-2026)
- [Mobile Gaming's Shift from Hyper to Hybrid-Casual — Unity](https://unity.com/blog/mobile-gaming-shift-hyper-hybrid-casual)
- [What happened to hypercasual? — PocketGamer.biz](https://www.pocketgamer.biz/what-happened-to-hypercasual-the-markets-evolution-over-the-past-year/)
- [Mobile Game KPIs 2026: 20 Benchmarks — Game Growth Advisor](https://gamegrowthadvisor.com/blog/2026-03-17-mobile-game-kpis-benchmarks-2026/)
- [Mobile Game Retention Guide 2026 — Game Growth Advisor](https://gamegrowthadvisor.com/blog/2026-03-17-mobile-game-retention-strategies-2026/)
- [Hybrid-Casual Monetization — Playio](https://blog.playio.co/hybrid-casual-games-monetization)
- [Beatstar: The Western World Finds its Rhythm — Naavik](https://naavik.co/deep-dives/beatstar-west-finds-rhythm/)
- [Steppy Pants — App Store](https://apps.apple.com/us/app/steppy-pants/id1094138419)
- [Beat Blade: Dash Dance guide — Pocket Tactics](https://www.pockettactics.com/beat-blade-dash-dance/guide)
- [Rhythm Game Crash Course: dspTime ile senkronizasyon — Native Audio](https://exceed7.com/native-audio/rhythm-game-crash-course/dsp-sync.html)
- [Coding to the Beat — Game Developer](https://www.gamedeveloper.com/audio/coding-to-the-beat---under-the-hood-of-a-rhythm-game-in-unity)
- [ironSource vs AppLovin MAX vs AdMob 2026 — UndrAds](https://undrads.com/blogs/ironsource-vs-applovin-max-vs-admob)

---

# İkinci tur araştırma (22 Eylül 2026)

## 7. Ekonomi ve ARPDAU

| Segment | ARPDAU aralığı |
| --- | --- |
| Sadece reklam, casual | 0,01–0,05 USD |
| Hypercasual | 0,03–0,08 USD |
| Hybrid casual | 0,15–0,50 USD |

Hybrid casual, hypercasual'ın yaklaşık **5 katı** ARPDAU üretiyor; sebep tek
oyuncudan iki gelir yolu (reklam + satın alma) açılması. IAP + reklam +
offerwall üçlüsü, tek kanala göre %15–25 ek ARPDAU getiriyor.

**Mind the Crack için hedef:** v1.0'da 0,12–0,20 USD. Hooked Inc tarzı
yükseltme ağacı + prestij yapısı v1.0'a alındığı için hedef hypercasual
bandından hybrid casual bandının alt ucuna çekildi (bkz. tasarım dokümanı
§7–8). v1.1 (İstanbul taşınması, yonca, günlük meydan okuma) sonrası
0,20–0,30 USD.

### Para ekonomisi tasarımı

2026 standardı çift para birimi: oynanışla kazanılan **yumuşak para**,
satın alınan veya ödülle verilen **sert para**. Mind the Crack için:

- **Yumuşak para (bozuk para):** kaldırımda toplanır, run sonu ödülü,
  günlük görev ödülü, çevrimdışı gelir. Harcama yeri (sink): **yükseltme
  ağacı (4 dal × 10 seviye — ana sink)**, ayakkabı/kıyafet kozmetiği,
  karakter açma (v1.1).
- **Sert para (yonca):** nadir, ölümü affeder. Kaynak: ödüllü reklam,
  günlük görev, IAP paketi. Bu v1.1 — ama ekonomi şimdiden buna göre
  tasarlanmalı, sonradan ikinci para birimi eklemek fiyat dengesini bozar.

**Kural:** her faucet'in (kaynak) bir sink'i (harcama yeri) olmalı. Açılacak
şey biterse para anlamsızlaşır ve run sonu ödülü ödüllendirici hissettirmez.
v1.0'da 8 kozmetik + 40 yükseltme seviyesi; üstel maliyet eğrisi sayesinde
sink prestij (taşınma) ile yeniden dolar — Hooked Inc'in okyanus bölgesi
mantığı.

## 8. Kullanıcı edinme ve kreatif

Bu bölüm v1.0 testinde ve sonrasında doğrudan kullanılacak.

- **TikTok**, 2026'da en hızlı büyüyen UA kanalı; üst düzey stüdyoların UA
  harcamasının %25–35'i. 35 yaş altı hedef ve görsel olarak okunaklı casual
  oyunlar için en verimli kanal. Mind the Crack her iki tarife de uyuyor.
- **İşe yarayan kreatif:** TikTok'a ait hissettiren içerik — ham oynanış,
  tatmin edici mekanik, UGC tarzı hook, yaratıcı yorumu. Cilalı reklam
  filmi değil.
- **Hook belirleyici:** güçlü bir hook aynı kampanyada CPI'ı %40–60
  düşürebiliyor. Hook = reklam kreatifi ile ilk oturumun birleşimi; yani
  reklamda gösterilen şey oyunun ilk 10 saniyesinde aynen olmalı.
- **Kreatif ömrü kısaldı:** Meta'da video kreatif ömrü 2024'te 14 gün iken
  2026'da 9,2 güne düştü; olgun pazarlarda pratik ömür 5 günün altında.
  Plan: her 2 haftada 10–20 yeni varyant.
- **Playable reklam** formatı bir yılda neredeyse ikiye katlandı.

### Mind the Crack için kreatif hipotezleri (test edilecek)

1. **"Basma!"** — ekranda tek çizgi, ayak ona yaklaşıyor, son anda uzun
   adım. Gerilim + rahatlama, 3 saniye.
2. **Ölüm derlemesi** — 4 batıl inanç ölümü arka arkaya, hızlı kesme.
   Mizah hook'u, ürünün en paylaşılabilir parçası.
3. **Seri/müzik katmanı** — Perfect serisi büyüdükçe müziğin katman
   eklemesi, ekranda çarpanın yükselmesi. "Tatmin edici mekanik" kategorisi.
4. **UGC tarzı** — el telefonda, ses açık, yakın çekim, "bir daha" tepkisi.

Batıl inanç premisi TikTok'ta yorum yazdırır ("ben hâlâ basmıyorum") — bu
Spark Ads için avantaj, çünkü yorumlar reklama taşınıyor.

## 9. Müzik tedariki

- Katmanlı (vertical layering) sistem için **stem'li** kütüphane şart;
  birçok royalty-free kütüphane sadece miks stereo dosya veriyor. Stem'in
  lisansa dahil olduğu kütüphaneler tercih edilir, pahalı ek paket olarak
  satılanlar değil.
- Lisans, mobil mağaza dağıtımını kapsamalı; Content ID temiz olmalı
  (TikTok/YouTube kreatiflerinde kullanılacak).
- Bütçe: 5.000 USD altı projelerde royalty-free paket mantıklı; adaptif stem
  gerekiyorsa ya stem içeren kütüphane ya da custom besteci.
- **Araç:** FMOD veya Wwise — ikisinin de indie ücretsiz katmanı ve Unity
  entegrasyonu var. Ama Mind the Crack'in katman sistemi basit (4 stem,
  aç/kapa). Unity'nin kendi `AudioSource`'larıyla, hepsi aynı anda
  `PlayScheduled` ile başlatılıp ses seviyeleriyle oynanarak yapılabilir.
  **Karar: önce Unity native ile dene, yetmezse FMOD.** Ek SDK, build boyutu
  ve senkronizasyon karmaşası demek.

## 10. Unity teknik notları

- **IL2CPP** Mono'ya göre %47 daha küçük build üretiyor (build süresi iki
  katı). Android için IL2CPP + ARM64, code stripping açık.
- **Bilinen çakışma:** Firebase Analytics + LevelPlay(AdMob) kombinasyonunda
  Android build hataları raporlanmış. Google Mobile Ads Unity Plugin v5.0.0
  Android IL2CPP desteğini düzeltti. Kurulumda plugin sürümleri not
  alınacak; sorun çıkarsa mediation'sız düz AdMob ile devam.
- Kullanılmayan paketler projeden çıkarılınca üretilen kod da küçülüyor —
  Unity'nin varsayılan paket setini Faz 0'da temizle.

## Ek kaynaklar

- [ARPDAU Benchmarks 2026 — Perkox](https://blog.perkox.com/2026/08/arpdau-benchmarks-2026/)
- [Revenue = DAU × ARPDAU — Playio](https://blog.playio.co/arpdau-benchmarks-mobile-games)
- [Game Economy Design: IAP, Hybrid, and D2C — Unity](https://unity.com/resources/game-economy-design-guide)
- [TikTok Ads for Mobile Games: 2026 UA Playbook — Game Growth Advisor](https://gamegrowthadvisor.com/blog/2026-05-07-tiktok-ads-mobile-games-ua-playbook-2026/)
- [Mobile Game Ad Creative Strategy 2026 — Game Growth Advisor](https://gamegrowthadvisor.com/blog/2026-05-12-mobile-game-ad-creative-strategy-2026/)
- [Royalty-Free Music for Indie Game Devs: Stems, Loops, Adaptive Audio — Layerhouse](https://www.layerhouse.io/blog/royalty-free-music-indie-game-developers)
- [Music Licensing For Video Games 2026 — Foxi](https://www.foximusic.com/blog/music-licensing-for-video-games-guide/)
- [IL2CPP build size optimizations — Unity Support](https://support.unity.com/hc/en-us/articles/208412186-IL2CPP-build-size-optimizations)
- [Firebase Unity SDK — AdMob/LevelPlay çakışması (issue #1127)](https://github.com/firebase/firebase-unity-sdk/issues/1127)
