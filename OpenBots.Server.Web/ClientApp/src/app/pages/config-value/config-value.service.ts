import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { HelperService } from '../../@core/services/helper.service';

@Injectable({
  providedIn: 'root'
})
export class ConfigValueService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient, private helperService: HelperService) { }

  getAllConfigvalue(tpage: any, spage: any) {
    let getconfigUrl = `/ConfigurationValues?$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getconfigUrl);
  }

  getAllConfigvalueOrder(tpage: any, spage: any, name) {
    let getconfigUrl = `/ConfigurationValues?$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getconfigUrl);
  }

  getConfigValuebyId(id) {
    let resoptions = {};
    resoptions = {
      observe: 'response' as 'body',
      responseType: 'json',
    };
    let getagentUrlbyId = `/ConfigurationValues/${id}`;
    return this.http.get(`${this.apiUrl}` + getagentUrlbyId, resoptions);
  }


  editConfigValue(id, obj, etag) {
    const headers = this.helperService.getETagHeaders(etag)
    let editassetUrl = `/ConfigurationValues/${id}`;
    return this.http.put(`${this.apiUrl}` + editassetUrl, obj, { headers });
  }


}
