# Mind the Crack — Oyun Tasarım Dokümanı (v2)

Bu doküman ilk taslağın üzerine kurulu. Değişen yerler **[DEĞİŞTİ]** ile
işaretli; gerekçeler `market-research.md` ve aşağıdaki bölümlerde.

**Tek cümle:** Kaldırımda müziğin vuruşuyla yürüyen karakteri, taş birleşim
çizgilerine bastırmadan olabildiğince uzağa götür.

**Tür etiketi (mağaza):** arcade / zamanlama. "Ritim oyunu" olarak
etiketlenmeyecek — ritim oyunu kategorisi lisanslı müzik kataloglarının
alanı, orada görünürlük alınamaz.

**Platform:** v1.0 yalnızca Android. iOS sonraya bırakıldı (karar
22 Eyl 2026) — Apple Developer hesabı, ATT onamı ve ayrı ses gecikmesi
profili ayrı bir iş kalemi.

**İlgili dokümanlar:** `economy.md` (ekonomi sayıları — tek doğru kaynak),
`onboarding.md` (ilk 60 saniye), `test-plan.md`, `market-research.md`,
`roadmap.md`.

---

## 1. Çekirdek oynanış

### 1.1 Adım modeli

Karakter otomatik yürür. Her müzik vuruşu bir adımdır. Oyuncu adımın
**türünü** ve **zamanlamasını** seçer.

| Girdi | Hareket | Mesafe |
| --- | --- | --- |
| Hiçbir şey | Normal adım | 1,0 birim |
| Tap | Uzun adım | 1,5 birim |
| Yukarı swipe | Zıplama | 2,5 birim |

### 1.2 Zamanlama penceresi **[DEĞİŞTİ — kritik]**

İlk taslakta girdi zamanlaması hiçbir şeyi etkilemiyordu; oyuncu vuruşlar
arasında istediği an dokunsa aynı sonucu alıyordu. Bu durumda oyun ritim
oyunu değil, sadece müzikli bir oyun olur. Düzeltme: girdi vuruşa göre
değerlendirilir.

| Sapma (vuruş anına göre) | Sonuç |
| --- | --- |
| ≤ ±60 ms | Perfect zamanlama |
| ≤ ±120 ms | Geçerli |
| > ±120 ms | Girdi yok sayılır → normal adım atılır (çoğu zaman ölüm) |

Pencereler zorluk eğrisine göre daralır: ilk 3 run'da ±200 ms, sonrasında
yukarıdaki değerler. Değerler Unity tarafında ScriptableObject ile ayarlanır,
kodda sabit yazılmaz — test sonrası en çok oynanacak sayılar bunlar.

### 1.3 Perfect tanımı **[DEĞİŞTİ]**

Perfect = **hem** ayak taşın orta %40'ına iniyor **hem de** girdi ±60 ms
içinde. İkisinden biri eksikse "İyi" sayılır (puan var, çarpan yok).

| Sonuç | Koşul | Puan |
| --- | --- | --- |
| Perfect | Orta %40 + ≤60 ms | 3 × çarpan |
| İyi | Taşın üstünde | 1 |
| Tökezleme | Çizgiye çok yakın (çizginin ±%5'i) | 0, bir vuruş kaybı |
| Ölüm | Ayak çizgide | Run biter |

Çarpan: ardışık Perfect sayısına göre 1× → 1,5× → 2× → 3× (5, 10, 20
Perfect'te). Seri kırılınca 1×'e döner.

**Girdisiz adımın zamanlaması** (Faz 0'da verilen karar): oyuncu hiçbir şey
yapmadığında adım vuruşun kendisi tarafından atılır, dolayısıyla zamanlaması
tanım gereği Perfect sayılır. Sadece oyuncunun talep ettiği bir adım
(tap/swipe) mistimed olabilir. Alternatif — girdisiz adım asla Perfect
olamaz — öğreticinin ilk 6 saniyesini ödülsüz bırakıyor ve "hiçbir şey
yapma" seçeneğini cezalandırıyordu. Faz 0 testinde "hiçbir şey yapmamak
fazla güvenli hissettiriyor" geri bildirimi gelirse yeniden açılacak.

### 1.4 Önizleme

Sonraki 2 adımın ineceği yer ekranda gölgeyle gösterilir (ilk taslakta 1
adımdı — 2 adım, uzun adım/zıplama kombinasyonlarının planlanabilmesi için
gerekli). Gölge rengi: güvenli = beyaz, çizgiye denk geliyor = kırmızı.

### 1.5 İlk ölüm yumuşatması **[DEĞİŞTİ]**

Anında ölüm hypercasual'da D1'i düşürüyor. Her run'da 1 "tökezleme" hakkı
var: ilk çizgi teması ölüm değil, sendeleme + çarpan sıfırlanması. İkincisi
öldürür. Yonca (v1.1) bunun üstüne gelir, yerine geçmez.

---

## 2. Ritim sistemi

- Başlangıç 100 BPM. Her 30 saniyede +6 BPM, 160 BPM'de tavan.
- Yürüme hızı BPM'e bağlı; bir adım her zaman tam bir vuruş.
- Müzik katmanları Perfect serisiyle açılır: 0–4 seri = davul + bas,
  5–9 = + perküsyon, 10–19 = + melodi, 20+ = + hook. Seri kırılınca en alt
  katmana düşülür (sert kesme değil, 1 bar içinde fade).
- **Tüm müzik orijinal.** Lisanslı şarkı kullanılmayacak (maliyet + mağaza
  riski). Katmanlı stem'ler olarak üretilir, hepsi aynı BPM ızgarasında.
- **Kaynak kararı (22 Eyl 2026): en sona bırakıldı.** Sıra geldiğinde önce
  ücretsiz/CC0 stem kütüphaneleri araştırılır, olmazsa AI üretim (Suno/Udio
  benzeri) denenir. Lisansın mobil mağaza dağıtımını ve reklam
  kreatiflerinde kullanımı (Content ID temiz) kapsaması şart. Faz 0–2
  boyunca placeholder metronom + tek loop yeterli.
- BPM artışı müzikte tempo değişimi olarak değil, **ayrı BPM'lerde
  hazırlanmış loop setleri** arasında geçişle yapılır (pitch shift kalitesi
  düşürür). 100/115/130/145/160 BPM için 5 set.

### 2.1 Teknik senkronizasyon (uygulama şartı)

- Zaman kaynağı `AudioSettings.dspTime`, asla `Time.time`.
- Müzik `AudioSource.PlayScheduled` ile başlatılır.
- Audio Settings → DSP Buffer Size = **Best latency**; klipler
  **Decompress On Load**.
- Unity UI Button kullanılmaz (callback basışta değil bırakışta gelir);
  girdi `Input`/`InputSystem` ile basış anında okunur.
- **Kalibrasyon iki aşamalı** (karar değişti — ilk açılışta zorunlu ekran
  D1'i düşürüyor, bkz. `onboarding.md` §4): (1) öğretici run'ındaki ilk 8
  geçerli girdiden sessiz tahmin, varsayılan offset olarak kabul edilir;
  (2) tahmini sapma 40 ms'yi aşıyorsa ilk run sonunda kalibrasyon teklifi.
  Ayarlardan her zaman erişilebilir ve elle değiştirilebilir. Android'de
  cihaz gecikmesi 0,01–0,2 sn aralığında değişiyor; offset opsiyonel değil,
  ama onu isteme biçimi oyunun içinde olmalı.

---

## 3. Zemin üretimi

### 3.1 Zemin türleri (v1.0 için 3 tanesi)

| Zemin | Taş uzunluğu | Not | Sürüm |
| --- | --- | --- | --- |
| Kare beton | 1,0 birim, sabit | Öğrenme alanı | v1.0 |
| Parke taş | 0,5 birim, sabit | Sık uzun adım gerekir | v1.0 |
| Altıgen karo | 0,8–1,4 birim, değişken | Okuma ister | v1.0 |
| Kırık beton | değişken + ek çatlaklar | Zor | v1.1 |
| Rögar kapağı | 2,0 birim güvenli ada | Özel olay | v1.1 |

### 3.2 Üretim algoritması **[DEĞİŞTİ — çözülebilirlik garantisi]**

Taşları rastgele dizip sonra "çözülebilir mi" diye kontrol etmek yerine
**ters üretim** yapılır:

1. Zorluğa göre bir adım dizisi seç (örn. `[1.0, 1.5, 1.0, 2.5, 1.0]`).
   Zorluk = uzun adım/zıplama oranı ve ardışık zor adım sayısı.
2. Adım dizisini kümülatif topla → ayağın ineceği kesin pozisyonlar.
3. Taş sınırlarını (çizgileri) bu pozisyonların **arasına** yerleştir;
   her iniş noktası bir taşın orta %40'ına denk gelecek şekilde hizala.
4. Seçilen zemin türünün taş uzunluğu kısıtına göre gerekirse ara taşlar
   ekle — ama iniş noktalarına dokunma.

Böylece her dizi tanım gereği en az bir geçerli çözüme sahip.

**Çözülebilirlik yetmez — gereklilik de şart [Faz 0'da bulundu].** İlk
uygulamada geniş boşluklara konan dolgu taşları yaklaşık 1,0 aralıklarla
diziliyordu; sonuç olarak hiçbir şey yapmayan oyuncu o taşların üstünde
sürüklenip gidiyordu. Cihaz testinde karakter **hiç dokunulmadan 1000 metre
yürüdü ve 1600 puan topladı**. Amaçlanan yol tek geçerli yol değildi, hatta
en kolayı bile değildi.

Kural eklendi: **normal adımdan uzun bir adımın amaçlandığı her yerde,
önceki iniş noktasından itibaren her tam 1,0 birimde bir çizgi vardır.**
Yani "hiçbir şey yapma" seçeneği doğrudan çizgiye götürür. Sadece düz normal
adımlarda ortaya güvenli çizgi konur.

Doğrulama (`Mind the Crack > Validate Generator`): hiçbir şey yapan oyuncu
500 seed'in hepsinde ölüyor, ortalama 4,1 adımda. Aynı anda amaçlanan yol
3,48 milyon adımda %100 güvenli ve Perfect'e uygun kalıyor.

**Güvenli açılış:** ilk 8 metre %100 normal adım. Zorluk eğrisi 0 metreden
başlayınca ilk adımların beşte biri uzun adım istiyordu ve hiçbir şey
öğretilmemiş oyuncu 4 adımda ölüyordu — `onboarding.md` §2'nin istediği
"ilk saniyeler kaybedilemez" buradan başlıyor.

Zorluk eğrisi: mesafeye göre `difficulty = clamp01(distance / 400)`. Bu değer
uzun adım oranını %20 → %55, zıplama oranını %5 → %25 çeker.

---

## 4. Engeller

| Engel | Etki | Sürüm |
| --- | --- | --- |
| Su birikintisi | Basınca kayarsın, sonraki adım zorunlu uzun adım | v1.0 |
| Köpek kakası | Anında ölüm (tökezleme hakkını yakar) | v1.0 |
| Düşmüş dondurma | Bir vuruş yapışırsın (zorunlu bekleme) | v1.1 |
| Yavaş yürüyen teyze | Bir vuruş beklemek zorundasın | v1.1 |

Zorunlu bekleme beat'i ritmik olarak iyi bir fikir ama üretim algoritmasında
ekstra durum demek; v1.1'e alındı.

---

## 5. Batıl inanç ölümleri

Çizgiye basınca rastgele biri tetiklenir, 1–2 sn. v1.0 için 4 tane:

1. Kara kedi önünden geçer, karakter donar.
2. Tepeden saksı düşer.
3. Merdiven üstüne devrilir.
4. Cebinden düşen ayna kırılır.

Aynı ölüm arka arkaya iki kez gösterilmez. Ölüm animasyonu **atlanabilir**
(ekrana dokunma) — tekrar deneme hızı D1 için animasyon komikliğinden daha
değerli. Animasyon uzunluğu 1,2 sn tavan.

---

## 6. Karakterler

Karakterler oynanışı değiştirir (kozmetik değil).

| Karakter | Normal adım | Not | Sürüm |
| --- | --- | --- | --- |
| Standart | 1,0 | Başlangıç | v1.0 |
| Uzun bacaklı | 1,3 | Büyük taşta rahat, parkede zor | v1.1 |
| Çocuk | 0,7 | Sık taşta avantajlı | v1.1 |
| Köpek | 4 ayak, 2 iniş noktası | Zor mod, ayrı üretim kuralı | v1.2 |

v1.0'da sadece standart karakter + kozmetik ayakkabı/kıyafet olacak. Farklı
adım uzunluğu, üretim algoritmasının karakter bazlı test edilmesini
gerektiriyor — meta hazır olmadan açılmaz.

---

## 7. Şehirler ve prestij **[DEĞİŞTİ — kozmetik değil, ilerleme yapısı]**

Şehirler artık v1.2'ye ertelenmiş tema paketi değil, **prestij döngüsünün
kendisi**. Bir şehri bitirince "taşınırsın": yükseltmeler sıfırlanır, kalıcı
çarpan kazanılır, yeni kaldırım deseni + müzik seti + yerel engel açılır.

| Şehir | Kaldırım | Yerel engel | Prestij çarpanı | Sürüm |
| --- | --- | --- | --- | --- |
| Başlangıç (jenerik) | Kare beton, parke, altıgen | Su birikintisi, köpek kakası | 1,0× | v1.0 |
| İstanbul | Arnavut kaldırımı | Sokak kedisi, simitçi tezgahı | 1,5× | v1.1 |
| Londra | Islak taş | Bol su birikintisi, otobüs durağı | 2,25× | v1.2 |
| Tokyo | Dar düzenli karo | Yaya akışı | 3,4× | v1.2 |
| Paris | Altıgen karo | Kafe masası | 5,0× | v1.2 |

Taşınma koşulu: o şehirde toplam X metre + yükseltme ağacının belirli bir
seviyesi. Çarpan üstel (×1,5) ilerler, böylece her taşınma bir öncekinden
hissedilir derecede hızlı olur — Hooked Inc'in okyanus bölgeleriyle aynı
mantık.

v1.0'da tek şehir var ama **prestij altyapısı v1.0'da kurulur** (kayıt
formatı, çarpan alanı, taşınma ekranı iskeleti). Sonradan eklenirse mevcut
oyuncuların ekonomisi bozulur.

---

## 8. İlerleme, ekonomi ve retention **[DEĞİŞTİ — Hooked Inc yapısı]**

Pazar verisi net: meta'sız hypercasual 2026'da ölçülebilir ürün değil.
Hybrid casual ARPDAU'su hypercasual'ın ~5 katı. Yapı üç katmanlı:

1. **Run (beceri)** — dokunulmaz. Yükseltmeler çekirdek beceriyi etkilemez.
2. **Yükseltme ağacı** — run'dan kazanılan parayla kalıcı ilerleme.
3. **Prestij (taşınma)** — §7.

### 8.1 Değişmez kural — yükseltme neye dokunur, neye dokunmaz

Ritim oyununda satın alınabilir isabet, oyunu beceri oyunu olmaktan çıkarır
ve günlük meydan okuma ile sıralamayı anlamsızlaştırır.

| Yükseltilebilir | Yükseltilemez |
| --- | --- |
| Para çarpanı | Zamanlama penceresi (±60 / ±120 ms) |
| Mıknatıs yarıçapı | Perfect için taş ortası toleransı (%40) |
| Başlangıç avansı (ilk N metre atlanır) | BPM rampası |
| Ekstra tökezleme hakkı (maks 2) | Engel sıklığı, üretim zorluk eğrisi |
| Yonca kapasitesi, pasif gelir | Adım mesafeleri (1,0 / 1,5 / 2,5) |

Günlük meydan okumada **tüm yükseltmeler normalize edilir** — herkes aynı
temel değerlerle oynar. Aksi halde sıralama, beceriyi değil oynama süresini
ölçer.

### 8.2 Yükseltme ağacı — 4 dal × 10 seviye

| Dal | Etkisi | Not |
| --- | --- | --- |
| **Ayakkabı** | Para çarpanı +%10/seviye | Ana ekonomi dalı |
| **Şans** | Mıknatıs yarıçapı, yonca düşme şansı | v1.1'de yonca ile tam açılır |
| **Dayanıklılık** | Başlangıç avansı, ekstra tökezleme (sv. 5 ve 10) | Yeni oyuncuya hissedilir rahatlama |
| **Mahalle** | Pasif gelir (çevrimdışı), sandık hızı | Geri dönüş sebebi |

Maliyet eğrisi üstel: `maliyet(n) = taban × 1,6^n`. Taban her dalda farklı.
Hedef eğri: ilk 3 seviye ilk oturumda alınabilir, 10. seviye prestij olmadan
alınamaz (taşınmaya zorlar).

### 8.3 Çift para birimi

| Para | Kaynak (faucet) | Harcama (sink) |
| --- | --- | --- |
| **Bozuk para** (yumuşak) | Run içi toplama, run sonu ödülü, günlük görev, ödüllü reklam 2× | Yükseltme ağacı, kozmetik |
| **Yonca** (sert) | Ödüllü reklam, günlük görev, nadir run düşüşü, IAP | Ölüm affı, yükseltme hızlandırma |

**Kural:** her faucet'in bir sink'i olmalı. Açılacak şey bitince para
anlamsızlaşır ve run sonu ödülü ödüllendirici hissettirmez.

Yonca v1.1'de devreye giriyor ama **ekonomi v1.0'da çift para birimine göre
tasarlanır**; sonradan ikinci para birimi eklemek tüm fiyat dengesini bozar.

### 8.4 Çevrimdışı gelir (kısıtlı idle)

Kurgu: açtığın kozmetikleri giyen NPC'ler sen yokken kaldırımda yürür ve az
miktar bozuk para biriktirir. **Tavan 2–4 saat** (Mahalle dalıyla uzar).
Amaç oyunu idle'a çevirmek değil, geri dönüş sebebi yaratmak. Oyunun
kendisi beceri oyunu olarak kalır.

### 8.5 v1.0'da olacaklar

- Para, yükseltme ağacı (4 dal, 10 seviye), çevrimdışı gelir, kozmetik.
- Günlük 3 görev, gece yarısı sıfırlama.
- Mesafe kilometre taşları (100/250/500/1000 m) — ilk oturumda ≥2 açılmalı.
- Prestij altyapısı (tek şehir, taşınma UI'si v1.1'de).
- Yonca, günlük meydan okuma + sıralama, paylaşım klibi → v1.1.

### 8.6 Ekonomi dengeleme — ayrı iş kalemi

Tek kişilik ekipte en çok hafife alınan iş. Sayılar kodda değil, bir
tabloda (Google Sheets / CSV) kurulur ve Unity'ye ScriptableObject olarak
aktarılır. Modellenecek: ortalama run süresi × run başına para × oturum
sayısı → hangi günde hangi seviyeye ulaşılıyor. Hedef eğri: 1. gün 3–4
yükseltme, 7. gün ilk taşınma.

### 8.7 Günlük meydan okuma (v1.1)

Herkese aynı seed, tek deneme, yükseltmeler normalize. Seed = tarih; üretim
algoritması deterministik olduğu için sunucu gerekmez, sadece skor tablosu
gerekir (Firebase).
---

## 9. Paylaşılabilirlik (v1.1)

Ölümün son 5 saniyesi kaydedilir, tek tuşla paylaşılır. Skor ekranında komik
metin: "Annemin sırtını 347 metre korudum." Metinler mesafeye göre havuzdan
seçilir, 20 varyant.

Not: ekran kaydı Unity'de `Recorder` paketi mobilde ağır; alternatif olarak
son 5 saniyenin **girdi kaydından** yeniden oynatılıp kaydedilmesi daha ucuz.
Karar v1.1'de, prototiple ölçülerek verilir.

---

## 10. Monetizasyon

Detaylı gerekçe `market-research.md` §5'te.

| Yerleşim | Kural | Sürüm |
| --- | --- | --- |
| Ödüllü — devam et | Ölünce, oturum başına 1 kez | v1.0 |
| Ödüllü — parayı 2×'le | Run sonu ekranı | v1.0 |
| Geçiş reklamı | 3–4 ölümde bir; **ilk 3 run'da asla**; 90 sn soğuma | v1.0 |
| Reklamsız + başlangıç paketi | Tek seferlik ~4,99 USD, abonelik yok | v1.0 |
| Ödüllü — çevrimdışı geliri 2×'le | Oyuna dönüşte, günde 2 kez | v1.0 |
| Ödüllü — yükseltme indirimi | Bir sonraki yükseltme %30 ucuz, günde 1 kez | v1.0 |
| Para paketi IAP | Bozuk para + yonca paketleri | v1.1 |

Yükseltme ağacı üç yeni ödüllü reklam yerleşimi doğuruyor (çevrimdışı gelir
2×, yükseltme indirimi, run sonu para 2×). Bunlar ARPDAU'nun asıl kaldıracı:
oyuncu reklamı ceza olarak değil, ilerlemeyi hızlandıran araç olarak görür.
Toplam ödüllü yerleşim sayısı 5'i geçmemeli — geçerse oyun reklam
kliklemeye dönüşür ve `ADS_POLICY.md` sınırları zorlanır.

Stüdyonun bağlayıcı reklam kuralları için `C:\Projects\pictures\ADS_POLICY.md`
okunacak — buradaki tablo ona aykırı olamaz.

---

## 11. Görsel ve ses yönü

- Kamera: arkadan-yukarıdan, ~35° eğim. Hem karakter hem önündeki 4–5 taş
  okunmalı.
- Stil: low-poly, pastel palet. **Okunabilirlik her şeyden önce**: taş
  birleşim çizgileri paletin en koyu değeri, taş yüzeyi en açık değeri.
  Renk körlüğü modu gerekmez çünkü ayrım renk değil kontrast üzerinden.
- Ses: her adımda vuruşa oturan tık, Perfect'te "ding", seri kırılınca kısa
  düşüş sesi, ölümde komik efekt.
- Titreşim (haptic): Perfect'te hafif, ölümde sert. Ayarlardan kapatılabilir.

---

## 12. Sürüm planı **[DEĞİŞTİ — meta öne çekildi]**

| Sürüm | İçerik | Hedef |
| --- | --- | --- |
| **v0.1 dikey dilim** | Gri kutular, tek zemin, metronom, ölüm yok, meta yok | "Eğlenceli mi?" sorusunun cevabı |
| **v1.0 (MVP)** | 3 zemin, 2 engel, 4 ölüm, standart karakter, kalibrasyon · **yükseltme ağacı (4 dal × 10 sv.), çift para birimi, çevrimdışı gelir, prestij altyapısı** · günlük görev, kilometre taşları · tüm reklam yerleşimleri | D1 ≥ %35, D7 ≥ %15 |
| **v1.1** | İstanbul (ilk gerçek taşınma) + taşınma UI'si, yonca, günlük meydan okuma, paylaşım klibi, 2 karakter, 2 zemin, 2 engel | D7 ≥ %20, ARPDAU ≥ 0,12 USD |
| **v1.2** | Londra/Tokyo/Paris, köpek karakteri, skin mağazası, sezonluk etkinlik | LiveOps |

**D7 hedefi yükseldi** (%12 → %15) çünkü v1.0 artık yükseltme ağacı ve
çevrimdışı gelirle geliyor; meta varsa D7 beklentisi de yükselir. Hybrid
casual bandı %15–22.

v1.1'e geçmeden önce v1.0 verisine bakılır: D1, D7, oturum süresi,
run/oturum, **yükseltme ağacında kaçıncı seviyede takılındığı**, çevrimdışı
gelir için dönüş oranı, ödüllü reklam izleme oranı, hangi mesafede ölündüğü
histogramı.
