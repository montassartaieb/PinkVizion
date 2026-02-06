// Auth models
export interface LoginRequest {
    email: string;
    password: string;
}

export interface RegisterRequest {
    email: string;
    password: string;
    confirmPassword: string;
    firstName: string;
    lastName: string;
    phone?: string;
    userType: 'PATIENT' | 'MEDECIN';
}

export interface AuthResponse {
    success: boolean;
    message: string;
    accessToken: string;
    refreshToken: string;
    expiresAt: Date;
    user: User;
}

export interface User {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    phone?: string;
    isActive: boolean;
    roles: string[];
    createdAt: Date;
    lastLoginAt?: Date;
}

// Patient models
export interface Patient {
    id: string;
    userId: string;
    firstName: string;
    lastName: string;
    email: string;
    phone?: string;
    dateOfBirth?: Date;
    age?: number;
    gender?: string;
    bloodGroup?: BloodGroup;
    weightKg?: number;
    heightCm?: number;
    bmi?: number;
    address?: string;
    city?: string;
    emergencyContactName?: string;
    emergencyContactPhone?: string;
    diseaseFollowup?: string;
    allergies?: string;
    medicalHistory?: string;
    isActive: boolean;
    createdAt: Date;
    updatedAt: Date;
}

export interface BloodGroup {
    id: number;
    code: string;
    description: string;
}

export interface VitalsHistory {
    id: string;
    patientId: string;
    measuredAt: Date;
    weightKg?: number;
    heightCm?: number;
    bloodPressureSystolic?: number;
    bloodPressureDiastolic?: number;
    heartRate?: number;
    temperature?: number;
    notes?: string;
}

// Imaging models
export interface MammographyImage {
    id: string;
    patientId: string;
    uploadedByUserId?: string;
    fileName: string;
    originalFileName: string;
    fileSizeBytes: number;
    contentType?: string;
    imageType?: string;
    viewPosition?: string;
    status: 'PENDING' | 'ANALYZING' | 'ANALYZED' | 'FAILED';
    notes?: string;
    uploadedAt: Date;
    analyzedAt?: Date;
    diagnosisResult?: DiagnosisResult;
}

export interface DiagnosisResult {
    id: string;
    imageId: string;
    patientId: string;
    label: 'BENIGN' | 'MALIGNANT';
    probability: number;
    probabilityPercent: string;
    pImage?: number;
    pTabular?: number;
    modelVersion?: string;
    degradedMode: boolean;
    heatmapPath?: string;
    riskLevel: 'VERY_LOW' | 'LOW' | 'MODERATE' | 'HIGH';
    doctorValidated: boolean;
    validatedByDoctorId?: string;
    doctorNotes?: string;
    finalDiagnosis?: string;
    createdAt: Date;
    validatedAt?: Date;
}

export interface AIServiceStatus {
    available: boolean;
    status: string;
    imageModelLoaded: boolean;
    tabularModelLoaded: boolean;
    imageModelError?: string;
    tabularModelError?: string;
}

// API Response wrapper
export interface ApiResponse<T> {
    success: boolean;
    message: string;
    data?: T;
    errors?: string[];
}

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}
