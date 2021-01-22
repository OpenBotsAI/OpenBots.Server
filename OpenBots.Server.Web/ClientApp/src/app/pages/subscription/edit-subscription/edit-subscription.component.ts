import { HttpResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NbToastrService } from '@nebular/theme';
import { SubscriptionService } from '../subscription.service';

@Component({
  selector: 'ngx-edit-subscription',
  templateUrl: './edit-subscription.component.html',
  styleUrls: ['./edit-subscription.component.scss'],
})
export class EditSubscriptionComponent implements OnInit {
  showTabview: boolean = true;
  show_filter_entity: any = [];
  show_filter_event: any = [];
  showQueues: any = [];
  subscriptionForm: FormGroup;
  submitted = false;
  automationSelection: string[] = ['HTTPS', 'Queue'];
  subscribeId: any = [];
  etag;
  constructor(
    private formBuilder: FormBuilder,
    private acroute: ActivatedRoute,
    private toastrService: NbToastrService,
    protected router: Router,
    protected SubscriptionService: SubscriptionService
  ) {
    this.acroute.queryParams.subscribe((params) => {
      this.subscribeId = params.id;
      this.getallEntity(params.id);
    });
    this.getQueueAndEntity();
  }

  ngOnInit(): void {
    this.subscriptionForm = this.formBuilder.group({
      createdBy: [''],
      createdOn: [''],
      deleteOn: [''],
      deletedBy: [''],
      entityID: [''],
      entityName: [''],
      entityType: [''],
      httP_AddHeader_Key: [''],
      httP_AddHeader_Value: [''],
      Max_RetryCount: [''],
      httP_URL: [''],
      id: [''],
      integrationEventName: [''],
      isDeleted: [''],
      name: [''],
      queuE_QueueID: [''],
      timestamp: [''],
      transportType: [''],
      updatedBy: [''],
      updatedOn: [''],
    });
  }

  get f() {
    return this.subscriptionForm.controls;
  }
  getQueueAndEntity() {
    this.SubscriptionService.get_EntityName().subscribe((data: any) => {
      this.show_filter_entity = data.integrationEntityTypeList;
      this.show_filter_event = data.integrationEventNameList;
    });
    this.SubscriptionService.getQueues().subscribe((data: any) => {
      this.showQueues = data.items;
    });
  }
  getallEntity(id) {
    this.SubscriptionService.getsubscribeID(id).subscribe(
      (data: HttpResponse<any>) => {
        this.etag = data.headers.get('ETag').replace(/\"/g, '');

        if (data.body.queuE_QueueID == null) {
          this.showTabview = true;
        } else if (data.body.queuE_QueueID != null) {
          this.showTabview = false;
        }

        if (data.body.transportType == 1) {
          data.body.transportType = 'HTTPS';
        } else if (data.body.transportType == 2) {
          data.body.transportType = 'Queue';
        }
        this.subscriptionForm.patchValue(data.body);
      }
    );
  }
  onSubmit() {
    this.submitted = true;

    this.SubscriptionService.updateSubscription(
      this.subscriptionForm.value,
      this.subscribeId,
      this.etag
    ).subscribe(
      (data: any) => {
        this.toastrService.success(
          'Subscription Update  Successfully!',
          'Success'
        );
        this.router.navigate(['/pages/subscription/list']);
      },
      () => (this.submitted = false)
    );
  }

  showHTTP(val) {
    if (val == 'HTTPS') {
      this.showTabview = true;
      this.subscriptionForm.get('queuE_QueueID').reset();
    } else if (val == 'Queue') {
      this.showTabview = false;
      this.subscriptionForm.get('httP_AddHeader_Key').reset();
      this.subscriptionForm.get('httP_AddHeader_Value').reset();
      this.subscriptionForm.get('Max_RetryCount').reset();
      this.subscriptionForm.get('httP_URL').reset();
    }
  }
}
