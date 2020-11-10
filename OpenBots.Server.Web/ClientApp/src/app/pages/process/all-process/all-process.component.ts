import { Component, OnInit, TemplateRef } from '@angular/core';
import { Router } from '@angular/router';
import { Page } from '../../../interfaces/paginateInstance';
import { ProcessService } from '../process.service';
import { DialogService } from '../../../@core/dialogservices/dialog.service';
import { NbToastrService } from '@nebular/theme';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { HelperService } from '../../../@core/services/helper.service';

@Component({
  selector: 'ngx-all-process',
  templateUrl: './all-process.component.html',
  styleUrls: ['./all-process.component.scss'],
})
export class AllProcessComponent implements OnInit {
  show_allprocess: any = [];
  process_id: any = [];
  show_filter_process: any = [];
  sortDir = 1;
  toggle: boolean;
  feild_name: any = [];
  page: Page = {};
  show_perpage_size: boolean = false;
  per_page_num: any = [];
  get_perPage: boolean = false;
  isDeleted = false;
  showTotalPage: [];
  itemsPerPage: ItemsPerPage[] = [];

  constructor(
    protected router: Router,
    private dialogService: DialogService,
    private toastrService: NbToastrService,
    private helperService: HelperService,
    protected processService: ProcessService
  ) {}

  ngOnInit(): void {
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.pagination(this.page.pageNumber, this.page.pageSize);
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  gotodetail(id) {
    this.router.navigate(['/pages/process/get-process-id'], {
      queryParams: { id: id },
    });
  }
  gotoedit(id) {
    this.router.navigate(['/pages/process/edit'], { queryParams: { id: id } });
  }

  goto_jobs(id) {
    this.router.navigate(['/pages/job/list'], {
      queryParams: { ProcessID: id },
    });
  }

  gotoadd() {
    this.router.navigate(['/pages/process/add']);
  }

  openDialog(ref, id) {
    this.process_id = id;
    this.dialogService.openDialog(ref);
  }

  deleteUser(ref) {
    this.isDeleted = true;
    this.processService.deleteProcess(this.process_id).subscribe(
      () => {
        ref.close();
        this.isDeleted = false;
        this.toastrService.success('You have successfully Delete ');
        this.pagination(this.page.pageNumber, this.page.pageSize);
      },
      () => (this.isDeleted = false)
    );
  }

  sort(filter_val, vale) {
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    this.feild_name = filter_val + '+' + vale;
    this.processService
      .getAllJobsOrder(this.page.pageSize, skip, this.feild_name)
      .subscribe((data: any) => {
        this.show_allprocess = data.items;
        this.page.totalCount = data.totalCount;
      });
  }

  per_page(val) {
    this.per_page_num = val;
    this.page.pageSize = val;
    this.show_perpage_size = true;
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    this.processService
      .getAllProcess(this.page.pageSize, skip)
      .subscribe((data: any) => {
        this.show_allprocess = data.items;
        this.page.totalCount = data.totalCount;
      });
  }

  get_AllJobs(top, skip) {
    this.feild_name = 'MachineName';
    this.processService.getAllProcess(top, skip).subscribe((data: any) => {
      this.show_allprocess = data.items;
      this.page.totalCount = data.totalCount;
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
