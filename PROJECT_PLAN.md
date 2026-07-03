# WhoIsInV2 - Urun ve Teknik Plan

## 1) Urun Vizyonu
WhoIsInV2, etkinliklerin uctan uca planlanmasi ve yonetilmesi icin tasarlanmis bir etkinlik yonetim platformudur:
- Etkinlik olusturma ve yayina alma
- Davetlileri ve katilimcilari yonetme
- Mekan, tarih, saat, kapasite ve durum takibi
- Katilim ve surec akisinin izlenmesi

Urun su bilesenlerden olusacak:
- Web uygulamasi (React)
- Mobil uygulama (Flutter)
- Backend API (ASP.NET Core, .NET 10)

## 2) Ana Hedefler
- Organizatörler icin hizli etkinlik kurulumu
- Davetliler icin net RSVP ve katilimci yasam dongusu
- Guvenilir katilimci ve kapasite yonetimi
- Web + mobil tarafinda ortak veri dogrulugu

## 3) Kapsam Tanimi

### Kapsam Dahilinde (Ilk Asama)
- Etkinlik yasam dongusu yonetimi
- Davetli yonetimi ve RSVP takibi
- Mekan/zaman/kapasite yonetimi
- Izleme amacli organizatör paneli
- Temel bildirim akisları (email ve uygulama ici, tasarima hazir)
- Rol bazli erisim (Organizer, Co-Organizer, Participant)

### Kapsam Disinda (Su An)
- Odeme/biletleme
- Gelismis onerı motoru
- Acik pazar yeri/kesfet ozellikleri
- Otomatik test gelistirmesi (bilerek ertelendi)

## 4) Ozellik Seti

### 4.1 MVP Ozellikleri
1. Kimlik Dogrulama ve Yetkilendirme
- Kayit, giris, refresh token, cikis
- Rol bazli yetkilendirme: Organizer, Co-Organizer, Participant

2. Etkinlik Yonetimi
- Baslik, aciklama, kategori ile etkinlik olusturma
- Tarih/saat ve timezone ayarlama
- Konum belirleme (fiziksel adres veya online baglanti)
- Maksimum katilimci sayisi tanimlama
- Draft/Published/Cancelled/Completed durumlari

3. Davetli ve RSVP Yonetimi
- Email listesi ve paylasilabilir davet linki ile davet
- RSVP durumlari: Pending, Accepted, Declined, Waitlisted
- Kapasite doldugunda waitlist'e otomatik yonlendirme

4. Katilimci Takibi
- Onayli katilimci listesi
- Waitlist kuyruk yonetimi
- Organizatör tarafinda manuel durum guncelleme

5. Organizatör Paneli
- Etkinlik ozet kartlari (kapasite, accepted, pending)
- Filtre/arama destekli katilimci listesi
- Son aktivite zaman cizelgesi (temel)

6. Bildirimler (MVP Seviyesi)
- Davet gonderimi
- RSVP durum guncellemeleri
- Etkinlik guncelleme/iptal bilgilendirmesi

### 4.2 MVP Sonrasi (V1+) Oneriler
1. QR tabanli check-in akisı
2. Takvim entegrasyonu (Google/Outlook/ICS)
3. Tekrarlayan etkinlikler
4. Coklu dil destegi (TR/EN)
5. Hafif analitik (katilim orani, no-show orani)
6. Dosya ekleri (gundem, harita, notlar)
7. Ekip ve organizasyon calisma alanlari

## 5) Teknik Yigin

### 5.1 Backend
- .NET 10 (ASP.NET Core Web API)
- Clean Architecture
- EF Core ORM
- PostgreSQL
- Serilog (simdilik Console sink)
- JWT tabanli kimlik dogrulama (access + refresh)

Onerilen tamamlayici kutuphaneler:
- FluentValidation (istek dogrulamasi)
- MediatR (uygulama use-case orkestrasyonu)
- Mapster veya AutoMapper (DTO mapleme)
- ProblemDetails (tutarlı API hata formatı)

### 5.2 Frontend (Web)
- React (TypeScript)
- Vite
- React Router
- TanStack Query (server state)
- Form kutuphanesi (React Hook Form)
- UI kit: tasarim asamasinda netlestirilecek (MUI, Chakra veya custom)

### 5.3 Mobil
- Flutter
- State management: Riverpod (onerilir)
- Networking: Dio
- Auth token yonetimi: secure storage

## 6) Mimari Taslak (Clean Architecture)

Onerilen backend proje yapisi:
- src/WhoIsInV2.Domain
- src/WhoIsInV2.Application
- src/WhoIsInV2.Infrastructure
- src/WhoIsInV2.Api

Katman sorumluluklari:
- Domain: Entities, Value Objects, Domain Rules, Domain Events
- Application: Use-case'ler, DTO'lar, interface'ler, validation, is akisları
- Infrastructure: EF Core, PostgreSQL repository'leri, dis servisler, Serilog konfigurasyonu
- Api: Controller/Endpoint'ler, auth middleware, DI composition root

## 7) Cekirdek Domain Modeli (Ilk)

Temel entity'ler:
- User
- Event
- EventInvite
- EventParticipant
- Venue (veya EventLocation value object)
- Notification

Temel iliskiler:
- Bir Event, bir Organizer'a (User) aittir
- Bir Event'in birden cok Invite'i olur
- Bir Event'in birden cok Participant'i olur
- Bir Invite, kabul sonrasi Participant'a donusebilir

## 8) API Yuzeyi (Ilk Yonu)

Kimlik Dogrulama:
- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/refresh
- POST /api/auth/logout

Etkinlikler:
- POST /api/events
- GET /api/events
- GET /api/events/{id}
- PUT /api/events/{id}
- PATCH /api/events/{id}/status

Davetler ve RSVP:
- POST /api/events/{id}/invites
- GET /api/events/{id}/invites
- POST /api/events/{id}/rsvp
- POST /api/events/{id}/waitlist/promote

Katilimcilar:
- GET /api/events/{id}/participants
- PATCH /api/events/{id}/participants/{participantId}

## 9) Loglama ve Gozlemlenebilirlik
- API baslangicinda Serilog konfigurasyonu
- Ilk asamada Console sink aktif
- Request trace icin correlation id
- Structured log alanlari: EventId, UserId, RequestPath, DurationMs

Gelecek eklemeler:
- File/Seq/ELK sink
- Temel metrikler ve health check'ler

## 10) Non-Functional Gereksinimler
- Timezone-guvenli datetime yonetimi (UTC saklama)
- Gerekli yerde soft delete destegi
- Etkinlik guncellemelerinde optimistic concurrency
- Liste endpoint'lerinde pagination/filtering
- Auth endpoint'lerinde temel rate limiting

## 11) Teslimat Plani (Fazli)

### Faz 0 - Temel Kurulum
- Repository kurulumu ve solution iskeleti
- Clean Architecture proje referanslari
- PostgreSQL + EF Core migration baslangic altyapisi
- Serilog console entegrasyonu
- Auth temeli (JWT)

### Faz 1 - MVP Cekirdek
- Event CRUD + durum gecisleri
- Davet ve RSVP akisları
- Kapasite ve waitlist yonetimi
- Organizatör panel endpoint'leri
- React web MVP ekranlari

### Faz 2 - Mobil MVP
- Flutter auth akisı
- Etkinlik liste/detay ekranlari
- RSVP aksiyonlari
- Organizatör icin temel kontroller

### Faz 3 - Saglamlastirma
- Hata yonetimi standardizasyonu
- Performans iyilestirmesi ve sorgu optimizasyonu
- Guvenlik iyilestirmesi (auth/session/policies)
- Production readiness checklist

## 12) Simdiye Kadar Kesinlesen Kararlar
- Backend: .NET 10 + Clean Architecture
- ORM: EF Core
- Veritabani: PostgreSQL
- Loglama: Serilog (simdilik console)
- Web frontend: React
- Mobil frontend: Flutter
- Otomatik testler: su an icin bilerek ertelendi

## 13) Sonraki Asamada Netlestirilecek Acik Kararlar
1. Kimlik modeli detaylari:
- Sadece email/password mi, yoksa social login de olacak mi?

2. Bildirim kanali onceligi:
- MVP'de sadece email mi, push bildirim de olacak mi?

3. Etkinlik gorunurlugu:
- Sadece private mi, yoksa private + public birlikte mi?

4. Multi-tenant ihtiyaci:
- Simdilik tek organizasyon mu, sonra coklu organizasyon mu?

5. Takvim entegrasyonu zamanlamasi:
- V1 icinde mi, V1 sonrasinda mi?

## 14) Onerilen Sonraki Anlik Adim
Clean Architecture proje yapisi ve temel paketlerle backend solution iskeleti olusturulup, User/Event/Invite/Participant icin ilk migration'lar tanimlanmali.