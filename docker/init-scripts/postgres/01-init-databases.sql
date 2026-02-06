-- PinkVision PostgreSQL Initialization Script
-- This script runs automatically when the PostgreSQL container starts for the first time

-- Create all service databases
CREATE DATABASE pinkvision_auth OWNER pinkvision;
CREATE DATABASE pinkvision_patient OWNER pinkvision;
CREATE DATABASE pinkvision_doctor OWNER pinkvision;
CREATE DATABASE pinkvision_imaging OWNER pinkvision;
CREATE DATABASE pinkvision_medical OWNER pinkvision;
CREATE DATABASE pinkvision_appointment OWNER pinkvision;
CREATE DATABASE pinkvision_dashboard OWNER pinkvision;

-- Grant all privileges
GRANT ALL PRIVILEGES ON DATABASE pinkvision_auth TO pinkvision;
GRANT ALL PRIVILEGES ON DATABASE pinkvision_patient TO pinkvision;
GRANT ALL PRIVILEGES ON DATABASE pinkvision_doctor TO pinkvision;
GRANT ALL PRIVILEGES ON DATABASE pinkvision_imaging TO pinkvision;
GRANT ALL PRIVILEGES ON DATABASE pinkvision_medical TO pinkvision;
GRANT ALL PRIVILEGES ON DATABASE pinkvision_appointment TO pinkvision;
GRANT ALL PRIVILEGES ON DATABASE pinkvision_dashboard TO pinkvision;

-- Log completion
\echo 'All PinkVision databases created successfully!'
