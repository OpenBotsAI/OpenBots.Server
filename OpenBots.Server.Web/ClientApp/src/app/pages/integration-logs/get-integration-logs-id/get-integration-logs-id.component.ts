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
  // changedToJson: any = [];
  payloadJSON: any = [];
  systemEventform: FormGroup;
  showChangedToJson: boolean = false;
  showpayloadSchemaJson: boolean = false;
  pipe: TimeDatePipe;
  constructor(
    private acroute: ActivatedRoute,
    private formBuilder: FormBuilder,
    protected router: Router,
    protected systemEventService: IntegrationLogsService
  ) {
    this.acroute.queryParams.subscribe((params) => {
      this.get_allagent(params.id);
    });
  }

  ngOnInit(): void {
    this.systemEventform = this.formBuilder.group({
      createdBy: [''],
      createdOn: [''],
      deleteOn: [''],
      deletedBy: [''],
      description: [''],
      entityType: [''],
      id: [''],
      isDeleted: [''],
      isSystem: [''],
      name: [''],
      payloadSchema: [''],
      timestamp: [''],
      updatedBy: [''],
      updatedOn: [''],
    });
  }

  get_allagent(id) {
    this.systemEventService.getSystemEventid(id).subscribe((data: any) => {
      this.showallsystemEvent = data;
      data.createdOn = this.transformDate(data.createdOn, 'lll');
      this.systemEventform.patchValue(data);
      this.systemEventform.disable();

      if (data.payloadJSON != null) {
        this.showpayloadSchemaJson = true;
        this.payloadJSON = data.payloadJSON;
        this.payloadJSON = JSON.parse(this.payloadJSON);
      }
      // if (data.changedToJson != null) {
      //   this.showChangedToJson = true;
      //   this.changedToJson = data.changedToJson;
      //   this.changedToJson = JSON.parse(this.changedToJson);
      // }
    });
  }

  transformDate(value, format) {
    this.pipe = new TimeDatePipe();
    return this.pipe.transform(value, `${format}`);
  }
}
