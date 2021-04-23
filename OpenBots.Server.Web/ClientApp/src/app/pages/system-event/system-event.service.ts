import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { integrationUrl } from '../../webApiUrls/integrationUrl';

@Injectable({
  providedIn: 'root',
})
export class SystemEventService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient) { }

  get_AllSystemEvent(tpage: any, spage: any) {
    let getagentUrl = `/${integrationUrl.IntegrationEvents}?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getIntegrationEventName() {
    let getagentUrl = `/${integrationUrl.IntegrationEvents}/${integrationUrl.IntegrationEventLookup}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  filterIntegrationEventName(entityname: any, tpage: any, spage: any) {
    let getagentUrl = `/${integrationUrl.IntegrationEvents}?$filter=${entityname}&$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }
  filterEntityNameOrderby(entityname: any, tpage: any, spage: any, name) {
    let getagentUrl = `/${integrationUrl.IntegrationEvents}?$filter=${entityname}&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllIntegrationEventorder(tpage: any, spage: any, name) {
    let getagentUrl = `/${integrationUrl.IntegrationEvents}?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllorderbyEntityname(entityname, tpage: any, spage: any, name) {
    let getagentUrl = `/${integrationUrl.IntegrationEvents}?$filter=${entityname}&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getSystemEventid(id) {
    let getagentUrlbyId = `/${integrationUrl.IntegrationEvents}/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }
}
