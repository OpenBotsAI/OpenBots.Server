import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpService } from '../../../@core/services/http.service';
import { ActivatedRoute, Router } from '@angular/router';
import { QueueItem } from '../../../interfaces/queueItem';
import { Queues } from '../../../interfaces/queues';
import { JsonEditorComponent, JsonEditorOptions } from 'ang-jsoneditor';
import { NbDateService } from '@nebular/theme';
@Component({
  selector: 'ngx-add-queue-items',
  templateUrl: './add-queue-items.component.html',
  styleUrls: ['./add-queue-items.component.scss'],
})
export class AddQueueItemsComponent implements OnInit {
  queryParamId: string;
  queueItemForm: FormGroup;
  queueItemsType = ['Json', 'Text'];
  queueItemId: string;
  dataGetById: QueueItem;
  title = 'Add';
  btnText = 'Save';
  isSubmitted = false;
  queuesArr: Queues[] = [];
  oldQueueFormValue: FormGroup;
  jsonValidity: boolean;
  min: Date;
  max: Date;
  public editorOptions: JsonEditorOptions;
  public data: any;
  @ViewChild(JsonEditorComponent) editor: JsonEditorComponent;

  constructor(
    private fb: FormBuilder,
    private httpService: HttpService,
    private route: ActivatedRoute,
    private router: Router,
    private dateService: NbDateService<Date>
  ) {}

  ngOnInit(): void {
    this.queueItemForm = this.initializigQueueItemForm();
    this.getQueues();
    this.editorOptions = new JsonEditorOptions();
    this.editorOptions.modes = ['code', 'text', 'tree', 'view'];
    this.queryParamId = this.route.snapshot.queryParams['id']; 
    this.queueItemId = this.route.snapshot.params['id'];
    if (this.queueItemId) {
      this.getQueueDataById();
      this.title = 'Update';
      this.btnText = 'Update';
    }
    this.min = new Date();
    this.max = new Date();
    this.min = this.dateService.addMonth(this.dateService.today(), 0);
    this.max = this.dateService.addMonth(this.dateService.today(), 1);
  }

  initializigQueueItemForm() {
    return this.fb.group({
      id: [''],
      organizationId: localStorage.getItem('ActiveOrganizationID'),
      processID: null,
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
        ],
      ],
      dataJson: [''], 
      queueId: ['', [Validators.required]],
      state: ['New'],
      type: ['', [Validators.required]],
      jsonType: [''], 
      expireOnUTC: [''],
      postponeUntilUTC: [''],
      source: ['', [Validators.minLength(3), Validators.maxLength(100)]],
      event: ['', [Validators.minLength(3), Validators.maxLength(100)]],
    });
  }
  get controls() {
    return this.queueItemForm.controls;
  }

  addQueueItem(): void {
    this.isSubmitted = true;
    if (this.queueItemForm.value.type === 'Json') {
      if (!this.editor.isValidJson()) {
        this.httpService.error('Provided json is not valid');
        this.isSubmitted = false;
      }
      this.queueItemForm.value.dataJson = JSON.stringify(this.editor.get());
    }
    if (this.queueItemId) this.updateItem();
    else this.addItem();
  }


  onQueueItemchange(): void {
    this.queueItemForm.get('dataJson').reset();
    this.queueItemForm.get('jsonType').reset();
    if (this.queueItemForm && this.queueItemForm.value.type == 'Json') {
      this.queueItemForm.get('dataJson').clearValidators();
      this.queueItemForm.get('dataJson').updateValueAndValidity();
      this.queueItemForm.get('jsonType').setValidators([Validators.required]);
      this.queueItemForm.get('jsonType').updateValueAndValidity();
    } else if (this.queueItemForm && this.queueItemForm.value.type == 'Text') {
      this.queueItemForm.get('jsonType').clearValidators();
      this.queueItemForm.get('jsonType').updateValueAndValidity();
      this.queueItemForm.get('dataJson').setValidators([Validators.required]);
      this.queueItemForm.get('dataJson').updateValueAndValidity();
    }
  }

  getQueueDataById(): void {
    this.httpService
      .get(`QueueItems/${this.queueItemId}`)
      .subscribe((response: QueueItem) => {
        if (response && response.type === 'Json')
          response.dataJson = JSON.parse(response.dataJson);
        this.queueItemForm.patchValue(response);
        this.oldQueueFormValue = this.queueItemForm.value;
      });
  }

  addItem(): void {
    this.httpService
      .post('QueueItems/Enqueue', this.queueItemForm.value)
      .subscribe(
        () => {
          this.httpService.success('Queue item created successfully');
          this.navigateToQueueItemsList();
          this.isSubmitted = false;
          this.queueItemForm.reset();
        },
        () => (this.isSubmitted = false)
      );
  }

  updateItem(): void {
    let data = [];
    for (let [oldKey, oldValue] of Object.entries(this.oldQueueFormValue)) {
      for (let [newkey, newValue] of Object.entries(this.queueItemForm.value)) {
        if (oldKey == newkey && oldValue != newValue) {
          const obj = {
            value: newValue,
            path: newkey,
            op: 'replace',
          };
          data.push(obj);
        }
      }
    }
    this.httpService.patch(`QueueItems/${this.queueItemId}`, data).subscribe(
      () => {
        this.httpService.success('Queue item updated successfully');
        this.navigateToQueueItemsList();
        this.isSubmitted = false;
        this.queueItemForm.reset();
      },
      () => (this.isSubmitted = false)
    );
  }

  navigateToQueueItemsList(): void {
    this.router.navigate(['pages/queueitems']);
  }

  getQueues(): void {
    const url = `Queues?$orderby=createdOn+desc`;
    this.httpService.get(url).subscribe((response) => {
      if (response && response.items.length !== 0)
        this.queuesArr = response.items;
      else this.queuesArr = [];
      if (this.queryParamId) {
        this.queueItemForm.patchValue({
          queueId: this.queryParamId,
        });
      } else {
        this.queueItemForm.patchValue({
          queueId: this.queuesArr[0].id,
        });
      }
    });
  }
}
