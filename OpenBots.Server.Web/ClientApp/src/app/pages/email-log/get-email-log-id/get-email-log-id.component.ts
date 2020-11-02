import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TimeDatePipe } from '../../../@core/pipe';
import { EmailLogService } from '../email-log.service';

@Component({
  selector: 'ngx-get-email-log-id',
  templateUrl: './get-email-log-id.component.html',
  styleUrls: ['./get-email-log-id.component.scss'],
})
export class GetEmailLogIdComponent implements OnInit {
  jsonValue: any = [];
  showEmail: any = [];
  emailform: FormGroup;
  pipe = new DatePipe('en-US');
  now = Date();
  show_createdon: any = [];

  constructor(
    private acroute: ActivatedRoute,
    protected elogService: EmailLogService,
    private formBuilder: FormBuilder,
    protected router: Router
  ) {
    this.acroute.queryParams.subscribe((params) => {
      this.get_allagent(params.id);
    });
  }

  ngOnInit(): void {
    this.emailform = this.formBuilder.group({
      createdBy: [''],
      createdOn: [''],
      deleteOn: [''],
      deletedBy: [''],
      emailAccountId: [''],
      emailObjectJson: [''],
      id: [''],
      isDeleted: [''],
      reason: [''],
      senderAddress: [''],
      senderUserId: [''],
      sentOnUTC: [''],
      status: [''],
      timestamp: [''],
      updatedBy: [''],
      updatedOn: [''],
      senderName: [''],
    });
  }

  get_allagent(id) {
    this.elogService.getEmailbyId(id).subscribe((data: any) => {
      this.showEmail = data;

      const filterPipe = new TimeDatePipe();
      const fiteredArr = filterPipe.transform(data.createdOn, 'lll');
      data.createdOn = filterPipe.transform(data.createdOn, 'lll');
      data.sentOnUTC = filterPipe.transform(data.createdOn, 'lll');
      this.emailform.patchValue(data);
      this.emailform.disable();
    });
  }
  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], { queryParams: { PageName: 'OpenBots.Server.Model.SystemConfiguration.EmailAccount', id: this.showEmail.id } })
  }
}
