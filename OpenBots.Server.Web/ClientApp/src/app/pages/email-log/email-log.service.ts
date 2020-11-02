import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EmailLogService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient) { }



  getAllEmailforfilter() {
    let getagentUrl = `/emailaccounts/getlookup`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }
  getAllEmail(tpage: any, spage: any) {
    let getagentUrl = `/EmailLogs?$orderby=createdOn desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllEmaillogOrder(tpage: any, spage: any, name) {
    let getagentUrl = `/EmailLogs?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }


  filter_emailaccount(email_name: any, tpage: any, spage: any) {
    let getagentUrl = `/EmailLogs?$filter=${email_name}&$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getEmailbyId(id) {
    let getagentUrlbyId = `/EmailLogs/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }


  getUserId(id) {
    let getagentUrlbyId = `/people/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }







}
