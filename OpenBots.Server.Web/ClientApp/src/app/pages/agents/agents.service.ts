import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { HelperService } from '../../@core/services/helper.service';
import { AgentApiUrl } from '../../webApiUrls/agentsUrl';
import { CredentialsApiUrl } from '../../webApiUrls/credential';

@Injectable()
export class AgentsService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient, private helperService: HelperService) { }

  getAllAgent(tpage: any, spage: any) {
    let getagentUrl = `/${AgentApiUrl.AgentsView}?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllAgentOrder(tpage: any, spage: any, name) {
    let getagentUrl = `/${AgentApiUrl.AgentsView}?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAgentbyID(id) {
    let resoptions = {};
    resoptions = {
      observe: 'response' as 'body',
      responseType: 'json',
    };
    let getagentUrlbyId = `/${AgentApiUrl.Agents}/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId, resoptions);
  }
  getAgentbyHeartBeatID(id, tpage: any, spage: any) {
    let getagentUrlbyId = `/${AgentApiUrl.Agents}/${id}/AgentHeartbeats?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }

  getAgentbyHeartBeatIDorder(id, tpage: any, spage: any, name) {
    let getagentUrlbyId = `/${AgentApiUrl.Agents}/${id}/AgentHeartbeats?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }

  getCred() {
    let getagentUrlbyId = `/${CredentialsApiUrl.credentials}/${CredentialsApiUrl.getLookUp}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }
  delAgentbyID(id) {
    let getagentUrlbyId = `/${AgentApiUrl.Agents}/${id}`;
    return this.http.delete(`${this.apiUrl}` + getagentUrlbyId);
  }

  addAgent(obj) {
    let addagentUrl = `/${AgentApiUrl.Agents}`;
    return this.http.post(`${this.apiUrl}` + addagentUrl, obj);
  }

  editAgent(id, obj, etag) {
    const headers = this.helperService.getETagHeaders(etag);
    let editagentUrl = `/${AgentApiUrl.Agents}/${id}`;
    return this.http.put(`${this.apiUrl}` + editagentUrl, obj, {
      headers,
    });
  }

  patchAgent(id, isenable) {
    let obj = [
      {
        op: 'replace',
        path: '/isEnabled',
        value: isenable,
      },
    ];
    let editagentUrl = `/${AgentApiUrl.Agents}/${id}`;
    return this.http.patch(`${this.apiUrl}` + editagentUrl, obj);
  }
}
