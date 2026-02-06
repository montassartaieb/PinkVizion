# 🔒 Sécurité - PinkVision

## 🎫 Authentification JWT

### Flow d'authentification

```
┌─────────────┐     1. Login Request      ┌──────────────┐
│   Frontend  │ ──────────────────────────▶│ Auth Service │
│  (Angular)  │                            │  (JWT)       │
└─────────────┘                            └──────────────┘
       │                                          │
       │     2. Validate credentials              │
       │     3. Generate JWT + Refresh Token      │
       │                                          │
       │◀─────────────────────────────────────────│
       │     4. Return tokens                     │
       │                                          │
       ▼                                          ▼
┌─────────────┐     5. API Request + JWT   ┌──────────────┐
│   Frontend  │ ──────────────────────────▶│  API Gateway │
└─────────────┘   Authorization: Bearer    │    (YARP)    │
                                           └──────────────┘
                                                  │
                                           6. Validate JWT
                                           7. Route to service
                                                  │
                                                  ▼
                                           ┌──────────────┐
                                           │ Microservice │
                                           └──────────────┘
```

### Structure du Token JWT

**Header:**
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload:**
```json
{
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "firstName": "Jean",
  "lastName": "Dupont",
  "role": ["PATIENT"],
  "iat": 1707170400,
  "exp": 1707174000,
  "iss": "PinkVision",
  "aud": "PinkVisionAPI",
  "jti": "unique-token-id"
}
```

### Configuration JWT

| Paramètre | Valeur | Description |
|-----------|--------|-------------|
| Algorithm | HS256 | HMAC SHA-256 |
| Access Token Expiry | 60 min | Durée de vie du token d'accès |
| Refresh Token Expiry | 7 jours | Durée de vie du refresh token |
| Issuer | PinkVision | Émetteur du token |
| Audience | PinkVisionAPI | Audience cible |

---

## 🔐 Gestion des Rôles

### Rôles disponibles

| Rôle | Description | Permissions |
|------|-------------|-------------|
| **ADMIN** | Administrateur | Accès total, gestion utilisateurs |
| **MEDECIN** | Médecin | Analyse images, validation diagnostics, accès patients |
| **PATIENT** | Patient | Consultation profil, historique, prise RDV |

### Matrice des permissions

| Endpoint | PATIENT | MEDECIN | ADMIN |
|----------|---------|---------|-------|
| `GET /api/patients/me` | ✅ | ❌ | ❌ |
| `GET /api/patients` | ❌ | ✅ | ✅ |
| `GET /api/patients/{id}` | ❌ | ✅ | ✅ |
| `POST /api/imaging/upload` | ✅ | ✅ | ✅ |
| `POST /api/imaging/{id}/analyze` | ❌ | ✅ | ✅ |
| `POST /api/imaging/diagnosis/validate` | ❌ | ✅ | ❌ |
| `GET /api/users` | ❌ | ❌ | ✅ |
| `POST /api/users/assign-role` | ❌ | ❌ | ✅ |

---

## 🔄 Refresh Token Flow

```
1. Access token expiré (401 Unauthorized)
         │
         ▼
2. Frontend envoie refresh token
   POST /api/auth/refresh
   { "refreshToken": "..." }
         │
         ▼
3. Auth Service valide le refresh token
   - Token non expiré?
   - Token non révoqué?
   - Utilisateur actif?
         │
         ▼
4. Génération de nouveaux tokens
   - Nouveau access token
   - Nouveau refresh token
   - Révocation de l'ancien refresh token
         │
         ▼
5. Retour des nouveaux tokens au frontend
```

### Sécurité du Refresh Token

- Stocké côté serveur avec hash
- Rotation à chaque utilisation
- Révocation automatique lors de:
  - Changement de mot de passe
  - Désactivation du compte
  - Déconnexion explicite
  - Détection d'activité suspecte

---

## 🛡️ Protection API Gateway (YARP)

### Validation JWT centralisée

```csharp
// Configuration YARP
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "PinkVision",
            ValidAudience = "PinkVisionAPI",
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero  // Pas de tolérance sur l'expiration
        };
    });
```

### Headers de sécurité

```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Strict-Transport-Security: max-age=31536000; includeSubDomains
Content-Security-Policy: default-src 'self'
```

---

## 🔐 Hachage des mots de passe

### BCrypt Configuration

```csharp
// Hachage avec BCrypt
var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

// Vérification
var isValid = BCrypt.Net.BCrypt.Verify(password, passwordHash);
```

**Paramètres:**
- Work Factor: 12 (ajustable selon la puissance serveur)
- Salt: Généré automatiquement par BCrypt
- Algorithme: BCrypt (résistant aux attaques par GPU)

---

## 📋 Audit et Logging

### Événements audités

| Événement | Données loguées |
|-----------|-----------------|
| Login | userId, email, IP, timestamp, success/failure |
| Logout | userId, timestamp |
| Password Change | userId, timestamp |
| Role Assignment | targetUserId, role, assignedBy, timestamp |
| Image Upload | imageId, patientId, uploadedBy, timestamp |
| Diagnosis Validation | diagnosisId, doctorId, timestamp |

### Format des logs

```json
{
  "timestamp": "2024-02-05T22:00:00Z",
  "level": "Information",
  "event": "UserLogin",
  "userId": "uuid",
  "email": "user@example.com",
  "ipAddress": "192.168.1.100",
  "userAgent": "Mozilla/5.0...",
  "success": true,
  "correlationId": "request-uuid"
}
```

---

## 🚨 Protection contre les attaques

### Rate Limiting

| Endpoint | Limite | Fenêtre |
|----------|--------|---------|
| `/api/auth/login` | 5 tentatives | 15 min |
| `/api/auth/register` | 3 tentatives | 1 heure |
| `/api/imaging/upload` | 10 uploads | 1 heure |
| Autres endpoints | 100 requêtes | 1 min |

### Protection CORS

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://pinkvision.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

### Validation des entrées

- Validation côté serveur avec FluentValidation
- Sanitization des inputs
- Paramètres SQL sécurisés (Entity Framework)
- Limite de taille des fichiers (50 MB max)

---

## 🔑 Gestion des secrets

### Variables d'environnement

```bash
# Ne jamais commiter ces valeurs!
JWT_SECRET=<généré-aléatoirement-32-chars-min>
POSTGRES_PASSWORD=<mot-de-passe-fort>
MONGO_PASSWORD=<mot-de-passe-fort>
RABBITMQ_PASSWORD=<mot-de-passe-fort>
SMTP_PASSWORD=<app-password>
```

### Recommandations

1. **Ne jamais hardcoder** les secrets dans le code
2. Utiliser des **variables d'environnement** ou un **vault**
3. **Rotation régulière** des clés JWT
4. **Mots de passe forts** (min 16 caractères, mixte)
5. **Chiffrement TLS** en production

---

## ✅ Checklist Sécurité

- [ ] JWT signé avec clé secrète forte (32+ caractères)
- [ ] Refresh tokens avec rotation
- [ ] Mots de passe hachés avec BCrypt
- [ ] Rate limiting activé
- [ ] CORS configuré correctement
- [ ] Headers de sécurité HTTP
- [ ] Validation des entrées
- [ ] Audit logging activé
- [ ] HTTPS en production
- [ ] Secrets dans variables d'environnement
