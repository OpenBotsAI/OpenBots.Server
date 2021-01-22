import { Component, OnInit, TemplateRef } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { NbToastrService } from '@nebular/theme';
import { DialogService } from '../../../@core/dialogservices';
import { Page } from '../../../interfaces/paginateInstance';
import { SubscriptionService } from '../subscription.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { HelperService } from '../../../@core/services/helper.service';

@Component({
  selector: 'ngx-all-event-subscriptions',
  templateUrl: './all-event-subscriptions.component.html',
  styleUrls: ['./all-event-subscriptions.component.scss'],
})
export class AllEventSubscriptionsComponent implements OnInit {
  isDeleted = false;
  showjobs: FormGroup;
  showalleventsubscription: any = [];
  show_filter_entity: any = [];
  show_filter_event: any = [];
  showpage: any = [];
  sortDir = 1;
  view_dialog: any;
  del_id: any = [];
  toggle: boolean;
  feild_name: any = [];
  page: Page = {};
  show_perpage_size: boolean = false;
  per_page_num: any = [];
  abc_filter: string;
  filter: string = '';
  filterEntity: string;
  filterEvent: string;
  itemsPerPage: ItemsPerPage[] = [];
  constructor(
    protected router: Router,
    private formBuilder: FormBuilder,
    private toastrService: NbToastrService,
    protected SubscriptionService: SubscriptionService,
    private dialogService: DialogService,
    private helperService: HelperService
  ) {
    this.showjobs = this.formBuilder.group({
      automationId: [''],
      agentId: [''],
    });
    this.getallEntity();
  }

  getallEntity() {
    this.SubscriptionService.get_EntityName().subscribe((data: any) => {
      this.show_filter_entity = data.integrationEntityTypeList;
      this.show_filter_event = data.integrationEventNameList;
    });
  }
  ngOnInit(): void {
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.pagination(this.page.pageNumber, this.page.pageSize);
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  gotodetail(id) {
    this.router.navigate(['/pages/subscription/get-subscription-id'], {
      queryParams: { id: id },
    });
  }
  gotoedit(id) {
    this.router.navigate(['/pages/subscription/edit'], {
      queryParams: { id: id },
    });
  }
  gotoAdd() {
    this.router.navigate(['/pages/subscription/add']);
  }

  open2(dialog: TemplateRef<any>, id: any) {
    this.del_id = [];
    this.view_dialog = dialog;
    this.dialogService.openDialog(dialog);
    this.del_id = id;
  }
  delSubscription(ref) {
    this.isDeleted = true;
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    this.SubscriptionService.delsubscriptionbyID(this.del_id).subscribe(
      () => {
        this.isDeleted = false;
        this.toastrService.success(
          'subscription Delete Successfully',
          'Success'
        );
        ref.close();
        this.get_AllJobs(this.page.pageSize, skip);
      },
      () => (this.isDeleted = false)
    );
  }
  gotoprocesslog(id) {
    this.router.navigate(['/pages/automationLogs'], {
      queryParams: { jobId: id },
    });
  }

  comon_Event(val) {
    this.filterEvent = val;
    this.filter_job();
  }

  common_Entity(val) {
    this.filterEntity = val;
    this.filter_job();
  }

  filter_job() {
    this.abc_filter = '';
    if (this.filterEntity != null && this.filterEntity != '') {
      this.abc_filter =
        this.abc_filter + `entityType+eq+'${this.filterEntity}' and `;
    }
    if (this.filterEvent != null && this.filterEvent != '') {
      this.abc_filter =
        this.abc_filter + `integrationEventName+eq+'${this.filterEvent}' and `;
    }

    if (this.abc_filter.endsWith(' and ')) {
      this.abc_filter = this.abc_filter.substring(
        0,
        this.abc_filter.length - 5
      );
    }

    if (this.abc_filter) {
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.SubscriptionService.filterSubscriptionName(
        `${this.abc_filter}`,
        this.page.pageSize,
        skip
      ).subscribe((data: any) => {
        this.showalleventsubscription = data.items;
        this.showpage = data;
        this.page.totalCount = data.totalCount;
      });
    } else {
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.get_AllJobs(this.page.pageSize, skip);
    }
  }

  sort(filter_val, vale) {
    console.log(filter_val, vale);
    if (this.abc_filter) {
      this.feild_name = filter_val + '+' + vale;
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.SubscriptionService.filterSubscriptionNameorderby(
        `${this.abc_filter}`,
        this.page.pageSize,
        skip,
        this.feild_name
      ).subscribe((data: any) => {
        this.showalleventsubscription = data.items;
        this.showpage = data;
        this.page.totalCount = data.totalCount;
      });
    } else if (this.abc_filter == undefined || this.abc_filter == '') {
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.feild_name = filter_val + '+' + vale;
      this.SubscriptionService.getAllSubscriptionOrder(
        this.page.pageSize,
        skip,
        this.feild_name
      ).subscribe((data: any) => {
        this.showalleventsubscription = data.items;
        this.showpage = data;
        this.page.totalCount = data.totalCount;
      });
    }
  }

  per_page(val) {
    if (this.abc_filter) {
      this.per_page_num = val;
      this.page.pageSize = val;
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.SubscriptionService.filterSubscriptionName(
        `${this.abc_filter}`,
        this.page.pageSize,
        skip
      ).subscribe((data: any) => {
        this.showalleventsubscription = data.items;
        this.showpage = data;
        this.page.totalCount = data.totalCount;
      });
    } else if (this.abc_filter == undefined || this.abc_filter == '') {
      this.per_page_num = val;
      this.page.pageSize = val;
      this.show_perpage_size = true;
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.SubscriptionService.getAllEventSubscription(
        this.page.pageSize,
        skip
      ).subscribe((data: any) => {
        this.showalleventsubscription = data.items;
        this.page.totalCount = data.totalCount;
      });
    }
  }

  get_AllJobs(top, skip) {
    this.SubscriptionService.getAllEventSubscription(top, skip).subscribe(
      (data: any) => {
        this.showalleventsubscription = data.items;

        this.showpage = data;
        this.page.totalCount = data.totalCount;
      }
    );
  }

  onSortClick(event, filter_val) {
    let target = event.currentTarget,
      classList = target.classList;
    if (classList.contains('fa-chevron-up')) {
      classList.remove('fa-chevron-up');
      classList.add('fa-chevron-down');
      let sort_set = 'desc';
      this.sort(filter_val, sort_set);
      this.sortDir = -1;
    } else {
      classList.add('fa-chevron-up');
      classList.remove('fa-chevron-down');
      let sort_set = 'asc';
      this.sort(filter_val, sort_set);
      this.sortDir = 1;
    }
  }
  pageChanged(event) {
    this.page.pageNumber = event;
    this.pagination(event, this.page.pageSize);
  }

  pagination(pageNumber, pageSize?) {
    if (this.abc_filter) {
      if (this.show_perpage_size == false) {
        const skip = (pageNumber - 1) * pageSize;

        this.SubscriptionService.filterSubscriptionName(
          `${this.abc_filter}`,
          this.page.pageSize,
          skip
        ).subscribe((data: any) => {
          this.showalleventsubscription = data.items;
          this.showpage = data;
          this.page.totalCount = data.totalCount;
        });
      } else if (this.show_perpage_size == true) {
        const top: number = this.per_page_num;
        const skip = (pageNumber - 1) * this.per_page_num;

        this.SubscriptionService.filterSubscriptionName(
          `${this.abc_filter}`,
          this.page.pageSize,
          skip
        ).subscribe((data: any) => {
          this.showalleventsubscription = data.items;
          this.showpage = data;
          this.page.totalCount = data.totalCount;
        });
      }
    } else if (this.abc_filter == undefined || this.abc_filter == '') {
      if (this.show_perpage_size == false) {
        const top: number = pageSize;
        const skip = (pageNumber - 1) * pageSize;
        this.get_AllJobs(top, skip);
      } else if (this.show_perpage_size == true) {
        const top: number = this.per_page_num;
        const skip = (pageNumber - 1) * this.per_page_num;
        this.get_AllJobs(top, skip);
      }
    }
  }

  trackByFn(index: number, item: unknown): number {
    if (!item) return null;
    return index;
  }
}
