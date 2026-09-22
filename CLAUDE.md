# Mind the Crack — Proje Notları

Hypercasual zamanlama/arcade oyunu. Kaldırımda müziğin vuruşuyla yürüyen
karakteri, taş birleşim çizgilerine bastırmadan olabildiğince uzağa
götürmek. Unity 6, Android (v1.0'da iOS yok).

## Dokümanlar — önce bunları oku

| Dosya | İçerik |
| --- | --- |
| `docs/game-design.md` | Oyun tasarımı, tüm mekanik kararları |
| `docs/economy.md` | **Ekonomi sayılarının tek doğru kaynağı** |
| `docs/onboarding.md` | İlk 60 saniye, saniye saniye |
| `docs/test-plan.md` | 5 katmanlı test planı, cihaz matrisi, kapılar |
| `docs/roadmap.md` | Fazlar, takvim, verilen kararlar |
| `docs/market-research.md` | Pazar verisi, rakipler, benchmark'lar |
| `tools/economy_sim.py` | Ekonomi simülasyonu — sayı değişince çalıştır |

## Değişmez kurallar

### 1. Yükseltmeler beceriye dokunmaz

Hiçbir yükseltme, satın alma veya ödül şunları değiştiremez: zamanlama
penceresi (±60 / ±120 ms), Perfect için taş ortası toleransı (%40), BPM
rampası, adım mesafeleri (1,0 / 1,5 / 2,5), üretim zorluk eğrisi.

Yükseltmeler yalnızca şunlara dokunur: para çarpanı, mıknatıs yarıçapı,
başlangıç avansı, ekstra tökezleme hakkı, çevrimdışı gelir, yonca.

Bu kural `docs/game-design.md` §8.1'de tablo, `docs/test-plan.md` §2'de
otomatik test. Doküman yeterli değil — testi de korunmalı.

### 2. Ekonomi sayıları kodda sabit yazılmaz

Tüm ekonomi değerleri `docs/economy.md`'den gelir ve Unity'ye
ScriptableObject olarak aktarılır. Sayı değiştirilecekse önce
`python tools/economy_sim.py` çalıştırılır; dört hedef kontrolü de OK
vermeden değer kabul edilmez.

### 3. Zaman kaynağı dspTime

Ritimle ilgili hiçbir yerde `Time.time` / `Time.deltaTime` kullanılmaz.
`AudioSettings.dspTime` + `AudioSource.PlayScheduled`. Unity UI Button
ritim girdisi için kullanılmaz (callback basışta değil bırakışta gelir).

### 4. Store görselleri bu repoya girmez

Yerel: `docs/store-assets-originals/` (gitignore'lu). Kalıcı: private
`Eren-Ozcan/pictures` reposunda `pictures/mind-the-crack/`.

### 5. Hesap doğrulaması

Google/Play Console/AdMob/Firebase paneline girmeden önce aktif hesap
doğrulanır. Beklenen hesap: `yilkgamesstudio@gmail.com`. Adresler ve
tuzaklar: `C:\Projects\pictures\STUDIO.md`. Reklam yerleşimi eklemeden
önce `C:\Projects\pictures\ADS_POLICY.md` okunur.

## Klasör düzeni

```
Assets/Scripts/Rhythm/     Conductor, zamanlama penceresi
Assets/Scripts/Gameplay/   Adım, iniş değerlendirmesi, kaldırım üretimi
Assets/Scripts/Config/     ScriptableObject ayarları
Assets/Scripts/Debug/      Geliştirme HUD'u
docs/                      Tasarım ve plan dokümanları
tools/                     Python yardımcıları (ekonomi simülasyonu)
```

## Faz durumu

Faz 0 (dikey dilim) — devam ediyor. Kapı kriteri: 10 kişiden ≥ 6'sı
5 dakika elden bırakmıyor. Geçilmeden Faz 1'e başlanmaz.
