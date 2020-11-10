import { Component, OnInit, TemplateRef } from '@angular/core';
import { Router } from '@angular/router';
import { DialogService } from '../../../@core/dialogservices';
import { HelperService } from '../../../@core/services/helper.service';
import { HttpService } from '../../../@core/services/http.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { Page } from '../../../interfaces/paginateInstance';
import { Queues } from '../../../interfaces/queues';

@Component({
  selector: 'ngx-all-queues',
  templateUrl: './all-queues.component.html',
  styleUrls: ['./all-queues.component.scss'],
})
export class AllQueuesComponent implements OnInit {
  page: Page = {};
  filterOrderBy: string;
  itemsPerPage: ItemsPerPage[] = [];
  allQueues: Queues[] = [];
  deleteId: string;
  isDeleted = false;
  constructor(
    private httpService: HttpService,
    private router: Router,
    private dialogService: DialogService,
    private helperService: HelperService
  ) {}

  ngOnInit(): void {
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.pagination(this.page.pageNumber, this.page.pageSize);
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  pagination(pageNumber: number, pageSize: number, orderBy?: string): void {
    const top = pageSize;
    this.page.pageSize = pageSize;
    const skip = (pageNumber - 1) * pageSize;
    this.getAllQueues(top, skip, orderBy);
  }

  getAllQueues(top: number, skip: number, orderBy?: string): void {
    let url: string;
    if (orderBy) url = `Queues?$orderby=${orderBy}&$top=${top}&$skip=${skip}`;
    else url = `Queues?$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
    this.httpService.get(url).subscribe((response) => {
      if (response) {
        this.page.totalCount = response.totalCount;
        if (response.items.length) {
          this.allQueues = [...response.items];
        } else this.allQueues = [];
      }
    });
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

  openDeleteDialog(ref: TemplateRef<any>, id: string): void {
    this.deleteId = id;
    this.dialogService.openDialog(ref);
  }

  deleteQueue(ref): void {
    this.isDeleted = true;
    this.httpService
      .delete(`Queues/${this.deleteId}`, { observe: 'response' })
      .subscribe(
        () => {
          ref.close();
          this.httpService.success('Deleted Successfully');
          this.isDeleted = false;
          if (this.allQueues.length == 1 && this.page.pageNumber > 1) {
            this.page.pageNumber--;
          }
          if (this.filterOrderBy) {
            this.pagination(
              this.page.pageNumber,
              this.page.pageSize,
              `${this.filterOrderBy}`
            );
          } else {
            this.pagination(this.page.pageNumber, this.page.pageSize);
          }
        },
        () => (this.isDeleted = false)
      );
  }

  pageChanged(event): void {
    this.page.pageNumber = event;
    if (this.filterOrderBy)
      this.pagination(event, this.page.pageSize, `${this.filterOrderBy}`);
    else this.pagination(event, this.page.pageSize);
  }

  viewQueue(id: string): void {
    this.router.navigate([`/pages/queueslist/view/${id}`]);
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

  editQueue(id: string): void {
    this.router.navigate([`/pages/queueslist/edit/${id}`]);
  }

  addQueue(): void {
    this.router.navigate([`/pages/queueslist/add`]);
  }
}
