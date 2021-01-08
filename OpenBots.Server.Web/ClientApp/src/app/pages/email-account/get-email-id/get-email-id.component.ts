import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TimeDatePipe } from '../../../@core/pipe';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { EmailAccountsService } from '../email-accounts.service';
import { HttpResponse } from '@angular/common/http';
import { NbToastrService } from '@nebular/theme';

@Component({
  selector: 'ngx-get-email-id',
  templateUrl: './get-email-id.component.html',
  styleUrls: ['./get-email-id.component.scss']
})
export class GetEmailIdComponent implements OnInit {

  jsonValue: any = [];
  showEmail: any = [];
  emailform: FormGroup;
  pipe = new DatePipe('en-US');
  now = Date();
  show_createdon: any = [];
  etag;
  emailId;
  constructor(
    private acroute: ActivatedRoute,
    protected emailService: EmailAccountsService,
    private toastrService: NbToastrService,
    private formBuilder: FormBuilder, protected router: Router,
  ) {
    this.acroute.queryParams.subscribe((params) => {
      this.emailId = params.id
      this.getEmailById(params.id);
    });
  }

  ngOnInit(): void {
    this.emailform = this.formBuilder.group({
      apiKey: [''],
      createdBy: [''],
      createdOn: [''],
      deleteOn: [''],
      deletedBy: [''],
      encryptedPassword: [''],
      endOnUTC: [''],
      fromEmailAddress: [''],
      fromName: [''],
      host: [''],
      id: [''],
      isDefault: [''],
      isDeleted: [''],
      isDisabled: [''],
      isSslEnabled: [''],
      name: [''],
      passwordHash: [''],
      port: [''],
      provider: [''],
      startOnUTC: [''],
      timestamp: [''],
      updatedBy: [''],
      updatedOn: [''],
      username: [''],
    });
  }





  getEmailById(id) {
    this.emailService.getEmailbyId(id).subscribe((data: HttpResponse<any>) => {
      this.showEmail = data.body;
      this.etag = data.headers.get('ETag').replace(/\"/g, '')
      const filterPipe = new TimeDatePipe();
      this.showEmail.createdOn = filterPipe.transform(this.showEmail.createdOn, 'lll');
      this.emailform.patchValue(this.showEmail);
      this.emailform.disable();
      console.log(this.showEmail.name)

    }, (error) => {
      console.log(error.status, error)
      if (error.error.status === 409) {
        this.toastrService.danger(error.error.serviceErrors, 'error')
        this.getEmailById(this.emailId)
      }
    });
  }



  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], { queryParams: { PageName: 'Configuration.EmailAccount', id: this.showEmail.id } })
  }

  gotoEmailTest() {
    this.router.navigate(['/pages/emailaccount/send-email'], { queryParams: { name: this.showEmail.name } })
    // email: this.showEmail.fromEmailAddress
  }
}
