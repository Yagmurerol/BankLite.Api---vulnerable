# 🔒 BankLite Security Fixes - Comprehensive Security Audit

## Overview
This document outlines all security vulnerabilities that have been identified and fixed in the BankLite banking application.

---

## ✅ Fixed Vulnerabilities

### 1. **IDOR (Insecure Direct Object Reference)**

#### Vulnerability
- **Location**: `AccountsController.cs` - `GetUserAccounts(int userId)` endpoint
- **Issue**: Users could access other users' account data by directly modifying the userId in the URL
- **Example**: User A could access User B's accounts by calling `/api/accounts/user/{userB_id}`

#### Fix Applied
```csharp
// REMOVED: Unsafe endpoint that allowed IDOR
// Old vulnerable endpoint:
[HttpGet("user/{userId:int}")]
public async Task<ActionResult<List<AccountResponse>>> GetUserAccounts(int userId)
```

**Solution**: Endpoint completely removed. Users now access only their own accounts through the secured `/accounts` endpoint which validates ownership from JWT token.

---

### 2. **Unauthorized Transfer Initiation**

#### Vulnerability
- **Location**: `TransfersController.cs` - `InitiateUnsafe()` endpoint
- **Issue**: `fromAccountId` was taken directly from request without validating ownership
- **Attack Scenario**: User A could transfer funds from User B's account if they knew the account ID
- **Code**:
```csharp
public async Task<ActionResult<InitiateTransferResponse>> InitiateUnsafe(InitiateTransferRequest req)
{
    var res = await _svc.InitiateAsync(req.FromAccountId, req); // ❌ No ownership check!
}
```

#### Fix Applied
```csharp
// REMOVED: Unsafe endpoint completely removed
// Secure endpoint validates ownership:
[HttpPost("initiate")]
public async Task<ActionResult<InitiateTransferResponse>> Initiate(InitiateTransferRequest req)
{
    // UserId is extracted from JWT token (trustworthy)
    var res = await _svc.InitiateAsync(UserId, req);
}
```

**Solution**: 
- Removed `InitiateUnsafe()` endpoint entirely
- Secure `/initiate` endpoint uses `UserId` from JWT token
- Backend `TransferService.InitiateAsync()` validates account ownership before processing

---

### 3. **Mass Assignment - Privilege Escalation**

#### Vulnerability
- **Location**: `AuthController.cs` - `Register()` endpoint and `UpdateUser()` endpoint
- **Issue**: Users could send a `role` field during registration/update to gain admin privileges
- **Attack**:
```json
POST /api/auth/register
{
  "username": "attacker",
  "password": "Password123",
  "fullName": "Attacker",
  "role": "Admin"  // ❌ Client can set role!
}
```

#### Fix Applied

**1. Registration Endpoint**:
```csharp
// ✅ FIXED: Role is always set to "User" - no client input allowed
var user = new User
{
    Username = req.Username,
    FullName = req.FullName ?? req.Username,
    PasswordHash = _pw.Hash(req.Password),
    Role = "User",  // Hardcoded - never changes
    CreatedAt = DateTime.UtcNow
};
```

**2. UpdateUser Endpoint**:
```csharp
// ✅ FIXED: Role field removed from DTO - impossible to set
public record UpdateUserRequest(string? Username, string? FullName);
// Note: Role field is completely absent
```

**3. DTO Changes**:
```csharp
// OLD (vulnerable):
public record UpdateUserRequest(string? Username, string? FullName, string? Role);

// NEW (secure):
public record UpdateUserRequest(string? Username, string? FullName);
```

---

### 4. **Stored/Reflected XSS (Cross-Site Scripting)**

#### Vulnerability
- **Location**: `xss-demo.component.ts` and `xss-demo.component.html`
- **Issue**: User input from URL query parameters was displayed using `[innerHTML]` with `bypassSecurityTrustHtml()`
- **Attack Vector**: 
```
/?q=<img src=x onerror="alert('XSS')">
/?q=<script>steal('tokens')</script>
```

#### Fix Applied

**TypeScript Component**:
```typescript
// OLD (vulnerable):
this.unsafeHtml = this.sanitizer.bypassSecurityTrustHtml(this.rawQuery);

// NEW (secure):
this.safeQuery = p.get('q') ?? '';  // Just store the string
```

**HTML Template**:
```html
<!-- OLD (vulnerable): -->
<div class="echo" [innerHTML]="unsafeHtml"></div>

<!-- NEW (secure - using interpolation which auto-escapes): -->
<div class="echo"><p>{{ safeQuery }}</p></div>
```

**Why this is secure**: Angular's string interpolation `{{ }}` automatically escapes all HTML special characters, making XSS attacks impossible.

---

### 5. **JWT Manipulation & Weak Validation**

#### Vulnerability
- **Location**: Frontend token storage and validation
- **Issues**:
  - Token stored without format validation
  - Token not checked for expiration before use
  - No validation that token is valid JWT format

#### Fix Applied

**Auth Service** (`auth.service.ts`):
```typescript
// ✅ FIXED: JWT format validation before storing
setToken(token: string) {
    if (!token || !token.trim()) {
        console.warn('Attempted to store invalid token');
        return;
    }
    // Validate JWT format before storing
    if (this.isValidJwtFormat(token)) {
        localStorage.setItem('token', token);
    }
}

private isValidJwtFormat(token: string): boolean {
    const parts = token.split('.');
    if (parts.length !== 3) return false;  // JWT must have 3 parts
    
    try {
        const payload = JSON.parse(atob(parts[1]));
        return typeof payload === 'object' && payload !== null;
    } catch {
        return false;
    }
}
```

**Auth Interceptor** (`auth.interceptor.ts`):
```typescript
// ✅ FIXED: Token expiration validation and format check
export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const token = doc.defaultView?.localStorage?.getItem('token');
    
    if (token && token.trim() && req.url.includes('/api/')) {
        if (isValidTokenFormat(token)) {  // Check before using
            return next(req.clone({
                setHeaders: { Authorization: `Bearer ${token}` }
            }));
        } else {
            localStorage.removeItem('token');  // Remove invalid token
        }
    }
    return next(req);
};

function isValidTokenFormat(token: string): boolean {
    const parts = token.split('.');
    if (parts.length !== 3) return false;
    
    try {
        const payload = JSON.parse(atob(parts[1]));
        // Check expiration
        if (payload.exp) {
            const expirationDate = new Date(payload.exp * 1000);
            if (expirationDate < new Date()) {
                return false;  // Token expired
            }
        }
        return true;
    } catch {
        return false;
    }
}
```

**Backend JWT Configuration** (`Program.cs`):
- ✅ Already properly configured with:
  - `ValidateIssuer = true`
  - `ValidateAudience = true`
  - `ValidateLifetime = true`
  - `ValidateIssuerSigningKey = true`

---

### 6. **Weak Password Policy**

#### Vulnerability
- **Location**: `AuthController.cs` - `Register()` endpoint
- **Issue**: Minimum 3 character passwords with no complexity requirements
- **Example**: "abc" was a valid password

#### Fix Applied

```csharp
// OLD (vulnerable):
if (req.Password.Length < 3)
    return BadRequest("Password must be at least 3 characters");

// NEW (secure):
if (req.Password.Length < 8 || 
    !req.Password.Any(char.IsUpper) || 
    !req.Password.Any(char.IsDigit))
    return BadRequest("Password must be at least 8 chars with uppercase letter and number");
```

**Password Requirements Now**:
- ✅ Minimum 8 characters
- ✅ At least 1 uppercase letter (A-Z)
- ✅ At least 1 digit (0-9)
- ✅ Special characters recommended in real applications

---

### 7. **Missing Old Password Verification on Password Change**

#### Vulnerability
- **Location**: `AuthController.cs` - `ChangePassword()` endpoint
- **Issue**: Users could change password without verifying the old password
- **Attack**: Account takeover if session is compromised

#### Fix Applied

**DTO Update**:
```csharp
// OLD (vulnerable):
public record ChangePasswordRequest(string NewPassword);

// NEW (secure):
public record ChangePasswordRequest(string OldPassword, string NewPassword);
```

**Controller Implementation**:
```csharp
// OLD (vulnerable):
// ❌ No old password check!

// NEW (secure):
if (string.IsNullOrWhiteSpace(req.OldPassword) || 
    !_pw.Verify(req.OldPassword, user.PasswordHash))
    return Unauthorized("Current password is incorrect");

// Also validate new password strength
if (req.NewPassword.Length < 8 || 
    !req.NewPassword.Any(char.IsUpper) || 
    !req.NewPassword.Any(char.IsDigit))
    return BadRequest("Password must contain uppercase letter and number");
```

---

### 8. **Information Disclosure - Exposed User Data**

#### Vulnerability
- **Location**: `AuthController.cs` - `ListAllUsers()` endpoint
- **Issue**: Publicly accessible endpoint returning all users with password hashes
- **Exposed Data**:
  - User IDs
  - Usernames
  - Full Names
  - Roles
  - **Password Hashes** (even though they're hashed, exposing them is a vulnerability)
  - Creation dates

#### Fix Applied

```csharp
// COMPLETELY REMOVED - This endpoint should not exist:
[HttpGet("users")]
public async Task<ActionResult> ListAllUsers()  // ❌ REMOVED
```

**Replacement**: Only authenticated users can access their own profile via:
- `GET /api/auth/profile` (to be implemented with authentication required)

---

### 9. **SQL Injection (Bonus Fix)**

#### Vulnerability
- **Location**: `AuthController.cs` - `FindUserUnsafe()` endpoint
- **Issue**: Direct string interpolation in SQL query
```csharp
// ❌ VULNERABLE:
.FromSqlRaw($"SELECT * FROM Users WHERE Username = '{username}'")
```

#### Fix Applied

```csharp
// COMPLETELY REMOVED - This endpoint should not exist
[HttpGet("find-user-unsafe")]
public async Task<ActionResult> FindUserUnsafe(string username)  // ❌ REMOVED
```

**Proper Implementation** (if needed):
```csharp
// ✅ SECURE:
.Where(u => u.Username == username)  // EF Core parameterizes automatically
```

---

### 10. **Open Redirect (Bonus Fix)**

#### Vulnerability
- **Location**: `AuthController.cs` - `OpenRedirect()` endpoint
- **Issue**: Unvalidated redirect URL

#### Fix Applied

```csharp
// COMPLETELY REMOVED:
[HttpGet("redirect")]
public IActionResult OpenRedirect(string to)  // ❌ REMOVED
```

---

## 🔐 Additional Security Measures Implemented

### CORS Configuration
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("dev", p => p
        .WithOrigins("http://localhost:4200")  // ✅ Specific origin
        .AllowAnyHeader()
        .AllowAnyMethod()
    );
});
```

### HTTPS in Production
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();  // ✅ Enforced in production
}
```

---

## 🧪 Testing Recommendations

### 1. IDOR Testing
```bash
# Verify user can only access their own accounts
GET /api/accounts (authenticated as User A) ✅ Returns User A's accounts
GET /api/accounts/user/2 (should be removed) ❌ 404 Not Found
```

### 2. Transfer Authorization Testing
```bash
# User A cannot initiate transfer from User B's account
POST /api/transfers/initiate
{
  "fromAccountId": 999,  // User B's account
  "toIban": "...",
  "amount": 1000
}
# Response: ❌ "Not allowed" (ownership check failed)
```

### 3. Mass Assignment Testing
```bash
# Try to register as admin
POST /api/auth/register
{
  "username": "hacker",
  "password": "Password123",
  "fullName": "Hacker",
  "role": "Admin"  // ❌ Ignored
}
# Response: User created with role "User" (not "Admin")
```

### 4. XSS Testing
```bash
GET /?q=<img src=x onerror="alert('XSS')">
# Response: Safe display - script doesn't execute
```

### 5. Password Change Testing
```bash
POST /api/auth/change-password
{
  "newPassword": "NewPassword123"  // Missing oldPassword
}
# Response: ❌ 400 Bad Request - oldPassword required
```

---

## 📋 Security Checklist

- [x] Fix IDOR vulnerabilities
- [x] Remove unsafe transfer endpoints
- [x] Prevent mass assignment attacks
- [x] Fix XSS vulnerabilities
- [x] Implement JWT validation
- [x] Enforce strong password policy
- [x] Require old password verification
- [x] Remove information disclosure endpoints
- [x] Remove SQL injection vulnerable code
- [x] Remove open redirect vulnerability
- [x] Configure CORS properly
- [x] Document all security fixes

---

## 🚀 Deployment Recommendations

1. **Before Production**:
   - [ ] Update password requirements in database
   - [ ] Force password change for existing users
   - [ ] Enable HTTPS
   - [ ] Update CORS origin to production domain
   - [ ] Implement rate limiting
   - [ ] Implement account lockout after failed attempts
   - [ ] Enable audit logging
   - [ ] Implement DDoS protection

2. **Infrastructure**:
   - [ ] Use environment variables for secrets
   - [ ] Implement Web Application Firewall (WAF)
   - [ ] Monitor for suspicious activities
   - [ ] Regular security updates for dependencies

3. **Ongoing**:
   - [ ] Conduct security audits quarterly
   - [ ] Update dependencies regularly
   - [ ] Perform penetration testing
   - [ ] Monitor security advisories

---

## 🔗 Related Resources

- OWASP Top 10: https://owasp.org/www-project-top-ten/
- CWE-639 (Mass Assignment): https://cwe.mitre.org/data/definitions/639.html
- JWT Best Practices: https://tools.ietf.org/html/rfc8725
- Angular Security Guide: https://angular.io/guide/security

---

**Last Updated**: 2026-01-30  
**Status**: ✅ All vulnerabilities fixed and documented
