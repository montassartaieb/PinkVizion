import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    Patient,
    BloodGroup,
    VitalsHistory,
    ApiResponse,
    PagedResult
} from '../models';

@Injectable({
    providedIn: 'root'
})
export class PatientService {
    private readonly baseUrl = `${environment.apiUrl}/patients`;

    constructor(private http: HttpClient) { }

    getMyProfile(): Observable<ApiResponse<Patient>> {
        return this.http.get<ApiResponse<Patient>>(`${this.baseUrl}/me`);
    }

    updateMyProfile(data: Partial<Patient>): Observable<ApiResponse<Patient>> {
        return this.http.put<ApiResponse<Patient>>(`${this.baseUrl}/me`, data);
    }

    getPatientById(id: string): Observable<ApiResponse<Patient>> {
        return this.http.get<ApiResponse<Patient>>(`${this.baseUrl}/${id}`);
    }

    getAllPatients(page = 1, pageSize = 20, search?: string): Observable<ApiResponse<PagedResult<Patient>>> {
        let url = `${this.baseUrl}?page=${page}&pageSize=${pageSize}`;
        if (search) {
            url += `&search=${encodeURIComponent(search)}`;
        }
        return this.http.get<ApiResponse<PagedResult<Patient>>>(url);
    }

    createPatient(data: Partial<Patient>): Observable<ApiResponse<Patient>> {
        return this.http.post<ApiResponse<Patient>>(this.baseUrl, data);
    }

    updatePatient(id: string, data: Partial<Patient>): Observable<ApiResponse<Patient>> {
        return this.http.put<ApiResponse<Patient>>(`${this.baseUrl}/${id}`, data);
    }

    deletePatient(id: string): Observable<ApiResponse<boolean>> {
        return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${id}`);
    }

    getBloodGroups(): Observable<ApiResponse<BloodGroup[]>> {
        return this.http.get<ApiResponse<BloodGroup[]>>(`${this.baseUrl}/blood-groups`);
    }

    recordVitals(patientId: string, vitals: Partial<VitalsHistory>): Observable<ApiResponse<VitalsHistory>> {
        return this.http.post<ApiResponse<VitalsHistory>>(`${this.baseUrl}/${patientId}/vitals`, vitals);
    }

    getVitalsHistory(patientId: string, limit = 10): Observable<ApiResponse<VitalsHistory[]>> {
        return this.http.get<ApiResponse<VitalsHistory[]>>(`${this.baseUrl}/${patientId}/vitals?limit=${limit}`);
    }
}
