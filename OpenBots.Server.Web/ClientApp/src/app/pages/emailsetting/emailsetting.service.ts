import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EmailsettingService {

  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient) { }
  getEmailbyId() {
    let getagentUrlbyId = `/EmailSettings`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }

  editEmail(id, obj) {
    let getagentUrlbyId = `/EmailSettings/${id}`;
    return this.http.put(`${this.apiUrl}` + getagentUrlbyId, obj);
  }

}
