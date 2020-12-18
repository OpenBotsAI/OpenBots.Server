import { Component, EventEmitter, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
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
  transportType: string[] = ['HTTPS', 'Queue'];
  constructor(
    private formBuilder: FormBuilder,
    private toastrService: NbToastrService,
    protected router: Router,
    protected SubscriptionService: SubscriptionService
  ) {   
     this.getallEntity();
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
      integrationEventName: ['', [Validators.required]],
      entityID: ['', [Validators.required]],
      entityName: ['', [Validators.required]],
      transportType: ['', [Validators.required]],
      httP_URL: [''],
      httP_AddHeader_Key: [''],
      httP_AddHeader_Value: [''],
      httP_Max_RetryCount: [''],
      queuE_QueueID: [''],
    });
  }

  get f() {
    return this.subscriptionForm.controls;
  }
  getallEntity() {
    this.SubscriptionService.get_EntityName().subscribe((data: any) => {
      this.show_filter_entity = data.integrationEntityTypeList;
      this.show_filter_event = data.integrationEventNameList;
    });
    this.SubscriptionService.getQueues().subscribe((data: any) => {
      this.showQueues = data.items;
    });
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
