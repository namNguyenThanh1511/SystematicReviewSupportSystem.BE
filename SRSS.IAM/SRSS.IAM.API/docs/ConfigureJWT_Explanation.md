# Giải thích `ConfigureJWT`

Tôi sẽ giải thích chi tiết từng dòng của hàm `ConfigureJWT`.

## 📘 Giải Thích Chi Tiết Hàm `ConfigureJWT`

```csharp
public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
{
    // 📖 BƯỚC 1: Đọc cấu hình JWT từ appsettings.json
    var jwtSettings = configuration.GetSection("JwtSettings");
    // → Lấy section "JwtSettings" từ file appsettings.json
    // → Chứa: validIssuer, validAudience, secretKey, AccessTokenExpirationMinutes...
    
    var secretKey = jwtSettings["secretKey"];
    // → Lấy giá trị secretKey từ JwtSettings
    // → Ví dụ: "DhftOS5uphK3vmCJQrexST1RsyjZBjXWRgJMFPU4"
    // → Đây là KEY BÍ MẬT dùng để ký (sign) và xác thực (verify) JWT token

    // 🔐 BƯỚC 2: Đăng ký Authentication Service
    services.AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        // → Scheme mặc định để AUTHENTICATE (xác thực) user
        // → Giá trị: "Bearer" 
        // → Khi user gửi request với header: "Authorization: Bearer <token>"
        // → Hệ thống sẽ dùng JWT Bearer scheme để verify token
        
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        // → Scheme mặc định để CHALLENGE (thách thức) khi authentication fails
        // → Nghĩa là: Khi user KHÔNG có token hoặc token SAI
        // → Response sẽ trả về 401 Unauthorized với WWW-Authenticate: Bearer
    })
    
    // 🎫 BƯỚC 3: Cấu hình JWT Bearer Authentication
    .AddJwtBearer(options =>
    {
        // 📋 TokenValidationParameters: Các tham số để VALIDATE (xác thực) token
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // ✅ ValidateIssuer = true
            // → BẮT BUỘC kiểm tra "iss" (issuer) claim trong token
            // → Token phải được phát hành bởi server mà mình tin tưởng
            // → Ngăn chặn: Token giả mạo từ server khác
            ValidateIssuer = true,
            
            // ✅ ValidateAudience = true
            // → BẮT BUỘC kiểm tra "aud" (audience) claim trong token
            // → Token phải được tạo ra cho đúng audience (client/app này)
            // → Ngăn chặn: Token được tạo cho app khác đem sang dùng
            ValidateAudience = true,
            
            // ✅ ValidateLifetime = true
            // → BẮT BUỘC kiểm tra thời gian sống của token
            // → Kiểm tra "exp" (expiration) claim
            // → Kiểm tra "nbf" (not before) claim
            // → Ngăn chặn: Token hết hạn, token chưa được kích hoạt
            ValidateLifetime = true,
            
            // ✅ ValidateIssuerSigningKey = true
            // → BẮT BUỘC xác thực chữ ký (signature) của token
            // → Dùng secretKey để verify signature
            // → Ngăn chặn: Token bị sửa đổi (tamper), token giả mạo
            ValidateIssuerSigningKey = true,
            
            // 🏢 ValidIssuer: Server nào được phép phát hành token?
            ValidIssuer = jwtSettings["validIssuer"],
            // → Giá trị từ appsettings.json: "SRSSAPI"
            // → Token phải có "iss": "SRSSAPI" thì mới hợp lệ
            // → Ví dụ check:
            //   if (token.Issuer != "SRSSAPI") → REJECT
            
            // 👥 ValidAudience: Token này dành cho ai?
            ValidAudience = jwtSettings["validAudience"],
            // → Giá trị từ appsettings.json: "SRSSClient"
            // → Token phải có "aud": "SRSSClient" thì mới hợp lệ
            // → Ví dụ check:
            //   if (token.Audience != "SRSSClient") → REJECT
            
            // 🔑 IssuerSigningKey: Khóa bí mật để verify signature
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            // → Convert secretKey (string) thành byte array
            // → Wrap trong SymmetricSecurityKey object
            // → Dùng để:
            //   1. VERIFY signature khi nhận token (authentication)
            //   2. SIGN token khi tạo mới (trong JwtService)
            // 
            // → Quy trình verify:
            //   1. Lấy header + payload từ token
            //   2. Dùng HMAC-SHA256 + secretKey → tạo signature mới
            //   3. So sánh signature mới với signature trong token
            //   4. Nếu GIỐNG → Token hợp lệ ✅
            //      Nếu KHÁC → Token bị sửa đổi ❌
        };
    });
}
```

---

## 🎯 Mục đích của config này

### 1. Bảo mật đa tầng (Defense in Depth)

- Validate signature: đảm bảo token không bị sửa đổi.
- Validate issuer: token được phát hành bởi nguồn tin cậy.
- Validate audience: token dành cho ứng dụng/khách hàng cụ thể.
- Validate lifetime: token còn hiệu lực.
┌─────────────────────────────────────────────────┐
│ REQUEST: Authorization: Bearer eyJhbGc...       │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│ ✅ LAYER 1: ValidateIssuerSigningKey            │
│    → Token có bị sửa đổi không?                 │
│    → Signature hợp lệ?                          │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│ ✅ LAYER 2: ValidateIssuer                      │
│    → Token từ server nào?                       │
│    → "iss" == "SRSSAPI"?                        │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│ ✅ LAYER 3: ValidateAudience                    │
│    → Token dành cho ai?                         │
│    → "aud" == "SRSSClient"?                     │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│ ✅ LAYER 4: ValidateLifetime                    │
│    → Token còn hạn không?                       │
│    → "exp" > DateTime.UtcNow?                   │
└─────────────────────────────────────────────────┘
                      ↓
              ✅ TOKEN HỢP LỆ

FLOW XÁC THỰC TOKEN

// 1️⃣ CLIENT GỬI REQUEST
GET /api/auth/profile
Headers:
  Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c

// 2️⃣ ASP.NET CORE MIDDLEWARE INTERCEPT
↓
UseAuthentication() middleware chặn request

// 3️⃣ JWT BEARER HANDLER XỬ LÝ
↓
JwtBearerHandler.HandleAuthenticateAsync()
  └─ Extract token từ "Authorization: Bearer {token}"
  └─ Gọi TokenValidationParameters để validate

// 4️⃣ VALIDATION PROCESS
↓
TokenValidationParameters validates:
  ├─ Signature (dùng IssuerSigningKey)
  ├─ Issuer ("iss" == ValidIssuer?)
  ├─ Audience ("aud" == ValidAudience?)
  └─ Lifetime (exp > now?)

// 5️⃣ KẾT QUẢ
✅ Success:
  → HttpContext.User = ClaimsPrincipal với claims từ token
  → User.Identity.IsAuthenticated = true
  → Controller có thể access User.Claims

❌ Failure:
  → Return 401 Unauthorized
  → Response header: WWW-Authenticate: Bearer error="invalid_token"

### 2. Ngăn chặn các loại tấn công

| Tấn công | Cách ngăn chặn | Cấu hình liên quan |
|---|---|---|
| Token forgery / tampering | Verify signature | ValidateIssuerSigningKey + IssuerSigningKey |
| Replay (token cũ) | Kiểm tra exp | ValidateLifetime |
| Token từ nguồn khác | Kiểm tra iss | ValidateIssuer + ValidIssuer |
| Token dùng sai mục đích | Kiểm tra aud | ValidateAudience + ValidAudience |

---

## 🔍 Chi tiết từng tham số

- ValidateIssuer / ValidIssuer: kiểm tra claim `iss` của token.
- ValidateAudience / ValidAudience: kiểm tra claim `aud` của token.
- ValidateLifetime: kiểm tra `nbf`/`exp`.
- ValidateIssuerSigningKey / IssuerSigningKey: xác thực chữ ký token bằng secret key.

---

## 💡 Best practices

- Secret key phải đủ mạnh (>=32 chars) và không commit vào mã nguồn.
- Sử dụng environment variables hoặc secret manager (Key Vault, Secrets Manager).
- Thiết lập `ClockSkew = TimeSpan.Zero` nếu cần độ chính xác cao hơn.
- Nếu cần chấp nhận nhiều issuer, dùng `ValidIssuers`.
- Log lý do validate fail ở mức debug/warn, không leak thông tin nhạy cảm.

---

## 🎓 Tóm tắt

Hàm `ConfigureJWT` bật authentication dựa trên JWT và thiết lập các kiểm tra để đảm bảo token hợp lệ: chữ ký, nguồn, đích và thời hạn. Đây là lớp phòng thủ chính để bảo vệ API khỏi token giả mạo hoặc token hết hạn.

