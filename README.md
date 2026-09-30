 🛡️️ BankLite.Api – Secure Banking REST API & Security Audit

BankLite.Api is an enterprise-grade backend project developed with .NET 8 Web API, designed to implement digital banking workflows while analyzing, demonstrating, and remediating common web and API vulnerabilities. The project includes an end-to-end security audit and hands-on remediation addressing authentication bypasses, broken object-level authorization (BOLA), and SQL Injection risks.

📌 The Platform Includes

* 🏦 **Account & Transaction Management:** Account creation, balance inquiry, money transfers, and account termination workflows (`Controllers`, `Services`).
* 🔐 **Authentication & Token Hardening:** Secure JWT rotation, token spoofing prevention, and token-based session verification.
* 🛡️ **Credential & Identifier Protection:** Salted password hashing, mandatory old password verification on updates, and ID obfuscation/hashing strategies.
* 🧱 **Custom Security Middleware & Attributes:** Specialized filters and middleware layers to validate inbound requests, enforce access policies, and restrict sensitive data paths (`Middleware`, `Attributes`).
* 🔍 **Security Audit Documentation:** In-depth technical reports detailing vulnerability mechanisms, exploit scenarios, and implemented remediation patches (`SECURITY_AUDIT_COMPLETE.md`, `SECURITY_FIXES.md`, `SECURITY_SQL_INJECTION_ANALYSIS.md`).
* 🗄️ **Data Architecture & DTO Isolation:** Entity Framework Core integration, secure database migrations, and decoupled DTO layers to prevent mass assignment vulnerabilities (`Migrations`, `Models`, `DTOs`).

💡 My Contribution

* 🛡️ **Vulnerability Analysis & Remediation:** Identified and patched critical security flaws, including cross-token transaction abuse (BOLA/IDOR), unauthorized resource access, and SQL Injection vectors.
* ⚙️ **Custom Middleware Development:** Engineered ASP.NET Core middleware pipelines for runtime request verification and password hash enforcement.
* 🔒 **Configuration Hardening:** Secured application environments by sanitizing sensitive keys and tightening configuration profiles across `appsettings.json` and development settings.
* 📝 **Security Reporting:** Authored comprehensive audit reports outlining attack vectors, proof-of-concept scenarios, and long-term mitigation standards.
* 🧩 **DTO & Input Validation:** Built dedicated DTO structures and request validation logic to eliminate direct domain model binding risks.

 🧠 What I Learned

* Applying Secure Software Development Lifecycle (SSDLC) principles within modern .NET 8 Web API architectures.
* Analyzing and mitigating high-impact threats from the OWASP API Security Top 10.
* Enforcing robust password hashing and secure token-based authorization schemes.
* Diagnosing SQL Injection attack mechanics and applying parameterized query safeguards via ORMs.
* Performing structured source code security audits and drafting industry-standard remediation guides.

🛠 Tech Stack

* **Language & Framework:** .NET 8 (C#), ASP.NET Core Web API
* **Database & ORM:** Entity Framework Core, Code-First Migrations, DTO Architecture
* **Security & Auth:** JWT, Cryptographic Password Hashing, Custom Middleware & Attributes, Input Validation
* **Testing & Exploration Tools:** REST Client / `.http` files, Postman
* **Documentation:** Markdown (Technical Audit and Remediation Reports)

 🚀 Project Purpose

* Expose common architectural and logic flaws inherent in standard financial workflows.
* Provide practical, hands-on experience in hardening vulnerable codebases up to enterprise standards.
* Bridge backend software engineering with defensive cybersecurity practices through an auditable, resilient API.
