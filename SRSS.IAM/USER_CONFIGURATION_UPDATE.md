# User Entity Configuration - IAM Service

## ? C?p Nh?t UserConfiguration

### ?? **Các Thay ??i Chính:**

#### 1. **Table Name**
- ? C?: `"User"`
- ? M?i: `"users"` (snake_case, plural form)

#### 2. **Column Name Mapping (Snake Case)**

| Property | Old Column | New Column | Status |
|----------|-----------|------------|--------|
| Id | (default) | `id` | ? |
| Username | (default) | `username` | ? |
| Password | (default) | `password` | ? |
| FullName | (default) | `full_name` | ? |
| Email | (default) | `email` | ? |
| Role | (default) | `role` | ? |
| IsActive | ? Missing | `is_active` | ? Added |
| RefreshToken | ? Missing | `refresh_token` | ? Added |
| IsRefreshTokenRevoked | ? Missing | `is_refresh_token_revoked` | ? Added |
| RefreshTokenExpiryTime | ? Missing | `refresh_token_expiry_time` | ? Added |
| CreatedAt | (default) | `created_at` | ? |
| ModifiedAt | (default) | `updated_at` | ? |

#### 3. **Default Values**
```csharp
// Added default values for boolean properties
IsActive: default = true
IsRefreshTokenRevoked: default = false
```

#### 4. **Max Length Constraints**
```csharp
Username: MaxLength(50)
Password: MaxLength(255)
FullName: MaxLength(100)
Email: MaxLength(100)
RefreshToken: MaxLength(500)
```

#### 5. **Indexes with Named Constraints**
- ? `ix_users_email` - Unique index on email
- ? `ix_users_username` - Unique index on username
- ? `ix_users_is_active` - Index on is_active for filtering active users

### ?? **Configuration Summary**

```csharp
Table: users
Primary Key: id (Guid)
Unique Constraints: 
  - username
  - email
Default Values:
  - is_active = true
  - is_refresh_token_revoked = false
Indexes:
  - ix_users_email (unique)
  - ix_users_username (unique)
  - ix_users_is_active
```

### ?? **Security & Performance Improvements**

1. **Password Storage**
   - MaxLength(255) - suitable for bcrypt/PBKDF2 hashes
   - Nullable - supports external authentication (OAuth, SSO)

2. **Refresh Token Management**
   - MaxLength(500) for JWT refresh tokens
   - Revocation flag for security
   - Expiry time for automatic cleanup

3. **Performance Indexes**
   - Username & Email: Unique indexes for fast lookup and authentication
   - IsActive: Filter index for querying active users only

### ? **Validation**

- ? Build successful
- ? All properties mapped
- ? Snake_case naming convention
- ? Consistent with Project service conventions
- ? Default values configured
- ? Indexes optimized

### ?? **Migration Required**

After this configuration update, you need to create a new migration:

```bash
cd Services/SRSS.IAM/SRSS.IAM.Repositories
dotnet ef migrations add UpdateUserConfiguration --startup-project ../SRSS.IAM.API
dotnet ef database update --startup-project ../SRSS.IAM.API
```

### ?? **Breaking Changes**

This update changes table and column names. Existing data will need migration:

**Old Schema:**
```sql
Table: User
Columns: Id, Username, Password, FullName, Email, Role, CreatedAt, ModifiedAt
```

**New Schema:**
```sql
Table: users
Columns: id, username, password, full_name, email, role, is_active, 
         refresh_token, is_refresh_token_revoked, refresh_token_expiry_time,
         created_at, updated_at
```

### ?? **Benefits**

1. **Consistency**: Matches naming convention across microservices
2. **Completeness**: All entity properties properly configured
3. **Performance**: Optimized indexes for common queries
4. **Security**: Proper constraints and token management
5. **Maintainability**: Clear, explicit configuration

---

## ?? **Comparison with Old Configuration**

### Before (12 lines)
```csharp
builder.ToTable("User");
builder.HasKey(u => u.Id);
builder.Property(u => u.Password).IsRequired(false);
builder.Property(u => u.Email).HasMaxLength(100).IsRequired(false);
builder.HasIndex(u => u.Email).IsUnique();
builder.Property(u => u.Username).HasMaxLength(50);
builder.HasIndex(u => u.Username).IsUnique();
builder.Property(u => u.FullName).HasMaxLength(100);
builder.Property(u => u.Role).IsRequired();
builder.Property(u => u.Role).HasConversion<string>();
builder.Property(u => u.CreatedAt);
builder.Property(u => u.ModifiedAt);
```

**Issues:**
- ? PascalCase table name
- ? No column name mapping
- ? Missing 4 properties (IsActive, RefreshToken, IsRefreshTokenRevoked, RefreshTokenExpiryTime)
- ? No default values
- ? Unnamed indexes
- ? Email marked as optional (should be required)

### After (89 lines)
- ? snake_case naming
- ? All 12 properties configured
- ? Default values set
- ? Named indexes
- ? Email required
- ? Explicit column mappings

---

## ?? **Related Files**

- Entity: `Services/SRSS.IAM/SRSS.IAM.Repositories/Entities/User.cs`
- Configuration: `Services/SRSS.IAM/SRSS.IAM.Repositories/Configurations/UserConfiguration.cs`
- DbContext: `Services/SRSS.IAM/SRSS.IAM.Repositories/AppDbContext.cs`

## ? **Next Steps**

1. Review the configuration changes
2. Create migration: `dotnet ef migrations add UpdateUserConfiguration`
3. Review the generated migration SQL
4. Apply migration: `dotnet ef database update`
5. Test authentication and user management features
6. Update any hardcoded SQL queries to use new column names
