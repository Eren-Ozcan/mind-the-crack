# Mind the Crack — Üretim Planı ve Yol Haritası

Tek kişilik geliştirme (Eren + Claude) varsayımıyla. Süreler takvim
haftası, tam zamanlı değil.

## 0. Motor kararı

**Öneri: Unity.**

Gerekçe: oyunun tek kritik teknik gereksinimi ses–girdi senkronizasyonu.
Unity'de bunun standart çözümü olgun ve belgeli (`AudioSettings.dspTime`,
`AudioSource.PlayScheduled`, DSP buffer ayarı). Stüdyonun diğer oyunları
Capacitor/web (Çengel Bulmaca, Reefy) ve Godot (Little Grand Hotel) — ikisi
de bu oyun için yanlış:

- **Capacitor/web:** Android WebView'da WebAudio gecikmesi cihaza göre çok
  değişken ve kontrol edilemez. Ritim oyunu için elenir.
- **Godot:** mobilde ses gecikmesi kontrolü Unity kadar olgun değil, ritim
  oyunu örneği ve topluluk bilgisi az.

Bedeli: stüdyoda Unity pipeline'ı yok — AdMob, Firebase, Play Console
entegrasyonları ilk kez Unity tarafında kurulacak. Bu, Faz 3'e yaklaşık 1
hafta ek yük demek. Buna değer, çünkü yanlış motorla senkronizasyon sorunu
çözülemez.

## 1. Fazlar

### Faz 0 — Dikey dilim (1–2 hafta) · **kapı: eğlenceli mi?**

Amaç: para, grafik, menü, reklam yok. Sadece "vuruşa basarak taşa inmek"
hissi.

- [x] Unity 6.1 (6000.1.17f1) projesi, `dspTime` tabanlı `Conductor`
      (vuruş sayacı, commit gecikmesi, sapma hesabı).
- [x] Gri kutu kaldırım — ama sabit taş yerine **ters üretim algoritması
      baştan yazıldı** (`SidewalkGenerator`). En büyük algoritma riski
      olduğu için Faz 1'den öne alındı.
- [x] Karakter = kapsül; adım = vuruştan vuruşa lerp, zıplamada yay.
- [x] Girdi: tap = uzun adım, swipe = zıplama, `StepConfig`
      ScriptableObject'inde ayarlanabilir zamanlama penceresi.
- [x] İniş değerlendirmesi: Perfect / İyi / Tökezleme / Ölüm.
- [x] Debug HUD: sapma ms, mesafe, skor, çarpan, seri, adım türü.
- [x] Prosedürel metronom (`ClickTrack`) — ses varlığı beklenmedi.
- [x] 3 seçeneğin iniş noktasını gösteren önizleme işaretleri.
- [x] Üretim doğrulama testi (10.000 seed) — `Mind the Crack > Validate
      Generator` menüsü veya `-executeMethod`.
- [ ] URP kurulumu (CLI ile açılan proje Built-in pipeline ile geldi;
      materyaller şimdilik Standard shader'a düşüyor).
- [ ] Telefonda test (APK) ve 10 kişilik oynanabilirlik seansı.

**Kapı kriteri:** Eren + en az 3 kişi telefonda oynar, 5 dakika elden
bırakmaz. Bırakıyorsa çekirdek mekanik düzeltilir; Faz 1'e geçilmez.

### Faz 1 — Çekirdek sistemler (2–3 hafta)

- [ ] Ters üretim algoritması (tasarım dokümanı §3.2) + birim testleri:
      üretilen her segment çözülebilir olmalı, 10.000 seed ile otomatik test.
- [ ] 3 zemin türü, zorluk eğrisi, BPM rampası ve 5 BPM loop seti geçişi.
- [ ] Müzik katman sistemi (seri → stem açma/kapama, bar sınırında fade).
- [ ] Kalibrasyon ekranı + offset kaydı.
- [ ] 2 engel (su birikintisi, köpek kakası).
- [ ] Ölüm akışı + 4 batıl inanç animasyonu (placeholder animasyonla başla).
- [ ] Skor, çarpan, run sonu ekranı.

### Faz 2 — Meta, ekonomi ve kabuk (4–5 hafta) **[GENİŞLEDİ]**

Hooked Inc tarzı yükseltme + prestij yapısı bu faza eklendi. Tasarım
dokümanı §7 ve §8.

**2a — Ekonomi modeli · ✅ TAMAMLANDI (22 Eyl 2026)**
- [x] Simülasyon: `tools/economy_sim.py`, dokümantasyon: `docs/economy.md`.
- [x] Dört hedef kontrolü de OK: 1. gün 3 yükseltme · ilk taşınma 8. gün ·
      7. günde hiçbir dal 10 değil · sink kurumuyor.
- [x] Maliyet formülü `taban × 1,90^şehir × 1,60^seviye`, dal tabanları
      50/70/90/120.
- [ ] Sayıları Unity ScriptableObject'e aktar. **Kodda sabit sayı
      yazılmaz** — `economy.md` tek kaynak.

**2b — Sistemler · 2 hafta**
- [ ] Kalıcı kayıt (local JSON, `PlayerPrefs` değil) — şema baştan çift
      para birimi + prestij çarpanı alanlarını içerir.
- [ ] Yükseltme ağacı: 4 dal × 10 seviye (Ayakkabı, Şans, Dayanıklılık,
      Mahalle).
- [ ] Yükseltmelerin run'a uygulanması — **sadece izin verilen alanlar**
      (tasarım §8.1 tablosu). Zamanlama penceresine dokunan kod yazılmaz.
- [ ] Çevrimdışı gelir: tavan 2–4 saat, Mahalle dalıyla uzar. Cihaz saati
      geri alınırsa gelir verilmez.
- [ ] Prestij altyapısı: şehir kimliği, kalıcı çarpan, taşınma koşulu
      kontrolü (v1.0'da tek şehir, taşınma UI'si v1.1'de).
- [ ] Kozmetik sistemi: 8 ayakkabı/kıyafet, para ile açılır.
- [ ] Günlük 3 görev, gece yarısı sıfırlama (cihaz saati manipülasyonuna
      karşı: en son görülen tarih geri giderse sıfırlama yok).
- [ ] Mesafe kilometre taşları ve ödülleri.

**2c — Kabuk · 1–2 hafta**
- [ ] Yükseltme ekranı (4 dal, seviye göstergesi, maliyet, "yetmiyor"
      durumu), çevrimdışı gelir karşılama ekranı.
- [ ] Ana menü, ayarlar (ses, titreşim, offset), gizlilik linkleri.
- [ ] Yerelleştirme altyapısı: EN + TR.

### Faz 3 — Yayın altyapısı (1–2 hafta)

- [ ] AdMob (Unity plugin) + UMP onam akışı. Yerleşimler
      `ADS_POLICY.md`'ye göre.
- [ ] Firebase Analytics: run_start, run_end (mesafe, ölüm sebebi, max
      çarpan), ad_shown, ad_rewarded, iap_purchase, calibration_offset,
      **upgrade_purchased (dal, seviye, o ana kadarki toplam para),
      offline_income_claimed, currency_balance (günlük snapshot)**.
- [ ] IAP: reklamsız + başlangıç paketi (Google Play Billing).
- [ ] Play Console listing (stüdyo hesabı `yilkgamesstudio@gmail.com`),
      Data Safety formu, gizlilik/hesap silme adresleri (stüdyo geneli
      adresler — oyuna özel sayfa açılmaz).
- [ ] Firebase API key kısıtlaması — proje kurulur kurulmaz, ertelenmez.
- [ ] Görsel varlıklar `docs/store-assets-originals/` (gitignore'lu) +
      `Eren-Ozcan/pictures` reposunda `pictures/mind-the-crack/`.

**Not:** 12 test kullanıcısı / 14 gün şartı bu oyun için **geçerli değil** —
stüdyo hesabının üretim erişimi zaten var (Çengel Bulmaca ve Reefy üzerinden
onaylandı). Yine de kapalı test yapılacak, çünkü cihaz çeşitliliğinde ses
gecikmesi ancak böyle görülür.

### Faz 4 — Test ve ölçüm (3–4 hafta)

- [ ] Kapalı test: en az 10 farklı Android cihaz, odak ses gecikmesi
      (cihaz matrisi ve protokol: `test-plan.md` §3).
- [ ] Ölçüm kohortu: ≥ 900 install, **organik** — ücretli kampanya yok.
      Kapı tarihe değil örneklem büyüklüğüne bağlı; 900'e ulaşılana kadar
      retention kararı verilmez (`test-plan.md` §6).
- [ ] Okunacak metrikler: D1, D7, oturum süresi, run/oturum, ölüm mesafesi
      histogramı, kalibrasyon offset dağılımı, ödüllü izleme oranı,
      **yükseltme ağacında takılma seviyesi, para bakiyesi dağılımı
      (biriken para = sink yetersiz), çevrimdışı gelir dönüş oranı**.
- [ ] Kapı: D1 ≥ %35 **ve** D7 ≥ %15. Altındaysa v1.1'e geçilmez — D1
      düşükse çekirdek, D7 düşükse ekonomi eğrisi revize edilir.

### Faz 5 — v1.1 (kapı geçilirse, 4–5 hafta)

İstanbul + ilk gerçek taşınma (prestij UI'si), yonca, günlük meydan okuma
(yükseltmeler normalize), paylaşım klibi, 2 karakter, 2 zemin, 2 engel,
para paketi IAP.

## 2. Toplam takvim

| Faz | Süre | Kümülatif |
| --- | --- | --- |
| 0 — dikey dilim | 1–2 hafta | 2 |
| 1 — çekirdek | 2–3 hafta | 5 |
| 2 — meta, ekonomi, kabuk | 4–5 hafta | 10 |
| 3 — yayın altyapısı | 1–2 hafta | 12 |
| 4 — test | 3–4 hafta | 16 |

v1.0 yayını için gerçekçi aralık: **4,5–5 ay.**

Yükseltme + prestij yapısı takvime ~3 hafta ekledi. Karşılığında beklenen:
D7 %12 → %15+, ARPDAU 0,05–0,08 → 0,12–0,20 USD. Pazar verisine göre
hybrid casual, hypercasual'ın ~5 katı ARPDAU üretiyor — bu 3 hafta,
oyunun ölçülebilir ürün olup olmamasının farkı.

## 3. Varlık listesi (v1.0)

| Kategori | Adet | Not |
| --- | --- | --- |
| Karakter modeli | 1 + 8 kozmetik varyant | Low-poly, tek rig — **Blender'da kendi üretimi** |
| Animasyon | yürüme, uzun adım, zıplama, tökezleme, 4 ölüm | 8 klip |
| Zemin | 3 tür × 3 varyasyon | Tile bazlı, tekrar eden |
| Engel | 2 | Su birikintisi, köpek kakası |
| Müzik | 5 BPM × 4 stem | 20 loop, orijinal. Kaynak kararı ertelendi (ücretsiz/CC0 araştırması, olmazsa AI); Faz 0–2 placeholder metronom |
| SFX | ~15 | Adım, ding, seri kırılma, ölümler, UI |
| UI | menü, HUD, run sonu, mağaza, **yükseltme ağacı**, **çevrimdışı gelir**, ayarlar, kalibrasyon | 8 ekran |
| Yükseltme ikonu | 4 dal × 10 seviye durumu | Dal başına 1 ikon + seviye çerçevesi yeterli |
| Store görseli | ikon, feature graphic, 6 ekran görüntüsü, tanıtım videosu | pictures reposuna. **ASO/mağaza planı oyun bitince yapılacak**; Faz 3'te sadece zorunlu alanlar |

## 4. Verilen kararlar (22 Eylül 2026)

| Konu | Karar | Sonucu |
| --- | --- | --- |
| Görsel üretim | **Blender** — karakter, animasyon, zemin, engeller kendi içinde | Asset store/freelance bütçesi yok; low-poly stil zaten bu üretime uygun |
| Müzik | **En sona bırakıldı.** Sırası gelince önce ücretsiz/CC0 stem araştırması, olmazsa AI üretim | Faz 0–2 placeholder metronomla ilerler |
| Test bütçesi | **Yok** — ücretli UA kampanyası yapılmayacak | Ölçüm stratejisi değişti, bkz. `test-plan.md` §6; v1.0'da A/B testi yapılmayacak |
| iOS | **Sonra** | v1.0 yalnızca Android |
| Mağaza/ASO planı | **Oyun bittikten sonra** | Faz 3'te sadece zorunlu listing alanları doldurulur |
| Ekonomi modeli | **Kuruldu ve doğrulandı** | `docs/economy.md` + `tools/economy_sim.py`, 4/4 hedef kontrolü OK |
| İlk oturum akışı | **Tanımlandı** | `docs/onboarding.md` — kalibrasyon ilk açılıştan çıkarıldı |
| Test planı | **Yazıldı** | `docs/test-plan.md` — 5 katman, cihaz matrisi, kapılar |

Açık kalan tek şey: **müzik kaynağı**, ve o bilinçli olarak Faz 2 sonuna
kadar ertelendi.

## 5. Bu depoya dair kurallar

- Store/pazarlama görselleri **bu repoya commit edilmez**. Yerel:
  `docs/store-assets-originals/` (gitignore'lu). Kalıcı: private
  `Eren-Ozcan/pictures` reposunda `pictures/mind-the-crack/`.
- Google/Play/AdMob/Firebase paneline girmeden önce aktif hesap doğrulanır;
  beklenen hesap `yilkgamesstudio@gmail.com`. Adresler ve tuzaklar:
  `C:\Projects\pictures\STUDIO.md`.
- Reklam yerleşimi eklemeden önce `C:\Projects\pictures\ADS_POLICY.md`
  okunur.
