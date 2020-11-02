import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
 
@Injectable({
  providedIn: 'root'
})
export class JobsService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient) { }



  getAllJobs(tpage: any, spage: any) {
    let getJobstUrl = `/Jobs/view?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getJobstUrl);
  }

  JobsFilter(filter_name: any, tpage: any, spage: any) {
    let getJobstUrl = `/Jobs/view?$filter=${filter_name}&$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getJobstUrl);
  }

  getAllJobsOrder(tpage: any, spage: any, name) {
    let getJobsUrl = `/Jobs/view?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getJobsUrl);
  }

  getAgentName() {
    let getAgentUrl = `/Agents/GetLookup`;
    return this.http.get(`${this.apiUrl}` + getAgentUrl);
  }

  getProcessName() {
    let getProcessUrl = `/Processes/GetLookup`;
    return this.http.get(`${this.apiUrl}` + getProcessUrl);
  }
  getJobsId(id) {
    let getagentUrlbyId = `/Jobs/view/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }
  getExportFile() {
    let getexportfile = '/Jobs/Export/zip'
    let options = {}
    options = {
      responseType: 'blob',
      observe: 'response',
    }
    return this.http.get(`${this.apiUrl}` + getexportfile, options)
  }

  getExportFilebyfilter(filter_name: any) {
    let getJobstUrl = `/Jobs/Export/zip?$filter=${filter_name}`;
    let options = {}
    options = {
      responseType: 'blob',
      observe: 'response',
    }
    return this.http.get(`${this.apiUrl}` + getJobstUrl, options)
  }



}
