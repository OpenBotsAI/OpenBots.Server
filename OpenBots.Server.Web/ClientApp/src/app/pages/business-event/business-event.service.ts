import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { HelperService } from '../../@core/services/helper.service';
import { integrationUrl } from '../../webApiUrls/integrationUrl';
@Injectable({
  providedIn: 'root',
})
export class BusinessEventService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient, private helperService: HelperService) { }

  get_AllSystemEvent(tpage: any, spage: any) {
    let getagentUrl = `/${integrationUrl.IntegrationEvents}?$filter=IsSystem+eq+false&$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getIntegrationEventName() {
    let getagentUrl = `/${integrationUrl.IntegrationEvents}/IntegrationEventLookup?$filter=IsSystem+eq+false`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getBusinessEventName() {
    let getagentUrl = `/${integrationUrl.IntegrationEvents}?$filter=IsSystem+eq+false`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  filterIntegrationEventName(entityname: any, tpage: any, spage: any) {
    let getagentUrl = `/${integrationUrl.IntegrationEvents}?$filter=${entityname}and IsSystem+eq+false&$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }
  filterEntityNameOrderby(entityname: any, tpage: any, spage: any, name) {
    let getagentUrl = `/${integrationUrl.IntegrationEvents}?$filter=${entityname}and IsSystem+eq+false&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllIntegrationEventorder(tpage: any, spage: any, name) {
    let getagentUrl = `/${integrationUrl.IntegrationEvents}?$filter=IsSystem+eq+false&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllorderbyEntityname(entityname, tpage: any, spage: any, name) {
    let getagentUrl = `/${integrationUrl.IntegrationEvents}?$filter=${entityname}and IsSystem+eq+false&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getSystemEventid(id) {
    let resoptions = {};
    resoptions = {
      observe: 'response' as 'body',
      responseType: 'json',
    };
    let getagentUrlbyId = `/${integrationUrl.IntegrationEvents}/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId, resoptions);
  }

  DeleteBusinessEventid(id) {
    let UrlbyId = `/${integrationUrl.IntegrationEvents}/${id}`;
    return this.http.delete(`${this.apiUrl}` + UrlbyId);
  }

  addBusinessEvent(obj) {
    let addUrl = `/${integrationUrl.IntegrationEvents}/BusinessEvents`;
    return this.http.post(`${this.apiUrl}` + addUrl, obj);
  }

  raiseBusinessEvent(obj, id) {
    let addUrl = `/${integrationUrl.IntegrationEvents}/BusinessEvents/RaiseEvent/${id}`;
    return this.http.post(`${this.apiUrl}` + addUrl, obj);
  }

  updateBusinessEvent(obj, id, etag) {
    const headers = this.helperService.getETagHeaders(etag);
    let Url = `/${integrationUrl.IntegrationEvents}/BusinessEvents/${id}`;
    return this.http.put(`${this.apiUrl}` + Url, obj, { headers });
  }

}
