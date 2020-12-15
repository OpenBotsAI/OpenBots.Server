import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class SubscriptionService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient) {}

  addsubscription(obj) {
    let addassetUrl = `/IntegrationEventSubscriptions`;
    return this.http.post(`${this.apiUrl}` + addassetUrl, obj);
  }
  delsubscriptionbyID(id) {
    let getagentUrlbyId = `/IntegrationEventSubscriptions/${id}`;
    return this.http.delete(`${this.apiUrl}` + getagentUrlbyId);
  }

  get_EntityName() {
    // /IntegrationEventLogsLookup
    let getagentUrl = `/IntegrationEventLogs/IntegrationEventLogsLookup`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }
  getQueues() {
    // /IntegrationEventLogsLookup
    let getagentUrl = `/Queues`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllEventSubscription(tpage: any, spage: any) {
    let getagentUrl = `/IntegrationEventSubscriptions?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  // get_EntityName() {
  //   // /IntegrationEventLogsLookup
  //   let getagentUrl = `/IntegrationEventSubscriptions`;
  //   return this.http.get(`${this.apiUrl}` + getagentUrl);
  // }

  filter_EntityName(entityname: any, tpage: any, spage: any) {
    let getagentUrl = `/IntegrationEventSubscriptions?$filter=${entityname}&$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  filter_EntityName_order_by(entityname: any, tpage: any, spage: any, order) {
    let getagentUrl = `/IntegrationEventSubscriptions?$filter=${entityname}&$orderby=${order}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllEntityorder(tpage: any, spage: any, name) {
    let getagentUrl = `/IntegrationEventSubscriptions?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  get_AllEntityorderbyEntityname(entityname, tpage: any, spage: any, name) {
    let getagentUrl = `/IntegrationEventSubscriptions?$filter=${entityname}&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getSystemEventid(id) {
    let getagentUrlbyId = `/IntegrationEventSubscriptions/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }
}
