import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TimeDatePipe } from '../../../@core/pipe';
import { ChangelogService } from '../change-log.service';


@Component({
  selector: 'ngx-get-audit-id',
  templateUrl: './get-change-log-id.component.html',
  styleUrls: ['./get-change-log-id.component.scss']
})
export class GetChangelogIdComponent implements OnInit {
  createdOn: any = []
  show_allaudit: any = []
  changedToJson: any = [];
  changedFromJson: any = [];
  changelogform: FormGroup;
  showChangedToJson: boolean = false;
  showChangedFromJson: boolean = false;
  pipe: TimeDatePipe;
  parmasServiceName: any = []
  constructor(private acroute: ActivatedRoute, private formBuilder: FormBuilder, protected router: Router,
    protected changelogService: ChangelogService) {
    this.acroute.queryParams.subscribe(params => {
      this.parmasServiceName = params.serviceName;
      this.get_allagent(params.id)

    });
  }

  ngOnInit(): void {
    this.changelogform = this.formBuilder.group({
      changedFromJson: [''],
      changedToJson: [''],
      createdBy: [''],
      objectId: [''],
      createdOn: [''],
      deleteOn: [''],
      deletedBy: [''],
      exceptionJson: [''],
      id: [''],
      isDeleted: [''],
      methodName: [''],
      parametersJson: [''],
      serviceName: [''],
      timestamp: [''],
      updatedBy: [''],
      updatedOn: [''],
    });
  }

  get_allagent(id) {

    this.changelogService.get_Audit_id(id).subscribe(
      (data: any) => {
        this.show_allaudit = data;
        this.show_allaudit.serviceName = this.parmasServiceName;
        data.createdOn = this.transformDate(data.createdOn, 'lll');
        this.changelogform.patchValue(data)
        this.changelogform.disable();

        if (data.changedFromJson != null) {
          this.showChangedFromJson = true;
          this.changedFromJson = data.changedFromJson;
          this.changedFromJson = JSON.parse(this.changedFromJson)
        }
        if (data.changedToJson != null) {
          this.showChangedToJson = true;
          this.changedToJson = data.changedToJson;
          this.changedToJson = JSON.parse(this.changedToJson)
        }




      });
  }

  goto() {
    // this.show_allaudit.serviceName 
    if (this.show_allaudit.serviceName == 'AgentModel') {
      this.router.navigate(['/pages/agents/get-agents-id'], { queryParams: { id: this.show_allaudit.objectId } })
    }
    else if (this.show_allaudit.serviceName == 'Schedule') {
      this.router.navigate([`/pages/schedules/view/${this.show_allaudit.objectId}`])
    }
    else if (this.show_allaudit.serviceName == 'Asset') {
      this.router.navigate(['/pages/asset/get-asset-id'], { queryParams: { id: this.show_allaudit.objectId } })
    }
    else if (this.show_allaudit.serviceName == 'Automation') {
      this.router.navigate(['/pages/automation/get-automation-id'], {
        queryParams: { id: this.show_allaudit.objectId },
      });
    }
    else if (this.show_allaudit.serviceName == 'Job') {
      this.router.navigate(['/pages/job/get-jobs-id'], { queryParams: { id: this.show_allaudit.objectId } })
    }
    else if (this.show_allaudit.serviceName == 'QueueItem') {
      this.router.navigate([`/pages/queueitems/view/${this.show_allaudit.objectId}`])
    }
    else if (this.show_allaudit.serviceName == 'Credential') {
      this.router.navigate([`/pages/credentials/view/${this.show_allaudit.objectId}`])
    }
    else if (this.show_allaudit.serviceName == 'Files') {
      this.router.navigate([`/pages/file/get-file-id/${this.show_allaudit.objectId}`])
    }
    else if (this.show_allaudit.serviceName == 'Configuration.ConfigurationValue') {
      this.router.navigate(['/pages/config/get-config-id'], { queryParams: { id: this.show_allaudit.objectId } })
    }
    else if (this.show_allaudit.serviceName == 'Configuration.EmailAccount') {
      this.router.navigate(['/pages/emailaccount/get-email-id'], { queryParams: { id: this.show_allaudit.objectId } })
    }
    else if (this.show_allaudit.serviceName == 'Configuration.Email') {
      this.router.navigate(['/pages/emaillog/get-emaillog-id'], { queryParams: { id: this.show_allaudit.objectId } })
    }






  }


  transformDate(value, format) {
    this.pipe = new TimeDatePipe();
    return this.pipe.transform(value, `${format}`);
  }

}
