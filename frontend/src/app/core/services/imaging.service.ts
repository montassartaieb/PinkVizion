import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    MammographyImage,
    DiagnosisResult,
    AIServiceStatus,
    ApiResponse
} from '../models';

export interface UploadImageRequest {
    file: File;
    patientId: string;
    imageType?: string;
    viewPosition?: string;
    notes?: string;
}

export interface PatientFeatures {
    age?: number;
    menopause?: string;
    tumorSize?: number;
    invNodes?: string;
    degMalig?: number;
    breast?: string;
}

export interface ValidateDiagnosisRequest {
    diagnosisId: string;
    finalDiagnosis: string;
    doctorNotes?: string;
}

@Injectable({
    providedIn: 'root'
})
export class ImagingService {
    private readonly baseUrl = `${environment.apiUrl}/imaging`;

    constructor(private http: HttpClient) { }

    getAIStatus(): Observable<ApiResponse<AIServiceStatus>> {
        return this.http.get<ApiResponse<AIServiceStatus>>(`${this.baseUrl}/ai/status`);
    }

    uploadImage(request: UploadImageRequest): Observable<ApiResponse<MammographyImage>> {
        const formData = new FormData();
        formData.append('file', request.file);
        formData.append('patientId', request.patientId);
        if (request.imageType) {
            formData.append('imageType', request.imageType);
        }
        if (request.viewPosition) {
            formData.append('viewPosition', request.viewPosition);
        }
        if (request.notes) {
            formData.append('notes', request.notes);
        }

        return this.http.post<ApiResponse<MammographyImage>>(`${this.baseUrl}/upload`, formData);
    }

    analyzeImage(imageId: string, features?: PatientFeatures): Observable<ApiResponse<DiagnosisResult>> {
        return this.http.post<ApiResponse<DiagnosisResult>>(`${this.baseUrl}/${imageId}/analyze`, features || {});
    }

    getImageById(id: string): Observable<ApiResponse<MammographyImage>> {
        return this.http.get<ApiResponse<MammographyImage>>(`${this.baseUrl}/${id}`);
    }

    getPatientImages(patientId: string): Observable<ApiResponse<MammographyImage[]>> {
        return this.http.get<ApiResponse<MammographyImage[]>>(`${this.baseUrl}/patient/${patientId}`);
    }

    validateDiagnosis(request: ValidateDiagnosisRequest): Observable<ApiResponse<DiagnosisResult>> {
        return this.http.post<ApiResponse<DiagnosisResult>>(`${this.baseUrl}/diagnosis/validate`, request);
    }

    getPendingValidations(): Observable<ApiResponse<DiagnosisResult[]>> {
        return this.http.get<ApiResponse<DiagnosisResult[]>>(`${this.baseUrl}/diagnosis/pending`);
    }

    deleteImage(id: string): Observable<ApiResponse<boolean>> {
        return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${id}`);
    }
}
