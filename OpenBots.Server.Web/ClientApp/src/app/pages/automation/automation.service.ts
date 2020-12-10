import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HelperService } from '../../@core/services/helper.service';
import { automationsApiUrl } from '../../webApiUrls/automations';

@Injectable({
  providedIn: 'root',
})
export class AutomationService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient, private helperService: HelperService) {}

  getAllProcess(tpage: any, spage: any) {
    let getprocessUrlbyId = `/${automationsApiUrl.automationsView}?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getprocessUrlbyId);
  }

  addProcess(obj) {
    let addassetUrl = `/${automationsApiUrl.automations}`;
    return this.http.post(`${this.apiUrl}` + addassetUrl, obj);
  }

  uploadProcessFile(process_id, obj) {
    let processUrl = `/${automationsApiUrl.automations}/${process_id}/upload`;
    return this.http.post(`${this.apiUrl}` + processUrl, obj);
  }

  uploadUpdateProcessFile(obj, process_id, etag) {
    const headers = this.helperService.getETagHeaders(etag);
    let processUrl = `/${automationsApiUrl.automations}/${process_id}/update`;
    return this.http.post(`${this.apiUrl}` + processUrl, obj, { headers });
  }

  updateProcess(obj, process_id, etag) {
    const headers = this.helperService.getETagHeaders(etag);
    let updateassetUrl = `/${automationsApiUrl.automations}/${process_id}`;
    return this.http.put(`${this.apiUrl}` + updateassetUrl, obj, { headers });
  }

  downloadProcess(process_id) {
    let exportUrl = `/${automationsApiUrl.automations}/${process_id}/Export`;
    return this.http.get(`${this.apiUrl}` + exportUrl);
  }
  getBlob(process_id: string): Observable<any> {
    let exportUrl = `/${automationsApiUrl.automations}/${process_id}/Export`;
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
    let getprocessUrlbyId = `/${automationsApiUrl.automationsView}?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getprocessUrlbyId);
  }

  getProcessId(id) {
    let resoptions = {};
    resoptions = {
      observe: 'response' as 'body',
      responseType: 'json',
    };
    let getprocessUrlbyId = `/${automationsApiUrl.automationsView}/${id}`;
    return this.http.get(`${this.apiUrl}` + getprocessUrlbyId, resoptions);
  }

  deleteProcess(id) {
    let getprocessUrlbyId = `/${automationsApiUrl.automations}/${id}`;
    return this.http.delete(`${this.apiUrl}` + getprocessUrlbyId);
  }
}
