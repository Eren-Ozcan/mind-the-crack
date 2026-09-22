# Mind the Crack — İlk Oturum Akışı

Hypercasual'da D1'i en çok belirleyen şey ilk 60 saniyedir. Bu dosya o 60
saniyeyi saniye saniye tanımlar ve verilen kararları gerekçesiyle yazar.

## 1. Temel kararlar

| Karar | Seçim | Gerekçe |
| --- | --- | --- |
| Açılış | Menü yok, logo beklemesi yok — uygulama açılır açılmaz oyun başlar | Menü ekranı ilk oturumda en yüksek terk noktası |
| Öğretici biçimi | Metin yok, diegetik ikon + gölge | Metin okunmuyor; ayrıca yerelleştirme yükü |
| İlk run | Kaybedilemez (sadece tökezleme) | İlk 30 saniyede ölüm = anında terk |
| Kalibrasyon | İlk açılışta **değil**, ilk run'dan sonra | Karar değişti, §4 |
| İlk reklam | İlk 3 run'da hiç yok | İlk oturumda reklam D1'in en yaygın katili |
| İlk yükseltme | İlk oturumda mutlaka alınmalı | Meta'nın varlığı ilk oturumda hissedilmezse D7 gelmez |

## 2. Saniye saniye ilk run

Müzik ilk kareden itibaren çalıyor, 100 BPM. Karakter zaten yürüyor.

| Süre | Ne oluyor | Oyuncudan beklenen |
| --- | --- | --- |
| 0–6 sn | Geniş kare beton, tüm taşlar 1,0 birim. Ayak gölgesi her adımda taşın ortasına düşüyor. Perfect sesi çalıyor, çarpan yazısı büyüyor. | Hiçbir şey. Sadece ritmi duyuyor. |
| 6–14 sn | Önde geniş bir taş belirir; normal adımla çizgiye denk gelecek. Gölge **kırmızıya** döner. Vuruşla nabız atan TAP ikonu belirir. Pencere ±200 ms. | İlk tap → uzun adım |
| — | Başaramazsa: tökezleme, hız düşer, aynı durum bir sonraki taşta tekrar eder. Ölüm yok, ikon büyür. | Tekrar dener |
| 14–22 sn | Aynı kurgu zıplama için: çok geniş boşluk, yukarı ok ikonu, swipe. | İlk swipe |
| 22–30 sn | İlk paralar belirir — güvenli taşların üstünde, toplaması kolay. İkon yok, para kendini anlatır. | Toplar |
| 30–45 sn | İkonlar kaybolur. Karışık desen başlar (tap + swipe birlikte), ama hâlâ tek tökezleme hakkı geri verilmiştir. Zemin parke taşa geçer. | Artık oynuyor |
| 45 sn+ | Normal oyun. Pencere ±200 ms'den kademeli olarak ±120/±60 ms'ye iner (3 run boyunca). | — |

İlk run **iki tökezlemeyle bitmez** — ölüm ancak 45. saniyeden sonra
mümkün. Bitiş her durumda 60–90 saniye arasında bir ölümle gelir; erken
biterse ikinci run da aynı kurallarla açılır.

### İlk run ekonomisi

Normal bir 70 metrelik run ~23 para verir; Ayakkabı sv1 ise 50. Yani
standart üretimle oyuncu ilk oturumda **hiçbir yükseltme alamaz**. Düzeltme:

- Öğretici run'ında para yoğunluğu 2× (0,44 para/metre).
- Run bitiminde tek seferlik "ilk yürüyüş" ödülü: **40 para**.

Böylece ilk run sonunda ~100 para olur → ilk yükseltme alınır. Bu sayılar
`tools/economy_sim.py`'ın dışında, tek seferlik bonus olarak tutulur.

## 3. İlk run sonrası

| Sıra | Ekran | Not |
| --- | --- | --- |
| 1 | Ölüm animasyonu (1,2 sn tavan, dokununca atlanır) | Batıl inanç ölümlerinden biri |
| 2 | Run sonu: mesafe, para, komik metin | Ödüllü "parayı 2×'le" butonu **burada henüz yok** — ilk 3 run'da reklam yok |
| 3 | **Kalibrasyon teklifi** | §4 |
| 4 | **Yükseltme ekranı zorunlu açılır** | Ayakkabı sv1 üstünde parlayan ok. Satın alınana kadar "Tekrar oyna" butonu ikincil görünümde durur ama basılabilir — zorla tıklatma yok |
| 5 | Tekrar oyna | — |

İkinci run'dan itibaren yükseltme ekranı kendiliğinden açılmaz.

## 4. Kalibrasyon — karar değişti

**Eski plan:** ilk açılışta kalibrasyon ekranı (8 kez metronoma dokun).
**Sorun:** oyunu görmeden istenen bir egzersiz; ilk 20 saniyede terk
sebebi. Ritim oyunu oynamayan biri niye dokunduğunu anlamaz.

**Yeni plan — iki aşama:**

1. **Sessiz tahmin.** Öğretici run'ındaki ilk 8 geçerli girdinin vuruşa
   göre ortalama sapması hesaplanır ve cihaz offset'i olarak **varsayılan**
   kabul edilir. Oyuncu hiçbir şey yapmaz.
2. **Teklif.** İlk run sonunda, sadece tahmini sapma 40 ms'yi aşıyorsa:
   "Ritmi telefonuna ayarlayalım mı?" → Evet/Sonra. Evet derse klasik
   8 vuruşluk kalibrasyon. Sonra derse bir daha sorulmaz; ayarlardan
   her zaman erişilebilir.

Ayarlar ekranında offset elle değiştirilebilir kalır (ritim oyuncuları
bunu arar).

## 5. İlk oturumun tamamı (hedef)

| Zaman | Olay |
| --- | --- |
| 0:00 | Oyun açılır, müzik çalar, karakter yürür |
| 0:08 | İlk tap |
| 0:20 | İlk swipe |
| 0:28 | İlk para |
| 1:00–1:30 | İlk ölüm, ilk komik ceza animasyonu |
| 1:40 | İlk yükseltme satın alınır |
| 2:00–5:00 | 4–5 run daha |
| ~3:30 | İlk geçiş reklamı (4. run sonrası, en erken) |
| 4:00 | İlk günlük görev tamamlanır ("bugün 20 Perfect yap" gibi kolay olan) |
| 5:00 | Oturum biter — hedef ≥ 4 dakika |

## 6. Ölçülecekler (Faz 4)

Bu akışın işleyip işlemediği şu olaylarla anlaşılır:

| Olay | Sorduğu soru |
| --- | --- |
| `tutorial_first_tap` (süre) | Tap ikonu anlaşılıyor mu? Hedef ≤ 12 sn |
| `tutorial_tap_attempts` | Kaç denemede başarıldı? Hedef ortalama ≤ 2 |
| `tutorial_first_swipe` (süre) | Swipe anlaşılıyor mu? |
| `tutorial_completed` | Öğreticiyi bitirme oranı. Hedef ≥ %90 |
| `first_upgrade_purchased` (oturumdaki süre) | İlk yükseltme oluyor mu? Hedef ≥ %70 ilk oturumda |
| `calibration_offered` / `calibration_completed` | Teklif ne sıklıkta çıkıyor, kaçı kabul ediyor |
| `session_end` (run sayısı, süre) | Oturum ≥ 4 dk mı? |

`tutorial_completed` %90'ın altındaysa sorun öğreticide, D1'i düzeltmek
için başka yere bakılmaz.

## 7. Açıkta bırakılanlar

- **Ses kapalı oyuncu.** Telefon sessizdeyse ritim duyulmaz. Çözüm: ekranda
  vuruşla nabız atan ince bir halka (her zaman açık, sadece sessizken
  belirginleşir) + titreşim. Faz 1'de eklenecek, ayrı tasarım gerektirmez.
- **Çok kısa ilk oturum (< 60 sn).** Oyuncu öğreticiyi bitirmeden çıkarsa
  ikinci açılışta öğretici baştan başlar (tamamlanmadıysa tekrar eder).
