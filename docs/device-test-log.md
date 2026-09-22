# Cihaz Test Kaydı

Test planının (`test-plan.md`) B ve D katmanlarının kaydı. Her cihaz turu
buraya yazılır; ses gecikmesi ölçümleri ayrıca `latency-results.md`'ye.

## 2026-09-22 — Faz 0 dikey dilim, ilk cihaz turu

**Cihaz:** Huawei POT-LX1 (P Smart 2019), Kirin 710, Android 10
**Build:** `mindthecrack-phase0.apk`, IL2CPP + ARM64, development build
**Yöntem:** adb ile kurulum, başlatma, logcat, ekran görüntüsü

### Sonuç: çalışıyor

| Kontrol | Durum |
| --- | --- |
| Kurulum | ✅ |
| Açılış, çökme yok | ✅ |
| Unity exception | ✅ yok (logcat temiz) |
| Oyun ilerliyor | ✅ 12 sn'de 13 m, 20 sn'de 29 m — 100 BPM'de beklenen tempo |
| Çizgiler okunuyor | ✅ koyu hatlar net |
| Önizleme işaretleri | ✅ üç seçenek de görünüyor, güvenlik rengine göre beyaz/turuncu/kırmızı |
| HUD | ✅ mesafe, skor, çarpan, sapma, adım türü |
| APK boyutu | 57 MB (development + engine stripping kapalı) |
| Native heap | ~23 MB — rahat |

### Bulunan ve düzeltilen 5 hata

Beşi de **sadece cihazda** ya da sadece çalışan oyunda görünüyordu.

1. **`Can't add component because class 'CapsuleCollider' doesn't exist`**
   IL2CPP fizik modülünü atmış (oyun fizik kullanmıyor), ama
   `GameObject.CreatePrimitive` collider ekliyor. Çözüm: küp/quad mesh'leri
   elle üretiliyor (`Greybox.cs`), fizik modülü build'e hiç girmiyor.

2. **`ArgumentNullException: Parameter name: shader`**
   `Shader.Find("Standard")` build'de null. Sahnedeki hiçbir materyal
   referans vermediği için built-in shader'lar atılmış. Çözüm: kendi
   shader'ımız `Assets/Resources/Greybox.shader`.

3. **Oyun hiç ilerlemiyordu (`0.0 m`).**
   `RunController.OnEnable` vuruş olayına abone oluyordu, ama `OnEnable`
   `AddComponent` içinde çalışıyor — Bootstrap `Conductor`'ı atamadan önce.
   Abonelik sessizce hiç kurulmuyordu. `Start`'a taşındı.

4. **`GUI.skin` null — HUD her karede exception.**
   Engine code stripping varsayılan GUI skin'ini de atmış (2 numara ile aynı
   kök sebep). Geçici çözüm: `PlayerSettings.stripEngineCode = false`.
   **Borç:** HUD gerçek UI olup sahne kurulunca stripping geri açılacak;
   şu an APK'yı şişiriyor.

5. **Çizgiler görünmüyordu.**
   Her taş ayrı yükseltilmiş küptü, çizgi de aralarındaki boşluktu. 29°
   kamera açısında 0,13'lük boşluğu öndeki taşın 0,26 yüksekliği tamamen
   kapatıyor. Boşluğu büyütmek yanlış çözümdü: tek parça kaldırım şeridi +
   üstüne boyanmış koyu çizgiler. Her açıda okunur.

### Ayarlananlar

Karakter 1,1 → 0,75 boy (önündeki iki taşı kapatıyordu — okunması gereken
tam alan), kamera 3,4/-4,2 → 3,9/-4,6, FOV 55 → 46, eğim 29° → 31°,
önizleme işaretleri inceltildi.

### Ölçülemeyenler

- **Ses gecikmesi.** `adb shell input tap` gecikmesi 100–300 ms ve değişken;
  ekrandaki sapma değeri cihazın ses gecikmesini değil adb'ninkini gösterir.
  Gerçek ölçüm elle yapılmalı (`test-plan.md` §3 protokolü).
- **Eğlenceli mi.** Faz 0 kapısı bu ve insan gerektiriyor.
- **Uzun oturum davranışı** (termal kısma, senkron kayması) — 20 saniyelik
  turlarla görülmez.

### Sıradaki

1. Elle oyna: sapma değeri sürekli aynı yöne kayıyor mu? (kalibrasyon
   ihtiyacının ilk işareti)
2. 10 kişilik oynanabilirlik seansı — kapı: ≥6 kişi 5 dakika bırakmamalı.

---

## 2026-09-22 — İkinci tur: ekran kaydı

Aynı cihaz. scrcpy ile 12–14 sn kayıt, ffmpeg ile kare kare inceleme.
(Cihazda `screenrecord` yok — Huawei kaldırmış; scrcpy ile alındı.)

### Bulunan: oyun kendi kendini oynuyordu

Kayıt **1015,5 m → 1027,5 m**, skor 1662 → 1683 gösterdi. Yani oyun on
dakikadır hiç dokunulmadan hayatta ve puan topluyordu. Zorluk eğrisi 400
metrede maksimuma çıktığı halde sabit 1,0'lık adım hayatta kalıyordu.

Sebep: ters üretim **çözülebilirliği** garanti ediyor ama **gerekliliği**
değil. Geniş boşluklardaki dolgu taşları ~1,0 aralıklarla diziliyordu,
hiçbir şey yapmayan oyuncu onların üstünde sürükleniyordu.

Düzeltme ve doğrulama: `game-design.md` §3.2.

### Bunun ortaya çıkardığı ikinci sorun

Düzeltmeden sonra oyun ilk adımdan itibaren öldürmeye başladı — zorluk
eğrisi 0 metreden başlıyordu, yani ilk adımların %20'si zaten uzun adım
istiyordu. `onboarding.md` §2 ise ilk saniyelerin kaybedilemez olmasını
istiyor. İlk 8 metre %100 normal adım yapıldı.

### Doğrulama sonuçları

| Kontrol | Önce | Sonra |
| --- | --- | --- |
| Amaçlanan yol güvenli | ✅ | ✅ 3,5M adım, 0 ihlal |
| Perfect erişilebilir | ✅ | ✅ |
| Hiçbir şey yapan ölüyor | ❌ hiç ölmüyordu | ✅ ortalama 13 adım |

13 adımın 8'i güvenli açılış. Eşik 15 — **pay az**. Güvenli açılışı
uzatmak testi düşürür; bilinçli.

### Son durum

Cihazda `3.0 m, seri 3, PERFECT` — güvenli açılışta art arda Perfect
alınıyor, önizleme normal adımı beyaz, uzun/zıplamayı kırmızı gösteriyor.
Unity hatası yok.
