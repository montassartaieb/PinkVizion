# 🧪 Plan de Tests - PinkVision

## 📊 Vue d'ensemble

| Type de test | Couverture cible | Outils |
|--------------|------------------|--------|
| Unitaires | 80% | xUnit, Moq |
| Intégration | 60% | xUnit, TestContainers |
| End-to-End | Flows critiques | Cypress, Playwright |
| Sécurité | OWASP Top 10 | OWASP ZAP |
| Performance | Endpoints critiques | k6, Artillery |

---

## 🔬 Tests Unitaires

### Auth Service

```csharp
public class AuthServiceTests
{
    [Fact]
    public async Task Register_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "SecurePass123!",
            ConfirmPassword = "SecurePass123!",
            FirstName = "Test",
            LastName = "User",
            UserType = "PATIENT"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.User);
        Assert.Equal("test@example.com", result.User.Email);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldFail()
    {
        // Test d'email déjà existant
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Test de connexion réussie
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldFail()
    {
        // Test de mot de passe incorrect
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        // Test de refresh token
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ShouldFail()
    {
        // Test de token expiré
    }

    [Fact]
    public async Task ChangePassword_ShouldRevokeAllTokens()
    {
        // Vérifier que tous les refresh tokens sont révoqués
    }
}
```

### Patient Service

```csharp
public class PatientServiceTests
{
    [Fact]
    public async Task CreatePatient_ShouldCalculateBMI()
    {
        // Vérifier le calcul automatique du BMI
    }

    [Fact]
    public async Task CreatePatient_WithExistingUserId_ShouldFail()
    {
        // Un utilisateur ne peut avoir qu'un seul profil patient
    }

    [Fact]
    public async Task UpdatePatient_ShouldUpdateOnlyProvidedFields()
    {
        // Mise à jour partielle
    }

    [Fact]
    public async Task RecordVitals_ShouldAddToHistory()
    {
        // Les mesures vitales sont historisées
    }

    [Fact]
    public async Task RecordVitals_ShouldUpdateCurrentValues()
    {
        // Les dernières mesures mettent à jour le patient
    }
}
```

### Imaging Service

```csharp
public class ImagingServiceTests
{
    [Fact]
    public async Task UploadImage_WithValidFile_ShouldStoreImage()
    {
        // Upload réussi
    }

    [Fact]
    public async Task UploadImage_WithInvalidFileType_ShouldFail()
    {
        // Rejet des fichiers non supportés
    }

    [Fact]
    public async Task AnalyzeImage_ShouldCallAIService()
    {
        // Vérifier l'appel à l'IA
    }

    [Fact]
    public async Task AnalyzeImage_WhenAIUnavailable_ShouldReturnError()
    {
        // Gestion de l'indisponibilité IA
    }

    [Fact]
    public async Task ValidateDiagnosis_ShouldSetDoctorInfo()
    {
        // Validation par médecin
    }
}
```

---

## 🔗 Tests d'Intégration

### Base de données

```csharp
public class PatientRepositoryIntegrationTests : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task GetPatientById_ShouldIncludeBloodGroup()
    {
        // Vérifier le chargement des relations
    }

    [Fact]
    public async Task SearchPatients_ShouldFilterCorrectly()
    {
        // Test de recherche avec filtres
    }
}
```

### API Endpoints

```csharp
public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task RegisterAndLogin_FullFlow()
    {
        // 1. Register
        var registerResponse = await _client.PostAsync("/api/auth/register", ...);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // 2. Login
        var loginResponse = await _client.PostAsync("/api/auth/login", ...);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // 3. Access protected resource
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var meResponse = await _client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
    }
}
```

### Communication inter-services

```csharp
public class ImagingAIIntegrationTests
{
    [Fact]
    public async Task AnalyzeImage_WithRealAIService_ShouldReturnPrediction()
    {
        // Test avec le vrai service IA (en environnement de test)
    }

    [Fact]
    public async Task AnalyzeImage_ShouldPublishEvent()
    {
        // Vérifier la publication RabbitMQ
    }
}
```

---

## 🌐 Tests End-to-End (E2E)

### Scénarios critiques

#### 1. Inscription et connexion patient

```typescript
describe('Patient Registration Flow', () => {
  it('should register a new patient', () => {
    cy.visit('/register');
    cy.get('[data-testid="email"]').type('patient@test.com');
    cy.get('[data-testid="password"]').type('SecurePass123!');
    cy.get('[data-testid="confirm-password"]').type('SecurePass123!');
    cy.get('[data-testid="first-name"]').type('Jean');
    cy.get('[data-testid="last-name"]').type('Dupont');
    cy.get('[data-testid="user-type"]').select('PATIENT');
    cy.get('[data-testid="submit"]').click();
    
    cy.url().should('include', '/dashboard');
    cy.contains('Bienvenue, Jean');
  });

  it('should login and access patient dashboard', () => {
    cy.login('patient@test.com', 'SecurePass123!');
    cy.visit('/patient/dashboard');
    cy.contains('Mon dossier médical');
  });
});
```

#### 2. Upload et analyse de mammographie

```typescript
describe('Mammography Analysis Flow', () => {
  beforeEach(() => {
    cy.loginAsDoctor();
  });

  it('should upload and analyze mammography image', () => {
    cy.visit('/imaging/upload');
    
    // Upload image
    cy.get('[data-testid="file-input"]').attachFile('mammography.jpg');
    cy.get('[data-testid="patient-select"]').select('patient-uuid');
    cy.get('[data-testid="upload-btn"]').click();
    
    cy.contains('Image uploadée avec succès');
    
    // Analyze
    cy.get('[data-testid="analyze-btn"]').click();
    cy.contains('Analyse en cours...');
    
    // Wait for result
    cy.contains('Analyse terminée', { timeout: 30000 });
    cy.get('[data-testid="diagnosis-label"]').should('exist');
    cy.get('[data-testid="probability"]').should('exist');
  });

  it('should validate diagnosis', () => {
    cy.visit('/imaging/pending');
    cy.get('[data-testid="validate-btn"]').first().click();
    cy.get('[data-testid="final-diagnosis"]').select('BENIGN');
    cy.get('[data-testid="doctor-notes"]').type('RAS');
    cy.get('[data-testid="confirm-validation"]').click();
    
    cy.contains('Diagnostic validé');
  });
});
```

#### 3. Prise de rendez-vous

```typescript
describe('Appointment Booking', () => {
  it('should book an appointment', () => {
    cy.loginAsPatient();
    cy.visit('/appointments/new');
    
    cy.get('[data-testid="doctor-select"]').select('Dr. Smith');
    cy.get('[data-testid="date-picker"]').type('2024-03-15');
    cy.get('[data-testid="time-slot"]').first().click();
    cy.get('[data-testid="reason"]').type('Suivi annuel');
    cy.get('[data-testid="book-btn"]').click();
    
    cy.contains('Rendez-vous confirmé');
  });
});
```

---

## 🔐 Tests de Sécurité

### OWASP Top 10

| Vulnérabilité | Test | Outil |
|---------------|------|-------|
| Injection SQL | Paramètres avec payloads | sqlmap |
| Broken Auth | Brute force, token manipulation | Burp Suite |
| XSS | Injection de scripts | OWASP ZAP |
| IDOR | Accès à ressources d'autres users | Manual + Burp |
| Security Misconfiguration | Headers, CORS | securityheaders.com |
| Sensitive Data Exposure | Tokens en clair, logs | Manual review |

### Tests d'autorisation

```csharp
[Fact]
public async Task Patient_ShouldNotAccessOtherPatientData()
{
    // Connexion en tant que Patient A
    var tokenA = await LoginAsPatient("patientA@test.com");
    
    // Tentative d'accès aux données de Patient B
    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
    var response = await _client.GetAsync($"/api/patients/{patientBId}");
    
    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
}

[Fact]
public async Task Patient_ShouldNotAnalyzeImages()
{
    var token = await LoginAsPatient();
    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    
    var response = await _client.PostAsync("/api/imaging/123/analyze", null);
    
    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
}
```

---

## ⚡ Tests de Performance

### Scénarios de charge

```javascript
// k6 load test
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '1m', target: 50 },   // Montée à 50 users
    { duration: '3m', target: 50 },   // Maintien
    { duration: '1m', target: 100 },  // Pic à 100 users
    { duration: '2m', target: 100 },  // Maintien
    { duration: '1m', target: 0 },    // Descente
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],  // 95% < 500ms
    http_req_failed: ['rate<0.01'],    // <1% d'erreurs
  },
};

export default function () {
  // Login
  let loginRes = http.post('http://localhost:5000/api/auth/login', {
    email: 'test@example.com',
    password: 'password',
  });
  check(loginRes, { 'login status 200': (r) => r.status === 200 });

  // Get patients list
  let token = loginRes.json('accessToken');
  let patientsRes = http.get('http://localhost:5000/api/patients', {
    headers: { Authorization: `Bearer ${token}` },
  });
  check(patientsRes, { 'patients status 200': (r) => r.status === 200 });

  sleep(1);
}
```

### Métriques cibles

| Endpoint | Temps moyen | P95 | P99 |
|----------|-------------|-----|-----|
| Login | < 200ms | < 400ms | < 800ms |
| Get Patients | < 150ms | < 300ms | < 500ms |
| Upload Image | < 2s | < 4s | < 8s |
| AI Analysis | < 10s | < 20s | < 30s |

---

## 🤖 Tests de l'intégration IA

### Mock AI Service

```csharp
public class MockAIService : IAIService
{
    public Task<AIPredictResponse?> PredictAsync(Stream image, string fileName, Dictionary<string, object> features)
    {
        // Retourner une réponse simulée
        return Task.FromResult<AIPredictResponse?>(new AIPredictResponse
        {
            Label = "BENIGN",
            Probability = 0.23,
            PImage = 0.20,
            PTabular = 0.28,
            ModelVersion = "mock-v1",
            DegradedMode = false
        });
    }
}
```

### Tests de timeout

```csharp
[Fact]
public async Task AnalyzeImage_WhenAITimeout_ShouldReturnGracefully()
{
    // Configurer un timeout très court
    _mockAIService.Setup(x => x.PredictAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
        .ThrowsAsync(new TimeoutException());

    var result = await _imagingService.AnalyzeImageAsync(imageId);

    Assert.False(result.Success);
    Assert.Contains("délai", result.Message.ToLower());
}
```

---

## ✅ Critères d'acceptation

| Critère | Seuil |
|---------|-------|
| Couverture tests unitaires | ≥ 80% |
| Couverture tests intégration | ≥ 60% |
| Tests E2E passants | 100% |
| Vulnérabilités critiques OWASP | 0 |
| Temps réponse P95 | < 500ms |
| Taux d'erreur sous charge | < 1% |
