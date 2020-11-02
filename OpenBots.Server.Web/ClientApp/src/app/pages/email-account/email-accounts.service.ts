import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EmailAccountsService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient) { }

  getAllEmail(tpage: any, spage: any) {
    let getagentUrl = `/EmailAccounts?$orderby=createdOn desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllAssetOrder(tpage: any, spage: any, name) {
    let getagentUrl = `/EmailAccounts?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getEmailbyId(id) {
    let getagentUrlbyId = `/EmailAccounts/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }

  delAssetbyID(id) {
    let getagentUrlbyId = `/EmailAccounts/${id}`;
    return this.http.delete(`${this.apiUrl}` + getagentUrlbyId);
  }


  editEmail(id, obj) {
    let editassetUrl = `/EmailAccounts/${id}`;
    return this.http.put(`${this.apiUrl}` + editassetUrl, obj);
  }

 

  addEmail(obj) {
    let editassetUrl = `/EmailAccounts`;
    return this.http.post(`${this.apiUrl}` + editassetUrl, obj);
  }

}
