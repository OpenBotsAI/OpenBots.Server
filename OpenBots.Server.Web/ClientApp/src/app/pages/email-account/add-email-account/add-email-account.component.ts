import { DatePipe } from '@angular/common';
import { HttpResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NbDateService, NbToastrService } from '@nebular/theme';
import { EmailAccountsService } from '../email-accounts.service';
import { TimeDatePipe } from '../../../@core/pipe';
@Component({
  selector: 'ngx-add-email-account',
  templateUrl: './add-email-account.component.html',
  styleUrls: ['./add-email-account.component.scss'],
})
export class AddEmailAccountComponent implements OnInit {
  min: Date;
  max: Date;
  isSslEnabled = true;
  checked = false;
  submitted = false;
  showEmail: any = [];
  emailform: FormGroup;
  pipe = new DatePipe('en-US');
  now = Date();
  show_createdon: any = [];
  urlId: string;
  etag;
  showEmailAccount: any = [];
  title = 'Add';
  constructor(
    private toastrService: NbToastrService,
    private dateService: NbDateService<Date>,
    protected emailService: EmailAccountsService,
    private formBuilder: FormBuilder,
    protected router: Router,
    private route: ActivatedRoute
  ) {
    this.urlId = this.route.snapshot.params['id'];
    if (this.urlId) {
      this.getallemail(this.urlId);
      this.title = 'Update';
    }
  }

  getallemail(id) {
    this.emailService.getEmailbyId(id).subscribe((data: HttpResponse<any>) => {
      this.showEmail = data.body;
      this.etag = data.headers.get('ETag').replace(/\"/g, '');
      const filterPipe = new TimeDatePipe();
      const fiteredArr = filterPipe.transform(this.showEmail.createdOn, 'lll');
      this.showEmail.createdOn = filterPipe.transform(
        this.showEmail.createdOn,
        'lll'
      );

      this.emailform.patchValue(this.showEmail);
    });
  }

  ngOnInit(): void {
    this.emailform = this.formBuilder.group({
      fromEmailAddress: [
        '',
        [
          Validators.required,
          Validators.pattern('^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+.[a-z]{2,4}$'),
        ],
      ],
      fromName: [''],
      host: [''],
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
          Validators.pattern('^[A-Za-z0-9_.-]{3,100}$'),
        ],
      ],
      encryptedPassword: [''],
      port: [''],
      provider: ['', [Validators.required]],
      username: [''],
      isDefault: [this.checked],
      isSslEnabled: [this.isSslEnabled],
    });
    this.min = new Date();
    this.max = new Date();
    this.min = this.dateService.addMonth(this.dateService.today(), 0);
    this.max = this.dateService.addMonth(this.dateService.today(), 1);
  }

  get f() {
    return this.emailform.controls;
  }

  check(checked: boolean) {
    this.checked = checked;
  }

  isSSl(checkSSL: boolean) {
    this.isSslEnabled = checkSSL;
  }
  gotoaudit() {

    this.router.navigate([`/pages/change-log/list/${'email'}/${this.urlId}`])
  }

  onSubmit() {
    if (this.urlId) {
      this.UpdateEmailAccount();
    } else {
      this.AddEmailAccount();
    }
  }

  AddEmailAccount() {
    this.submitted = true;
    this.emailService.addEmail(this.emailform.value).subscribe(
      () => {
        this.toastrService.success(
          'Email account created successfully',
          'Success'
        );
        this.router.navigate(['pages/emailaccount/list']);
        this.submitted = false;
      },
      () => (this.submitted = false)
    );
  }
  UpdateEmailAccount() {
    this.submitted = true;
    this.emailService
      .editEmail(this.urlId, this.emailform.value, this.etag)
      .subscribe(
        () => {
          this.toastrService.success(
            'Email details Updated successfully',
            //  'Update',
            'Success'
          );
          this.router.navigate(['pages/emailaccount/list']);
        },
        (error) => {
          if (error.error.status === 409) {
            this.toastrService.danger(error.error.serviceErrors, 'error');
            this.getallemail(this.urlId);
          }
        }
      );

    this.submitted = false;
  }
}
