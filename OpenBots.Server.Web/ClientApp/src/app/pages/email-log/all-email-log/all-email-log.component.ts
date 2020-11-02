import { Component, OnInit, TemplateRef } from '@angular/core';
import { Router } from '@angular/router';
import { NbDialogService, NbToastrService } from '@nebular/theme';
import { HelperService } from '../../../@core/services/helper.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { EmailLogService } from '../email-log.service';
import { Page } from '../../../interfaces/paginateInstance';
import { FormBuilder, FormGroup } from '@angular/forms';

@Component({
  selector: 'ngx-all-email-log',
  templateUrl: './all-email-log.component.html',
  styleUrls: ['./all-email-log.component.scss'],
})
export class AllEmailLogComponent implements OnInit {
  showallemailaccount: any = [];
  emailFIlterName: any = [];
  emaillogfilter: FormGroup;
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
    private dialogService: NbDialogService,
    protected emailService: EmailLogService,
    private helperService: HelperService,
    private toastrService: NbToastrService,
    private formBuilder: FormBuilder
  ) {
    this.getAllemailAccount();
    this.emaillogfilter = this.formBuilder.group({
      page_name: [''],
    });
  }

  ngOnInit(): void {
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.pagination(this.page.pageNumber, this.page.pageSize);
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  getAllemailAccount() {
    this.emailService.getAllEmailforfilter().subscribe((data: any) => {
      this.showallemailaccount = data;
    });
  }
  getEmailAccount(val) {
    if (val) {
      this.emailFIlterName = val;
      const skip = (this.page.pageNumber - 1) * this.per_page_num;
      this.emailService
        .filter_emailaccount(
          `emailAccountId eq guid'${this.emailFIlterName}'`,
          this.page.pageSize,
          skip
        )
        .subscribe((data: any) => {
          this.showpage = data;
          this.showallEmail = data.items;
          this.page.totalCount = data.totalCount;
        });
    } else if (val == null || val == '' || val == undefined) {
      this.pagination(this.page.pageNumber, this.page.pageSize);
    }
  }

  gotodetail(id) {
    this.router.navigate(['/pages/emaillog/get-emaillog-id'], {
      queryParams: { id: id },
    });
  }

  sort(filter_val, vale) {
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    this.feild_name = filter_val + '+' + vale;
    this.emailService
      .getAllEmaillogOrder(this.page.pageSize, skip, this.feild_name)
      .subscribe((data: any) => {
        this.showpage = data;
        this.showallEmail = data.items;
      });
  }

  open2(dialog: TemplateRef<any>, id: any) {
    this.del_id = [];
    this.view_dialog = dialog;
    this.dialogService.open(dialog);
    this.del_id = id;
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
        .getAllEmaillogOrder(this.page.pageSize, skip, this.feild_name)
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
          .getAllEmaillogOrder(top, skip, this.feild_name)
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
        .getAllEmaillogOrder(top, skip, this.feild_name)
        .subscribe((data: any) => {
          this.showpage = data;
          this.showallEmail = data.items;
          this.page.totalCount = data.totalCount;
        });
    }
  }
}
