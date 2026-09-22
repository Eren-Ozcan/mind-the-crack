# Mind the Crack — Ekonomi Modeli

Bu dosya ekonomi sayılarının **tek doğru kaynağıdır**. Unity tarafında
ScriptableObject'e buradaki değerler aktarılır; kodda sabit sayı yazılmaz.

Model çalıştırılabilir: `python tools/economy_sim.py`. Sayı değiştirileceği
zaman önce simülasyon çalıştırılır, dört hedef kontrolü de OK vermeden
değer kabul edilmez.

## 1. Hedef eğri ve doğrulama durumu

| Hedef | Durum |
| --- | --- |
| 1. gün 3–4 yükseltme alınabilmeli | ✅ 3 |
| ~7. gün ilk taşınma mümkün olmalı | ✅ 8. gün |
| 7. günde hiçbir dal 10. seviyede olmamalı | ✅ hiçbiri |
| Sink kurumamalı (40/40 seviyedeyken taşınma yoksa para yığılır) | ✅ yok |

Taşınma günleri (30 günlük simülasyon): **8, 13, 17, 22, 28** — aralık
5, 4, 5, 6 gün. Aralığın zamanla açılması istenen davranış: her şehir bir
öncekinden biraz uzun sürer, oyun uzun vadede tükenmez.

## 2. Maliyet formülü

```
maliyet(dal, seviye, şehir) = taban[dal] × 1,90^şehir × 1,60^seviye
```

- `seviye` 0 tabanlı (ilk satın alma seviye 0'ın fiyatıdır).
- `şehir` kaçıncı taşınma (0 = başlangıç şehri).
- **1,90 > 1,50** (prestij çarpanı) bilinçli: maliyet gelirden hızlı
  büyür, yoksa her taşınma bir öncekinden kısa sürer ve oyun 3 haftada
  anlamsızlaşır.

### Taban maliyetler ve ilk şehir tablosu

| Dal | Taban | sv1 | sv2 | sv3 | sv4 | sv5 | sv6 | sv7 | sv8 | sv9 | sv10 | Toplam |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Ayakkabı | 50 | 50 | 80 | 128 | 205 | 328 | 524 | 839 | 1342 | 2147 | 3436 | 9.079 |
| Dayanıklılık | 70 | 70 | 112 | 179 | 287 | 459 | 734 | 1174 | 1879 | 3006 | 4810 | 12.710 |
| Mahalle | 90 | 90 | 144 | 230 | 369 | 590 | 944 | 1510 | 2416 | 3865 | 6185 | 16.343 |
| Şans | 120 | 120 | 192 | 307 | 492 | 786 | 1258 | 2013 | 3221 | 5154 | 8246 | 21.789 |

İlk şehirde 40 seviyenin tamamı: **59.921 para**.

Taban sıralaması rastgele değil: Ayakkabı en ucuz çünkü para çarpanı ana
ekonomi motoru — oyuncunun ilk aldığı şey o olmalı. Şans en pahalı çünkü
v1.0'da etkisi en zayıf dal (yonca v1.1'de açılıyor).

## 3. Dal etkileri

| Dal | Seviye başına etki | Seviye 10'da |
| --- | --- | --- |
| **Ayakkabı** | Para çarpanı +%10 | ×2,0 |
| **Dayanıklılık** | Başlangıç avansı +12 m | +120 m; sv5 ve sv10'da ekstra tökezleme hakkı |
| **Mahalle** | Çevrimdışı +6 para/saat, tavan +0,4 saat | 60 para/saat, 6 saat tavan |
| **Şans** | Mıknatıs → toplama verimi +%4 | +%40; v1.1'de yonca düşme şansı |

Çarpanlar çarpımsal birleşir: `para çarpanı = (1 + 0,10×Ayakkabı) ×
(1 + 0,04×Şans) × prestij çarpanı`.

**Hiçbiri zamanlama penceresine, Perfect toleransına, BPM rampasına veya
adım mesafelerine dokunmaz** — tasarım dokümanı §8.1'deki değişmez kural.

## 4. Run ekonomisi

| Değişken | Değer | Not |
| --- | --- | --- |
| Kaldırımdan toplanan | 0,22 para/metre | Yaklaşık her 4–5 metrede bir para |
| Run sonu bonusu | 0,08 para/metre | Ödüllü reklamla 2× |
| Ödüllü 2× izleme oranı | %35 | Varsayım — Faz 4'te ölçülüp güncellenecek |
| Çevrimdışı alım | Günde 2 kez | Tavan Mahalle seviyesine bağlı |

Yeni oyuncu, 70 metrelik bir run'dan yaklaşık **23 para** kazanır. İlk gün
(2 oturum × 5 run) ~230 para → 3 yükseltme.

## 5. Prestij (taşınma)

- **Koşul:** toplam 28/40 seviye. Mesafe şartı **yok** (gerekçe §7).
- **Ödül:** kalıcı para çarpanı ×1,5 (birikimli: 1,5 → 2,25 → 3,38 → …).
- **Sıfırlanan:** tüm yükseltme seviyeleri, mevcut para.
- **Sıfırlanmayan:** kozmetikler, rekor mesafe, günlük görev ilerlemesi,
  prestij çarpanı.
- **Açılan:** yeni şehir — kaldırım deseni, müzik seti, yerel engel.

Taşınma sonrası ilk gün 9–12 yükseltme birden alınır. Bu kasıtlı: prestij
oyunlarında taşınma sonrası ilk saat en tatmin edici andır. Maliyet
tabanının ×1,90 büyümesi bu hızı ikinci günden itibaren normale döndürür.

## 6. Para birimleri ve sink dengesi

| Para | Kaynak (faucet) | Harcama (sink) |
| --- | --- | --- |
| **Bozuk para** | Run içi toplama, run sonu bonusu, günlük görev, çevrimdışı gelir, ödüllü 2× | Yükseltme ağacı (ana sink), kozmetik |
| **Yonca** (v1.1) | Ödüllü reklam, günlük görev, nadir run düşüşü, IAP | Ölüm affı, yükseltme hızlandırma |

### Kozmetik fiyatları (v1.0, 8 parça — ikincil sink)

| Parça | Fiyat |
| --- | --- |
| Ayakkabı × 3 | 250 / 900 / 2.500 |
| Kıyafet × 3 | 400 / 1.400 / 3.500 |
| Şapka × 2 | 1.800 / 6.000 |

Toplam 16.750 para. Yükseltme ağacının yanında bilinçli olarak küçük:
kozmetik ana sink değil, yükseltmeyi geciktirmeyen bir yan hedef.

## 7. Reddedilen varyantlar (tekrar denenmesin)

**Mesafe şartlı prestij** — "o şehirde toplam 12.000 m yürü + 20 seviye"
denendi. Sonuç: oyuncu 40/40 seviyeye ulaşıp harcayacak yer kalmadan
mesafe şartını bekliyor; 19.–22. günlerde bakiye 145.000 paraya yığıldı ve
her yükseltme maksimumdaydı. **Sink kurudu.** Prestij koşulu yalnızca
seviyeye bağlandı.

**Şehir maliyet çarpanı 1,45** — prestij çarpanından (1,5) küçük olduğu
için her taşınma bir öncekinden kısa sürdü (aralık 5 → 4 → 3 → 3 gün) ve
30. günde çarpan 11,4×'e fırladı. 1,90'a çıkarıldı.

**Prestij eşiği 30/40 seviye** — ilk taşınma 9. güne kaydı, 7 gün hedefinin
dışına çıktı. 28'e indirildi.

## 8. v1.0'ın bilinen sınırı

v1.0'da tek şehir var, yani **taşınma yok**. Simülasyon (prestij kapalı)
40/40 seviyeye **15. günde** ulaşıldığını gösteriyor. O noktadan sonra
yükseltme sink'i biter, elde kozmetikler ve skor kovalamak kalır.

Bu v1.0 için kabul edilebilir: v1.0 ölçüm sürümü, ölçülecek pencere D1–D7.
Ama **v1.1 (İstanbul + taşınma) iki hafta içinde yetişmeli**, yoksa erken
oyuncular tükenmiş bir oyunda kalır.

## 9. Varsayımlar — Faz 4'te ölçülüp güncellenecek

Bu sayılar oyun yokken kuruldu, hepsi hipotez:

| Varsayım | Değer | Nasıl ölçülür |
| --- | --- | --- |
| Ortalama ilk gün run mesafesi | 70 m | `run_end.distance` histogramı |
| Beceri artışı | gün başına +%18, tavan 650 m | Gün bazlı ortalama mesafe |
| Oturum/gün | 1. gün 2, sonra 3 | Firebase oturum sayısı |
| Run/oturum | 5 | `run_start` / oturum |
| Ödüllü 2× izleme oranı | %35 | `ad_rewarded` / `run_end` |
| Çevrimdışı alım/gün | 2 | `offline_income_claimed` |

Gerçek veri geldiğinde `tools/economy_sim.py` başındaki sabitler
güncellenir, dört hedef kontrolü tekrar çalıştırılır, maliyet tabanları
gerekiyorsa yeniden ayarlanır.
