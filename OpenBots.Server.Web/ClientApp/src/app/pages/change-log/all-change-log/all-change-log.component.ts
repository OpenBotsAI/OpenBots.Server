import { HttpResponse } from '@angular/common/http';
import { Component, OnInit, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NbToastrService, NbDialogService } from '@nebular/theme';
import { FileSaverService } from 'ngx-filesaver';
import { Page } from '../../../interfaces/paginateInstance';
import { ChangelogService } from '../change-log.service';
import { HelperService } from '../../../@core/services/helper.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';

@Component({
  selector: 'ngx-all-change-log',
  templateUrl: './all-change-log.component.html',
  styleUrls: ['./all-change-log.component.scss'],
})
export class AllChangeLogComponent implements OnInit {
  showExportbtn = false
  showpage: any = [];
  params_page_name: any = [];
  auditlog: FormGroup;
  params_id: any = [];
  show_allagents: any = [];
  show_service_name: any = [];
  select_serice_name: any = [];
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
    private dialogService: NbDialogService,
    private acroute: ActivatedRoute,
    private helperService: HelperService,
    private _FileSaverService: FileSaverService,
    protected changelogService: ChangelogService,
    private toastrService: NbToastrService,
    private formBuilder: FormBuilder
  ) {
    this.acroute.queryParams.subscribe((params) => {
      this.params_page_name = params.PageName;
      this.params_id = params.id;
      if (this.params_page_name != undefined && this.params_id != undefined) {
        this.params = true;
      }
    });

    this.auditlog = this.formBuilder.group({
      page_name: [''],
    });

    this.auditlog.patchValue({ page_name: this.params_page_name });
    this.service_name();
  }

  service_name() {
    this.service_name_Arr = [];
    this.changelogService.get_servicename().subscribe((data: any) => {
      this.show_service_name = data.serviceNameList;
    });
  }

  exportFile() {
    let fileName: string;
    this.changelogService
      .getExportFile(`ServiceName eq '${this.select_serice_name}'and ObjectId eq guid'${this.params_id}'`)
      .subscribe((data: HttpResponse<Blob>) => {
        fileName = data.headers
          .get('content-disposition')
          .split(';')[1]
          .split('=')[1]
          .replace(/\"/g, '');
        this._FileSaverService.save(data.body, fileName);
      });
  }

  gotodetail(id) {
    this.router.navigate(['/pages/change-log/get-change-log-id'], {
      queryParams: { id: id },
    });
  }

  ngOnInit(): void {
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    if (this.params_page_name != undefined || this.params_page_name != null) {
      this.showExportbtn = true ;

      this.get_service_name(this.params_page_name);
    } else if (
      this.params_page_name == undefined ||
      this.params_page_name == null
    ) {
      this.pagination(this.page.pageNumber, this.page.pageSize);
    }
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  sort(filter_value, vale) {
    if (this.service_name_page == true) {
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.feild_name = filter_value + '+' + vale;
      this.changelogService
        .get_AllAgent_order_by_servicename(
          `ServiceName eq '${this.select_serice_name}'`,
          this.page.pageSize,
          skip,
          this.feild_name
        )
        .subscribe((data: any) => {
          this.showpage = data;
          this.show_allagents = data.items;
          this.value = data.items.serviceName;
          this.get_perPage = true;
        });
    } else if (this.service_name_page == false) {
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.feild_name = filter_value + '+' + vale;
      this.changelogService
        .get_AllAgent_order(this.page.pageSize, skip, this.feild_name)
        .subscribe((data: any) => {
          this.showpage = data;
          this.show_allagents = data.items;
          this.value = data.items.serviceName;
          this.get_perPage = true;
        });
    }
  }

  get_service_name(val) {
    if (val) {

      if (this.params == false) {
        this.service_name_page = true;
        this.select_serice_name = val;
        const skip = (this.page.pageNumber - 1) * this.per_page_num;
        this.changelogService
          .filter_servicename(
            `ServiceName eq '${this.select_serice_name}'`,
            this.page.pageSize,
            skip
          )
          .subscribe((data: any) => {
            this.showpage = data;
            this.show_allagents = data.items;
            this.page.totalCount = data.totalCount;
            this.get_perPage = true;
          });
      } else if (this.params == true) {

          this.service_name_page = true;
        this.select_serice_name = val;
        const skip = (this.page.pageNumber - 1) * this.per_page_num;
        this.changelogService
          .filter_servicename(
            `ServiceName eq '${this.select_serice_name}'and ObjectId eq guid'${this.params_id}'`,
            this.page.pageSize,
            skip
          )
          .subscribe((data: any) => {
            this.showpage = data;
            this.show_allagents = data.items;
            this.page.totalCount = data.totalCount;
            this.get_perPage = true;
          });

      }
    }
    else if (val == null || val == '' || val == undefined) {
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
        this.changelogService
          .filter_servicename_order_by(
            `ServiceName eq '${this.select_serice_name}'`,
            this.page.pageSize,
            skip,
            this.feild_name
          )
          .subscribe((data: any) => {
            this.showpage = data;
            this.show_allagents = data.items;
            this.page.totalCount = data.totalCount;
            this.get_perPage = true;
          });
      } else if (this.feild_name.length == 0) {
        this.changelogService
          .filter_servicename(
            `ServiceName eq '${this.select_serice_name}'`,
            this.page.pageSize,
            skip
          )
          .subscribe((data: any) => {
            this.showpage = data;
            this.show_allagents = data.items;
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
        this.changelogService
          .get_AllAgent_order(this.page.pageSize, skip, this.feild_name)
          .subscribe((data: any) => {
            this.showpage = data;
            this.show_allagents = data.items;
            this.page.totalCount = data.totalCount;
            this.get_perPage = true;
          });
      } else if (this.feild_name.length == 0) {
        this.show_perpage_size = true;
        this.changelogService
          .get_AllAudits(this.page.pageSize, skip)
          .subscribe((data: any) => {
            this.showpage = data;
            this.show_allagents = data.items;
            this.page.totalCount = data.totalCount;
            this.get_perPage = true;
          });
      }
    }
  }

  get_allagent(top, skip) {
    this.get_perPage = false;
    this.changelogService.get_AllAudits(top, skip).subscribe((data: any) => {
      this.showpage = data;
      this.show_allagents = data.items;
      this.page.totalCount = data.totalCount;
      this.get_perPage = true;
    });
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
        this.changelogService
          .filter_servicename(
            `ServiceName eq '${this.select_serice_name}'`,
            top,
            skip
          )
          .subscribe((data: any) => {
            this.showpage = data;
            this.show_allagents = data.items;
            this.page.totalCount = data.totalCount;
            this.get_perPage = true;
          });
      } else if (this.show_perpage_size == true) {
        if (this.feild_name.length != 0) {
          const top: number = this.per_page_num;
          const skip = (pageNumber - 1) * this.per_page_num;
          this.service_name_page = true;
          this.changelogService
            .filter_servicename_order_by(
              `ServiceName eq '${this.select_serice_name}'`,
              top,
              skip,
              this.feild_name
            )
            .subscribe((data: any) => {
              this.showpage = data;
              this.show_allagents = data.items;
              this.page.totalCount = data.totalCount;
              this.get_perPage = true;
            });
        } else if (this.feild_name.length == 0) {
          const top: number = this.per_page_num;
          const skip = (pageNumber - 1) * this.per_page_num;
          this.service_name_page = true;
          this.changelogService
            .filter_servicename(
              `ServiceName eq '${this.select_serice_name}'`,
              top,
              skip
            )
            .subscribe((data: any) => {
              this.showpage = data;
              this.show_allagents = data.items;
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
          this.changelogService
            .get_AllAgent_order(top, skip, this.feild_name)
            .subscribe((data: any) => {
              this.showpage = data;
              this.show_allagents = data.items;
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
          this.changelogService
            .get_AllAgent_order(top, skip, this.feild_name)
            .subscribe((data: any) => {
              this.showpage = data;
              this.show_allagents = data.items;
              this.page.totalCount = data.totalCount;
              this.get_perPage = true;
            });
        }
      }
    }
  }
}
