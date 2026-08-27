# WhoIsInV2 - Progress Takibi

Bu dosya proje ilerlemesini takip etmek icin kullanilir.
Durumlar:
- `DONE`: Tamamlandi
- `IN_PROGRESS`: Uzerinde calisiliyor
- `TODO`: Sirada
- `BLOCKED`: Engel var

## Genel Durum
- Son guncelleme: 2026-08-27
- Aktif Faz: Faz 1 - MVP Cekirdek (yetkilendirme ve gercek API entegrasyonu)
- Not: Test yazimi su asamada kapsam disi (istenmedigi icin).
- Genel ilerleme: Faz 0 tamamlandi; Faz 1 MVP akislarinin buyuk bolumu backend ve web tarafinda calisiyor, saglamlastirma ve kalan urun kararlari bekliyor.

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
| M1-08 | Logout server-side token revoke | DONE | Cikis aksiyonunda `/api/auth/revoke` cagrisi ve sonrasinda local session temizligi eklendi |
| M1-09 | Landing page + register yonlendirmesi | DONE | Ana sayfa landing olarak degisti, register formu eklendi ve uygulama paneli `/app` altina tasindi |
| M1-10 | Event sahiplik ve rol yetkilendirmesi | DONE | Create artik JWT claim'deki kullaniciyi organizer olarak kullaniyor; update/status/participant sahiplik kontrolleri eklendi, public list/detail sorgularinda Published + organizer-owned gorunurluk uygulandi, davetli kullanici icin private erisim eklendi |
| M1-11 | Event CRUD ve durum gecis kurallari | DONE | PUT/update endpointi ve temel Draft/Published/Cancelled/Completed gecisleri eklendi; create title validation ve kapasite-confirmed kontrolu eklendi |
| M1-12 | Invite/participant sorgu ve yonetim endpointleri | DONE | Organizer tum invite'lari, davetli kullanici yalnizca kendi invite kaydini okuyabiliyor; participant GET/PATCH ve organizer waitlist promote endpointleri eklendi; pagination eklendi |
| M1-13 | Web event API entegrasyonu | DONE | Events, EventDetail, Invites ve Dashboard gercek API'ye baglandi |
| M1-14 | Web event olusturma ve RSVP aksiyonlari | DONE | Event olusturma, status PATCH, invite gonderme, participant status, RSVP ve waitlist promote UI'ya baglandi |
| M1-15 | Organizer dashboard gercek metrikleri | DONE | Organizer summary endpointi ile active events, accepted, waitlist, fill rate ve upcoming events gercek veriden geliyor |
| M1-16 | MVP bildirim akislarinin tasarlanmasi | TODO | Invite, RSVP ve event update/iptal bildirimleri icin kanal ve teslim modeli netlestirilmeli |

## Uygulama Sirasi ve Bitirme Kriterleri

Asagidaki sira, API guvenligi ve veri kontratlari oturmadan web ekranlarinin tekrar tekrar degismemesi icin secildi:

1. **M1-10 Yetkilendirme ve sahiplik (IN_PROGRESS)**
	- JWT kullanicisi organizer olarak request body yerine server claim'den belirlenir.
	- Event detay, status, invite, participant ve dashboard sorgularinda organizer/rol kontrolu bulunur.
	- Public event listesi ile organizer event listesi ayrimi netlestirilir.

2. **M1-11 Event CRUD ve durum kurallari**
	- Create, list, detail, update ve status endpointleri tamamlanir.
	- Gecersiz tarih, kapasite, bos baslik ve gecersiz status gecisleri tutarli hata doner.
	- Guncelleme sirasinda UpdatedAtUtc ve optimistic concurrency ihtiyaci korunur.

3. **M1-12 Invite/participant API yuzeyi**
	- Invite listeleme, participant listeleme ve organizer tarafli participant status guncelleme eklenir.
	- RSVP kabul, decline, capacity ve waitlist promote kurallari tek akis olarak dogrulanir.
	- Pagination/filtering ihtiyaci MVP kapsaminda gereken endpointlerde uygulanir.

4. **M1-13/M1-14 Web entegrasyonu**
	- Frontend API tipleri ve fonksiyonlari backend response modelleriyle eslestirilir.
	- Sabit demo listeleri kaldirilip loading, empty, error ve auth-expired durumlari ele alinir.
	- Event olusturma, detay, status, invite ve RSVP aksiyonlari calisan UI akislarina donusturulur.

5. **M1-15 Dashboard ve bildirim hazirligi**
	- Dashboard metrikleri ve son aktiviteler gercek sorgulardan beslenir.
	- Bildirim event'leri/arayuzleri belirlenir; MVP'de secilen kanal icin entegrasyon noktasi hazirlanir.

6. **Faz 2 - Mobil MVP**
	- Flutter auth, etkinlik liste/detay, RSVP ve organizer temel kontrolleri.
	- Web ile ayni API kontratlari ve UTC/timezone davranisi kullanilir.

7. **Faz 3 - Saglamlastirma**
	- ProblemDetails ve validation standardizasyonu.
	- Rate limiting, guvenli JWT secret yonetimi, refresh token cihaz/oturum yonetimi.
	- Sorgu optimizasyonu, production config, health/readiness ve release checklist.

## Acik Kararlar

- Kimlik: MVP email/password ile devam ediyor; social login karari ertelendi.
- Gorunurluk: public event listelemesi ve private davet modeli netlestirilmeli.
- Bildirim: email mi, uygulama ici mi, push mu once gelecek belirlenmeli.
- Organizasyon: multi-tenant ihtiyaci MVP sonrasina birakilmali.
- Takvim: Google/Outlook/ICS entegrasyonu Faz 2 veya V1+ olarak ele alinmali.

## Yakindaki Sonraki Adimlar
1. M1-16 bildirim akisi tasarimi: invite, RSVP ve event update/iptal bildirimleri icin kanal ve teslim modeli netlestirilmeli.
2. ProblemDetails standardizasyonu tamamlandi; AuthController ve UsersController'da da ayni yapi uygulanabilir.
3. RSVP/waitlist concurrency ve edge-case saglamlastirmasi yapilabilir.
4. Faz 2 Flutter mobil MVP'ye devam edilebilir.
5. Faz 3 saglamlastirma: rate limiting, guvenli JWT secret yonetimi, health check.

## Degisiklik Kaydi
- 2026-08-27: Pagination/filtering ve ProblemDetails standardizasyonu eklendi. `GET /api/events` artik `?search=`, `?status=`, `?page=`, `?pageSize=` query parametrelerini destekliyor; invites ve participants endpointleri de sayfalama yapiyor. API hata yanitlari RFC 7807 ProblemDetails formatina donusturuldu. Web EventsPage'e arama, status filtresi ve sayfalama kontrolleri eklendi; InvitesPage, DashboardPage ve EventDetailPage sayfalanmis yanitta `.items` alaniyla guncellendi.
- 2026-08-21: Faz 2 Flutter mobil MVP baslatildi; `mobile/` altinda proje olusturuldu, auth/events/dashboard ozellikleri backend API'sine baglandi. Detaylar icin [MOBILE_PLAN.md](MOBILE_PLAN.md). `flutter analyze` temiz, `flutter build web --release` basarili.
- 2026-08-21: RSVP butonlari mevcut duruma gore karsilikli tutuldu; Accepted davette Decline, Declined davette Accept, Pending/Waitlisted davette iki aksiyon da gorunuyor. Frontend build basarili.
- 2026-08-21: Event kategorileri ayri `Categories` tablosuna tasindi; migration mevcut kategori metinlerini koruyup varsayilan secenekleri seed ediyor. Public/InviteOnly gorunurluk, organizer approval ve public RSVP akisiyla birlikte backend/web'e eklendi; veritabani migration'i uygulandi, solution ve frontend build basarili.
- 2026-08-21: Davetli kullanicinin Invites ekraninda organizer-only 403 almasi duzeltildi; GET invites organizer icin tum kayitlari, davetli icin yalnizca kendi kaydini donduruyor. Backend build basarili.
- 2026-08-21: Event detail sayfasinda participant sorgusu ve lifecycle/invite/participant kontrolleri organizer sahipligi ile sinirlandi; private event davetlileri 403 kaynakli sayfa hatasi almiyor. Frontend build basarili.
- 2026-08-21: RSVP akisi Draft, Cancelled ve Completed eventlerde engellendi; kabul/ret ve waitlist mutasyonu yalnizca Published eventlerde yapiliyor. Backend build basarili.
- 2026-08-21: Authenticated invite sahibi kullanicilarin private eventleri listede ve detayda gorebilmesi saglandi; anonymous erisim yalnizca Published eventlerle sinirli kaldi. Backend build basarili.
- 2026-08-21: Event list/detail gorunurlugu Published eventler ve authenticated organizer'in kendi eventleri ile sinirlandi; create baslik zorunlulugu ve kapasitenin confirmed participant sayisinin altina dusmesini engelleyen validation eklendi. Backend/frontend build basarili.
- 2026-08-20: Clean Architecture katmanlari etkinlestirilmeye baslandi. Auth, Users ve Events endpointlerindeki veri erisimi ve use-case kurallari Application servislerine tasindi; API controllerlari HTTP adaptoru olarak sadeleştirildi.
- 2026-08-20: Swagger UI, ASP.NET Core `AddOpenApi`/`MapOpenApi` tarafindan uretilen `/openapi/v1.json` dokumanina acikca baglandi. Solution build basarili ve NU1903 guvenlik uyarisi gorulmedi.
- 2026-08-20: RSVP aksiyonlari yalnizca oturum kullanicisinin kendi invite satirinda gosterildi; decline sonrasi waitlist promote sirasi duzeltildi. Frontend/backend build basarili.
- 2026-08-20: Waitlist promote endpointi ve participant status/promote UI aksiyonlari eklendi; invite ekranina RSVP Accept/Decline akisina baglandi. Backend/frontend build basarili.
- 2026-08-20: Event detail ekranina invite gonderme formu ve organizer sahiplik kontrollu participant listesi eklendi. `npm run build` basarili.
- 2026-08-20: Web event olusturma formu POST `/api/events` endpointine, event detail lifecycle secimi PATCH status endpointine baglandi. `npm run build` basarili.
- 2026-08-20: Organizer event summary endpointi ve gercek dashboard metrikleri/upcoming events entegrasyonu eklendi. Backend ve frontend build basarili.
- 2026-08-20: Web Events, EventDetail ve Invites sayfalari gercek API istemcilerine baglandi; loading/error/empty durumlari eklendi. `npm run build` basarili.
- 2026-08-20: Organizer sahiplik kontrollu invite listeleme ve participant status guncelleme endpointleri eklendi. Solution build basarili; `Microsoft.OpenApi` NU1903 advisory uyarisi devam ediyor.
- 2026-08-20: Event create organizerId body alanindan cikarildi; JWT kullanicisi sahipligi, update/status/participant organizer kontrolleri ve temel status gecis kurallari eklendi. Solution build basarili.
- 2026-08-20: Proje durumu yeniden degerlendirildi; Faz 0 tamamlandi, Faz 1 icin M1-10..M1-16 is paketleri ve sirali bitirme kriterleri eklendi.
- 2026-07-03: M1-02 tamamlandi, Event invite + RSVP + waitlist endpointleri eklendi.
- 2026-07-03: M1-01 tamamlandi, JWT access+refresh akisi ve auth migration'i eklendi.
- 2026-07-03: M1-03 tamamlandi, Web frontend icin React + Vite kuruldu ve ilk gorunur UI olusturuldu.
- 2026-07-03: M1-04 tamamlandi, React Router ile cok sayfali FE iskeleti ve ortak layout eklendi.
- 2026-07-03: M1-05 tamamlandi, Auth sayfasi login endpointine baglandi ve localStorage session saklama eklendi.
- 2026-07-03: M1-06 tamamlandi, uygulama acilisinda `/api/auth/me` ile oturum dogrulama ve route guard eklendi.
- 2026-07-03: M1-07 tamamlandi, 401 sonrasinda refresh token ile otomatik access token yenileme eklendi.
- 2026-07-03: M1-08 tamamlandi, logout sirasinda backend revoke endpoint cagrisi eklendi.
- 2026-07-03: M1-09 tamamlandi, landing page ana sayfa yapildi; register formu eklendi ve dashboard rotalari `/app` altina tasindi.
- 2026-07-03: M0-08 tamamlandi, `dotnet ef database update` basariyla calisti.
- 2026-07-03: M0-08 denemesi yapildi, PostgreSQL auth hatasi (28P01) nedeniyle bloklandi.
- 2026-07-03: M0-07 tamamlandi, `InitialCreate` migration'i olusturuldu.
- 2026-07-03: Baslangic dosyasi olusturuldu, mevcut backend ilerlemesi işlendi.
