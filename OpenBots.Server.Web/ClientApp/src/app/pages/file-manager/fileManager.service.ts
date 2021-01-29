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
    //   ?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}
    let filesurl = `/files?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + filesurl);
  }

  getAllFilesOrder(tpage: any, spage: any, name) {
    //   ?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}
    let filesurl = `/files?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + filesurl);
  }

  // getAllEmail(tpage: any, spage: any) {
  //   let getagentUrl = `/EmailAccounts?$orderby=createdOn desc&$top=${tpage}&$skip=${spage}`;
  //   return this.http.get(`${this.apiUrl}` + getagentUrl);
  // }

  // getAllEmailOrder(tpage: any, spage: any, name) {
  //   let getagentUrl = `/EmailAccounts?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
  //   return this.http.get(`${this.apiUrl}` + getagentUrl);
  // }

  getFileFloder(parentId) {
    let filesurl = `/files/${parentId}`;
    return this.http.get(`${this.apiUrl}` + filesurl);
  }

  getFiledownload(Id): Observable<any> {
    let downloadurl = `/files/${Id}/download`;
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
