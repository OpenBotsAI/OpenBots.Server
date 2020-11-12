import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';


@Injectable()
export class AgentsService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient) { }


  getAllAgent(tpage:any,spage:any) {
    let getagentUrl = `/Agents?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllAgentOrder(tpage:any,spage:any,name) {
    let getagentUrl = `/Agents?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAgentbyID(id) {
    let options = {};
    options = {
      observe: 'response' as 'body',
      responseType: 'json',
    };
    let getagentUrlbyId = `/Agents/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId, options);
  }
  getCred() {
    let getagentUrlbyId = `/Credentials/GetLookup`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId);
  }
  delAgentbyID(id) {
    let getagentUrlbyId = `/Agents/${id}`;
    return this.http.delete(`${this.apiUrl}` + getagentUrlbyId);
  }

  addAgent(obj) {
    let addagentUrl = `/Agents`;
    return this.http.post(`${this.apiUrl}` + addagentUrl, obj);
  }

  editAgent(id, obj, etag) {
    const headers = new HttpHeaders({ 'If-Match': etag });

    // headers.append('Content-Type', 'application/json');
    // headers.append('If-Match', etag);
    let editagentUrl = `/Agents/${id}`;
    return this.http.put(`${this.apiUrl}` + editagentUrl, obj, { headers: headers });
  }

  patchAgent(id,isenable) {
    let obj = [{
      "op": "replace",
      "path": "/isEnabled",
      "value": isenable }]
    let editagentUrl = `/Agents/${id}`;
    return this.http.patch(`${this.apiUrl}` + editagentUrl, obj);
  }
 }

 
