import { Component, OnInit, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Page } from '../../../interfaces/paginateInstance';
import { HelperService } from '../../../@core/services/helper.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { BusinessEventService } from '../business-event.service';
import { DialogService } from '../../../@core/dialogservices';



@Component({
  selector: 'ngx-all-business-event',
  templateUrl: './all-business-event.component.html',
  styleUrls: ['./all-business-event.component.scss']
})
export class AllBusinessEventComponent implements OnInit {
  delId: any = [];
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
  viewDialog: any;

  constructor(
    protected router: Router,
    // private acroute: ActivatedRoute,
    private dialogService: DialogService,
    private helperService: HelperService,
    protected BusinessEventservice: BusinessEventService,
    private formBuilder: FormBuilder
  ) {
    this.systemEventform = this.formBuilder.group({
      page_name: [''],
    });
    this.entityName();
  }

  entityName() {
    this.BusinessEventservice.getIntegrationEventName().subscribe((data: any) => {
      this.show_Entityname = data.entityNameList;
    });
  }

  gotodetail(id) {
    this.router.navigate([`/pages/business-event/view/${id}`]);
  }

  gotoadd() {
    this.router.navigate(['/pages/business-event/add']);
  }

  gotoedit(id) {
    this.router.navigate([`/pages/business-event/edit/${id}`]);
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
      this.BusinessEventservice.getAllorderbyEntityname(
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
      this.BusinessEventservice.getAllIntegrationEventorder(
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
  open2(dialog: TemplateRef<any>, id: any) {
    this.delId = [];
    this.viewDialog = dialog;
    this.dialogService.openDialog(dialog);
    this.delId = id;
  }

  getEntityname(val) {
    if (val) {
      this.service_name_page = true;
      this.selectEntityname = val;
      const skip = (this.page.pageNumber - 1) * this.per_page_num;
      this.BusinessEventservice.filterIntegrationEventName(
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
        this.BusinessEventservice.filterEntityNameOrderby(
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
        this.BusinessEventservice.filterIntegrationEventName(
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
        this.BusinessEventservice.getAllIntegrationEventorder(
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
        this.BusinessEventservice.get_AllSystemEvent(
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
    this.BusinessEventservice.get_AllSystemEvent(top, skip).subscribe(
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
        this.BusinessEventservice.filterIntegrationEventName(
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
          this.BusinessEventservice.filterEntityNameOrderby(
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
          this.BusinessEventservice.filterIntegrationEventName(
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
          this.BusinessEventservice.getAllIntegrationEventorder(
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
          this.BusinessEventservice.getAllIntegrationEventorder(
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
