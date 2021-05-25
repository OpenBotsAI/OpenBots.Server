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
  urlId;

  constructor(
    private acroute: ActivatedRoute,
    protected elogService: EmailLogService,
    private formBuilder: FormBuilder,
    protected router: Router
  ) {

    this.urlId = this.acroute.snapshot.params['id'];
    this.getallemail(this.urlId);

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

  getallemail(id) {
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
    console.log()
    this.router.navigate([`/pages/change-log/list/${'Configuration.Email'}/${this.urlId}`]);

  }
}
