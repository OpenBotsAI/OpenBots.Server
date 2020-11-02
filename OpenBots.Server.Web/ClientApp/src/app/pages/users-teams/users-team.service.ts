/*
 * Copyright (c) Akveo 2019. All Rights Reserved.
 * Licensed under the Single Application / Multi Application License.
 * See LICENSE_SINGLE_APP / LICENSE_MULTI_APP in the 'docs' folder for license information on type of purchased license.
 */
 
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UsersTeamService {
  headerData = new HttpHeaders({ 'Content-Type': 'application/json' });
  get apiUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient) { }


   
   getPeople(org,tpage:any,spage :any) {
    let getPeopleUrl = `/Organizations/${org}/OrganizationMembers/People?$orderby=joinedOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getPeopleUrl);
  }
  getPeopleOrder(org,tpage:any,spage :any,filter) {
    let getPeopleUrl = `/Organizations/${org}/OrganizationMembers/People?$orderby=${filter}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getPeopleUrl);
  }


  getPeoplePending(org,tpage:any,spage :any) {
    let pendinigRequestUrl = `/Organizations/${org}/AccessRequests/Pending?$orderby=joinedOn+desc&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + pendinigRequestUrl);
  }


  getPeoplePendingOrder(org,tpage:any,spage :any,filter) {
    let getPeopleUrl = `/Organizations/${org}/AccessRequests/Pending?$orderby=${filter}&$top=${tpage}&$skip=${spage}`;
    return this.http.get(`${this.apiUrl}` + getPeopleUrl);
  }
  deleteMember(org, personId) {
    let url = `/Organizations/${org}/OrganizationMembers/${personId}`;
    return this.http.delete(`${this.apiUrl}` + url);
  }

  inviteMember(obj) {
    let url = `/Organizations/${localStorage.getItem('ActiveOrganizationID')}/OrganizationMembers/InviteUser`;
    return this.http.post(`${this.apiUrl}` + url, obj);
  }

  ApproveMember(OrganizationID, personId, obj) {
    const approveRequestUrl = `/Organizations/${OrganizationID}/AccessRequests/${personId}/Approve`;
    return this.http.put(`${this.apiUrl}` + approveRequestUrl, obj);
  }

  RejectMember(OrganizationID, personId, obj) {
    const rejectAccessUrl = `/Organizations/${OrganizationID}/AccessRequests/${personId}/Reject`;
    return this.http.put(`${this.apiUrl}` + rejectAccessUrl, obj);
  }



    AllowgrantAdmin(personId, org) {
      const AccessUrl = `/People/${personId}/Organizations/${org}/GrantAdmin`;
      return this.http.put(`${this.apiUrl}` + AccessUrl, this.headerData);
    }
  
  
    RemovegrantAdmin(personId, org) {
      const RevokeUrl = `/People/${personId}/Organizations/${org}/RevokeAdmin`;
      return this.http.put(`${this.apiUrl}` + RevokeUrl, this.headerData);
    }
}
