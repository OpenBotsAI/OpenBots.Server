import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class SystemEventService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient) {}

  get_AllSystemEvent(tpage: any, spage: any) {
    let getagentUrl = `/IntegrationEvents?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  get_EntityName() {
    let getagentUrl = `/IntegrationEvents`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  filter_EntityName(entityname: any, tpage: any, spage: any) {
    let getagentUrl = `/IntegrationEvents?$filter=${entityname}&$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }
  filter_EntityName_order_by(entityname: any, tpage: any, spage: any, name) {
    let getagentUrl = `/IntegrationEvents?$filter=${entityname}&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllEntityorder(tpage: any, spage: any, name) {
    let getagentUrl = `/IntegrationEvents?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  get_AllEntityorderbyEntityname(entityname, tpage: any, spage: any, name) {
    let getagentUrl = `/IntegrationEvents?$filter=${entityname}&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getSystemEventid(id) {
    let getagentUrlbyId = `/IntegrationEvents/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }
}