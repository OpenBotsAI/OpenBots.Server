import { Component, OnInit, TemplateRef } from '@angular/core';
import { HttpService } from '../../../@core/services/http.service';
import { Schedule } from '../../../interfaces/schedule';
import { Router } from '@angular/router';
import { Page } from '../../../interfaces/paginateInstance';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { DialogService } from '../../../@core/dialogservices/dialog.service';
import { HelperService } from '../../../@core/services/helper.service';

@Component({
  selector: 'ngx-all-schedule',
  templateUrl: './all-schedule.component.html',
  styleUrls: ['./all-schedule.component.scss'],
})
export class AllScheduleComponent implements OnInit {
  token: any = [];
  allScehduleData: Schedule[] = [];
  deleteId: string;
  isDeleted = false;
  page: Page = {};
  filterOrderBy: string;
  isPagination = false;
  itemsPerPage: ItemsPerPage[] = [];

  constructor(
    private httpService: HttpService,
    private router: Router,
    private dialogService: DialogService,
    private helperService: HelperService
  ) {
    this.token = localStorage.getItem('accessToken');
  }

  ngOnInit(): void {
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.pagination(this.page.pageNumber, this.page.pageSize);
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  gotoHangfire() {
    window.open(
      '/hangfire?access_token={{token}}',
      '_blank'
    );
  }

  getAllSchedule(top, skip, orderBy?): void {
    let url: string;
    if (orderBy)
      url = `Schedules?$orderby=${orderBy}&$top=${top}&$skip=${skip}`;
    else url = `Schedules?$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
    this.httpService.get(url).subscribe((response) => {
      this.page.totalCount = response.totalCount;
      if (response && response.items.length !== 0)
        this.allScehduleData = response.items;
      else this.allScehduleData = [];
    });
  }

  editSchedule(id: string): void {
    this.router.navigate([`/pages/schedules/edit/${id}`]);
  }

  viewSchedule(id: string): void {
    this.router.navigate([`/pages/schedules/view/${id}`]);
  }
  openDeleteDialog(ref: TemplateRef<any>, id: string): void {
    this.dialogService.openDialog(ref);
    this.deleteId = id;
  }

  deleteSchedule(ref) {
    this.isDeleted = true;
    this.httpService.delete(`Schedules/${this.deleteId}`).subscribe(() => {
      ref.close();
      if (this.allScehduleData.length == 1 && this.page.pageNumber > 1) {
        this.page.pageNumber--;
      }
      this.pagination(this.page.pageNumber, this.page.pageSize);
      this.isDeleted = false;
    }),
      () => (this.isDeleted = false);
  }

  pagination(pageNumber: number, pageSize: number, orderBy?: string): void {
    const top = pageSize;
    this.page.pageSize = pageSize;
    const skip = (pageNumber - 1) * pageSize;
    this.getAllSchedule(top, skip, orderBy);
  }

  onSortClick(event, param: string): void {
    let target = event.currentTarget,
      classList = target.classList;
    if (classList.contains('fa-chevron-up')) {
      classList.remove('fa-chevron-up');
      classList.add('fa-chevron-down');
      this.filterOrderBy = `${param}+asc`;
      this.pagination(this.page.pageNumber, this.page.pageSize, `${param}+asc`);
    } else {
      classList.remove('fa-chevron-down');
      classList.add('fa-chevron-up');
      this.filterOrderBy = `${param}+desc`;
      this.pagination(
        this.page.pageNumber,
        this.page.pageSize,
        `${param}+desc`
      );
    }
  }

  pageChanged(event): void {
    this.page.pageNumber = event;
    if (this.filterOrderBy)
      this.pagination(event, this.page.pageSize, `${this.filterOrderBy}`);
    else this.pagination(event, this.page.pageSize);
  }

  selectChange(event): void {
    this.page.pageSize = +event.target.value;
    this.page.pageNumber = 1;
    if (event.target.value && this.filterOrderBy) {
      this.pagination(
        this.page.pageNumber,
        this.page.pageSize,
        `${this.filterOrderBy}`
      );
    } else this.pagination(this.page.pageNumber, this.page.pageSize);
  }

  addSchedule(): void {
    this.router.navigate(['/pages/schedules/add']);
  }
}
