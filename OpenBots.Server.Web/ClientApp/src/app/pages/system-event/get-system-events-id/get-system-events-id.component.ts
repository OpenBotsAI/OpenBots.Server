import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TimeDatePipe } from '../../../@core/pipe';
import { SystemEventService } from '../system-event.service';

@Component({
  selector: 'ngx-get-system-events-id',
  templateUrl: './get-system-events-id.component.html',
  styleUrls: ['./get-system-events-id.component.scss'],
})
export class GetSystemEventsIdComponent implements OnInit {
  createdOn: any = [];
  showallsystemEvent: any = [];
  payloadSchema: any = [];
  systemEventform: FormGroup;
  showChangedToJson: boolean = false;
  showpayloadSchemaJson: boolean = false;
  pipe: TimeDatePipe;

  constructor(
    private acroute: ActivatedRoute,
    private formBuilder: FormBuilder,
    protected router: Router,
    protected systemEventService: SystemEventService
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

      if (data.payloadSchema != null) {
        this.showpayloadSchemaJson = true;
        this.payloadSchema = data.payloadSchema;
        this.payloadSchema = JSON.parse(this.payloadSchema);
      }
      
    });
  }

  transformDate(value, format) {
    this.pipe = new TimeDatePipe();
    return this.pipe.transform(value, `${format}`);
  }
}
