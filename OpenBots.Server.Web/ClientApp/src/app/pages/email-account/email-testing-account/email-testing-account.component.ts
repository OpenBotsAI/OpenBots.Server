import { Component, EventEmitter, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NbToastrService } from '@nebular/theme';
import {
  UploaderOptions,
  UploadFile,
  UploadInput,
  UploadOutput,
} from 'ngx-uploader';
import { EmailAccountsService } from '../email-accounts.service';

@Component({
  selector: 'ngx-email-testing-account',
  templateUrl: './email-testing-account.component.html',
  styleUrls: ['./email-testing-account.component.scss'],
})
export class EmailTestingAccountComponent implements OnInit {
  @ViewChild('myckeditor') ckeditor: any;
  uploadInput: EventEmitter<UploadInput>;
  submitted = false;
  showEmail: any = [];
  getEmail: any = [];
  emailform: FormGroup;
  ckeConfig: any;
  options: UploaderOptions;
  files: UploadFile[];
  queryParamName: string;
  queryParamEmail: string;
  fileSize = false;
  showUpload = false;
  fileArray: any[] = [];

  constructor(
    private toastrService: NbToastrService,
    private route: ActivatedRoute,
    protected emailService: EmailAccountsService,
    private formBuilder: FormBuilder,
    protected router: Router
  ) {
    this.route.queryParams.subscribe((params) => {
      this.queryParamName = params.name;
      this.queryParamEmail = localStorage.getItem('UserEmail');
    });
    this.ckeConfig = {
      allowedContent: false,
      extraPlugins: 'divarea',
      forcePasteAsPlainText: true,
      removePlugins: 'about',
      removeButtons: 'Save,NewPage,Print,Preview',
    };

    this.getEmailAccount();
  }

  ngOnInit(): void {
    this.emailform = this.formBuilder.group({
      address: [
        '',
        [
          Validators.required,
          Validators.pattern('^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+.[a-z]{2,4}$'),
        ],
      ],
      name: ['', [Validators.required]],
      subject: ['', [Validators.required]],
      body: [''],
      cc: [
        '',
        [Validators.pattern('^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+.[a-z]{2,4}$')],
      ],
      bcc: [
        '',
        [Validators.pattern('^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+.[a-z]{2,4}$')],
      ],
      isBodyHtml: [true],
    });

    this.emailform.patchValue({
      name: this.queryParamName,
      address: this.queryParamEmail,
    });
  }

  onUploadOutput(output: UploadOutput): void {
    switch (output.type) {
      case 'addedToQueue':
        if (typeof output.file !== 'undefined') {
          if (!output.file.size) this.fileSize = true;
          else this.fileSize = false;
          if (!this.fileSize) {
            this.fileArray.push(output);
          }
        }
    }
  }

  getEmailAccount() {
    this.emailService.getAllEmailforfilter().subscribe((data: any) => {
      this.getEmail = data;
    });
  }
  onChange($event: any): void {
    console.log('onChange');
  }

  onPaste($event: any): void {
    console.log('onPaste');
  }

  get f() {
    return this.emailform.controls;
  }

  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], {
      queryParams: {
        PageName: 'email',
        id: this.showEmail.id,
      },
    });
  }

  onSubmit() {
    console.log('email', this.emailform.value);
    this.submitted = true;
    const formData = new FormData();
    let obj = {
      to: [
        {
          name: this.emailform.value.address,
          address: this.emailform.value.address,
        },
      ],
      cc: [
        {
          name: this.emailform.value.cc,
          address: this.emailform.value.cc,
        },
      ],
      bcc: [
        {
          name: this.emailform.value.bcc,
          address: this.emailform.value.bcc,
        },
      ],
      subject: this.emailform.value.subject,
      body: this.emailform.value.body,
      isBodyHtml: this.emailform.value.isBodyHtml,
    };

    if (this.fileArray.length) {
      for (let data of this.fileArray) {
        formData.append('Files', data.file.nativeFile, data.file.name);
      }
      formData.append('EmailMessageJson', JSON.stringify(obj));
    }

    if (this.fileArray.length == 0) {
      formData.append('EmailMessageJson', JSON.stringify(obj));
    }

    this.emailService.SendEmail(this.emailform.value.name, formData).subscribe(
      () => {
        this.toastrService.success('Email Send successfully.', 'Success');
        this.router.navigate(['pages/emailaccount/list']);
        this.submitted = false;
      },
      () => (this.submitted = false)
    );
  }
}
