import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { HelperService } from '../../@core/services/helper.service';
@Injectable({
  providedIn: 'root',
})
export class SubscriptionService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient, private helperService: HelperService) {}

  addsubscription(obj) {
    let addassetUrl = `/IntegrationEventSubscriptions`;
    return this.http.post(`${this.apiUrl}` + addassetUrl, obj);
  }

  updateSubscription(obj, id, etag) {
    const headers = this.helperService.getETagHeaders(etag);
    let updateassetUrl = `/IntegrationEventSubscriptions/${id}`;
    return this.http.put(`${this.apiUrl}` + updateassetUrl, obj, { headers });
  }

  delsubscriptionbyID(id) {
    let getagentUrlbyId = `/IntegrationEventSubscriptions/${id}`;
    return this.http.delete(`${this.apiUrl}` + getagentUrlbyId);
  }

  get_EntityName() {
    let getagentUrl = `/IntegrationEventLogs/IntegrationEventLogsLookup`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }
  getQueues() {
    let getagentUrl = `/Queues`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllEventSubscription(tpage: any, spage: any) {
    let getagentUrl = `/IntegrationEventSubscriptions?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  filterSubscriptionName(entityname: any, tpage: any, spage: any) {
    let getagentUrl = `/IntegrationEventSubscriptions?$filter=${entityname}&$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  filterSubscriptionNameorderby(
    entityname: any,
    tpage: any,
    spage: any,
    order
  ) {
    let getagentUrl = `/IntegrationEventSubscriptions?$filter=${entityname}&$orderby=${order}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllSubscriptionOrder(tpage: any, spage: any, name) {
    let getagentUrl = `/IntegrationEventSubscriptions?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  // getAllsubscriptionOrderbyEntityname(entityname, tpage: any, spage: any, name) {
  //   let getagentUrl = `/IntegrationEventSubscriptions?$filter=${entityname}&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
  //   return this.http.get(`${this.apiUrl}` + getagentUrl);
  // }

  getsubscribeID(id) {
    let resoptions = {};
    resoptions = {
      observe: 'response' as 'body',
      responseType: 'json',
    };
    let getagentUrlbyId = `/IntegrationEventSubscriptions/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId, resoptions);
  }

  
}
