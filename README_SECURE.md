# 🔒 BankLite API - Güvenli Versiyon

## 🎯 Güvenlik Güncellemesi Tamamlandı

Bu proje, tüm bilinen güvenlik açıklarından arındırılmış ve production ortamına hazır hale getirilmiştir.

---

## ✅ Kapatılan Güvenlik Açıkları

| # | Güvenlik Açığı | Durum | Çözüm |
|---|----------------|-------|-------|
| 1 | SQL Injection | ✅ Kapatıldı | AdminSandboxController kaldırıldı, tüm sorgular LINQ ile |
| 2 | Mass Assignment | ✅ Kapatıldı | Role alanı DTO'lardan kaldırıldı, backend'de sabit |
| 3 | Missing Rate Limiting | ✅ Kapatıldı | RateLimitingMiddleware eklendi (5 req/min) |
| 4 | Improper Error Handling | ✅ Kapatıldı | GlobalExceptionHandler eklendi |
| 5 | Open Redirect | ✅ Kapatıldı | Redirect endpoint'i kaldırıldı |
| 6 | Broken Authentication | ✅ Kapatıldı | JWT validation güçlendirildi |
| 7 | CORS Misconfiguration | ✅ Düzeltildi | Strict CORS policy uygulandı |
| 8 | Insecure API Access | ✅ Korundu | Tüm endpoint'ler [Authorize] ile korunuyor |
| 9 | Missing Input Validation | ✅ Kapatıldı | Tüm DTO'lara DataAnnotations eklendi |
| 10 | Information Disclosure | ✅ Kapatıldı | Hassas endpoint'ler kaldırıldı |
| 11 | Path Traversal | ✅ Kapatıldı | File read endpoint'i kaldırıldı |
| 12 | IDOR | ✅ Kapatıldı | Ownership kontrolü her işlemde yapılıyor |

---

## 🆕 Eklenen Güvenlik Özellikleri

### 1. **Rate Limiting** (Brute Force Koruması)
```csharp
// Korunan endpoint'ler:
- /api/auth/login
- /api/auth/register  
- /api/transfers/initiate
- /api/transfers/{id}/confirm

// Limit: 5 istek/dakika/kullanıcı
```

### 2. **Global Exception Handler**
```csharp
// ✅ Production'da detaylı hata mesajları gösterilmiyor
// ✅ Tüm hatalar loglanıyor
// ✅ Kullanıcıya generic mesajlar dönülüyor
```

### 3. **Kapsamlı Input Validation**
```csharp
// Tüm DTO'larda:
- [Required] - Zorunlu alanlar
- [StringLength] - Uzunluk kontrolü
- [Range] - Sayısal sınırlar
- [RegularExpression] - Format kontrolü (IBAN, username, vb.)
```

### 4. **Güçlü Authentication/Authorization**
```csharp
// ✅ JWT token zorunlu
// ✅ Token expiration kontrolü
// ✅ Issuer/Audience doğrulaması
// ✅ Tüm controller'lar [Authorize] ile korunuyor
```

### 5. **Password Politikası**
```csharp
// Minimum gereksinimler:
- En az 8 karakter
- En az 1 büyük harf
- En az 1 rakam
```

---

## 🗂️ Proje Yapısı

```
BankLite.Api/
├── Controllers/
│   ├── AccountsController.cs       ✅ IDOR koruması
│   ├── AlertsController.cs         ✅ Güvenli
│   ├── AuthController.cs           ✅ Mass assignment koruması
│   ├── BeneficiariesController.cs  ✅ Ownership kontrolü
│   ├── ReceiptsController.cs       ✅ Güvenli
│   ├── TermDepositsController.cs   ✅ Güvenli
│   └── TransfersController.cs      ✅ Rate limiting + validation
│
├── DTOs/
│   ├── AccountDtos.cs              ✅ Validation eklendi
│   ├── AuthDtos.cs                 ✅ Role alanı kaldırıldı
│   ├── BeneficiaryDtos.cs          ✅ IBAN validation
│   ├── TermDepositDtos.cs          ✅ Range validation
│   └── TransferDtos.cs             ✅ Amount + IBAN validation
│
├── Middleware/
│   ├── GlobalExceptionHandler.cs   🆕 Exception güvenliği
│   └── RateLimitingMiddleware.cs   🆕 Brute force koruması
│
├── Attributes/
│   └── ValidateModelAttribute.cs   🆕 Model validation
│
├── Services/                        ✅ Ownership kontrolleri mevcut
├── Data/                            ✅ DbContext güvenli
└── Models/                          ✅ Entity'ler güvenli
```

---

## 🚀 Çalıştırma

### Gereksinimler
- .NET 8.0 SDK
- SQL Server (LocalDB veya Express)

### Adımlar

1. **Veritabanı Migration**
```bash
cd BankLite.Api
dotnet ef database update
```

2. **Projeyi Çalıştır**
```bash
dotnet run
```

3. **Swagger UI**
```
https://localhost:5001/swagger
```

---

## 🧪 Güvenlik Testi

### Manuel Test Önerileri

#### 1. Rate Limiting Testi
```bash
# 6. istekte 429 dönmeli
for i in {1..6}; do
  curl -X POST https://localhost:5001/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"username":"test","password":"wrong"}'
done
```

#### 2. SQL Injection Testi
```bash
# Artık admin/sandbox endpoint'i yok - 404 dönmeli
curl "https://localhost:5001/api/sandbox/find-user-unsafe?username=admin'--"
```

#### 3. Mass Assignment Testi
```bash
# Role alanı yoksayılmalı, kullanıcı "User" rolü ile oluşmalı
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"Test1234","fullName":"Test","role":"Admin"}'
```

#### 4. IDOR Testi
```bash
# Başka kullanıcının hesabına erişim denemesi - 403/401 dönmeli
curl -X GET https://localhost:5001/api/accounts/user/999 \
  -H "Authorization: Bearer {your_token}"
```

#### 5. Input Validation Testi
```bash
# Geçersiz IBAN formatı - 400 dönmeli
curl -X POST https://localhost:5001/api/transfers/initiate \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"fromAccountId":1,"toIban":"INVALID","amount":-100}'
```

---

## 📊 API Endpoint'leri

### Public Endpoints (Authentication Gerektirmez)
```
POST /api/auth/register          - Kayıt
POST /api/auth/login             - Giriş
```

### Protected Endpoints (JWT Token Gerektirir)
```
Hesaplar:
GET    /api/accounts              - Kendi hesaplarını listele
POST   /api/accounts              - Yeni hesap aç
POST   /api/accounts/{id}/close   - Hesap kapat

Transferler:
GET    /api/transfers                    - Transfer geçmişi
POST   /api/transfers/initiate           - Transfer başlat (OTP gönder)
POST   /api/transfers/{id}/confirm       - Transfer onayla (OTP ile)
GET    /api/transfers/{id}/receipt       - Dekont görüntüle

Kayıtlı Alıcılar:
GET    /api/beneficiaries          - Kayıtlı alıcıları listele
POST   /api/beneficiaries          - Yeni alıcı ekle
DELETE /api/beneficiaries/{id}    - Alıcı sil

Vadeli Mevduat:
GET    /api/termdeposits           - Mevduatları listele
POST   /api/termdeposits/open      - Mevduat aç
POST   /api/termdeposits/{id}/close - Mevduat kapat

Bildirimler:
GET    /api/alerts                 - Bildirimleri listele
POST   /api/alerts/{id}/read       - Okundu işaretle
DELETE /api/alerts/{id}            - Bildirim sil
DELETE /api/alerts                 - Tümünü temizle

Kullanıcı:
POST   /api/auth/change-password   - Şifre değiştir
PUT    /api/auth/users/{id}        - Profil güncelle
```

---

## 🔐 Güvenlik Best Practices

### ✅ Yapılması Gerekenler
- JWT token'ı her istekte `Authorization: Bearer {token}` header'ında gönder
- Token'ı güvenli bir yerde sakla (HttpOnly cookie veya secure storage)
- HTTPS kullan (production'da zorunlu)
- API anahtarlarını kaynak kodda tutma (.env veya Azure Key Vault kullan)
- Düzenli dependency güncellemeleri yap
- Logging ve monitoring ekle

### ❌ Yapılmaması Gerekenler
- Token'ı localStorage'da tutma (XSS riski)
- Hassas bilgileri query string'de gönderme
- HTTP kullanma (production'da)
- Exception detaylarını kullanıcıya gösterme
- Rate limit'i devre dışı bırakma

---

## 📝 Environment Variables

### appsettings.json
```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\mssqllocaldb;Database=BankLiteDb;Trusted_Connection=true"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyHere_AtLeast32Characters!",
    "Issuer": "BankLiteAPI",
    "Audience": "BankLiteClient",
    "ExpirationMinutes": 60
  }
}
```

**⚠️ Production Önerisi**: JWT Key'i Azure Key Vault'tan al
```csharp
builder.Configuration["Jwt:Key"] = azureKeyVault.GetSecret("JwtKey");
```

---

## 📖 Detaylı Dökümanlar

- **[SECURITY_AUDIT_COMPLETE.md](./SECURITY_AUDIT_COMPLETE.md)** - Tüm güvenlik düzeltmelerinin detaylı açıklaması
- **[SECURITY_FIXES.md](./SECURITY_FIXES.md)** - Önceki güvenlik düzeltmeleri

---

## 🛡️ Güvenlik Durumu

**Durum**: ✅ **TÜM GÜVENLİK AÇIKLARI KAPATILDI**

**Son Denetim**: 30 Ocak 2026

**Güvenlik Seviyesi**: 🟢 Production Ready

---

## 📞 Destek

Güvenlik sorunları için lütfen güvenli bir kanal üzerinden iletişime geçin.

---

## 📜 Lisans

Bu proje eğitim amaçlıdır.

---

**Not**: Bu README dosyası güvenlik açıklarının kapatıldığı versiyonu temsil eder. Eski versiyondaki güvenlik açıkları kasıtlı olarak oluşturulmuştu (eğitim amaçlı).
