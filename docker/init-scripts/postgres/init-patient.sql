-- Create blood_groups table (lookup)
CREATE TABLE IF NOT EXISTS blood_groups (
    id INTEGER PRIMARY KEY,
    code VARCHAR(10) NOT NULL,
    description VARCHAR(50)
);

-- Insert default blood groups
INSERT INTO blood_groups (id, code, description) VALUES
(1, 'A+', 'A Positif'),
(2, 'A-', 'A Négatif'),
(3, 'B+', 'B Positif'),
(4, 'B-', 'B Négatif'),
(5, 'AB+', 'AB Positif'),
(6, 'AB-', 'AB Négatif'),
(7, 'O+', 'O Positif'),
(8, 'O-', 'O Négatif')
ON CONFLICT (id) DO NOTHING;

-- Create patients table
CREATE TABLE IF NOT EXISTS patients (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    email VARCHAR(255),
    phone VARCHAR(20),
    date_of_birth TIMESTAMP,
    age INTEGER,
    gender VARCHAR(20),
    blood_group_id INTEGER,
    weight_kg DECIMAL,
    height_cm DECIMAL,
    address VARCHAR(500),
    city VARCHAR(100),
    emergency_contact_name VARCHAR(100),
    emergency_contact_phone VARCHAR(20),
    disease_followup TEXT,
    allergies TEXT,
    medical_history TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (blood_group_id) REFERENCES blood_groups(id) ON DELETE SET NULL
);

-- Create indexes for patients
CREATE UNIQUE INDEX IF NOT EXISTS idx_patients_user_id ON patients(user_id);
CREATE INDEX IF NOT EXISTS idx_patients_email ON patients(email);
CREATE INDEX IF NOT EXISTS idx_patients_is_active ON patients(is_active);

-- Create vitals_history table
CREATE TABLE IF NOT EXISTS vitals_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id UUID NOT NULL,
    measured_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    weight_kg DECIMAL,
    height_cm DECIMAL,
    blood_pressure_systolic INTEGER,
    blood_pressure_diastolic INTEGER,
    heart_rate INTEGER,
    temperature DECIMAL,
    notes TEXT,
    recorded_by_doctor_id UUID,
    
    FOREIGN KEY (patient_id) REFERENCES patients(id) ON DELETE CASCADE
);

-- Create index for vitals_history
CREATE INDEX IF NOT EXISTS idx_vitals_history_patient_measured ON vitals_history(patient_id, measured_at);

-- Create EF migrations history table if not exists
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    PRIMARY KEY ("MigrationId")
);

-- Insert dummy migration for Patient service
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES
('20250101000000_InitialCreate', '8.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;