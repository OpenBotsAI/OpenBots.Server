import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { HelperService } from '../../../@core/services/helper.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { Page } from '../../../interfaces/paginateInstance';
import { ConfigValueService } from '../config-value.service';

@Component({
  selector: 'ngx-all-config-value',
  templateUrl: './all-config-value.component.html',
  styleUrls: ['./all-config-value.component.scss'],
})
export class AllConfigValueComponent implements OnInit {
  isDeleted = false;
  showpage: any = [];
  showallconfigValue: any = [];
  sortDir = 1;
  view_dialog: any;
  showData;
  del_id: any = [];
  toggle: boolean;
  feild_name: any = [];
  page: Page = {};
  get_perPage: boolean = false;
  show_perpage_size: boolean = false;
  per_page_num: any = [];
  itemsPerPage: ItemsPerPage[] = [];

  constructor(
    private router: Router,
    private configService: ConfigValueService,
    private helperService: HelperService
  ) {}

  ngOnInit(): void {
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.pagination(this.page.pageNumber, this.page.pageSize);
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  gotoedit(id) {
    this.router.navigate(['/pages/config/edit'], {
      queryParams: { id: id },
    });
  }

  gotodetail(id) {
    this.router.navigate(['/pages/config/get-config-id'], {
      queryParams: { id: id },
    });
  }

  sort(filter_val, vale) {
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    this.feild_name = filter_val + '+' + vale;
    this.configService
      .getAllConfigvalueOrder(this.page.pageSize, skip, this.feild_name)
      .subscribe((data: any) => {
        this.showpage = data;
        this.showallconfigValue = data.items;
      });
  }

  getAllConfigValue(top, skip) {
    this.get_perPage = false;
    this.configService.getAllConfigvalue(top, skip).subscribe((data: any) => {
      for (const item of data.items) {
        if (item.valueJson) {
          item.valueJson = JSON.stringify(item.valueJson);
          item.valueJson = JSON.parse(item.valueJson);
        }
      }
      this.showpage = data;
      this.showallconfigValue = data.items;
      this.page.totalCount = data.totalCount;
      if (data.totalCount == 0) {
        this.get_perPage = false;
      } else if (data.totalCount != 0) {
        this.get_perPage = true;
      }
    });
  }

  onSortClick(event, fil_val) {
    let target = event.currentTarget,
      classList = target.classList;
    if (classList.contains('fa-chevron-up')) {
      classList.remove('fa-chevron-up');
      classList.add('fa-chevron-down');
      let sort_set = 'desc';
      this.sort(fil_val, sort_set);
      this.sortDir = -1;
    } else {
      classList.add('fa-chevron-up');
      classList.remove('fa-chevron-down');
      let sort_set = 'asc';
      this.sort(fil_val, sort_set);
      this.sortDir = 1;
    }
  }

  per_page(val) {
    this.per_page_num = val;
    this.page.pageSize = val;
    const skip = (this.page.pageNumber - 1) * this.per_page_num;
    if (this.feild_name.length == 0) {
      this.configService
        .getAllConfigvalue(this.page.pageSize, skip)
        .subscribe((data: any) => {
          this.showpage = data;
          this.showallconfigValue = data.items;
          this.page.totalCount = data.totalCount;
        });
    } else if (this.feild_name.length != 0) {
      this.show_perpage_size = true;
      this.configService
        .getAllConfigvalueOrder(this.page.pageSize, skip, this.feild_name)
        .subscribe((data: any) => {
          this.showpage = data;
          this.showallconfigValue = data.items;
          this.page.totalCount = data.totalCount;
        });
    }
  }

  pageChanged(event) {
    this.page.pageNumber = event;
    this.pagination(event, this.page.pageSize);
  }

  pagination(pageNumber, pageSize?) {
    if (this.show_perpage_size == false) {
      const top: number = pageSize;
      const skip = (pageNumber - 1) * pageSize;
      if (this.feild_name.length == 0) {
        this.getAllConfigValue(top, skip);
      } else if (this.feild_name.lenght != 0) {
        this.configService
          .getAllConfigvalueOrder(top, skip, this.feild_name)
          .subscribe((data: any) => {
            this.showpage = data;
            this.showallconfigValue = data.items;
            this.page.totalCount = data.totalCount;
          });
      }
    } else if (this.show_perpage_size == true) {
      const top: number = this.per_page_num;
      const skip = (pageNumber - 1) * this.per_page_num;
      this.configService
        .getAllConfigvalueOrder(top, skip, this.feild_name)
        .subscribe((data: any) => {
          this.showpage = data;
          this.showallconfigValue = data.items;
          this.page.totalCount = data.totalCount;
        });
    }
  }
  trackByFn(index: number, item: unknown): number {
    if (!item) return null;
    return index;
  }
}
