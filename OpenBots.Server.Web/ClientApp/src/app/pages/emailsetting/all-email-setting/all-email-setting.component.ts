import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NbToastrService } from '@nebular/theme';
import { TimeDatePipe } from '../../../@core/pipe';
import { EmailLogService } from '../../email-log/email-log.service';
import { EmailsettingService } from '../emailsetting.service';

@Component({
  selector: 'ngx-all-email-setting',
  templateUrl: './all-email-setting.component.html',
  styleUrls: ['./all-email-setting.component.scss'],
})
export class AllEmailSettingComponent implements OnInit {
  showupdate = false;
  showEmail: any = [];
  emailform: FormGroup;
  pipe = new DatePipe('en-US');
  now = Date();
  show_createdon: any = [];
  emailId: any = [];

  constructor(
    protected esettingService: EmailsettingService,
    private toastrService: NbToastrService,
    private formBuilder: FormBuilder,
    protected router: Router
  ) {
    this.get_allagent();
  }

  ngOnInit(): void {
    this.emailform = this.formBuilder.group({
      addBCCAddress: [''],
      addCCAddress: [''],
      addToAddress: [''],
      allowedDomains: [''],
      blockedDomains: [''],
      bodyAddPrefix: [''],
      bodyAddSuffix: [''],
      createdBy: [''],
      createdOn: [''],
      deleteOn: [''],
      deletedBy: [''],
      id: [''],
      isDeleted: [''],
      isEmailDisabled: [''],
      organizationId: [''],
      subjectAddPrefix: [''],
      subjectAddSuffix: [''],
      timestamp: [''],
      updatedBy: [''],
      updatedOn: [''],
    });
  }

  get_allagent() {
    this.esettingService.getEmailbyId().subscribe((data: any) => {
      this.showEmail = data.items[0];
      this.emailId = this.showEmail.id;

      const filterPipe = new TimeDatePipe();

      data.items[0].updatedOn = filterPipe.transform(
        data.items[0].updatedOn,
        'lll'
      );

      this.emailform.patchValue(this.showEmail);
      this.emailform.disable();
    });
  }

  formEnable() {
    this.emailform.enable();
    this.showupdate = true;
  }
  onSubmit() {
    this.esettingService
      .editEmail(this.emailId, this.emailform.value)
      .subscribe((data) => {
        this.toastrService.success('Updated successfully', 'Success');
        this.showupdate = false;
        this.emailform.disable();
      });
  }
}
