import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class BusinessEventService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient) { }

  get_AllSystemEvent(tpage: any, spage: any) {
    let getagentUrl = `/IntegrationEvents?$filter=IsSystem+eq+false&$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getIntegrationEventName() {
    let getagentUrl = `/IntegrationEvents/IntegrationEventLookup?$filter=IsSystem+eq+false`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  filterIntegrationEventName(entityname: any, tpage: any, spage: any) {
    let getagentUrl = `/IntegrationEvents?$filter=${entityname}and IsSystem+eq+false&$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }
  filterEntityNameOrderby(entityname: any, tpage: any, spage: any, name) {
    let getagentUrl = `/IntegrationEvents?$filter=${entityname}and IsSystem+eq+false&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllIntegrationEventorder(tpage: any, spage: any, name) {
    let getagentUrl = `/IntegrationEvents?$filter=IsSystem+eq+false&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllorderbyEntityname(entityname, tpage: any, spage: any, name) {
    let getagentUrl = `/IntegrationEvents?$filter=${entityname}and IsSystem+eq+false&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getSystemEventid(id) {
    let getagentUrlbyId = `/IntegrationEvents/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }

  DeleteBusinessEventid(id) {
    let UrlbyId = `/IntegrationEvents/${id}`;
    return this.http.delete(`${this.apiUrl}` + UrlbyId);
  }

  addBusinessEvenet(obj) {
    let addUrl = `/IntegrationEvents/BusinessEvents`;
    return this.http.post(`${this.apiUrl}` + addUrl, obj);
  }

  updateBusinessEvenet(obj, id) {
    let Url = `/IntegrationEvents/BusinessEvents/${id}`;
    return this.http.put(`${this.apiUrl}` + Url, obj);
  }
}
