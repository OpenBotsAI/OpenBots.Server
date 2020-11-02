import { Component, OnInit, TemplateRef, OnDestroy } from '@angular/core';
import { HttpService } from '../../../@core/services/http.service';
import { Router } from '@angular/router';
import { QueueItem } from '../../../interfaces/queueItem';
import { Page } from '../../../interfaces/paginateInstance';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Queues } from '../../../interfaces/queues';
import { SignalRService } from '../../../@core/services/signal-r.service';
import * as signalR from '@aspnet/signalr';
import { environment } from '../../../../environments/environment';
import { DialogService } from '../../../@core/dialogservices/dialog.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { HelperService } from '../../../@core/services/helper.service';

@Component({
  selector: 'ngx-all-queue-items',
  templateUrl: './all-queue-items.component.html',
  styleUrls: ['./all-queue-items.component.scss'],
})
export class AllQueueItemsComponent implements OnInit, OnDestroy {
  selectedQueueId: string;
  allQueueItemData: QueueItem[] = [];
  deleteId: string;
  page: Page = {};
  queuesArr: Queues[] = [];
  queueForm: FormGroup;
  show = true;
  isDeleted = false;
  hubConnection: signalR.HubConnection;
  itemsPerPage: ItemsPerPage[] = [];
  filterOrderBy: string;
  constructor(
    private httpService: HttpService,
    private router: Router,
    private dialogService: DialogService,
    private fb: FormBuilder,
    public signalRService: SignalRService,
    private helperService: HelperService
  ) {}

  ngOnInit(): void {
    this.getQueues();
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.queueForm = this.initQueueForm();
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  initQueueForm() {
    return this.fb.group({
      id: [''],
      name: [''],
      itemsPerPage: 5,
    });
  }

  getQeueItemsList(top: number, skip: number, orderBy?: string): void {
    let url: string;
    if (orderBy)
      url = `QueueItems?$orderby=${orderBy}&$top=${top}&$skip=${skip}&$filter= Queueid eq guid'${this.queueForm.value.id}'`;
    else
      url = `QueueItems?$orderby=createdOn+desc&$top=${top}&$skip=${skip}&$filter= Queueid eq guid'${this.queueForm.value.id}'`;
    this.httpService.get(url).subscribe((response) => {
      if (response) {
        this.page.totalCount = response.totalCount;
        this.allQueueItemData = [...response.items];
      } else this.allQueueItemData = [];
    });
  }

  addItem(): void {
    if (this.selectedQueueId) {
      this.router.navigate(['pages/queueitems/new'], {
        queryParams: { id: `${this.selectedQueueId}` },
      });
    } else {
      this.router.navigate(['pages/queueitems/new'], {
        queryParams: { id: `${this.queuesArr[0].id}` },
      });
    }
  }

  editQueueItem(id: string): void {
    this.router.navigate([`pages/queueitems/edit/${id}`]);
  }

  openDeleteDialog(ref: TemplateRef<any>, id: string): void {
    this.deleteId = id;
    this.dialogService.openDialog(ref);
  }

  deleteQueueItem(ref): void {
    this.isDeleted = true;
    this.httpService.delete(`QueueItems/${this.deleteId}`).subscribe(
      () => {
        ref.close();
        this.httpService.success('Deleted Successfully');
        this.isDeleted = false;
        this.pagination(this.page.pageNumber, this.page.pageSize);
      },
      () => (this.isDeleted = false)
    );
  }

  viewQueueItem(id: string): void {
    this.router.navigate([`pages/queueitems/view/${id}`]);
  }

  pageChanged(event): void {
    if (this.filterOrderBy) {
      this.page.pageNumber = event;
      this.pagination(event, this.page.pageSize, `${this.filterOrderBy}`);
    } else {
      this.page.pageNumber = event;
      this.pagination(event, this.page.pageSize);
    }
  }

  pagination(pageNumber: number, pageSize: number, orderBy?: string): void {
    const top = pageSize;
    this.page.pageSize = pageSize;
    const skip = (pageNumber - 1) * pageSize;
    this.getQeueItemsList(top, skip, orderBy);
  }

  onSortClick(event, param: string): void {
    let target = event.currentTarget,
      classList = target.classList;
    if (classList.contains('fa-chevron-up')) {
      classList.remove('fa-chevron-up');
      classList.add('fa-chevron-down');
      this.filterOrderBy = `${param}+asc`;
      this.pagination(this.page.pageNumber, this.page.pageSize, `${param}+asc`);
    } else {
      classList.remove('fa-chevron-down');
      classList.add('fa-chevron-up');
      this.filterOrderBy = `${param}+desc`;
      this.pagination(
        this.page.pageNumber,
        this.page.pageSize,
        `${param}+desc`
      );
    }
  }

  addQueue(): void {
    this.router.navigate(['pages/queueslist/add']);
  }

  selectChange(event, param: string): void {
    this.selectedQueueId = event.target.value;
    if (event.target.value && param === 'pageSize') {
      this.page.pageNumber = 1;
      this.page.pageSize = +event.target.value;
      if (this.filterOrderBy)
        this.pagination(
          this.page.pageNumber,
          this.page.pageSize,
          `${this.filterOrderBy}`
        );
    } else if (event.target.value && param === 'nothing') {
      if (this.filterOrderBy)
        this.pagination(
          this.page.pageNumber,
          this.page.pageSize,
          `${this.filterOrderBy}`
        );
    }
    this.pagination(this.page.pageNumber, this.page.pageSize);
  }

  getQueues(): void {
    const url = `Queues?$orderby=createdOn+desc`;
    this.httpService.get(url).subscribe((response) => {
      if (response && response.items.length !== 0)
        this.queuesArr = [...response.items];
      else this.queuesArr = [];
      this.queueForm.patchValue(this.queuesArr[0]);
      this.pagination(this.page.pageNumber, this.page.pageSize);
    });
  }

  watchRealTimeData(event) {
    event.target.checked == true
      ? ((this.show = false), this.startConnection(), this.queueForm.disable())
      : this.closeHubConnection();
  }

  private startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl)
      .build();
    this.hubConnection
      .start()
      .then(() => {
        this.httpService.success('Watch mode is active now');
        this.hubConnection.on('sendnotification', (data) => {
          this.httpService.success(data);
          this.pagination(this.page.pageNumber, this.page.pageSize);
        });
      })
      .catch((err) => {
        this.httpService.error(err);
      });
  };

  closeHubConnection() {
    if (this.hubConnection) {
      this.show = true;
      this.queueForm.enable();
      this.hubConnection
        .stop()
        .then(() => this.httpService.warning('Watch mode is off now'));
      this.hubConnection = undefined;
    }
  }

  ngOnDestroy(): void {
    this.closeHubConnection();
  }
}
