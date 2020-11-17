import { DatePipe } from '@angular/common';
import { HttpResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NbToastrService } from '@nebular/theme';
import { TimeDatePipe } from '../../../@core/pipe';
import { EmailAccountsService } from '../email-accounts.service';

@Component({
  selector: 'ngx-edit-email-account',
  templateUrl: './edit-email-account.component.html',
  styleUrls: ['./edit-email-account.component.scss']
})
export class EditEmailAccountComponent implements OnInit {
  emailId: any = [];
  submitted = false;
  showEmail: any = [];
  emailform: FormGroup;
  pipe = new DatePipe('en-US');
  now = Date();
  show_createdon: any = [];
  etag;
  constructor(
    private acroute: ActivatedRoute, private toastrService: NbToastrService,
    protected emailService: EmailAccountsService,
    private formBuilder: FormBuilder, protected router: Router,
  ) {
    this.acroute.queryParams.subscribe((params) => {
      this.emailId = params.id
      this.getallemail(params.id);
    });
  }

  ngOnInit(): void {
    this.emailform = this.formBuilder.group({
      fromEmailAddress: ['', [Validators.required, Validators.pattern('^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[a-z]{2,4}$')]],
      fromName: [''],
      host: [''],
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100), Validators.pattern('^[A-Za-z0-9_.-]{3,100}$')]],
      passwordHash: [''],
      port: [''],
      provider: ['', [Validators.required]],
      username: [''],
      apiKey: [''],
      createdBy: [''],
      createdOn: [''],
      deleteOn: [''],
      deletedBy: [''],
      encryptedPassword: [''],
      endOnUTC: [''],
      id: [''],
      isDefault: [''],
      isDeleted: [''],
      isDisabled: [''],
      isSslEnabled: [''],
      startOnUTC: [''],
      timestamp: [''],
      updatedBy: [''],
      updatedOn: [''],

    });
  }




  get f() {
    return this.emailform.controls;
  }

  getallemail(id) {
    this.emailService.getEmailbyId(id).subscribe((data: HttpResponse<any>) => {
      this.showEmail = data.body;
      this.etag = data.headers.get('ETag').replace(/\"/g, '')
      const filterPipe = new TimeDatePipe();
      const fiteredArr = filterPipe.transform(this.showEmail.createdOn, 'lll');
      this.showEmail.createdOn = filterPipe.transform(this.showEmail.createdOn, 'lll');

      this.emailform.patchValue(this.showEmail);

    });
  }



  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], { queryParams: { PageName: 'OpenBots.Server.Model.email', id: this.showEmail.id } })
  }



  onSubmit() {
    this.submitted = true;
    this.emailService
      .editEmail(this.emailId, this.emailform.value, this.etag)
      .subscribe(() => {
        this.toastrService.success('Email Details Update Successfully', 'Success');
        this.router.navigate(['pages/emailaccount/list']);
      }, (error) => {
        console.log(error.status, error)
        if (error.error.status === 409) {
          this.toastrService.danger(error.error.serviceErrors, 'error')
          this.getallemail(this.emailId)
        }
      });

    this.submitted = false;

  }
}