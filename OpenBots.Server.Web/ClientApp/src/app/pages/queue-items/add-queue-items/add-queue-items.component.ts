import {
  Component,
  EventEmitter,
  OnInit,
  TemplateRef,
  ViewChild,
} from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpService } from '../../../@core/services/http.service';
import { ActivatedRoute, Router } from '@angular/router';
import { QueueItem } from '../../../interfaces/queueItem';
import { Queues } from '../../../interfaces/queues';
import { JsonEditorComponent, JsonEditorOptions } from 'ang-jsoneditor';
import { NbDateService } from '@nebular/theme';
import { HelperService } from '../../../@core/services/helper.service';
import {
  UploadOutput,
  UploadInput,
  UploadFile,
  UploaderOptions,
} from 'ngx-uploader';
import { BinaryFile } from '../../../interfaces/file';
import { FileSaverService } from 'ngx-filesaver';
import {
  FilesApiUrl,
  QueueItemsApiUrl,
  QueuesApiUrls,
} from '../../../webApiUrls';
import { DialogService } from '../../../@core/dialogservices';
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
  eTag: string;
  options: UploaderOptions;
  files: UploadFile[];
  uploadInput: EventEmitter<UploadInput>;
  fileSize = false;
  showUpload = false;
  fileArray: any[] = [];
  singleFile: any;
  myFiles: UploadFile[] = [];
  queueItemFiles: BinaryFile[] = [];
  queuefiles: string[] = [];
  isDeleted = false;
  deleteId: string;
  state: { name: string; value: number }[] = [
    { name: 'New', value: 0 },
    // { name: 'InProgress', value: 1 },
    { name: 'Failed', value: 2 },
    { name: 'Success', value: 3 },
    { name: 'Expired', value: 4 },
  ];
  @ViewChild(JsonEditorComponent) editor: JsonEditorComponent;

  constructor(
    private fb: FormBuilder,
    private httpService: HttpService,
    private route: ActivatedRoute,
    private router: Router,
    private dateService: NbDateService<Date>,
    private helperService: HelperService,
    private fileSaverService: FileSaverService,
    private dialogService: DialogService
  ) {}

  ngOnInit(): void {
    this.min = new Date();
    this.max = new Date();
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
    } else this.queueItemForm.get('state').disable();
    this.min = this.dateService.addMonth(this.dateService.today(), 0);
    this.max = this.dateService.addMonth(this.dateService.today(), 1);
  }

  initializigQueueItemForm() {
    return this.fb.group({
      id: [''],
      // organizationId: localStorage.getItem('ActiveOrganizationID'),
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
    if (this.queueItemForm.value.expireOnUTC) {
      this.queueItemForm.value.expireOnUTC = this.helperService.transformDate(
        this.queueItemForm.value.expireOnUTC,
        'lll'
      );
    }
    if (this.queueItemForm.value.postponeUntilUTC) {
      this.queueItemForm.value.postponeUntilUTC = this.helperService.transformDate(
        this.queueItemForm.value.postponeUntilUTC,
        'lll'
      );
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
      .get(
        `${QueueItemsApiUrl.QueueItems}/${QueueItemsApiUrl.view}/${this.queueItemId}`,
        { observe: 'response' }
      )
      .subscribe((response) => {
        if (response && response.status === 200) {
          this.min = response.body.expireOnUTC;
          this.eTag = response.headers.get('etag');
          this.queuefiles = response.body.binaryObjectIds;
          if (this.queuefiles) this.getQueueItemFiles();
          if (response.body.type.toLowerCase() === 'json')
            response.body.dataJson = JSON.parse(response.body.dataJson);
          this.queueItemForm.patchValue(response.body);
          this.oldQueueFormValue = this.queueItemForm.value;
        }
      });
  }

  addItem(): void {
    delete this.queueItemForm.value.id;
    this.httpService
      .post(
        `${QueueItemsApiUrl.QueueItems}/${QueueItemsApiUrl.enqueue}`,
        this.queueItemForm.value
      )
      .subscribe(
        (response) => {
          if (response && response.id) {
            let count = 0;
            if (this.fileArray.length) {
              const formData = new FormData();
              for (let data of this.fileArray) {
                formData.append('files', data.file.nativeFile, data.file.name);
              }
              this.httpService
                .post(
                  `${QueueItemsApiUrl.QueueItems}/${response.id}/${QueueItemsApiUrl.queueitemattachments}`,
                  formData,
                  { observe: 'response' }
                )
                .subscribe((response) => {
                  if (response && response.status === 200)
                    this.httpService.success('Queue item created successfully');
                  this.navigateToQueueItemsList();
                  this.isSubmitted = false;
                  this.queueItemForm.reset();
                });
              // for (let data of this.fileArray) {
              //   formData.append('files', data.file.nativeFile, data.file.name);
              //   this.httpService
              //     .post(
              //       `${QueueItemsApiUrl.QueueItems}/${response.id}/${QueueItemsApiUrl.queueitemattachments}`,
              //       formData
              //     )
              //     .subscribe(
              //       (response) => {
              //         if (response) {
              //           count++;
              //           if (count == this.fileArray.length) {
              //             this.httpService.success(
              //               'Queue item created successfully'
              //             );
              //             this.navigateToQueueItemsList();
              //             this.isSubmitted = false;
              //             this.queueItemForm.reset();
              //           }
              //         }
              //       },
              //       () => (this.isSubmitted = false)
              //     );
              // }
            } else {
              this.navigateToQueueItemsList();
              this.isSubmitted = false;
              this.queueItemForm.reset();
            }
          }
        },
        () => (this.isSubmitted = false)
      );
  }

  updateItem(): void {
    const headers = this.helperService.getETagHeaders(this.eTag);
    if (this.fileArray.length) {
      const formData = new FormData();
      formData.append('Name', this.queueItemForm.value.name);
      formData.append('QueueId', this.queueItemForm.value.queueId);
      formData.append('Source', this.queueItemForm.value.source);
      formData.append('Event', this.queueItemForm.value.event);
      if (this.queueItemForm.value.expireOnUTC)
        formData.append('ExpireOnUTC', this.queueItemForm.value.expireOnUTC);
      if (this.queueItemForm.value.postponeUntilUTC)
        formData.append(
          'PostponeUntilUTC',
          this.queueItemForm.value.postponeUntilUTC
        );
      formData.append('Type', this.queueItemForm.value.type);
      formData.append('DataJson', this.queueItemForm.value.dataJson);
      formData.append('State', this.queueItemForm.value.state);

      for (let data of this.fileArray) {
        formData.append('Files', data.file.nativeFile, data.file.name);
      }
      this.httpService
        .put(`${QueueItemsApiUrl.QueueItems}/${this.queueItemId}`, formData, {
          headers,
          observe: 'response',
        })
        .subscribe(
          (response) => {
            if (response && response.status === 200) {
              this.httpService.success('Queue item updated successfully');
              this.navigateToQueueItemsList();
              this.isSubmitted = false;
              this.queueItemForm.reset();
            }
          },
          () => (this.isSubmitted = false)
        );
    } else {
      let data = [];
      for (let [oldKey, oldValue] of Object.entries(this.oldQueueFormValue)) {
        for (let [newkey, newValue] of Object.entries(
          this.queueItemForm.value
        )) {
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
      this.httpService
        .patch(`${QueueItemsApiUrl.QueueItems}/${this.queueItemId}`, data, {
          headers,
          observe: 'response',
        })
        .subscribe(
          (res) => {
            if (res && res.status === 200) {
              this.httpService.success('Queue item updated successfully');
              this.navigateToQueueItemsList();
              this.isSubmitted = false;
              this.queueItemForm.reset();
            }
          },
          (error) => {
            this.isSubmitted = false;
            if (error && error.error && error.error.status === 409) {
              this.httpService.error(error.error.serviceErrors);
              this.getQueueDataById();
            }
          }
        );
    }
  }

  navigateToQueueItemsList(): void {
    this.router.navigate(['pages/queueitems']);
  }

  getQueues(): void {
    const url = `${QueuesApiUrls.Queues}?$orderby=createdOn+desc`;
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

  onUploadOutput(output: UploadOutput): void {
    switch (output.type) {
      case 'addedToQueue':
        if (typeof output.file !== 'undefined') {
          if (!output.file.size) this.fileSize = true;
          else this.fileSize = false;
          if (!this.fileSize) {
            this.fileArray.push(output);
          }
        }
    }
  }

  getQueueItemFiles(): void {
    // for (let binaryObjectId of this.queuefiles) {
    this.httpService
      // .get(`${FilesApiUrl.BinaryObjects}/${binaryObjectId}`)
      .get(
        `${QueueItemsApiUrl.QueueItems}/${this.queueItemId}/${QueueItemsApiUrl.queueitemattachments}/${QueueItemsApiUrl.view}`
      )
      .subscribe((response) => {
        if (response && response.items && response.items.length)
          this.queueItemFiles = [...response.items];
        // this.queueItemFiles.push(response);
      });
    // }
  }

  downloadFile(id: string): void {
    this.httpService
      .get(`${FilesApiUrl.BinaryObjects}/${id}/${FilesApiUrl.download}`, {
        responseType: 'blob',
        observe: 'response',
      })
      .subscribe((response) => {
        this.fileSaverService.save(
          response.body,
          response.headers
            .get('content-disposition')
            .split(';')[1]
            .split('=')[1]
            .replace(/\"/g, '')
        );
      });
  }

  openDeleteDialog(ref: TemplateRef<any>, id: string): void {
    this.deleteId = id;
    this.dialogService.openDialog(ref);
  }

  deleteFiles(ref): void {
    this.isDeleted = true;
    this.httpService
      .delete(
        `${QueueItemsApiUrl.QueueItems}/${this.queueItemId}/${QueueItemsApiUrl.queueitemattachments}/${this.deleteId}`,
        { observe: 'response' }
      )
      .subscribe(
        (response) => {
          if (response && response.status === 200) {
            ref.close();
            let index = this.queueItemFiles.findIndex(
              (file) => file.id === this.deleteId
            );
            if (index > -1) {
              this.queueItemFiles.splice(index, 1);
            }
            this.httpService.success('Deleted Successfully');
            this.isDeleted = false;
          }
        },
        () => (this.isDeleted = false)
      );
  }
}
