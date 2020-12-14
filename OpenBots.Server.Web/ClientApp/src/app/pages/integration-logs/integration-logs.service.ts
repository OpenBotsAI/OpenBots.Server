import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { automationsApiUrl } from '../../webApiUrls';

@Injectable({
  providedIn: 'root',
})
export class IntegrationLogsService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient) {}

  get_AllSystemEvent(tpage: any, spage: any) {
    let getagentUrl = `/IntegrationEventLogs?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  get_EntityName() {
    let getagentUrl = `/IntegrationEventLogs`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  filter_EntityName(entityname: any, tpage: any, spage: any) {
    let getagentUrl = `/IntegrationEventLogs?$filter=${entityname}&$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  filter_EntityName_order_by(entityname: any, tpage: any, spage: any, order) {
    let getagentUrl = `/IntegrationEventLogs?$filter=${entityname}&$orderby=${order}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllJobsOrder(tpage: any, spage: any, name) {
    let getJobsUrl = `/Jobs/view?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getJobsUrl);
  }
   
  getAllEntityorder(tpage: any, spage: any, name) {
    let getagentUrl = `/IntegrationEventLogs?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  get_AllEntityorderbyEntityname(entityname, tpage: any, spage: any, name) {
    let getagentUrl = `/IntegrationEventLogs?$filter=${entityname}&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getSystemEventid(id) {
    let getagentUrlbyId = `/IntegrationEventLogs/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }

  
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   