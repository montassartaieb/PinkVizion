-- Script d'initialisation PostgreSQL
-- Création des bases de données pour chaque microservice

-- Auth Service Database
CREATE DATABASE pinkvision_auth;

-- Patient Service Database  
CREATE DATABASE pinkvision_patient;

-- Doctor Service Database
CREATE DATABASE pinkvision_doctor;

-- Imaging Service Database
CREATE DATABASE pinkvision_imaging;

-- Medical Record Service Database
CREATE DATABASE pinkvision_medical;

-- Appointment Service Database
CREATE DATABASE pinkvision_appointment;

-- Dashboard Service Database
CREATE DATABASE pinkvision_dashboard;

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE pinkvision_auth TO pinkvision;
GRANT ALL PRIVILEGES ON DATABASE pinkvision_patient TO pinkvision;
GRANT ALL PRIVILEGES ON DATABASE pinkvision_doctor TO pinkvision;
GRANT ALL PRIVILEGES ON DATABASE pinkvision_imaging TO pinkvision;
GRANT ALL PRIVILEGES ON DATABASE pinkvision_medical TO pinkvision;
GRANT ALL PRIVILEGES ON DATABASE pinkvision_appointment TO pinkvision;
GRANT ALL PRIVILEGES ON DATABASE pinkvision_dashboard TO pinkvision;
