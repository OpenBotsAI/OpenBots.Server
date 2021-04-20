import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { BusinessEventService } from '../business-event.service';
import { TimeDatePipe } from '../../../@core/pipe';
import { HttpResponse } from '@angular/common/http';
@Component({
  selector: 'ngx-view-business-event',
  templateUrl: './view-business-event.component.html',
  styleUrls: ['./view-business-event.component.scss']
})
export class ViewBusinessEventComponent implements OnInit {

  submitted = false;
  createdOn: any = [];
  showallsystemEvent: any = [];
  payloadSchema: any = [];
  businessEventform: FormGroup;
  showChangedToJson: boolean = false;
  pipe: TimeDatePipe;
  UrlID;
  jsonValue: any = [];

  constructor(
    private acroute: ActivatedRoute,
    private formBuilder: FormBuilder,
    protected router: Router,
    protected BusinessEventservice: BusinessEventService,
  ) {

    this.UrlID = this.acroute.snapshot.params['id']
    this.getBusinessEventID(this.UrlID)
  }

  ngOnInit(): void {
    this.businessEventform = this.formBuilder.group({
      name: [''],
      description: [''],
      entityType: [''],
      payloadSchema: ['']
    });
  }




  getBusinessEventID(id) {
    this.BusinessEventservice.getSystemEventid(id).subscribe((data: HttpResponse<any>) => {
      this.showallsystemEvent = data.body;
      this.businessEventform.patchValue(data.body);
      this.businessEventform.disable();
      if (this.showallsystemEvent.payloadSchema) {
        this.jsonValue = this.showallsystemEvent.payloadSchema;
        this.jsonValue = JSON.parse(this.jsonValue);
      }
    });
  }
}