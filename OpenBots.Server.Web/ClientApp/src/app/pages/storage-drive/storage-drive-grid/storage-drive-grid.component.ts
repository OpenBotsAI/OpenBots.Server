import { Component, OnInit, TemplateRef } from '@angular/core';
import { Router } from '@angular/router';
import { DialogService } from '../../../@core/dialogservices';
import { HelperService } from '../../../@core/services/helper.service';
import { HttpService } from '../../../@core/services/http.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { Page } from '../../../interfaces/paginateInstance';
import { StorageDrive } from '../../../interfaces/storageDrive';
import { StorageDriveApiUrl } from '../../../webApiUrls/storageDriveUrl';

@Component({
  selector: 'ngx-storage-drive-grid',
  templateUrl: './storage-drive-grid.component.html',
  styleUrls: ['./storage-drive-grid.component.scss'],
})
export class StorageDriveGridComponent implements OnInit {
  storageDriveArr: StorageDrive[] = [];
  page: Page = {};
  deleteId: string;
  filterOrderBy: string;
  isDeleted = false;
  itemsPerPage: ItemsPerPage[] = [];
  searchedValue: string;
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
    this.getAllStorageDrives(top, skip, orderBy);
  }

  getAllStorageDrives(top: number, skip: number, orderBy?: string): void {
    let url: string;
    if (this.searchedValue) {
      if (orderBy)
        url = `${StorageDriveApiUrl.storage}/${StorageDriveApiUrl.drives}?$filter=substringof(tolower('${this.searchedValue}'), tolower(Name))&$orderby=${orderBy}&$top=${top}&$skip=${skip}`;
      else
        url = `${StorageDriveApiUrl.storage}/${StorageDriveApiUrl.drives}?$filter=substringof(tolower('${this.searchedValue}'), tolower(Name))&$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
    } else if (orderBy)
      url = `${StorageDriveApiUrl.storage}/${StorageDriveApiUrl.drives}?$orderby=${orderBy}&$top=${top}&$skip=${skip}`;
    else
      url = `${StorageDriveApiUrl.storage}/${StorageDriveApiUrl.drives}?$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
    this.httpService.get(url).subscribe((response) => {
      if (response && response.items && response.items.length) {
        this.storageDriveArr = [...response.items];
        this.page.totalCount = response.totalCount;
      } else this.storageDriveArr = [];
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

  trackByFn(index: number, item: unknown): number {
    if (!item) return null;
    return index;
  }
  viewStorageDrive(id: string): void {
    this.router.navigate([`/pages/storagedrive/view/${id}`]);
  }
  editStorageDrive(id: string): void {
    this.router.navigate([`/pages/storagedrive/edit/${id}`]);
  }
  addStorageDrive(): void {
    this.router.navigate([`/pages/storagedrive/add`]);
  }

  deleteStorageDrive(ref): void {
    this.isDeleted = true;
    this.httpService
      .delete(
        `${StorageDriveApiUrl.storage}/${StorageDriveApiUrl.drives}/${this.deleteId}`,
        { observe: 'response' }
      )
      .subscribe(
        (response) => {
          if (response && response.status == 200) {
            this.httpService.success('Storage drive deleted successfully');
            ref.close();
            this.isDeleted = false;
            if (this.filterOrderBy) {
              this.pagination(
                this.page.pageNumber,
                this.page.pageSize,
                `${this.filterOrderBy}`
              );
            } else this.pagination(this.page.pageNumber, this.page.pageSize);
          }
        },
        () => (this.isDeleted = false)
      );
  }
  searchValue(event) {
    if (event.target.value.length >= 2) {
      this.searchedValue = event.target.value;
      if (this.filterOrderBy) {
        this.pagination(
          this.page.pageNumber,
          this.page.pageSize,
          this.filterOrderBy
        );
      } else {
        this.httpService
          .get(
            `${StorageDriveApiUrl.storage}/${StorageDriveApiUrl.drives}?$filter=substringof(tolower('${event.target.value}'), tolower(Name))`
          )
          .subscribe((response) => {
            this.storageDriveArr = [...response.items];
            this.page.totalCount = response.totalCount;
          });
      }
    } else if (!event.target.value.length) {
      this.searchedValue = null;
      if (this.filterOrderBy) {
        this.pagination(
          this.page.pageNumber,
          this.page.pageSize,
          this.filterOrderBy
        );
      } else this.pagination(this.page.pageNumber, this.page.pageSize);
    }
  }
}
