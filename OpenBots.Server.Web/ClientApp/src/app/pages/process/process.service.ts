import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class ProcessService {
  headerData = new HttpHeaders({ 'Content-Type': 'application/json' });
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient) {}

  

  getAllProcess(tpage: any, spage: any) {
    let getprocessUrlbyId = `/processes?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getprocessUrlbyId);
  }

  addProcess(obj) {
    let addassetUrl = `/processes`;
    return this.http.post(`${this.apiUrl}` + addassetUrl, obj);
  }

  uploadProcessFile(process_id, obj) {
    let processUrl = `/processes/${process_id}/upload`;
    return this.http.post(`${this.apiUrl}` + processUrl, obj);
  }

  uploadUpdateProcessFile(obj, process_id) {
    let processUrl = `/processes/${process_id}/update`;
    return this.http.post(`${this.apiUrl}` + processUrl, obj);
  }

  updateProcess(obj, process_id) {
    let updateassetUrl = `/processes/${process_id}`;

    return this.http.put(`${this.apiUrl}` + updateassetUrl, obj);
  }

  downloadProcess(process_id) {
    let exportUrl = `/processes/${process_id}/Export`;
    return this.http.get(`${this.apiUrl}` + exportUrl);
  }
  getBlob(process_id: string): Observable<any> {
    let exportUrl = `/processes/${process_id}/Export`;
    let options = {};
    options = {
      responseType: 'blob',
      observe: 'response',
    };
    return this.http.get<any>(`${this.apiUrl}` + exportUrl, options).pipe(
      catchError((error) => {
        return throwError(error);
      })
    );
  }

  viewFile(data: any): void {
    let blob = new Blob([data], { type: data.type });
    if (window.navigator && window.navigator.msSaveOrOpenBlob) {
      window.navigator.msSaveOrOpenBlob(blob);
    } else {
      let anchor = document.createElement('a');
      anchor.href = window.URL.createObjectURL(blob);
      anchor.target = '_blank';
      anchor.click();
    }
  }

  getAllJobsOrder(tpage: any, spage: any, name) {
    let getprocessUrlbyId = `/processes?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getprocessUrlbyId);
  }

  getProcessId(id) {
    let options = {};
    options = {
      observe: 'response' as 'body', 
      responseType: 'json',
    };
    let getprocessUrlbyId = `/processes/${id}`;
    return this.http.get(`${this.apiUrl}` + getprocessUrlbyId, options);
  }

  deleteProcess(id) {
    let getprocessUrlbyId = `/processes/${id}`;
    return this.http.delete(`${this.apiUrl}` + getprocessUrlbyId);
  }
}
