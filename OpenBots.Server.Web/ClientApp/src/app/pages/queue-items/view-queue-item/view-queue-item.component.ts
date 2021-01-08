import { Component, OnInit, ViewChild } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { JsonEditorComponent, JsonEditorOptions } from 'ang-jsoneditor';
import { FileSaverService } from 'ngx-filesaver';
import { FileSizePipe } from 'ngx-filesize';
import { HelperService } from '../../../@core/services/helper.service';
import { HttpService } from '../../../@core/services/http.service';
import { BinaryFile } from '../../../interfaces/file';
import { QueueItem } from '../../../interfaces/queueItem';
import { FilesApiUrl, QueueItemsApiUrl } from '../../../webApiUrls';

@Component({
  selector: 'ngx-view-queue-item',
  templateUrl: './view-queue-item.component.html',
  styleUrls: ['./view-queue-item.component.scss'],
})
export class ViewQueueItemComponent implements OnInit {
  showQueueItemForm: FormGroup;
  queueItemId: string;
  public editorOptions: JsonEditorOptions;
  public data: any;
  attachedFiles: string[] = [];
  queueItemFiles: BinaryFile[] = [];
  @ViewChild(JsonEditorComponent) editor: JsonEditorComponent;
  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private httpService: HttpService,
    private router: Router,
    private helperService: HelperService,
    private fileSaverService: FileSaverService
  ) { }

  ngOnInit(): void {
    this.queueItemId = this.route.snapshot.params['id'];
    this.editorOptions = new JsonEditorOptions();
    this.editorOptions.modes = ['code', 'text', 'tree', 'view'];
    this.showQueueItemForm = this.initializeForm();
    if (this.queueItemId) {
      this.getQueueDataById();
    }
  }

  initializeForm() {
    return this.fb.group({
      organizationId: [''],
      processID: null,
      name: [''],
      type: [''],
      jsonType: [''],
      dataJson: [''],
      dontDequeueUntil: [''],
      dontDequeueAfter: [''],
      isDequeued: [''],
      isLocked: [''],
      lockedOnUTC: [''],
      lockedUntilUTC: [''],
      lockedBy: [''],
      lockTransactionKey: [''],
      retryCount: [],
      lastOccuredError: [''],
      state: [''],
      stateMessage: [''],
      timestamp: [''],
      isError: [''],
      errorCode: [''],
      errorMessage: [''],
      event: [''],
      source: [''],
      expireOnUTC: [''],
      postponeUntilUTC: [''],
      resultJSON: [''],
      payloadSizeInBytes: [],
    });
  }

  getQueueDataById(): void {
    this.httpService
      .get(
        `${QueueItemsApiUrl.QueueItems}/${QueueItemsApiUrl.view}/${this.queueItemId}`
      )
      .subscribe((response: QueueItem) => {
        if (response) {
          if (response.type.toLowerCase() === 'json')
            response.dataJson = JSON.parse(response.dataJson);
          response.isDequeued = this.helperService.changeBoolean(
            response.isDequeued
          );
          response.isLocked = this.helperService.changeBoolean(
            response.isLocked
          );
          response.isError = this.helperService.changeBoolean(response.isError);
          response.lockedOn = this.helperService.transformDate(
            response.lockedOn,
            'lll'
          );
          response.lockedUntil = this.helperService.transformDate(
            response.lockedUntil,
            'lll'
          );
          response.expireOnUTC = this.helperService.transformDate(
            response.expireOnUTC,
            'lll'
          );
          response.postponeUntilUTC = this.helperService.transformDate(
            response.postponeUntilUTC,
            'lll'
          );
          response.payloadSizeInBytes = this.helperService.getFileSize(
            +response.payloadSizeInBytes
          );
          this.attachedFiles = response.binaryObjectIds;
          this.showQueueItemForm.patchValue(response);
          this.showQueueItemForm.disable();
          this.getFilesById();
        }
      });
  }

  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], {
      queryParams: {
        PageName: 'QueueItem',
        id: this.queueItemId,
      },
    });
  }

  getFilesById(): void {
    // for (let attachedFileId of this.attachedFiles)
    this.httpService
      // .get(`${FilesApiUrl.BinaryObjects}/${attachedFileId}`)
      .get(
        `${QueueItemsApiUrl.QueueItems}/${this.queueItemId}/${QueueItemsApiUrl.queueitemattachments}/${QueueItemsApiUrl.view}`
      )
      .subscribe((response) => {
        if (response && response.items && response.items.length)
          this.queueItemFiles = [...response.items];
      });
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
}
