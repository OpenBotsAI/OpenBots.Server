import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { HelperService } from '../../@core/services/helper.service';
import { AssetApiUrl } from '../../webApiUrls/assetsUrl';
@Injectable({
  providedIn: 'root',
})
export class AssetService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient, private helperService: HelperService) { }

  getAllAsset(tpage: any, spage: any) {
    let getagentUrl = `/${AssetApiUrl.Assets}?$orderby=createdOn desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllAssetOrder(tpage: any, spage: any, name) {
    let getagentUrl = `/${AssetApiUrl.Assets}?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
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

  AssetFile(id, file) {
    let editassetUrl = `/${AssetApiUrl.Assets}/${id}/upload`;
    return this.http.post(`${this.apiUrl}` + editassetUrl, file);
  }
  editAssetbyUpload(id, obj, etag) {
    const headers = new HttpHeaders({ 'If-Match': etag });
    let editassetUrl = `/${AssetApiUrl.Assets}/${id}/update`;
    return this.http.put(`${this.apiUrl}` + editassetUrl, obj, {
      headers: headers,
    });
  }
  editAsset(id, obj, etag) {
    const headers = this.helperService.getETagHeaders(etag)
    // const headers = new HttpHeaders({ 'If-Match': etag });
    let editassetUrl = `/${AssetApiUrl.Assets}/${id}`;
    return this.http.put(`${this.apiUrl}` + editassetUrl, obj, {
      headers,
    });
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
