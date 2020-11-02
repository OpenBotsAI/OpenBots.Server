import { Component, OnInit, ViewChild } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { JsonEditorComponent, JsonEditorOptions } from 'ang-jsoneditor';
import { TimeDatePipe } from '../../../@core/pipe';
import { HelperService } from '../../../@core/services/helper.service';
import { HttpService } from '../../../@core/services/http.service';
import { QueueItem } from '../../../interfaces/queueItem';

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
  @ViewChild(JsonEditorComponent) editor: JsonEditorComponent;
  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private httpService: HttpService,
    private router: Router,
    private helperService: HelperService
  ) {}

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
      type: [''], //queueItemType: [''],
      jsonType: [''], // entityType: [''],
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
    });
  }

  getQueueDataById(): void {
    this.httpService
      .get(`QueueItems/${this.queueItemId}`)
      .subscribe((response: QueueItem) => {
        if (response) {
          if (response.type === 'Json')
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
          this.showQueueItemForm.patchValue(response);
          this.showQueueItemForm.disable();
        }
      });
  }

  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], {
      queryParams: {
        PageName: 'OpenBots.Server.Model.QueueItem',
        id: this.queueItemId,
      },
    });
  }
}
