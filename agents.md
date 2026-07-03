# Agents Notlari

Bu dosya, ileride kullanacagimiz ajan/yardimci calisma kurallarini ve tekrar kullanilabilir prompt notlarini tutmak icindir.

## Amac
- Tekrarlanan gorevlerde hizli ve tutarli calismak
- Ajanlara verilecek gorev tanimlarini standartlastirmak
- Projede alinmis teknik kararlarla uyumlu kalmak

## Proje Kurallari (Ozet)
- Backend: .NET 10, Clean Architecture
- ORM: EF Core
- DB: PostgreSQL
- Logging: Serilog (su an console)
- API yaklasimi: Controller tabanli, Minimal API yok
- OpenAPI/Swagger: Zorunlu
- Test: Su an istek disi
- Ek paketler: FluentValidation/MediatR/AutoMapper su an kullanilmiyor

## Ajan Prompt Sablonu
Asagidaki sablonu ilgili goreve gore doldur:

```text
Gorev: <kisa gorev tanimi>
Baglam: WhoIsInV2 backend (Clean Architecture)
Kisitlar:
- Minimal API kullanma
- Swagger/OpenAPI aktif kalsin
- Gereksiz kutuphane ekleme
Cikti:
- Degisen dosyalar
- Yapilan teknik kararlar
- Dogrulama adimlari (build/run)
```

## Tekrarlanabilir Gorev Tipleri
- `migration`: Yeni migration olusturup veritabanina uygula
- `endpoint`: Controller endpoint ekleme/guncelleme
- `refactor`: Katman sinirlari korunarak kod duzenleme
- `ops`: Konfigurasyon, loglama, startup davranisi

## Notlar
- Her buyuk adimdan sonra `progress.md` guncellenmeli.
- Yeni teknik karar alininca once bu dosyaya not dusulmeli.
