import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { HelperService } from '../../@core/services/helper.service';

@Injectable({
  providedIn: 'root',
})
export class AssetService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient, private helperService: HelperService) { }

  getAllAsset(tpage: any, spage: any) {
    let getagentUrl = `/Assets?$orderby=createdOn desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAllAssetOrder(tpage: any, spage: any, name) {
    let getagentUrl = `/Assets?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getagentUrl);
  }

  getAssetbyId(id) {
    let resoptions = {};
    resoptions = {
      observe: 'response' as 'body',
      responseType: 'json',
    };
    let getagentUrlbyId = `/Assets/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId, resoptions);
  }

  delAssetbyID(id) {
    let getagentUrlbyId = `/Assets/${id}`;
    return this.http.delete(`${this.apiUrl}` + getagentUrlbyId);
  }

  addAsset(obj) {
    let addassetUrl = `/Assets`;
    return this.http.post(`${this.apiUrl}` + addassetUrl, obj);
  }

  AssetFile(id, file) {
    let editassetUrl = `/Assets/${id}/upload`;
    return this.http.post(`${this.apiUrl}` + editassetUrl, file);
  }
  editAssetbyUpload(id, obj, etag) {
    const headers = new HttpHeaders({ 'If-Match': etag });
    let editassetUrl = `/Assets/${id}/update`;
    return this.http.put(`${this.apiUrl}` + editassetUrl, obj, {
      headers: headers,
    });
  }
  editAsset(id, obj, etag) {
    const headers = this.helperService.getETagHeaders(etag)
    // const headers = new HttpHeaders({ 'If-Match': etag });
    let editassetUrl = `/Assets/${id}`;
    return this.http.put(`${this.apiUrl}` + editassetUrl, obj, {
      headers,
    });
  }

  assetFileExport(id) {
    let fileExportUrl = `/Assets/${id}/export`;
    let options = {};
    options = {
      responseType: 'blob',
      observe: 'response',
    };
    return this.http.get<any>(`${this.apiUrl}` + fileExportUrl, options);
  }
}
