# Setup Battle

## 1. Buat asset ScriptableObject default

1. Tunggu Unity selesai compile.
2. Jalankan `Tools > Brewmasters > Create or Refresh Default Battle Assets`.
3. Asset akan dibuat di `Assets/GameData/Battle`.
4. Buka setiap asset karakter, lalu pasang `Portrait` dan `Battle Prefab` bila diperlukan.

Asset yang dibuat:

- `Rei` dan `Kayla` (`BattleCharacterData`)
- `GardenOverflow` dan `InsomniacPhantom` (`BattleCharacterData`)
- `FirstBattleEncounter` dan `SecondBattleEncounter` (`BattleEncounterData`)

Menu refresh mengembalikan angka combat ke default GDD, tetapi tidak menghapus referensi Portrait dan Battle Prefab.

## 2. Konfigurasi BattleManager

Pada object `BattleManager` di `Scene_Garden`, pasang:

- `First Battle Encounter` = `FirstBattleEncounter`
- `Second Battle Encounter` = `SecondBattleEncounter`
- `Battle Hud` = komponen `BattleHUD` yang dibuat pada langkah berikutnya
- Pertahankan referensi `Flowchart` dan `After Battle Scene Name` yang sudah ada

Untuk menampilkan prefab karakter di arena secara otomatis:

- Buat transform seperti `ReiSpawn`, `KaylaSpawn`, dan `EnemySpawn`.
- Masukkan ke `Ally Spawn Points` dan `Enemy Spawn Points` sesuai urutan array encounter.
- Pasang prefab pada field `Battle Prefab` milik setiap asset karakter.

Bagian spawn point bersifat opsional. Battle tetap dapat berjalan tanpa prefab arena.

## 3. Hierarchy UI yang disarankan

```text
BattleCanvas (Canvas, CanvasScaler, GraphicRaycaster, CanvasGroup, BattleHUD)
├── TurnQueue
│   ├── QueueSlot0 (BattleTurnQueueSlot)
│   ├── QueueSlot1 (BattleTurnQueueSlot)
│   └── ...
├── PartyPanel
│   ├── AllyView0 (BattleUnitView)
│   └── AllyView1 (BattleUnitView)
├── EnemyPanel
│   └── EnemyView0 (BattleUnitView)
├── InfoPanel
│   ├── CurrentTurnText
│   ├── SkillPointsText
│   └── BattleLogText
├── CommandPanel
│   ├── BasicButton
│   ├── SkillButton
│   ├── UltimateButton0 (BattleUltimateButtonView)
│   └── UltimateButton1 (BattleUltimateButtonView)
├── TutorialPanel
├── TutorialBanner
└── ResultPanel
```

Biarkan `BattleCanvas` selalu aktif. `BattleHUD` menyembunyikannya melalui `CanvasGroup` selama intro Fungus berjalan. Jangan menonaktifkan GameObject `BattleCanvas` dari awal.

Gunakan Canvas Scaler `Scale With Screen Size`. Reference resolution `1920 x 1080` dapat digunakan sebagai titik awal.

## 4. Setup unit card

Pasang `BattleUnitView` pada setiap card ally dan enemy, lalu isi:

- `Content Root`: keseluruhan card, atau kosongkan untuk memakai GameObject yang sama
- `Portrait Image`, Name, Flavor/Weakness, dan Status TMP text
- HP Slider dan HP TMP text
- `Energy Root`, Slider, dan text untuk ally
- `Break Root`, Slider, dan text untuk enemy
- `Target Button` dan `Selected Indicator` untuk enemy

Semua Slider HP, Energy, dan Break harus memakai `Min Value = 0` dan `Max Value = 1`. Script mengirim nilai yang sudah dinormalisasi.

Pada ally card, `Target Button` boleh kosong. Pada enemy card, pasang Button transparan yang menutupi card agar enemy dapat dipilih.

## 5. Setup turn queue

Buat sejumlah slot sesuai panjang preview yang diinginkan. Pasang `BattleTurnQueueSlot` pada setiap slot, lalu isi:

- Name TMP text
- Current Marker TMP text
- Background Image
- Warna ally, enemy, dan current bila ingin diubah

Masukkan semua slot ke `BattleHUD > Turn Queue Slots` sesuai urutan tampil.

## 6. Setup command

Pada `BattleHUD`, pasang:

- Basic dan Skill Button beserta label TMP-nya
- Satu `BattleUltimateButtonView` untuk setiap ally
- Current Turn, Skill Points, dan Battle Log TMP text

Setiap ultimate view membutuhkan Button, label TMP, dan Energy Slider `0..1`.

Tidak perlu mengisi event `OnClick` secara manual. `BattleHUD` mendaftarkan seluruh callback ketika runtime.

## 7. Setup tutorial dan result

`TutorialPanel` adalah popup blocking untuk step penjelasan. Isi title, message, dan Continue Button.

`TutorialBanner` adalah instruksi non-blocking saat player harus memilih Basic, Skill, melakukan Break, atau memakai Ultimate. Isi title dan message.

`ResultPanel` membutuhkan title/message TMP, satu Button, dan label Button. Script otomatis menggantinya antara Continue Story dan Retry.

## 8. Isi seluruh reference BattleHUD

Isi array sesuai urutan encounter:

- `Ally Views`: Rei, lalu Kayla
- `Enemy Views`: sediakan slot sebanyak jumlah enemy maksimum
- `Ultimate Buttons`: Rei, lalu Kayla
- `Turn Queue Slots`: sesuai urutan visual kiri-ke-kanan atau atas-ke-bawah

Pasang CanvasGroup milik UI battle saja ke `Root Canvas Group`.

Masuk Play Mode melalui story flow. Setelah block Fungus Garden selesai, BattleCanvas akan muncul dan encounter pertama memulai tutorial.
