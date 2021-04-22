import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { HelperService } from '../../@core/services/helper.service';

@Injectable({
  providedIn: 'root',
})
export class BusinessEventService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient, private helperService: HelperService) { }

  get_AllSystemEvent(tpage: any, spage: any) {
    let getagentUrl = `/IntegrationEvents?$filter=IsSystem+eq+false&$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getIntegrationEventName() {
    let getagentUrl = `/IntegrationEvents/IntegrationEventLookup?$filter=IsSystem+eq+false`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getBusinessEventName() {
    let getagentUrl = `/IntegrationEvents?$filter=IsSystem+eq+false`;
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

  // getSystemEventid(id) {
  //   let getagentUrlbyId = `/IntegrationEvents/${id}`;
  //   return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  // }


  getSystemEventid(id) {
    let resoptions = {};
    resoptions = {
      observe: 'response' as 'body',
      responseType: 'json',
    };
    let getagentUrlbyId = `/IntegrationEvents/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId, resoptions);
  }

  DeleteBusinessEventid(id) {
    let UrlbyId = `/IntegrationEvents/${id}`;
    return this.http.delete(`${this.apiUrl}` + UrlbyId);
  }

  addBusinessEvent(obj) {
    let addUrl = `/IntegrationEvents/BusinessEvents`;
    return this.http.post(`${this.apiUrl}` + addUrl, obj);
  }

  raiseBusinessEvent(obj, id) {
    let addUrl = `/IntegrationEvents/BusinessEvents/RaiseEvent/${id}`;
    return this.http.post(`${this.apiUrl}` + addUrl, obj);
  }
  // https://dev.server.openbots.io/api/v1/IntegrationEvents/BusinessEvents/RaiseEvent/{id}

  updateBusinessEvent(obj, id, etag) {
    const headers = this.helperService.getETagHeaders(etag);
    let Url = `/IntegrationEvents/BusinessEvents/${id}`;
    return this.http.put(`${this.apiUrl}` + Url, obj, { headers });
  }


  // updateProcess(obj, process_id, etag) {
  //   const headers = this.helperService.getETagHeaders(etag);
  //   let updateassetUrl = `/${automationsApiUrl.automations}/${process_id}`;
  //   return this.http.put(`${this.apiUrl}` + updateassetUrl, obj, { headers });
  // }
}
