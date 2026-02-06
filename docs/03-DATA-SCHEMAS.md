# 🗄️ Schémas de Données - PinkVision

## 📊 PostgreSQL - Base Relationnelle

### Auth Service Database (`pinkvision_auth`)

```sql
-- Table des utilisateurs
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    phone VARCHAR(20),
    is_active BOOLEAN DEFAULT true,
    email_confirmed BOOLEAN DEFAULT false,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP
);

-- Table des rôles
CREATE TABLE roles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255)
);

-- Insertion des rôles par défaut
INSERT INTO roles (name, description) VALUES 
    ('ADMIN', 'Administrateur système'),
    ('MEDECIN', 'Médecin spécialiste'),
    ('PATIENT', 'Patient');

-- Table de liaison utilisateur-rôle
CREATE TABLE user_roles (
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    role_id INTEGER REFERENCES roles(id) ON DELETE CASCADE,
    assigned_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, role_id)
);

-- Table des refresh tokens
CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    token TEXT NOT NULL UNIQUE,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    revoked_at TIMESTAMP,
    replaced_by_token TEXT,
    revoked_reason VARCHAR(255)
);

-- Index pour optimisation
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_refresh_tokens_user ON refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_token ON refresh_tokens(token);
```

---

### Patient Service Database (`pinkvision_patient`)

```sql
-- Table des groupes sanguins
CREATE TABLE blood_groups (
    id SERIAL PRIMARY KEY,
    code VARCHAR(10) NOT NULL UNIQUE,
    description VARCHAR(50)
);

-- Insertion des groupes sanguins
INSERT INTO blood_groups (code, description) VALUES 
    ('A+', 'A Positif'), ('A-', 'A Négatif'),
    ('B+', 'B Positif'), ('B-', 'B Négatif'),
    ('AB+', 'AB Positif'), ('AB-', 'AB Négatif'),
    ('O+', 'O Positif'), ('O-', 'O Négatif');

-- Table des patients
CREATE TABLE patients (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL UNIQUE,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    email VARCHAR(255),
    phone VARCHAR(20),
    date_of_birth DATE,
    age SMALLINT,
    gender VARCHAR(20),
    blood_group_id INTEGER REFERENCES blood_groups(id),
    weight_kg DECIMAL(5,2),
    height_cm DECIMAL(5,2),
    address VARCHAR(500),
    city VARCHAR(100),
    emergency_contact_name VARCHAR(100),
    emergency_contact_phone VARCHAR(20),
    disease_followup TEXT,
    allergies TEXT,
    medical_history TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Historique des mesures vitales
CREATE TABLE vitals_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id UUID REFERENCES patients(id) ON DELETE CASCADE,
    measured_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    weight_kg DECIMAL(5,2),
    height_cm DECIMAL(5,2),
    blood_pressure_systolic INTEGER,
    blood_pressure_diastolic INTEGER,
    heart_rate INTEGER,
    temperature DECIMAL(4,2),
    notes TEXT,
    recorded_by_doctor_id UUID
);

-- Index
CREATE INDEX idx_patients_user ON patients(user_id);
CREATE INDEX idx_patients_email ON patients(email);
CREATE INDEX idx_vitals_patient ON vitals_history(patient_id, measured_at);
```

---

### Imaging Service Database (`pinkvision_imaging`)

```sql
-- Table des images mammographie
CREATE TABLE mammography_images (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id UUID NOT NULL,
    uploaded_by_user_id UUID,
    file_name VARCHAR(255) NOT NULL,
    original_file_name VARCHAR(255) NOT NULL,
    file_path VARCHAR(500),
    file_size_bytes BIGINT,
    content_type VARCHAR(100),
    image_type VARCHAR(50),        -- LEFT, RIGHT
    view_position VARCHAR(50),     -- CC, MLO
    status VARCHAR(50) DEFAULT 'PENDING',  -- PENDING, ANALYZING, ANALYZED, FAILED
    notes TEXT,
    uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    analyzed_at TIMESTAMP
);

-- Table des résultats de diagnostic
CREATE TABLE diagnosis_results (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    image_id UUID REFERENCES mammography_images(id) ON DELETE CASCADE UNIQUE,
    patient_id UUID NOT NULL,
    label VARCHAR(50) NOT NULL,     -- BENIGN, MALIGNANT
    probability DOUBLE PRECISION NOT NULL,
    p_image DOUBLE PRECISION,
    p_tabular DOUBLE PRECISION,
    model_version VARCHAR(50),
    degraded_mode BOOLEAN DEFAULT false,
    heatmap_path VARCHAR(500),
    doctor_validated BOOLEAN DEFAULT false,
    validated_by_doctor_id UUID,
    doctor_notes TEXT,
    final_diagnosis VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    validated_at TIMESTAMP
);

-- Index
CREATE INDEX idx_images_patient ON mammography_images(patient_id);
CREATE INDEX idx_images_status ON mammography_images(status);
CREATE INDEX idx_diagnosis_patient ON diagnosis_results(patient_id);
CREATE INDEX idx_diagnosis_validated ON diagnosis_results(doctor_validated);
```

---

### Appointment Service Database (`pinkvision_appointment`)

```sql
-- Table des rendez-vous
CREATE TABLE appointments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id UUID NOT NULL,
    doctor_id UUID NOT NULL,
    scheduled_at TIMESTAMP NOT NULL,
    duration_minutes INTEGER DEFAULT 30,
    status VARCHAR(50) DEFAULT 'PENDING',  -- PENDING, CONFIRMED, CANCELLED, COMPLETED
    reason TEXT,
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    cancelled_at TIMESTAMP,
    cancellation_reason TEXT
);

-- Index
CREATE INDEX idx_appointments_patient ON appointments(patient_id, scheduled_at);
CREATE INDEX idx_appointments_doctor ON appointments(doctor_id, scheduled_at);
CREATE INDEX idx_appointments_status ON appointments(status);
```

---

### Medical Record Service Database (`pinkvision_medical`)

```sql
-- Table des dossiers médicaux
CREATE TABLE medical_records (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id UUID NOT NULL,
    created_by_doctor_id UUID,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    record_date DATE NOT NULL,
    record_type VARCHAR(50),  -- CONSULTATION, DIAGNOSIS, FOLLOW_UP
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Table des diagnostics
CREATE TABLE diagnoses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    medical_record_id UUID REFERENCES medical_records(id) ON DELETE CASCADE,
    code VARCHAR(20),          -- Code ICD-10
    label VARCHAR(255) NOT NULL,
    severity VARCHAR(50),
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Index
CREATE INDEX idx_records_patient ON medical_records(patient_id);
CREATE INDEX idx_diagnoses_record ON diagnoses(medical_record_id);
```

---

## 📄 MongoDB - Base Documents

### Notification Service (`pinkvision_notifications`)

```javascript
// Collection: notifications
{
  "_id": ObjectId("..."),
  "userId": "uuid-string",
  "type": "APPOINTMENT",  // APPOINTMENT, DIAGNOSIS, SYSTEM, REMINDER
  "title": "Nouveau rendez-vous confirmé",
  "content": "Votre rendez-vous du 10/02/2024 a été confirmé",
  "isRead": false,
  "priority": "NORMAL",  // LOW, NORMAL, HIGH, URGENT
  "metadata": {
    "appointmentId": "uuid",
    "patientId": "uuid",
    "doctorId": "uuid"
  },
  "createdAt": ISODate("2024-02-05T22:00:00Z"),
  "readAt": null,
  "expiresAt": ISODate("2024-03-05T22:00:00Z")
}

// Collection: notification_preferences
{
  "_id": ObjectId("..."),
  "userId": "uuid-string",
  "emailEnabled": true,
  "inAppEnabled": true,
  "smsEnabled": false,
  "preferences": {
    "appointments": { "email": true, "inApp": true },
    "diagnosis": { "email": true, "inApp": true },
    "reminders": { "email": false, "inApp": true }
  },
  "updatedAt": ISODate("2024-02-05T22:00:00Z")
}
```

### Imaging Logs (`pinkvision_imaging_logs`)

```javascript
// Collection: analysis_logs
{
  "_id": ObjectId("..."),
  "imageId": "uuid-string",
  "patientId": "uuid-string",
  "action": "ANALYSIS_REQUESTED",  // UPLOAD, ANALYSIS_REQUESTED, ANALYSIS_COMPLETED, VALIDATION
  "userId": "uuid-string",
  "userRole": "MEDECIN",
  "details": {
    "aiServiceUrl": "http://ia-mammographic:8001",
    "requestDuration": 1250,
    "modelVersion": "mammographic-app-v1"
  },
  "result": {
    "success": true,
    "label": "BENIGN",
    "probability": 0.23
  },
  "ipAddress": "192.168.1.100",
  "userAgent": "Mozilla/5.0...",
  "timestamp": ISODate("2024-02-05T22:00:00Z")
}
```

### Chatbot Conversations (`pinkvision_chatbot`)

```javascript
// Collection: conversations
{
  "_id": ObjectId("..."),
  "sessionId": "session-uuid",
  "userId": "user-uuid",
  "messages": [
    {
      "role": "user",
      "content": "Quels sont les symptômes du cancer du sein ?",
      "timestamp": ISODate("2024-02-05T22:00:00Z")
    },
    {
      "role": "assistant",
      "content": "Les symptômes courants incluent...",
      "sources": ["source1", "source2"],
      "timestamp": ISODate("2024-02-05T22:00:05Z")
    }
  ],
  "startedAt": ISODate("2024-02-05T22:00:00Z"),
  "lastActivityAt": ISODate("2024-02-05T22:00:05Z"),
  "messageCount": 2
}
```

---

## 🔗 Relations entre Services

```
┌─────────────────┐       ┌─────────────────┐
│   Auth Service  │◄──────│  User (userId)  │
│   (users)       │       └─────────────────┘
└────────┬────────┘                │
         │                         │
         ▼                         ▼
┌─────────────────┐       ┌─────────────────┐
│ Patient Service │       │ Doctor Service  │
│   (patients)    │       │   (doctors)     │
└────────┬────────┘       └────────┬────────┘
         │                         │
         ▼                         ▼
┌─────────────────────────────────────────────┐
│              Imaging Service                 │
│  (mammography_images, diagnosis_results)    │
└─────────────────────────────────────────────┘
         │                         │
         ▼                         ▼
┌─────────────────┐       ┌─────────────────┐
│Medical Records  │       │  Appointments   │
└─────────────────┘       └─────────────────┘
```
