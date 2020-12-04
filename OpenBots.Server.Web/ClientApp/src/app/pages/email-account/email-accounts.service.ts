import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { HelperService } from '../../@core/services/helper.service';

@Injectable({
  providedIn: 'root'
})
export class EmailAccountsService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient, private helperService: HelperService) { }

  getAllEmail(tpage: any, spage: any) {
    let getagentUrl = `/EmailAccounts?$orderby=createdOn desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllEmailOrder(tpage: any, spage: any, name) {
    let getagentUrl = `/EmailAccounts?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getEmailbyId(id) {
    let resoptions = {};
    resoptions = {
      observe: 'response' as 'body',
      responseType: 'json',
    };
    let getagentUrlbyId = `/EmailAccounts/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId, resoptions);
  }

  delAssetbyID(id) {
    let getagentUrlbyId = `/EmailAccounts/${id}`;
    return this.http.delete(`${this.apiUrl}` + getagentUrlbyId);
  }


  editEmail(id, obj, etag) {
    const headers = this.helperService.getETagHeaders(etag)
    let editassetUrl = `/EmailAccounts/${id}`;
    return this.http.put(`${this.apiUrl}` + editassetUrl, obj, { headers });
  }

  SendEmail(accountName, obj) {
    let testEmail = `/Emails/send`;
    return this.http.post(`${this.apiUrl}` + testEmail, obj);
  }

  addEmail(obj) {
    let editassetUrl = `/EmailAccounts`;
    return this.http.post(`${this.apiUrl}` + editassetUrl, obj);
  }


  getConfigValue() {
    let getagentUrlbyId = `/ConfigurationValues`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }
  
  getAllEmailforfilter() {
    let getagentUrl = `/emailaccounts/getlookup`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }



}
