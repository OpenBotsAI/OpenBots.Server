import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { HelperService } from '../../@core/services/helper.service';
import { catchError } from 'rxjs/operators';
import { Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class FileManagerService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient, private helperService: HelperService) {}

  getAllFiles(tpage: any, spage: any) {
    let filesurl = `/files?driveName=Files&$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + filesurl);
  }

  getAllFilesOrder(tpage: any, spage: any, name) {
    let filesurl = `/files?driveName=Files&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + filesurl);
  }

  Createfolder(obj) {
    let createfile = `/files?driveName=Files`;
    return this.http.post(`${this.apiUrl}` + createfile, obj);
  }

  getFileFloder(parentId) {
    let filesurl = `/files?driveName=Files&$filter=${parentId}`;
    return this.http.get(`${this.apiUrl}` + filesurl);
  }
  DeleteFileFloder(id) {
    let filesurl = `/files/${id}?driveName=Files`;
    return this.http.delete(`${this.apiUrl}` + filesurl);
  }
  getFiledownload(Id): Observable<any> {
    let downloadurl = `/files/${Id}/download?driveName=Files`;
    let options = {};
    options = {
      responseType: 'blob',
      observe: 'response',
    };
    return this.http.get<any>(`${this.apiUrl}` + downloadurl, options).pipe(
      catchError((error) => {
        return throwError(error);
      })
    );
  }
}
