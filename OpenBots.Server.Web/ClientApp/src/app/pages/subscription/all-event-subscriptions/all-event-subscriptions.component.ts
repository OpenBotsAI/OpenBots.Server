import { Component, OnInit, TemplateRef } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { NbToastrService } from '@nebular/theme';
import { FileSaverService } from 'ngx-filesaver';
import { DialogService } from '../../../@core/dialogservices';
import { Page } from '../../../interfaces/paginateInstance';
import { SubscriptionService } from '../subscription.service';

@Component({
  selector: 'ngx-all-event-subscriptions',
  templateUrl: './all-event-subscriptions.component.html',
  styleUrls: ['./all-event-subscriptions.component.scss'],
})
export class AllEventSubscriptionsComponent implements OnInit {
  // process_id: any = [];
  // agent_id: any = [];
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
  filter_agent_id: string;
  filter_process_id: string;
  filter_successful: string;

  constructor(
    protected router: Router,
    private formBuilder: FormBuilder,
    private toastrService: NbToastrService,
    protected SubscriptionService: SubscriptionService,
    private dialogService: DialogService
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
  }

  gotodetail(id) {
    this.router.navigate(['/pages/subscription/get-integration-log-id'], {
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
    this.filter_process_id = val;
    this.filter_job();
  }

  common_Entity(val) {
    this.filter_agent_id = val;
    this.filter_job();
  }

  filter_job() {
    this.abc_filter = '';
    if (this.filter_agent_id != null && this.filter_agent_id != '') {
      this.abc_filter =
        this.abc_filter + `entityType+eq+'${this.filter_agent_id}' and `;
    }
    if (this.filter_process_id != null && this.filter_process_id != '') {
      this.abc_filter =
        this.abc_filter +
        `integrationEventName+eq+'${this.filter_process_id}' and `;
    }

    if (this.abc_filter.endsWith(' and ')) {
      this.abc_filter = this.abc_filter.substring(
        0,
        this.abc_filter.length - 5
      );
    }

    if (this.abc_filter) {
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.SubscriptionService.filter_EntityName(
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
      this.SubscriptionService.filter_EntityName_order_by(
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
      this.SubscriptionService.getAllEntityorder(
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
      this.SubscriptionService.filter_EntityName(
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

        this.SubscriptionService.filter_EntityName(
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

        this.SubscriptionService.filter_EntityName(
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
  ngOnDestroy() {}

  trackByFn(index: number, item: unknown): number | null {
    if (!item) return null;
    return index;
  }
}
