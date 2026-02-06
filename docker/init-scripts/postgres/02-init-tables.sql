-- PinkVision PostgreSQL Table Initialization Script
-- This script creates all tables for all services
-- Runs after databases are created in 01-init-databases.sql

-- ===========================================
-- PATIENT SERVICE TABLES (pinkvision_patient)
-- ===========================================
\c pinkvision_patient;

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

-- ===========================================
-- DOCTOR SERVICE TABLES (pinkvision_doctor)
-- ===========================================
\c pinkvision_doctor;

-- Create doctors table
CREATE TABLE IF NOT EXISTS doctors (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    email VARCHAR(255),
    phone VARCHAR(20),
    specialization VARCHAR(200),
    license_number VARCHAR(50),
    hospital_name VARCHAR(200),
    years_of_experience INTEGER,
    consultation_fee DECIMAL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for doctors
CREATE UNIQUE INDEX IF NOT EXISTS idx_doctors_user_id ON doctors(user_id);
CREATE INDEX IF NOT EXISTS idx_doctors_email ON doctors(email);
CREATE UNIQUE INDEX IF NOT EXISTS idx_doctors_license_number ON doctors(license_number);

-- Create EF migrations history table if not exists
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    PRIMARY KEY ("MigrationId")
);

-- Insert dummy migration for Doctor service
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES
('20250101000000_InitialCreate', '8.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;

-- ===========================================
-- APPOINTMENT SERVICE TABLES (pinkvision_appointment)
-- ===========================================
\c pinkvision_appointment;

-- Create appointments table
CREATE TABLE IF NOT EXISTS appointments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id UUID NOT NULL,
    doctor_id UUID NOT NULL,
    appointment_date TIMESTAMP NOT NULL,
    duration_minutes INTEGER DEFAULT 30,
    status VARCHAR(50) DEFAULT 'SCHEDULED',
    reason TEXT,
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for appointments
CREATE INDEX IF NOT EXISTS idx_appointments_patient_id ON appointments(patient_id);
CREATE INDEX IF NOT EXISTS idx_appointments_doctor_id ON appointments(doctor_id);
CREATE INDEX IF NOT EXISTS idx_appointments_date_status ON appointments(appointment_date, status);

-- Create EF migrations history table if not exists
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    PRIMARY KEY ("MigrationId")
);

-- Insert dummy migration for Appointment service
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES
('20250101000000_InitialCreate', '8.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;

-- ===========================================
-- MEDICAL RECORD SERVICE TABLES (pinkvision_medical)
-- ===========================================
\c pinkvision_medical;

-- Create medical_records table
CREATE TABLE IF NOT EXISTS medical_records (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id UUID NOT NULL,
    doctor_id UUID,
    record_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    diagnosis TEXT,
    prescription TEXT,
    notes TEXT,
    attachments JSONB,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for medical_records
CREATE INDEX IF NOT EXISTS idx_medical_records_patient_id ON medical_records(patient_id);
CREATE INDEX IF NOT EXISTS idx_medical_records_doctor_id ON medical_records(doctor_id);

-- Create EF migrations history table if not exists
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    PRIMARY KEY ("MigrationId")
);

-- Insert dummy migration for Medical Record service
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES
('20250101000000_InitialCreate', '8.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;

-- ===========================================
-- IMAGING SERVICE TABLES (pinkvision_imaging)
-- ===========================================
\c pinkvision_imaging;

-- Create mammography_images table
CREATE TABLE IF NOT EXISTS mammography_images (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id UUID NOT NULL,
    doctor_id UUID,
    image_url VARCHAR(500) NOT NULL,
    file_name VARCHAR(255),
    file_size BIGINT,
    mime_type VARCHAR(100),
    analysis_result JSONB,
    confidence_score DECIMAL(5,4),
    status VARCHAR(50) DEFAULT 'PENDING',
    uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    analyzed_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for mammography_images
CREATE INDEX IF NOT EXISTS idx_mammography_images_patient_id ON mammography_images(patient_id);
CREATE INDEX IF NOT EXISTS idx_mammography_images_doctor_id ON mammography_images(doctor_id);
CREATE INDEX IF NOT EXISTS idx_mammography_images_status ON mammography_images(status);

-- Create EF migrations history table if not exists
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    PRIMARY KEY ("MigrationId")
);

-- Insert dummy migration for Imaging service
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES
('20250101000000_InitialCreate', '8.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;

-- ===========================================
-- DASHBOARD SERVICE TABLES (pinkvision_dashboard)
-- ===========================================
\c pinkvision_dashboard;

-- Create dashboard_stats table
CREATE TABLE IF NOT EXISTS dashboard_stats (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    stat_date DATE NOT NULL,
    total_patients INTEGER DEFAULT 0,
    total_doctors INTEGER DEFAULT 0,
    total_appointments INTEGER DEFAULT 0,
    pending_appointments INTEGER DEFAULT 0,
    completed_appointments INTEGER DEFAULT 0,
    total_images INTEGER DEFAULT 0,
    images_analyzed INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create unique index on stat_date
CREATE UNIQUE INDEX IF NOT EXISTS idx_dashboard_stats_date ON dashboard_stats(stat_date);

-- Create EF migrations history table if not exists
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    PRIMARY KEY ("MigrationId")
);

-- Insert dummy migration for Dashboard service
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES
('20250101000000_InitialCreate', '8.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;

-- ===========================================
-- AUTH SERVICE already has tables, but we ensure migrations table exists
-- ===========================================
\c pinkvision_auth;

-- Ensure EF migrations history table exists (should already exist)
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    PRIMARY KEY ("MigrationId")
);

-- Insert dummy migration if not exists
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES
('20250101000000_InitialCreate', '8.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;

-- Log completion
\echo 'All PinkVision tables created successfully!';