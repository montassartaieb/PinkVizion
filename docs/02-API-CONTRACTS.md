# 📋 Contrats API - PinkVision

## 🔐 Auth Service (Port 5001)

### POST /api/auth/register
**Description:** Inscription d'un nouvel utilisateur

**Rôles requis:** Aucun (public)

**Request Body:**
```json
{
  "email": "patient@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "firstName": "Jean",
  "lastName": "Dupont",
  "phone": "+33612345678",
  "userType": "PATIENT"  // ou "MEDECIN"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Inscription réussie",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-refresh-token",
  "expiresAt": "2024-02-05T23:00:00Z",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "patient@example.com",
    "firstName": "Jean",
    "lastName": "Dupont",
    "roles": ["PATIENT"],
    "createdAt": "2024-02-05T22:00:00Z"
  }
}
```

**Codes d'erreur:**
- `400` - Email déjà utilisé ou données invalides
- `500` - Erreur serveur

---

### POST /api/auth/login
**Description:** Connexion utilisateur

**Request Body:**
```json
{
  "email": "patient@example.com",
  "password": "SecurePass123!"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Connexion réussie",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-refresh-token",
  "expiresAt": "2024-02-05T23:00:00Z",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "patient@example.com",
    "firstName": "Jean",
    "lastName": "Dupont",
    "roles": ["PATIENT"],
    "lastLoginAt": "2024-02-05T22:00:00Z"
  }
}
```

**Codes d'erreur:**
- `401` - Email ou mot de passe incorrect
- `401` - Compte désactivé

---

### POST /api/auth/refresh
**Description:** Rafraîchir le token JWT

**Request Body:**
```json
{
  "refreshToken": "base64-encoded-refresh-token"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "accessToken": "new-access-token",
  "refreshToken": "new-refresh-token",
  "expiresAt": "2024-02-05T23:00:00Z"
}
```

---

### POST /api/auth/logout
**Description:** Déconnexion (révocation du refresh token)

**Headers:** `Authorization: Bearer {accessToken}`

**Request Body:**
```json
{
  "refreshToken": "base64-encoded-refresh-token"
}
```

---

## 👤 Users Service (Port 5001)

### GET /api/users/me
**Description:** Récupérer le profil de l'utilisateur connecté

**Headers:** `Authorization: Bearer {accessToken}`

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "patient@example.com",
    "firstName": "Jean",
    "lastName": "Dupont",
    "phone": "+33612345678",
    "isActive": true,
    "roles": ["PATIENT"],
    "createdAt": "2024-02-05T22:00:00Z"
  }
}
```

---

## 🏥 Patient Service (Port 5002)

### GET /api/patients/me
**Description:** Récupérer le profil patient de l'utilisateur connecté

**Rôles requis:** PATIENT

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "patient-uuid",
    "userId": "user-uuid",
    "firstName": "Jean",
    "lastName": "Dupont",
    "email": "patient@example.com",
    "phone": "+33612345678",
    "dateOfBirth": "1985-03-15",
    "age": 39,
    "gender": "M",
    "bloodGroup": {
      "id": 1,
      "code": "A+",
      "description": "A Positif"
    },
    "weightKg": 75.5,
    "heightCm": 180,
    "bmi": 23.3,
    "diseaseFollowup": "Suivi annuel",
    "allergies": "Aucune",
    "createdAt": "2024-02-05T22:00:00Z"
  }
}
```

### GET /api/patients
**Description:** Liste des patients (paginée)

**Rôles requis:** MEDECIN, ADMIN

**Query Params:**
- `page` (int, default: 1)
- `pageSize` (int, default: 20)
- `search` (string, optional)

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 150,
    "page": 1,
    "pageSize": 20,
    "totalPages": 8
  }
}
```

### GET /api/patients/blood-groups
**Description:** Liste des groupes sanguins

**Rôles requis:** Aucun (public)

**Response:**
```json
{
  "success": true,
  "data": [
    { "id": 1, "code": "A+", "description": "A Positif" },
    { "id": 2, "code": "A-", "description": "A Négatif" },
    ...
  ]
}
```

---

## 🖼️ Imaging Service (Port 5004)

### GET /api/imaging/ai/status
**Description:** Vérifier le statut du service IA

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "available": true,
    "status": "ok",
    "imageModelLoaded": true,
    "tabularModelLoaded": true,
    "imageModelError": null,
    "tabularModelError": null
  }
}
```

### POST /api/imaging/upload
**Description:** Uploader une image de mammographie

**Rôles requis:** PATIENT, MEDECIN, ADMIN

**Content-Type:** `multipart/form-data`

**Form Data:**
- `file` (File, required) - Image JPG/PNG/DICOM
- `patientId` (UUID, required)
- `imageType` (string, optional) - LEFT, RIGHT
- `viewPosition` (string, optional) - CC, MLO
- `notes` (string, optional)

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Image uploadée avec succès",
  "data": {
    "id": "image-uuid",
    "patientId": "patient-uuid",
    "fileName": "generated-filename.jpg",
    "originalFileName": "mammography.jpg",
    "status": "PENDING",
    "uploadedAt": "2024-02-05T22:00:00Z"
  }
}
```

### POST /api/imaging/{id}/analyze
**Description:** Analyser une image avec l'IA

**Rôles requis:** MEDECIN, ADMIN

**Request Body (optional):**
```json
{
  "age": 55,
  "menopause": "ge40",
  "tumorSize": 25,
  "invNodes": "0-2",
  "degMalig": 2,
  "breast": "left"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Analyse terminée avec succès",
  "data": {
    "id": "diagnosis-uuid",
    "imageId": "image-uuid",
    "patientId": "patient-uuid",
    "label": "MALIGNANT",
    "probability": 0.78,
    "probabilityPercent": "78.0%",
    "pImage": 0.82,
    "pTabular": 0.71,
    "modelVersion": "mammographic-app-v1",
    "degradedMode": false,
    "riskLevel": "HIGH",
    "doctorValidated": false,
    "createdAt": "2024-02-05T22:05:00Z"
  }
}
```

### POST /api/imaging/diagnosis/validate
**Description:** Valider un diagnostic (médecin)

**Rôles requis:** MEDECIN

**Request Body:**
```json
{
  "diagnosisId": "diagnosis-uuid",
  "finalDiagnosis": "MALIGNANT",
  "doctorNotes": "Confirmation du diagnostic, recommandation de biopsie"
}
```

### GET /api/imaging/diagnosis/pending
**Description:** Diagnostics en attente de validation

**Rôles requis:** MEDECIN, ADMIN

---

## 🤖 AI Service (FastAPI - Port 8001)

### GET /health
**Description:** Vérifier la santé du service IA

**Response:**
```json
{
  "status": "ok",
  "image_loaded": true,
  "tabular_loaded": true,
  "image_model_error": null,
  "tabular_model_error": null,
  "models_available": true
}
```

### POST /v1/predict
**Description:** Prédiction IA sur une mammographie

**Content-Type:** `multipart/form-data`

**Form Data:**
- `file` (File) - Image mammographie
- `features_json` (string) - JSON des features tabulaires

**Response:**
```json
{
  "label": "MALIGNANT",
  "probability": 0.78,
  "p_image": 0.82,
  "p_tabular": 0.71,
  "model_version": "mammographic-app-v1",
  "degraded_mode": false
}
```

---

## 💬 Chatbot Service (FastAPI - Port 8002)

### POST /chat
**Description:** Poser une question au chatbot médical

**Request Body:**
```json
{
  "message": "Quels sont les symptômes du cancer du sein ?"
}
```

**Response:**
```json
{
  "answer": "Les symptômes courants du cancer du sein incluent...",
  "sources": [
    "Source 1: Information médicale...",
    "Source 2: Recommandations..."
  ]
}
```

---

## 🔒 Codes HTTP Standards

| Code | Signification |
|------|---------------|
| 200 | Succès |
| 201 | Créé avec succès |
| 400 | Requête invalide |
| 401 | Non authentifié |
| 403 | Accès refusé (droits insuffisants) |
| 404 | Ressource non trouvée |
| 500 | Erreur serveur |
| 503 | Service indisponible (ex: IA en panne) |
