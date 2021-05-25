import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { integrationUrl } from '../../webApiUrls/integrationUrl';

@Injectable({
  providedIn: 'root',
})
export class IntegrationLogsService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient) { }

  get_AllSystemEvent(tpage: any, spage: any) {
    let getagentUrl = `/${integrationUrl.IntegrationEventLogs}?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  get_EntityName() {
    // /IntegrationEventLogsLookup
    let getagentUrl = `/${integrationUrl.IntegrationEventLogs}/${integrationUrl.IntegrationEventLogsLookup}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  filter_EntityName(entityname: any, tpage: any, spage: any) {
    let getagentUrl = `/${integrationUrl.IntegrationEventLogs}?$filter=${entityname}&$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  filter_EntityName_order_by(entityname: any, tpage: any, spage: any, order) {
    let getagentUrl = `/${integrationUrl.IntegrationEventLogs}?$filter=${entityname}&$orderby=${order}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllEntityorder(tpage: any, spage: any, name) {
    let getagentUrl = `/${integrationUrl.IntegrationEventLogs}?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  get_AllEntityorderbyEntityname(entityname, tpage: any, spage: any, name) {
    let getagentUrl = `/${integrationUrl.IntegrationEventLogs}?$filter=${entityname}&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getSystemEventid(id) {
    let getagentUrlbyId = `/${integrationUrl.IntegrationEventLogs}/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }

  getIntegrationEventlogsPayload(entityname) {
    let getagentUrlbyId = `/${integrationUrl.IntegrationEventSubscriptionAttempts}?$filter=${entityname}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }
}
