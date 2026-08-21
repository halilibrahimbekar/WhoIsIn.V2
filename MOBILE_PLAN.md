# WhoIsInV2 - Mobil Uygulama (Flutter) Ilerleme Plani

Bu dosya Faz 2 kapsaminda Flutter mobil uygulamasinin planlamasini ve ilerlemesini takip eder.
Durumlar: `DONE`, `IN_PROGRESS`, `TODO`, `BLOCKED`.

## Genel Durum
- Son guncelleme: 2026-08-21
- Aktif Faz: Faz 2 - Mobil MVP (kod iskeleti ve temel akislar tamamlandi)
- Onkosul: Backend auth + event + invite/RSVP API'lari calisir durumda (bkz. [progress.md](progress.md)).
- Not: Test yazimi bu asamada kapsam disi (kullanici tercihi); varsayilan `test/widget_test.dart` kaldirildi.
- Dogrulama: `flutter analyze` temiz, `flutter build web --release` basarili.

## 1) Teknik Yigin Kararlari
- Framework: Flutter (stable channel)
- Dil: Dart (null-safety)
- State management: Riverpod
- Networking: Dio (interceptor ile auth header ve 401 refresh akisi)
- Secure storage: `flutter_secure_storage` (access/refresh token)
- Routing: `go_router`
- Form/validasyon: `flutter_form_builder` veya native `Form` + `TextFormField` (basit tutulacak)
- Ortam yonetimi: `--dart-define` ile API base URL (dev/prod)

## 2) Proje Yapisi (Onerilen)
```
mobile/
  lib/
    core/
      network/        (dio client, interceptors, api exceptions)
      storage/         (secure token storage)
      router/          (go_router config, route guard)
      theme/
    features/
      auth/            (login, register, session bootstrap)
      events/          (list, detail, create/edit, status)
      invites/         (invite listesi, RSVP aksiyonlari)
      participants/    (organizer participant yonetimi)
      dashboard/       (organizer summary)
    main.dart
  test/                (simdilik bos - test kapsam disi)
```

## 3) Backend API Kontrati (Mobil Tarafindan Kullanilacak)

### Auth
- POST `/api/auth/register`
- POST `/api/auth/login`
- POST `/api/auth/refresh`
- POST `/api/auth/revoke`

### Events
- GET `/api/events` (public + organizer-owned gorunurluk)
- GET `/api/events/summary` (Authorize, organizer dashboard metrikleri)
- GET `/api/events/{id}`
- POST `/api/events` (Authorize)
- PUT `/api/events/{id}` (Authorize)
- PATCH `/api/events/{id}/status` (Authorize)

### Invite / RSVP / Participant
- POST `/api/events/{id}/invites`
- GET `/api/events/{id}/invites`
- POST `/api/events/{id}/rsvp`
- POST `/api/events/{id}/waitlist/promote`
- GET `/api/events/{id}/participants`
- PATCH `/api/events/{id}/participants/{participantId}`

Not: Web frontend ile ayni DTO/response semasi kullanilacak; kontrat degisirse [progress.md](progress.md) ve bu dosya birlikte guncellenmeli.

## 4) Milestone Checklist

| ID | Gorev | Durum | Not |
|---|---|---|---|
| MOB-00 | Flutter projesinin olusturulmasi (`flutter create`) | DONE | `mobile/` klasorunde `com.whoisin` org ile olusturuldu |
| MOB-01 | Proje yapisi ve klasor iskeleti (core/features) | DONE | core/network, core/storage, core/router, core/providers ve features/auth,events,dashboard |
| MOB-02 | Dio client + base URL + interceptor kurulumu | DONE | `ApiConfig` (`--dart-define=API_BASE_URL`) ve `ApiClient` interceptor'lari eklendi |
| MOB-03 | Secure storage ile token saklama | DONE | `TokenStorage` (`flutter_secure_storage`) eklendi |
| MOB-04 | Auth ekranlari (login/register) | DONE | `LoginPage`/`RegisterPage`, `AuthController` (AsyncNotifier) ile baglandi |
| MOB-05 | Oturum bootstrap + route guard (go_router) | DONE | `app_router.dart` icinde `redirect` ile auth durumuna gore yonlendirme |
| MOB-06 | 401 sonrasi otomatik refresh + retry | DONE | `ApiClient._onError` icinde kuyruklama ve `_retry` ile yeniden deneme |
| MOB-07 | Etkinlik listesi ekrani | DONE | `EventsListPage` (pull-to-refresh, loading/empty/error) |
| MOB-08 | Etkinlik detay ekrani | DONE | `EventDetailPage` (organizer/katilimci gorunumu ayri) |
| MOB-09 | Etkinlik olusturma/duzenleme formu | DONE | `EventFormPage` create+update ortak kullanir |
| MOB-10 | Etkinlik durum gecisi aksiyonlari | DONE | Draft->Published, Published->Cancelled/Completed butonlari |
| MOB-11 | Davet gonderme (organizer) | DONE | Etkinlik detayinda davet formu ve liste |
| MOB-12 | RSVP aksiyonlari (accept/decline) | DONE | Katilimci gorunumunde Kabul Et/Reddet butonlari |
| MOB-13 | Waitlist promote (organizer) | DONE | Katilimci listesinde bekleme listesi yukseltme aksiyonu |
| MOB-14 | Participant listesi ve durum guncelleme (organizer) | IN_PROGRESS | Liste goruntuleme yapildi; tekil durum degistirme (PATCH participant) UI aksiyonu eklenmedi |
| MOB-15 | Organizer dashboard/summary ekrani | DONE | `DashboardPage`, `/api/events/summary` ile metrik ve yaklasan etkinlikler |
| MOB-16 | Hata/loading/empty state standardizasyonu | IN_PROGRESS | Ana ekranlarda uygulandi; ortak widget'a cikarma yapilmadi |
| MOB-17 | Uygulama ikonu, isim, splash ekrani | TODO | Marka/gorsel netlesince yapilacak |
| MOB-18 | Android/iOS build dogrulamasi | DONE | `whoisin_pixel` AVD (Pixel 6, API 37) olusturuldu; `flutter run -d emulator-5554 --debug` ile uygulama emulatorde acildi, Login ekrani goruntulendi. iOS icin macOS/Xcode ortami yok. |

## 5) Uygulama Sirasi
1. MOB-00 -> MOB-03: Proje iskeleti ve altyapi (network, storage, router).
2. MOB-04 -> MOB-06: Auth akisi ve oturum yonetimi (web ile ayni davranis).
3. MOB-07 -> MOB-10: Etkinlik listeleme/detay/olusturma/durum yonetimi.
4. MOB-11 -> MOB-14: Davet, RSVP, waitlist, participant yonetimi.
5. MOB-15 -> MOB-16: Dashboard ve genel UX saglamlastirmasi.
6. MOB-17 -> MOB-18: Cilalama ve build dogrulamasi.

## 6) Acik Kararlar
- Push bildirim: MVP kapsaminda mi, sonraya mi birakilacak (backend bildirim kanali karari ile birlikte netlesecek).
- Min. desteklenen Android/iOS surumleri belirlenmedi.
- Uygulama ikonu/marka gorseli henuz yok.
- Android emulator'den backend'e erisim icin `API_BASE_URL` `10.0.2.2` host'una isaret etmeli (emulator'de `localhost` kendi cihazidir); `flutter run --dart-define=API_BASE_URL=https://10.0.2.2:7042` ile calistirilmali.

## 7) Sonraki Anlik Adim
Participant tekil durum guncelleme aksiyonu (MOB-14 tamamlanmasi) ve Android/iOS toolchain ile gercek cihaz/emulator build dogrulamasi (MOB-18) yapilmali.

## Degisiklik Kaydi
- 2026-08-22: `whoisin_pixel` Android emulator (Pixel 6, API 37, Google Play) olusturuldu ve uygulama uzerinde calistirildi; Login ekrani basariyla goruntulendi. Backend'e baglanmak icin emulatorden `API_BASE_URL=https://10.0.2.2:<port>` kullanilmasi gerektigi not edildi.
- 2026-08-21: Android SDK dogrulandi (SDK 36.1.0, tum lisanslar kabul edilmis); `flutter_secure_storage` uyumsuzlugu nedeniyle `android/app/build.gradle.kts` icinde `compileSdk` 37'ye sabitlendi. `flutter build apk --debug` basariyla tamamlandi. iOS build icin macOS/Xcode gerekiyor, bu ortamda mevcut degil.
- 2026-08-21: Flutter mobil projesi `mobile/` altinda olusturuldu; Riverpod + Dio + go_router + flutter_secure_storage + intl bagimliliklari eklendi. core/network (ApiClient + 401 refresh interceptor), core/storage (TokenStorage), core/router (auth guard'li go_router) katmanlari yazildi. Auth (login/register/session), Events (liste/detay/olusturma/durum/davet/RSVP/waitlist) ve Dashboard (summary) ozellikleri backend API kontratina gore entegre edildi. `flutter analyze` temiz, `flutter build web --release` basarili. Varsayilan sablon test dosyasi kaldirildi.
- 2026-08-21: Mobil (Flutter) ilerleme plani dosyasi olusturuldu; backend API kontrati ve milestone checklist tanimlandi.
