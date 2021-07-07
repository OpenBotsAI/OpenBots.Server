import { HttpResponse } from '@angular/common/http';
import { Component, EventEmitter, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NbToastrService } from '@nebular/theme';
import { SubscriptionService } from '../subscription.service';

@Component({
  selector: 'ngx-add-subscription',
  templateUrl: './add-subscription.component.html',
  styleUrls: ['./add-subscription.component.scss'],
})
export class AddSubscriptionComponent implements OnInit {
  showTabview: boolean = true;
  show_filter_entity: any = [];
  show_filter_event: any = [];
  showQueues: any = [];
  subscriptionForm: FormGroup;
  submitted = false;
  filterValue: any;
  EntityFilterValue: any = [];
  transportType: string[] = ['HTTPS', 'Queue'];
  etag;
  showAutomation: any = [];
  title = 'Add';
  urlId: string;
  getSubscription: any = [];
  getbusiness = false;
  getEntityShow = true;

  constructor(
    private formBuilder: FormBuilder,
    private toastrService: NbToastrService,
    protected router: Router,
    private route: ActivatedRoute,
    protected SubscriptionService: SubscriptionService
  ) {
    this.getQueueAndEntity();
    this.urlId = this.route.snapshot.params['id'];
    if (this.urlId) {
      this.getSubscriptionbyID(this.urlId);
      this.title = 'Update';
    }
  }

  ngOnInit(): void {
    this.subscriptionForm = this.formBuilder.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
          Validators.pattern('^[A-Za-z0-9_.-]{3,100}$'),
        ],
      ],
      entityType: [''],
      integrationEventName: [''],
      entityID: [''],
      entityName: this.urlId ? [''] : ['', [Validators.required]],
      transportType: ['', [Validators.required]],
      httP_URL: [''],
      httP_AddHeader_Key: [''],
      httP_AddHeader_Value: [''],
      Max_RetryCount: [''],
      queuE_QueueID: [''],
      getbusines: false
    });
    //  this.subscriptionForm.get('state').reset();
    this.subscriptionForm.get('integrationEventName').disable();
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




  getSubscriptionbyID(id) {
    this.SubscriptionService.getsubscribeID(id).subscribe(
      (data: HttpResponse<any>) => {
        this.getSubscription = data.body;

        this.etag = data.headers.get('ETag').replace(/\"/g, '');
        if (this.getSubscription.entityName == "" || this.getSubscription.entityName == '') {
          console.log('yes')
          this.getbusiness = false;
          this.subscriptionForm.get('integrationEventName').enable();
          this.check(true)
          this.subscriptionForm.patchValue({ getbusines: true });
        }
        else {
          this.getEntityName(this.getSubscription.entityName);
        }

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


  check(e) {
    console.log(e)
    if (e == true) {
      this.getEntityShow = false;
      this.subscriptionForm.get('entityName').clearValidators();
      this.subscriptionForm.get('entityName').updateValueAndValidity();
      this.SubscriptionService.getIntegrationEventName().subscribe((data: any) => {
        console.log(data)
        this.subscriptionForm.get('integrationEventName').enable();
        this.getbusiness = true;
        this.EntityFilterValue = data.entityNameList;
      })

    } else if (e == false) {
      this.subscriptionForm.get('entityName').setValidators([Validators.required]);
      this.subscriptionForm.get('entityName').updateValueAndValidity();
      this.getbusiness = false;
      this.EntityFilterValue = [];
      this.getEntityShow = true;
      this.subscriptionForm.get('integrationEventName').disable();
    }

  }

  getEntityName(e) {

    if (this.urlId) {
      if (e == this.getSubscription.entityName) {
        this.filterValue = e;
        this.SubscriptionService.filterIntegrationEventName(
          `entityType+eq+'${this.filterValue}'`
        ).subscribe((data: any) => {
          this.EntityFilterValue = data.items;
          this.subscriptionForm.get('integrationEventName').enable();
        });
      } else if (e !== this.getSubscription.entityName) {
        this.filterValue = e.target.value;
        this.SubscriptionService.filterIntegrationEventName(
          `entityType+eq+'${this.filterValue}'`
        ).subscribe((data: any) => {
          this.EntityFilterValue = data.items;
          this.subscriptionForm.get('integrationEventName').enable();
        });
      }
    } else {
      this.filterValue = e.target.value;
      this.SubscriptionService.filterIntegrationEventName(
        `entityType+eq+'${this.filterValue}'`
      ).subscribe((data: any) => {
        this.EntityFilterValue = data.items;
        this.subscriptionForm.get('integrationEventName').enable();
      });
    }
  }

  onSubmit() {
    if (this.urlId) {
      this.UpdateSubscription();
    } else {
      this.AddSubscription();
    }
  }

  AddSubscription() {
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

  UpdateSubscription() {
    // this.submitted = true;
    this.SubscriptionService.updateSubscription(
      this.subscriptionForm.value,
      this.urlId,
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
    } else if (val == 'Queue') {
      this.showTabview = false;
    }
  }
}
