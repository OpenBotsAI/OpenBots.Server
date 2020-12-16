import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TimeDatePipe } from '../../../@core/pipe';
import { IntegrationLogsService } from '../integration-logs.service';

@Component({
  selector: 'ngx-get-integration-logs-id',
  templateUrl: './get-integration-logs-id.component.html',
  styleUrls: ['./get-integration-logs-id.component.scss'],
})
export class GetIntegrationLogsIdComponent implements OnInit {
  createdOn: any = [];
  showallsystemEvent: any = [];
  showallpayload: any = [];
  payloadJSON: any = [];
  systemEventform: FormGroup;
  showChangedToJson: boolean = false;
  showpayloadSchemaJson: boolean = false;
  pipe: TimeDatePipe;
  showAttempt: boolean;
  constructor(
    private acroute: ActivatedRoute,
    private formBuilder: FormBuilder,
    protected router: Router,
    protected systemEventService: IntegrationLogsService
  ) {
    this.acroute.queryParams.subscribe((params) => {
      this.get_allagent(params.id);
      this.getintegrationEventlogpayload(params.id);
    });
  }

  ngOnInit(): void {
    this.systemEventform = this.formBuilder.group({
      createdBy: [''],
      createdOn: [''],
      deleteOn: [''],
      deletedBy: [''],
      entityID: [''],
      entityType: [''],
      id: [''],
      integrationEventName: [''],
      isDeleted: [''],
      message: [''],
      occuredOnUTC: [''],
      payloadJSON: [''],
      status: [''],
      timestamp: [''],
      updatedBy: [''],
      updatedOn: [''],
    });
  }

  get_allagent(id) {
    this.systemEventService.getSystemEventid(id).subscribe((data: any) => {
      this.showallsystemEvent = data;
      data.occuredOnUTC = this.transformDate(data.occuredOnUTC, 'lll');
      this.systemEventform.patchValue(data);
      this.systemEventform.disable();

      if (data.payloadJSON != null) {
        this.showpayloadSchemaJson = true;
        this.payloadJSON = data.payloadJSON;
        this.payloadJSON = JSON.parse(this.payloadJSON);
      }
    });
  }
  getintegrationEventlogpayload(id) {
    this.systemEventService
      .getIntegrationEventlogsPayload(`eventLogID+eq+guid'${id}'`)
      .subscribe((data: any) => {
        if (data.items.length == 0) {
          this.showAttempt = false;
        } else if (data.items.length !== 0) {
          this.showAttempt = true;
          this.showallpayload = data.items;
        }
       
      });
  }
  transformDate(value, format) {
    this.pipe = new TimeDatePipe();
    return this.pipe.transform(value, `${format}`);
  }
}
