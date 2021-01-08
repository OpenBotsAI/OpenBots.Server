import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ChangeLogsApiUrl } from '../../webApiUrls/ChangesLogUrls';
@Injectable({
  providedIn: 'root'
})
export class ChangelogService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient) { }


  get_AllAudits(tpage: any, spage: any) {
    let getagentUrl = `/${ChangeLogsApiUrl.AuditLogsView}?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }


  get_servicename() {
    let getagentUrl = `/${ChangeLogsApiUrl.AuditLogsgetLookup}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }


  filter_servicename(service_name: any, tpage: any, spage: any) {
    let getagentUrl = `/${ChangeLogsApiUrl.AuditLogsView}?$filter=${service_name}&$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }
  filter_servicename_order_by(service_name: any, tpage: any, spage: any, name) {
    let getagentUrl = `/${ChangeLogsApiUrl.AuditLogsView}?$filter=${service_name}&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  get_AllAgent_order(tpage: any, spage: any, name) {
    let getagentUrl = `/${ChangeLogsApiUrl.AuditLogsView}?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }


  get_AllAgent_order_by_servicename(service_name, tpage: any, spage: any, name) {
    let getagentUrl = `/${ChangeLogsApiUrl.AuditLogsView}?$filter=${service_name}&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }


  get_Audit_id(id) {
    let getagentUrlbyId = `/${ChangeLogsApiUrl.AuditLogs}/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }




  getExportFile(serviceName) {
    let getexportfile = `/${ChangeLogsApiUrl.AuditLogsExportzip}?$filter=${serviceName}`;
    let options = {}
    options = {
      responseType: 'blob',
      observe: 'response',
    }

    return this.http.get(`${this.apiUrl}` + getexportfile, options)
  }

}
