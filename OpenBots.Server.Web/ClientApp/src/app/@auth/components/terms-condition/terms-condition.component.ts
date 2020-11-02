import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { HttpService } from '../../../@core/services/http.service';
import { HttpHeaders } from '@angular/common/http';
import { Consent } from '../../../interfaces/consent';

@Component({
  selector: 'ngx-terms-condition',
  templateUrl: './terms-condition.component.html',
  styleUrls: ['./terms-condition.component.scss']
})
export class TermsConditionComponent implements OnInit {
  agreementId: string;
  agreementConsent: Consent;
  personId: string;
  constructor(protected router: Router, protected httpService: HttpService) { }

  ngOnInit(): void {
    this.agreementConsent = <Consent>{
      personId: ''
    }
    this.userAgreemnt();
    this.personId = localStorage.getItem('personId');
  }

  rejectTerms() : void{
    this.userConsent(false);
    this.router.navigate(['auth/login']);
    localStorage.clear();
  }

  accpetTerms(): void {
    this.userConsent(true);
    localStorage.setItem('isUserConsentRequired', 'false');
    this.router.navigate(['pages/dashboard']);
  }

  userConsent(isAccepted: boolean) : void{
    const headers = { headers: new HttpHeaders({ 'Authorization': 'Bearer ' + localStorage.getItem('accessToken'), 'Content-Type': 'application/json' }) };
    this.agreementConsent.personId = this.personId;
    this.agreementConsent.userAgreementID = this.agreementId;
    this.agreementConsent.isAccepted = isAccepted;
    this.httpService.post('UserConsent', this.agreementConsent, headers).subscribe();
  }

  userAgreemnt(): void {
    const headers = { headers: new HttpHeaders({ 'Authorization': 'Bearer ' + localStorage.getItem('accessToken'), 'Content-Type': 'application/json' }) };
    this.httpService.get('UserAgreement', headers).subscribe((response) => {
      this.agreementId = response.id;
    });
  }
}
