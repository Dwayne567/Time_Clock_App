import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = 'api';

  constructor(private http: HttpClient) { }

  login(credentials: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/Account/Login`, credentials);
  }

  register(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/Account/Register`, data, { responseType: 'text' });
  }

  logout(): Observable<any> {
    return this.http.post(`${this.baseUrl}/Account/Logout`, {});
  }

  getDashboard(weekSelect?: string, userId?: string): Observable<any> {
    let params = new HttpParams();
    if (weekSelect) {
      params = params.set('WeekSelect', weekSelect);
    }
    if (userId) {
      params = params.set('userId', userId);
    }
    return this.http.get(`${this.baseUrl}/Dashboard/Index`, { params });
  }

  clockInOut(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/Dashboard/ClockInOut`, data);
  }

  addTaskEntry(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/Dashboard/AddTaskEntry`, data, { responseType: 'text' });
  }

  deleteTaskEntry(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Dashboard/DeleteTaskEntry/${id}`, { responseType: 'text' });
  }

  addLeave(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/Dashboard/AddLeave`, data, { responseType: 'text' });
  }

  deleteLeave(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Dashboard/DeleteLeave/${id}`, { responseType: 'text' });
  }

  deleteDay(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Dashboard/DeleteDay/${id}`, { responseType: 'text' });
  }

  addJob(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/Dashboard/AddJob`, data, { responseType: 'text' });
  }

  getJobs(searchTerm: string = ''): Observable<any> {
    let params = new HttpParams();
    if (searchTerm) params = params.set('searchTerm', searchTerm);
    return this.http.get(`${this.baseUrl}/Jobs`, { params });
  }

  deleteJob(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Jobs/${id}`, { responseType: 'text' });
  }

  getTasks(): Observable<any> {
    return this.http.get(`${this.baseUrl}/Tasks`);
  }

  deleteTask(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Tasks/${id}`, { responseType: 'text' });
  }

  createTask(taskDescription: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/Tasks`, { taskDescription }, { responseType: 'text' });
  }

  exportTimeSheet(group: string, fromDate?: string, toDate?: string): Observable<Blob> {
    let params = new HttpParams().set('group', group);
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    return this.http.get(`${this.baseUrl}/Dashboard/ExportTimeSheet`, { params, responseType: 'blob' });
  }
}
