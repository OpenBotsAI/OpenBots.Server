import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Page } from '../../../interfaces/paginateInstance';
import { HelperService } from '../../../@core/services/helper.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { IntegrationLogsService } from '../integration-logs.service';
import { TimeDatePipe } from '../../../@core/pipe';

@Component({
  selector: 'ngx-all-integration-logs',
  templateUrl: './all-integration-logs.component.html',
  styleUrls: ['./all-integration-logs.component.scss'],
})
export class AllIntegrationLogsComponent implements OnInit {
  pipe: TimeDatePipe;
  showExportbtn = false;
  showpage: any = [];
  params_page_name: any = [];
  systemEventform: FormGroup;
  params_id: any = [];
  show_allsystemevent: any = [];
  show_Entityname: any = [];
  selectEntityname: any = [];
  service_name_page: boolean = false;
  sortDir = 1;
  view_dialog: any;
  del_id: any = [];
  toggle: boolean;
  feild_name: any = [];
  page: Page = {};
  value: any = [];
  service_name_Arr = [];
  show_perpage_size: boolean = false;
  get_perPage: boolean = false;
  per_page_num: any = [];
  params: boolean = false;
  itemsPerPage: ItemsPerPage[] = [];

  constructor(
    protected router: Router,
    private acroute: ActivatedRoute,
    private helperService: HelperService,
    protected SystemEventService: IntegrationLogsService,
    private formBuilder: FormBuilder
  ) {
    this.systemEventform = this.formBuilder.group({
      page_name: [''],
    });

    // this.systemEventform.patchValue({ page_name: this.params_page_name });
    this.service_name();
  }

  service_name() {
    this.service_name_Arr = [];
    this.SystemEventService.get_EntityName().subscribe((data: any) => {
      this.show_Entityname = data.items;
    });
  }

  gotodetail(id) {
    this.router.navigate(['/pages/integration-logs/get-integration-log-id'], {
      queryParams: { id: id },
    });
  }

  ngOnInit(): void {
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    // if (this.params_page_name != undefined || this.params_page_name != null) {
    //   this.showExportbtn = true;

    //   this.getEntityname(this.params_page_name);
    // } else if (
    //   this.params_page_name == undefined ||
    //   this.params_page_name == null
    // ) {
    this.pagination(this.page.pageNumber, this.page.pageSize);
    // }
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  sort(filter_value, vale) {
    if (this.service_name_page == true) {
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.feild_name = filter_value + '+' + vale;
      this.SystemEventService.get_AllEntityorderbyEntityname(
        `entityType+eq+'${this.selectEntityname}'`,
        this.page.pageSize,
        skip,
        this.feild_name
      ).subscribe((data: any) => {
        this.showpage = data;
        this.show_allsystemevent = data.items;
        this.get_perPage = true;
      });
    } else if (this.service_name_page == false) {
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.feild_name = filter_value + '+' + vale;
      this.SystemEventService.getAllEntityorder(
        this.page.pageSize,
        skip,
        this.feild_name
      ).subscribe((data: any) => {
        this.showpage = data;
        this.show_allsystemevent = data.items;
        this.get_perPage = true;
      });
    }
  }

  getEntityname(val) {
    if (val) {
      // if (this.params == false) {
      this.service_name_page = true;
      this.selectEntityname = val;
      const skip = (this.page.pageNumber - 1) * this.per_page_num;
      this.SystemEventService.filter_EntityName(
        `entityType+eq+'${this.selectEntityname}'`,
        this.page.pageSize,
        skip
      ).subscribe((data: any) => {
        this.showpage = data;
        this.show_allsystemevent = data.items;
        this.page.totalCount = data.totalCount;
        this.get_perPage = true;
      });
      // } else if (this.params == true) {
      //   this.service_name_page = true;
      //   this.select_serice_name = val;
      //   const skip = (this.page.pageNumber - 1) * this.per_page_num;
      //   this.SystemEventService
      //     .filter_servicename(
      //       `ServiceName eq '${this.select_serice_name}'and ObjectId eq guid'${this.params_id}'`,
      //       this.page.pageSize,
      //       skip
      //     )
      //     .subscribe((data: any) => {
      //       this.showpage = data;
      //       this.show_allsystemevent = data.items;
      //       this.page.totalCount = data.totalCount;
      //       this.get_perPage = true;
      //     });
      // }
    } else if (val == null || val == '' || val == undefined) {
      this.service_name_page = false;
      this.pagination(this.page.pageNumber, this.page.pageSize);
    }
  }

  per_page(val) {
    if (this.service_name_page == true) {
      this.service_name_page = true;
      this.per_page_num = val;
      this.page.pageSize = val;
      this.show_perpage_size = true;
      const skip = (this.page.pageNumber - 1) * this.per_page_num;
      if (this.feild_name.length != 0) {
        this.SystemEventService.filter_EntityName_order_by(
          `entityType+eq+'${this.selectEntityname}'`,
          this.page.pageSize,
          skip,
          this.feild_name
        ).subscribe((data: any) => {
          this.showpage = data;
          this.show_allsystemevent = data.items;
          this.page.totalCount = data.totalCount;
          this.get_perPage = true;
        });
      } else if (this.feild_name.length == 0) {
        this.SystemEventService.filter_EntityName(
          `entityType+eq+'${this.selectEntityname}'`,
          this.page.pageSize,
          skip
        ).subscribe((data: any) => {
          this.showpage = data;
          this.show_allsystemevent = data.items;
          this.page.totalCount = data.totalCount;
          this.get_perPage = true;
        });
      }
    } else if (this.service_name_page == false) {
      this.page.pageSize = val;
      this.per_page_num = val;
      const skip = (this.page.pageNumber - 1) * this.per_page_num;
      if (this.feild_name.length != 0) {
        this.show_perpage_size = true;
        this.SystemEventService.getAllEntityorder(
          this.page.pageSize,
          skip,
          this.feild_name
        ).subscribe((data: any) => {
          this.showpage = data;
          this.show_allsystemevent = data.items;
          this.page.totalCount = data.totalCount;
          this.get_perPage = true;
        });
      } else if (this.feild_name.length == 0) {
        this.show_perpage_size = true;
        this.SystemEventService.get_AllSystemEvent(
          this.page.pageSize,
          skip
        ).subscribe((data: any) => {
          this.showpage = data;
          this.show_allsystemevent = data.items;
          this.page.totalCount = data.totalCount;
          this.get_perPage = true;
        });
      }
    }
  }

  get_allagent(top, skip) {
    this.get_perPage = false;
    this.SystemEventService.get_AllSystemEvent(top, skip).subscribe(
      (data: any) => {
        // data.occuredOnUTC = this.transformDate(data.occuredOnUTC, 'lll');
        this.showpage = data;
        this.show_allsystemevent = data.items;
        this.page.totalCount = data.totalCount;
        this.get_perPage = true;
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
    if (this.service_name_page == true) {
      if (this.show_perpage_size == false) {
        const top: number = pageSize;
        const skip = (pageNumber - 1) * pageSize;
        this.service_name_page = true;
        this.SystemEventService.filter_EntityName(
          `entityType+eq+'${this.selectEntityname}'`,
          top,
          skip
        ).subscribe((data: any) => {
          this.showpage = data;
          this.show_allsystemevent = data.items;
          this.page.totalCount = data.totalCount;
          this.get_perPage = true;
        });
      } else if (this.show_perpage_size == true) {
        if (this.feild_name.length != 0) {
          const top: number = this.per_page_num;
          const skip = (pageNumber - 1) * this.per_page_num;
          this.service_name_page = true;
          this.SystemEventService.filter_EntityName_order_by(
            `entityType+eq+'${this.selectEntityname}'`,
            top,
            skip,
            this.feild_name
          ).subscribe((data: any) => {
            this.showpage = data;
            this.show_allsystemevent = data.items;
            this.page.totalCount = data.totalCount;
            this.get_perPage = true;
          });
        } else if (this.feild_name.length == 0) {
          const top: number = this.per_page_num;
          const skip = (pageNumber - 1) * this.per_page_num;
          this.service_name_page = true;
          this.SystemEventService.filter_EntityName(
            `entityType+eq+'${this.selectEntityname}'`,
            top,
            skip
          ).subscribe((data: any) => {
            this.showpage = data;
            this.show_allsystemevent = data.items;
            this.page.totalCount = data.totalCount;
            this.get_perPage = true;
          });
        }
      }
    } else {
      if (this.show_perpage_size == false) {
        const top: number = pageSize;
        const skip = (pageNumber - 1) * pageSize;
        if (this.feild_name.length == 0) {
          this.get_allagent(top, skip);
        } else if (this.feild_name.length != 0) {
          this.SystemEventService.getAllEntityorder(
            top,
            skip,
            this.feild_name
          ).subscribe((data: any) => {
            this.showpage = data;
            this.show_allsystemevent = data.items;
            this.page.totalCount = data.totalCount;
            this.get_perPage = true;
          });
        }
      } else if (this.show_perpage_size == true) {
        if (this.feild_name.length == 0) {
          const top: number = pageSize;
          const skip = (pageNumber - 1) * pageSize;
          this.get_allagent(top, skip);
        } else if (this.feild_name.length != 0) {
          const top: number = this.per_page_num;
          const skip = (pageNumber - 1) * this.per_page_num;
          this.SystemEventService.getAllEntityorder(
            top,
            skip,
            this.feild_name
          ).subscribe((data: any) => {
            this.showpage = data;
            this.show_allsystemevent = data.items;
            this.page.totalCount = data.totalCount;
            this.get_perPage = true;
          });
        }
      }
    }
  }

  trackByFn(index: number, item: unknown): number | null {
    if (!item) return null;
    return index;
  }
  transformDate(value, format) {
    this.pipe = new TimeDatePipe();
    return this.pipe.transform(value, `${format}`);
  }
}
