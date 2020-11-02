import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TimeDatePipe } from '../../../@core/pipe';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { EmailAccountsService } from '../email-accounts.service';

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

  constructor(
    private acroute: ActivatedRoute,
    protected assetService: EmailAccountsService,
    private formBuilder: FormBuilder, protected router: Router,
  ) {
    this.acroute.queryParams.subscribe((params) => {
      this.get_allagent(params.id);
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





  get_allagent(id) {
    this.assetService.getEmailbyId(id).subscribe((data: any) => {
      this.showEmail = data;
      const filterPipe = new TimeDatePipe();
      const fiteredArr = filterPipe.transform(data.createdOn, 'lll');
      data.createdOn = filterPipe.transform(data.createdOn, 'lll');

      this.emailform.patchValue(data);
      this.emailform.disable();
    });
  }
  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], { queryParams: { PageName: 'OpenBots.Server.Model.SystemConfiguration.EmailAccount', id: this.showEmail.id } })
  }
}
