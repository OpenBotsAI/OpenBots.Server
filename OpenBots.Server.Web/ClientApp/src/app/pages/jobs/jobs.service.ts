import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { automationsApiUrl } from '../../webApiUrls';
import { JobsApiUrl } from '../../webApiUrls/jobsUrl';
import { AgentApiUrl } from '../../webApiUrls/agentsUrl';

@Injectable({
  providedIn: 'root',
})
export class JobsService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient) {}

  getAllJobs(tpage: any, spage: any) {
    let getJobstUrl = `/${JobsApiUrl.Jobs}?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getJobstUrl);
  }

  JobsFilter(filter_name: any, tpage: any, spage: any) {
    let getJobstUrl = `/${JobsApiUrl.Jobs}?$filter=${filter_name}&$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getJobstUrl);
  }

  getAllJobsOrder(tpage: any, spage: any, name) {
    let getJobsUrl = `/${JobsApiUrl.Jobs}?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getJobsUrl);
  }

  getAgentName() {
    let getAgentUrl = `/${AgentApiUrl.Agents}/${AgentApiUrl.getLookup}`;
    return this.http.get(`${this.apiUrl}` + getAgentUrl);
  }

  getProcessName() {
    let getProcessUrl = `/${automationsApiUrl.getLookUp}`;
    return this.http.get(`${this.apiUrl}` + getProcessUrl);
  }
  getJobsId(id) {
    let getagentUrlbyId = `/${JobsApiUrl.Jobs}/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }
  getExportFile() {
    let getexportfile = `/${JobsApiUrl.JobsExportzip}`;
    let options = {};
    options = {
      responseType: 'blob',
      observe: 'response',
    };
    return this.http.get(`${this.apiUrl}` + getexportfile, options);
  }

  getExportFilebyfilter(filter_name: any) {
    let getJobstUrl = `/${JobsApiUrl.JobsExportzip}?$filter=${filter_name}`;
    let options = {};
    options = {
      responseType: 'blob',
      observe: 'response',
    };
    return this.http.get(`${this.apiUrl}` + getJobstUrl, options);
  }
}
