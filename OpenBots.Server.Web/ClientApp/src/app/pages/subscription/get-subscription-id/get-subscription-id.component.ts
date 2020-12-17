import { HttpResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NbToastrService } from '@nebular/theme';
import { SubscriptionService } from '../subscription.service';

@Component({
  selector: 'ngx-get-subscription-id',
  templateUrl: './get-subscription-id.component.html',
  styleUrls: ['./get-subscription-id.component.scss'],
})
export class GetSubscriptionIdComponent implements OnInit {
  showTabview: boolean = true;
  show_filter_entity: any = [];
  show_filter_event: any = [];
  showQueues: any = [];
  subscriptionForm: FormGroup;
  submitted = false;
  automationSelection: string[] = ['HTTPS', 'Queue'];
  subscribeId: any = [];
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
      httP_Max_RetryCount: [''],
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
  getallEntity(id) {
    this.SubscriptionService.getsubscribeID(id).subscribe(
      (data: HttpResponse<any>) => {
        console.log(data);
        if (data.body.queuE_QueueID == null) {
          this.showTabview = true;
        } else if (data.body.queuE_QueueID != null) {
          this.SubscriptionService.getQueues().subscribe((data: any) => {
            this.showQueues = data.items;
          });
          this.showTabview = false;
        }
        if (data.body.transportType == 1) {
          data.body.transportType = 'HTTPS';
        } else if (data.body.transportType == 2) {
          data.body.transportType = 'Queue';
        }

        this.subscriptionForm.patchValue(data.body);
        this.subscriptionForm.disable();
      }
    );
  }
  onSubmit() {
    this.submitted = true;

    this.SubscriptionService.addsubscription(
      this.subscriptionForm.value
    ).subscribe(
      (data: any) => {
        this.toastrService.success(
          'Subscription Add  Successfully!',
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
    } else if (val == 'Queue') {
      this.showTabview = false;
    }
  }
}
