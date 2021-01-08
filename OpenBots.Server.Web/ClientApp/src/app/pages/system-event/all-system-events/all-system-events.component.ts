import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Page } from '../../../interfaces/paginateInstance';
import { HelperService } from '../../../@core/services/helper.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { SystemEventService } from '../system-event.service';

@Component({
  selector: 'ngx-all-system-events',
  templateUrl: './all-system-events.component.html',
  styleUrls: ['./all-system-events.component.scss'],
})
export class AllSystemEventsComponent implements OnInit {
  showExportbtn = false;
  showpage: any = [];
  params_page_name: any = [];
  systemEventform: FormGroup;
  show_allsystemevent: any = [];
  show_Entityname: any = [];
  selectEntityname: any = [];
  service_name_page: boolean = false;
  sortDir = 1;
  toggle: boolean;
  feild_name: any = [];
  page: Page = {};
  value: any = [];
  show_perpage_size: boolean = false;
  get_perPage: boolean = false;
  per_page_num: any = [];
  params: boolean = false;
  itemsPerPage: ItemsPerPage[] = [];

  constructor(
    protected router: Router,
    private acroute: ActivatedRoute,
    private helperService: HelperService,
    protected SystemEventService: SystemEventService,
    private formBuilder: FormBuilder
  ) {
    this.systemEventform = this.formBuilder.group({
      page_name: [''],
    });
    this.entityName();
  }

  entityName() {
    this.SystemEventService.getIntegrationEventName().subscribe((data: any) => {
      this.show_Entityname = data.entityNameList;
    });
    // let duplicatePushArray = [];
    // for (let i = 0; i < this.show_Entityname.length; i++) {
    //   if (
    //     duplicatePushArray.indexOf(this.show_Entityname[i].entityType) === -1
    //   ) {
    //     duplicatePushArray.push(this.show_Entityname[i].entityType);
    //   }
    // }

    // this.show_Entityname = duplicatePushArray;
  }

  gotodetail(id) {
    this.router.navigate(['/pages/system-event/get-system-event-id'], {
      queryParams: { id: id },
    });
  }

  ngOnInit(): void {
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.pagination(this.page.pageNumber, this.page.pageSize);
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  sort(filter_value, vale) {
    if (this.service_name_page == true) {
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.feild_name = filter_value + '+' + vale;
      this.SystemEventService.getAllorderbyEntityname(
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
    } else if (this.service_name_page == false) {
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.feild_name = filter_value + '+' + vale;
      this.SystemEventService.getAllIntegrationEventorder(
        this.page.pageSize,
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

  getEntityname(val) {
    if (val) {
      this.service_name_page = true;
      this.selectEntityname = val;
      const skip = (this.page.pageNumber - 1) * this.per_page_num;
      this.SystemEventService.filterIntegrationEventName(
        `entityType+eq+'${this.selectEntityname}'`,
        this.page.pageSize,
        skip
      ).subscribe((data: any) => {
        this.showpage = data;
        this.show_allsystemevent = data.items;
        this.page.totalCount = data.totalCount;
        this.get_perPage = true;
      });
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
        this.SystemEventService.filterEntityNameOrderby(
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
        this.SystemEventService.filterIntegrationEventName(
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
        this.SystemEventService.getAllIntegrationEventorder(
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

  getAllSystemEvent(top, skip) {
    this.get_perPage = false;
    this.SystemEventService.get_AllSystemEvent(top, skip).subscribe(
      (data: any) => {
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
    console.log(target);
    if (classList.contains('fa-chevron-up')) {
      classList.remove('fa-chevron-up');
      classList.add('fa-chevron-down');
      console.log(classList);
      let sort_set = 'desc';
      this.sort(filter_val, sort_set);
      this.sortDir = -1;
    } else {
      classList.add('fa-chevron-up');
      classList.remove('fa-chevron-down');
      console.log(classList);
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
        this.SystemEventService.filterIntegrationEventName(
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
          this.SystemEventService.filterEntityNameOrderby(
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
          this.SystemEventService.filterIntegrationEventName(
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
          this.getAllSystemEvent(top, skip);
        } else if (this.feild_name.length != 0) {
          this.SystemEventService.getAllIntegrationEventorder(
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
          this.getAllSystemEvent(top, skip);
        } else if (this.feild_name.length != 0) {
          const top: number = this.per_page_num;
          const skip = (pageNumber - 1) * this.per_page_num;
          this.SystemEventService.getAllIntegrationEventorder(
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

  trackByFn(index: number, item: unknown): number {
    if (!item) return null;
    return index;
  }
}
