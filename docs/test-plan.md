# Mind the Crack — Test Planı

Kapsam: v1.0 (Android). iOS sonraya bırakıldı. **Ücretli test trafiği
bütçesi yok** — bu, ölçüm stratejisini kökten değiştiriyor, §6'ya bak.

## 1. Test katmanları

| Katman | Ne zaman | Kim | Neyi cevaplar |
| --- | --- | --- | --- |
| A — Otomatik testler | Her commit | CI yok, elle `python`/Unity Test Runner | Üretim bozuldu mu, kayıt bozuldu mu |
| B — Ses gecikmesi matrisi | Faz 1 sonu, Faz 3 sonu | Eren | Oyun farklı cihazlarda adil mi |
| C — Oynanabilirlik seansı | Faz 0 kapısı, Faz 2 sonu | 8–10 kişi, yanında oturarak | Eğlenceli mi, öğretici anlaşılıyor mu |
| D — Kapalı test (Play Console) | Faz 3 sonu, 2–3 hafta | 12–30 tanıdık | Çökme, cihaz uyumu, uzun vadeli hatalar |
| E — Üretim ölçümü | Yayından sonra sürekli | Organik oyuncular | D1/D7, ekonomi eğrisi, reklam performansı |

## 2. Katman A — Otomatik testler

Bunlar elle test edilemeyecek kadar çok durum içeriyor; test yazılmazsa
hatalar üretimde bulunur.

| Test | Yöntem | Geçme kriteri |
| --- | --- | --- |
| Üretim çözülebilirliği | 10.000 seed × 3 zemin türü üret, her segmentte en az bir geçerli adım dizisi olduğunu doğrula | %100 |
| Üretim determinizmi | Aynı seed iki kez → bayt bayt aynı segment | %100 (günlük meydan okuma buna dayanıyor) |
| Zorluk eğrisi sınırları | 0–2000 m arası uzun adım/zıplama oranları tanımlı aralıkta mı | Sapma yok |
| Kayıt yükle/kaydet | Rastgele 1.000 durum yaz-oku | Kayıpsız |
| Kayıt geri uyumluluk | v1.0 kaydı sonraki şema ile açılır | Çökme yok, eksik alan varsayılana düşer |
| Ekonomi modeli | `python tools/economy_sim.py` | 4 hedef kontrolü de OK |
| Çevrimdışı gelir istismarı | Cihaz saati ileri/geri alınmış senaryolar | Geri alınca gelir verilmez, ileri alınca tavanı aşmaz |
| Yükseltme sınırları | Hiçbir yükseltme zamanlama penceresine/Perfect toleransına yazamaz | Kod seviyesinde test: run parametreleri değişmez |

Son madde önemli: tasarımın en kritik kuralı (§8.1) sadece doküman değil,
**test** olmalı. Aksi halde bir gün birisi "Şans dalı pencereyi 10 ms
açsın" der ve oyunun adaleti sessizce ölür.

## 3. Katman B — Ses gecikmesi matrisi

Ritim oyununun tek varoluşsal riski. Android'de cihaz gecikmesi 0,01–0,2
saniye arasında değişiyor.

### Cihaz matrisi (en az 8 cihaz)

| Sınıf | Örnek | Neden |
| --- | --- | --- |
| Düşük uç, eski Android (9–11) | Android Go, 2–3 GB RAM | En kötü ses gecikmesi burada |
| Orta sınıf Samsung | A serisi | Türkiye'de en yaygın |
| Orta sınıf Xiaomi/Redmi | MIUI ses yolu farklı | MIUI'nin kendi ses işleme katmanı var |
| Amiral gemisi, 120 Hz | Yüksek tazeleme hızı | Kare hızı ile `dspTime` ilişkisi |
| Tablet | 60 Hz, büyük ekran | Düzen kırılması |
| Bluetooth kulaklık takılı | Herhangi biri | BT gecikmesi 100–300 ms, felaket senaryosu |
| Hoparlör vs kablolu kulaklık | Aynı cihaz iki mod | Çıkış yolu değişince offset değişir |
| Düşük pil / termal kısma | Uzun oturum sonrası | Kare düşünce senkron bozuluyor mu |

### Protokol

1. Cihazda test modu açılır (debug menüsü): metronom çalar, oyuncu 20 kez
   vuruşa dokunur.
2. Kaydedilen: her dokunuşun `dspTime` sapması (ms), ortalama, standart
   sapma.
3. Aynı test kulaklıklı/kulaklıksız tekrarlanır.
4. Sonuç tabloya yazılır: `docs/latency-results.md` (cihaz, Android sürümü,
   çıkış yolu, ortalama sapma, std sapma, tarih).

### Geçme kriterleri

| Ölçüt | Eşik |
| --- | --- |
| Kalibrasyon sonrası ortalama sapma | ≤ 25 ms |
| Sapmanın standart sapması | ≤ 30 ms (bu düzelmiyorsa cihaz tutarsız demektir) |
| Otomatik tahminin elle kalibrasyona farkı | ≤ 20 ms |
| Bluetooth'ta oyun | Kalibrasyon sonrası oynanabilir olmalı; olmuyorsa oyun BT tespit edip uyarı göstermeli |

**Kapı:** matristeki cihazların en az 7'sinde kriterler sağlanmadan Faz 4'e
geçilmez. Sağlanamıyorsa çözüm sırası: (1) DSP buffer ayarı, (2) Native
Audio benzeri eklenti, (3) FMOD'a geçiş.

## 4. Katman C — Oynanabilirlik seansı

Sayı değil gözlem üretir. 8–10 kişi, her biri 10 dakika, yanında
oturularak. **Yardım edilmez, soru sorulmaz, sadece izlenir.**

### Faz 0 kapısı (dikey dilim)

Tek soru: eğlenceli mi? Ölçüt: kişi 5 dakika elden bırakmıyor ve en az bir
kez kendiliğinden "bir daha" diyor. 10 kişiden 6'sı bu davranışı
göstermiyorsa çekirdek mekanik değişir, Faz 1'e geçilmez.

### Faz 2 sonu (meta ile birlikte)

| Gözlenecek | Kırmızı bayrak |
| --- | --- |
| İlk tap kaç saniyede geldi | > 15 sn → ikon okunmuyor |
| Tap ikonunu kaç denemede geçti | > 3 → pencere dar veya ikon geç çıkıyor |
| Yükseltme ekranını kendi açtı mı | Hiç açmadıysa meta görünmüyor |
| Hangi dalı ilk aldı | Ayakkabı değilse fiyat sıralaması yanlış |
| Ölünce ne dedi/yaptı | "Haksızlık" tepkisi → üretim veya gecikme sorunu |
| Ölüm animasyonunu atladı mı | Hepsi atlıyorsa animasyonlar uzun |

### Seans sonrası 3 soru (sadece bunlar)

1. Ne yapman gerektiğini nereden anladın?
2. Öldüğünde neden öldüğünü anladın mı?
3. Bir daha oynar mıydın? (Cevaba değil, duraksamaya bak.)

## 5. Katman D — Kapalı test

Play Console kapalı test kanalı, 12–30 tanıdık, en az 2 hafta.

**Not:** 12 test kullanıcısı / 14 gün şartı bu hesapta **geçerli değil** —
stüdyonun üretim erişimi var. Kapalı test buna rağmen yapılır, çünkü cihaz
çeşitliliği ve çökme verisi başka türlü toplanamaz.

| İzlenecek | Kaynak | Eşik |
| --- | --- | --- |
| Çökmesiz oturum | Crashlytics | ≥ %99,5 |
| ANR oranı | Play Console vitals | ≤ %0,47 (Play eşiği) |
| Soğuk açılış süresi | Vitals | ≤ 3 sn orta sınıf cihazda |
| Aşırı pil kullanımı | Vitals | Uyarı yok |
| Kayıt bozulması | Destek geri bildirimi | 0 vaka |
| Ekonomi sapması | `currency_balance` günlük snapshot | Simülasyondan ±%40 içinde |

Son satır kritik: gerçek oyuncuların para bakiyesi simülasyondan çok
saparsa (özellikle yukarı), sink yetersiz demektir — v1.1 beklemeden
maliyet tabanları güncellenir.

## 6. Katman E — Ücretli bütçe olmadan ölçüm

**Gerçeği kabul ederek başlamak gerekiyor:** D1'i ±3 puan hassasiyetle
ölçmek için ~900 install'lık kohort gerekiyor. Organik yeni bir oyun bunu
haftalarca toplayamayabilir. Yani:

- 20–30 kişilik kapalı testten çıkan "D1 %40" sayısı **istatistik değildir**,
  gürültüdür. Karar dayanağı yapılmaz.
- Retention kapıları (D1 ≥ %35, D7 ≥ %15) geçerliliğini koruyor ama
  **ölçülebilir hale gelmeleri zaman alacak**. Kapı tarihe değil, örneklem
  büyüklüğüne bağlanır: kohort 900'e ulaşmadan karar verilmez.

### Bütçesiz strateji

1. **Önce örneklem gerektirmeyen şeyleri düzelt.** Öğretici tamamlama
   oranı, ilk tap süresi, çökme, ses gecikmesi — bunlar 30 kişilik
   örneklemde bile okunur, çünkü aranan etki büyük (ya %90 tamamlıyor ya
   %50).
2. **Üretime çık, biriktir.** v1.0 üretim kanalında yayınlanır, organik
   install birikir. Play Console retention verisi kendiliğinden toplanır.
3. **Organik hızlandırıcılar (para değil emek):** TikTok/Reels'e oyunun
   kendi klipleri (ölüm derlemesi, Perfect serisi), stüdyonun mevcut
   oyunlarında çapraz tanıtım, yilkgames.com'da oyun kartı, Reddit/oyun
   toplulukları.
4. **Kohort eşiği dolunca kapıyı oku.** 900 install'a ulaşıldığında D1/D7
   değerlendirilir ve v1.1 kararı verilir.
5. **Bütçe fikri değişirse:** 300–500 USD'lik tek seferlik TikTok kampanyası
   bu bekleme süresini haftalardan günlere indirir. Şu an planda yok, ama
   kapı kriterleri aynı kalır.

### Örneklem büyüklüğü kılavuzu

| Ölçülen | Beklenen etki | Gereken örneklem |
| --- | --- | --- |
| Öğretici tamamlama (%90 mı %50 mi) | Çok büyük | 30 |
| İlk tap süresi ortalaması | Büyük | 50 |
| Çökme oranı | — | 100+ oturum |
| D1 (±10 puan) | Kaba | ~100 |
| D1 (±3 puan) | Karar için | ~900 |
| İki sürüm A/B farkı (3 puan) | İnce | 2.000+/kol — bütçesiz ulaşılamaz, A/B yapılmayacak |

Son satır bir karar: **v1.0'da A/B testi yapılmayacak.** Yeterli trafik
olmadan A/B, yanlış kararın istatistik kılığına girmesidir.

## 7. Sürüm öncesi kontrol listesi

Her üretim sürümünden önce elle geçilecek:

- [ ] Katman A testleri yeşil
- [ ] Ses gecikmesi matrisi en az 3 cihazda tekrar edildi (regresyon)
- [ ] Yeni kurulum akışı baştan sona oynandı (kayıt silinmiş cihazda)
- [ ] Eski kayıtla açılış denendi (bir önceki sürümün kaydı)
- [ ] Uçak modunda açılış: reklam yok, oyun çalışıyor, çökme yok
- [ ] Ödüllü reklam iptal edilince ödül verilmiyor, kapatılınca oyun donmuyor
- [ ] IAP satın alma + iade senaryosu
- [ ] UMP onam ekranı AB/Türkiye için çıkıyor, reddedince oyun çalışıyor
- [ ] Sessiz modda oynanabiliyor (görsel vuruş halkası + titreşim)
- [ ] Çağrı geldiğinde / uygulama arka plana atıldığında müzik ve senkron
      doğru toparlanıyor
- [ ] Düşük pil / termal kısma altında 10 dakikalık oturum: senkron kaymıyor

Son iki madde ritim oyununa özel ve en çok atlanan yerlerdir.

## 8. Kapılar özeti

| Kapı | Kriter | Geçilmezse |
| --- | --- | --- |
| Faz 0 | 10 kişiden ≥ 6'sı 5 dk bırakmıyor | Çekirdek mekanik değişir |
| Faz 1 | Üretim testleri %100, ilk gecikme matrisi geçti | Motor/ses yolu revize |
| Faz 2 | Ekonomi simülasyonu 4/4 OK, oynanabilirlik kırmızı bayrak yok | Ekonomi veya öğretici revize |
| Faz 3 | Sürüm öncesi kontrol listesi tamam | Yayın yok |
| Faz 4 | Çökmesiz ≥ %99,5 · öğretici tamamlama ≥ %90 · gecikme matrisi 7/8 | v1.1'e geçilmez |
| Faz 4 (geç) | Kohort ≥ 900 iken D1 ≥ %35 **ve** D7 ≥ %15 | D1 düşükse çekirdek, D7 düşükse ekonomi revize |
