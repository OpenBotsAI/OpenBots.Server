import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { NbToastrService, NbDialogService } from '@nebular/theme';
import { DialogService } from '../../../@core/dialogservices';
import { HelperService } from '../../../@core/services/helper.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { Page } from '../../../interfaces/paginateInstance';
import { EmailAccountsService } from '../email-accounts.service';

@Component({
  selector: 'ngx-all-email-account',
  templateUrl: './all-email-account.component.html',
  styleUrls: ['./all-email-account.component.scss']
})
export class AllEmailAccountComponent implements OnInit {
  isDeleted = false;
  showpage: any = [];
  showallEmail: any = [];
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
    protected router: Router,
    private dialogService: DialogService,
    protected emailService: EmailAccountsService,
    private helperService: HelperService,
    private toastrService: NbToastrService
  ) {

  }

  ngOnInit(): void {
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.pagination(this.page.pageNumber, this.page.pageSize);
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  gotoadd() {
    this.router.navigate(['/pages/emailaccount/add']);
  }
  gotoedit(id) {
    this.router.navigate(['/pages/emailaccount/edit'], { queryParams: { id: id } });
  }
  gotodetail(id) {
    this.router.navigate(['/pages/emailaccount/get-email-id'], {
      queryParams: { id: id },
    });
  }

  sort(filter_val, vale) {
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    this.feild_name = filter_val + '+' + vale;
    this.emailService
      .getAllAssetOrder(this.page.pageSize, skip, this.feild_name)
      .subscribe((data: any) => {
        this.showpage = data;
        this.showallEmail = data.items;
      });
  }

  open2(dialog: TemplateRef<any>, id: any) {
    this.del_id = [];
    this.view_dialog = dialog;
    this.dialogService.openDialog(dialog)
    this.del_id = id;
  }

  del_agent(ref) {
    this.isDeleted = true;
  const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    this.emailService.delAssetbyID(this.del_id).subscribe(() => {
      this.toastrService.success('Deleted Successfully');
      ref.close();
      this.get_allasset(this.page.pageSize, skip);
    }, () => (this.isDeleted = false)
    );
  }

  get_allasset(top, skip) {
    this.get_perPage = false;
    this.emailService.getAllEmail(top, skip).subscribe((data: any) => {
      for (const item of data.items) {
        if (item.valueJson) {
          item.valueJson = JSON.stringify(item.valueJson);
          item.valueJson = JSON.parse(item.valueJson);
        }
      }
      this.showpage = data;
      this.showallEmail = data.items;

      this.page.totalCount = data.totalCount;
      this.get_perPage = true;
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
      this.emailService
        .getAllEmail(this.page.pageSize, skip)
        .subscribe((data: any) => {
          this.showpage = data;
          this.showallEmail = data.items;
          this.page.totalCount = data.totalCount;
        });
    } else if (this.feild_name.length != 0) {
      this.show_perpage_size = true;
      this.emailService
        .getAllAssetOrder(this.page.pageSize, skip, this.feild_name)
        .subscribe((data: any) => {
          this.showpage = data;
          this.showallEmail = data.items;
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
        this.get_allasset(top, skip);
      } else if (this.feild_name.lenght != 0) {
        this.emailService
          .getAllAssetOrder(top, skip, this.feild_name)
          .subscribe((data: any) => {
            this.showpage = data;
            this.showallEmail = data.items;
            this.page.totalCount = data.totalCount;
          });
      }
    } else if (this.show_perpage_size == true) {
      const top: number = this.per_page_num;
      const skip = (pageNumber - 1) * this.per_page_num;
      this.emailService
        .getAllAssetOrder(top, skip, this.feild_name)
        .subscribe((data: any) => {
          this.showpage = data;
          this.showallEmail = data.items;
          this.page.totalCount = data.totalCount;
        });
    }
  }
}
