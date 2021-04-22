import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { HelperService } from '../../@core/services/helper.service';
import { AssetApiUrl } from '../../webApiUrls/assetsUrl';
import { AgentApiUrl } from '../../webApiUrls';
@Injectable({
  providedIn: 'root',
})
export class AssetService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient, private helperService: HelperService) {}

  getFilterPagination(
    top: number,
    skip: number,
    orderBy: string,
    searchedValue?: string
  ) {
    let url: string;
    if (searchedValue) {
      if (orderBy != 'createdOn+desc') {
        url = `/${AssetApiUrl.AssetsView}?$filter=substringof(tolower('${searchedValue}'), tolower(Name))&$orderby=${orderBy}&$top=${top}&$skip=${skip}`;
      } else {
        url = `/${AssetApiUrl.AssetsView}?$filter=substringof(tolower('${searchedValue}'), tolower(Name))&$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
      }
    } else if (orderBy) {
      url = `/${AssetApiUrl.AssetsView}?$filter=agentId+eq+null&$orderby=${orderBy}&$top=${top}&$skip=${skip}`;
    } else {
      url = `/${AssetApiUrl.AssetsView}?$filter=agentId+eq+null&$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
    }
    return this.http.get(this.apiUrl + url);
  }

  // getAllAsset(tpage: any, spage: any) {
  //   let getagentUrl = `/${AssetApiUrl.AssetsView}?$orderby=createdOn desc&$top=${tpage}&$skip=${spage}`;
  //   return this.http.get(`${this.apiUrl}` + getagentUrl);
  // }

  // getAllAssetOrder(tpage: any, spage: any, name) {
  //   let getagentUrl = `/${AssetApiUrl.AssetsView}?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
  //   return this.http.get(`${this.apiUrl}` + getagentUrl);
  // }

  // getFilterAsset(tpage: any, spage: any, filterName) {
  //   let getagentUrl = `/${AssetApiUrl.AssetsView}?$filter=substringof(tolower('${filterName}'), tolower(name))&$top=${tpage}&$skip=${spage}`;
  //   return this.http.get(`${this.apiUrl}` + getagentUrl);
  // }

  getAssetByname(AssetName, id) {
    let getagentUrl = `/${AssetApiUrl.AssetsView}?$filter=name+eq+'${AssetName}'and agentId+ne+null`;

    //`/${AssetApiUrl.Assets}?$filter=substringof(tolower('${AssetName}'), tolower(name)) and Assetid  eq guid '${id}' `;
    //filter=substringof(tolower('${AssetName}'), tolower(name))and agentId+eq+null`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }
  addAssetAgent(obj) {
    let addassetUrl = `/${AssetApiUrl.Assets}/${AssetApiUrl.AddAgentAsset}`;
    return this.http.post(`${this.apiUrl}` + addassetUrl, obj);
  }
  getAgentName() {
    let getAgentUrl = `/${AgentApiUrl.Agents}/${AgentApiUrl.getLookup}`;
    return this.http.get(`${this.apiUrl}` + getAgentUrl);
  }

  getAssetbyId(id) {
    let resoptions = {};
    resoptions = {
      observe: 'response' as 'body',
      responseType: 'json',
    };
    let getagentUrlbyId = `/${AssetApiUrl.Assets}/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId, resoptions);
  }

  delAssetbyID(id) {
    let getagentUrlbyId = `/${AssetApiUrl.Assets}/${id}`;
    return this.http.delete(`${this.apiUrl}` + getagentUrlbyId);
  }

  addAsset(obj) {
    let addassetUrl = `/${AssetApiUrl.Assets}`;
    return this.http.post(`${this.apiUrl}` + addassetUrl, obj);
  }

  editAssetbyUpload(id, obj, etag) {
    const headers = new HttpHeaders({ 'If-Match': etag });
    let editassetUrl = `/${AssetApiUrl.Assets}/${id}/update`;
    return this.http.put(`${this.apiUrl}` + editassetUrl, obj, {
      headers: headers,
    });
  }
  editAsset(id, obj, etag) {
    const headers = this.helperService.getETagHeaders(etag);
    let editassetUrl = `/${AssetApiUrl.Assets}/${id}`;
    return this.http.put(`${this.apiUrl}` + editassetUrl, obj, { headers });
  }

  assetFileExport(id) {
    let fileExportUrl = `/${AssetApiUrl.Assets}/${id}/export`;
    let options = {};
    options = {
      responseType: 'blob',
      observe: 'response',
    };
    return this.http.get<any>(`${this.apiUrl}` + fileExportUrl, options);
  }
}
