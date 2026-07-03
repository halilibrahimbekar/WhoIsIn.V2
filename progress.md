# WhoIsInV2 - Progress Takibi

Bu dosya proje ilerlemesini takip etmek icin kullanilir.
Durumlar:
- `DONE`: Tamamlandi
- `IN_PROGRESS`: Uzerinde calisiliyor
- `TODO`: Sirada
- `BLOCKED`: Engel var

## Genel Durum
- Son guncelleme: 2026-07-03
- Aktif Faz: Faz 0 - Temel Kurulum
- Not: Test yazimi su asamada kapsam disi (istenmedigi icin).

## Milestone Checklist

| ID | Gorev | Durum | Not |
|---|---|---|---|
| M0-01 | Clean Architecture solution iskeleti | DONE | Domain/Application/Infrastructure/Api projeleri olusturuldu |
| M0-02 | Controller tabanli API iskeleti | DONE | Minimal API kullanilmadi |
| M0-03 | Swagger/OpenAPI entegrasyonu | DONE | Swagger UI development ortaminda aktif |
| M0-04 | Serilog console loglama | DONE | Program.cs ve appsettings ile aktif |
| M0-05 | EF Core + PostgreSQL baglantisi | DONE | DbContext ve Npgsql provider eklendi |
| M0-06 | Ilk domain entity seti | DONE | User/Event/Invite/Participant eklendi |
| M0-07 | Ilk migration olusturma | DONE | InitialCreate migration olusturuldu |
| M0-08 | Veritabani update ve dogrulama | DONE | InitialCreate veritabanina uygulandi |
| M1-01 | JWT auth (access + refresh) | DONE | Register/Login/Refresh/Revoke endpointleri eklendi |
| M1-02 | Event invite ve RSVP akisi | DONE | Invite + RSVP + waitlist endpointleri eklendi |
| M1-03 | Web frontend MVP baslangic ekrani | DONE | React + Vite kuruldu, dashboard benzeri acilis ekrani eklendi |
| M1-04 | Web frontend cok sayfali routing iskeleti | DONE | React Router ile layout, dashboard, events, detail, invites, auth sayfalari eklendi |
| M1-05 | Auth sayfasi login endpoint entegrasyonu | DONE | Auth formu `/api/auth/login` ile baglandi, token saklama ve hata gosterimi eklendi |
| M1-06 | Auth session bootstrap ve route guard | DONE | Authorization header wrapper, `/api/auth/me` ile acilis kontrolu ve korumali rotalar eklendi |
| M1-07 | FE otomatik access token yenileme | DONE | 401 durumunda `/api/auth/refresh` ile token yenileme ve istek tekrar deneme eklendi |

## Yakindaki Sonraki Adimlar
1. Auth gerektiren endpointlerde sahiplik/rol kontrolunu detaylandir.
2. Refresh token icin cihaz bazli oturum yonetimini ekle.
3. OpenAPI uzerinden auth + RSVP akisini manuel dogrula.
4. JWT imzalama anahtarini ortam bazli guvenli konfigurasyona tasi.

## Degisiklik Kaydi
- 2026-07-03: M1-02 tamamlandi, Event invite + RSVP + waitlist endpointleri eklendi.
- 2026-07-03: M1-01 tamamlandi, JWT access+refresh akisi ve auth migration'i eklendi.
- 2026-07-03: M1-03 tamamlandi, Web frontend icin React + Vite kuruldu ve ilk gorunur UI olusturuldu.
- 2026-07-03: M1-04 tamamlandi, React Router ile cok sayfali FE iskeleti ve ortak layout eklendi.
- 2026-07-03: M1-05 tamamlandi, Auth sayfasi login endpointine baglandi ve localStorage session saklama eklendi.
- 2026-07-03: M1-06 tamamlandi, uygulama acilisinda `/api/auth/me` ile oturum dogrulama ve route guard eklendi.
- 2026-07-03: M1-07 tamamlandi, 401 sonrasinda refresh token ile otomatik access token yenileme eklendi.
- 2026-07-03: M0-08 tamamlandi, `dotnet ef database update` basariyla calisti.
- 2026-07-03: M0-08 denemesi yapildi, PostgreSQL auth hatasi (28P01) nedeniyle bloklandi.
- 2026-07-03: M0-07 tamamlandi, `InitialCreate` migration'i olusturuldu.
- 2026-07-03: Baslangic dosyasi olusturuldu, mevcut backend ilerlemesi işlendi.
