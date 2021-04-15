import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpService } from '../../../@core/services/http.service';
import { TimeDatePipe } from '../../../@core/pipe';
import { HelperService } from '../../../@core/services/helper.service';

@Component({
  selector: 'ngx-view-credentials',
  templateUrl: './view-credentials.component.html',
  styleUrls: ['./view-credentials.component.scss'],
})
export class ViewCredentialsComponent implements OnInit {
  credentialViewForm: FormGroup;
  currentUrlId: string;
  pipe: TimeDatePipe;
  providerArr = [
    { id: 'AD', name: 'Active Directory' },
    { id: 'A', name: 'Application' },
  ];
  constructor(
    protected router: Router,
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private httpService: HttpService,
    private helperService: HelperService
  ) { }

  ngOnInit(): void {
    this.currentUrlId = this.route.snapshot.params['id'];
    if (this.currentUrlId) {
      this.getCredentialById();
    }
    this.credentialViewForm = this.initizlizeForm();
  }

  initizlizeForm() {
    return this.fb.group({
      name: [''],
      createdBy: [''],
      createdOn: [''],
      domain: [''],
      endDate: [],
      id: [''],
      isDeleted: false,
      passwordHash: [''],
      passwordSecret: [''],
      provider: [''],
      startDate: [],
      timestamp: [''],
      updatedBy: [''],
      updatedOn: [''],
      userName: [''],
    });
  }

  getCredentialById(): void {
    this.httpService
      .get(`Credentials/View/${this.currentUrlId}`, { observe: 'response' })
      .subscribe((response) => {
        if (response && response.status == 200) {
          response.body.startDate = this.helperService.transformDate(
            response.body.startDate,
            'll'
          );
          response.body.endDate = this.helperService.transformDate(
            response.body.endDate,
            'll'
          );
          response.body.createdOn = this.helperService.transformDate(
            response.body.createdOn,
            'lll'
          );
          response.body.updatedOn = this.helperService.transformDate(
            response.body.updatedOn,
            'lll'
          );
          if (response.body.provider === 'AD')
            response.body.provider = 'Active Directory';
          else if (response.body.provider === 'A')
            response.body.provider = 'Application';
          this.credentialViewForm.patchValue({ ...response.body });
          this.credentialViewForm.disable();
        }
      });
  }

  navigateToAudit(): void {
    this.router.navigate([`/pages/change-log/list/${'Credential'}/${this.currentUrlId}`]);
  }
}
