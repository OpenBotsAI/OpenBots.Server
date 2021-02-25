import { Component, OnInit, TemplateRef } from '@angular/core';
import { Credentils } from '../../../@core/interfaces/credentials';
import { HttpService } from '../../../@core/services/http.service';
import { Page } from '../../../interfaces/paginateInstance';
import { Router } from '@angular/router';
import { DialogService } from '../../../@core/dialogservices/dialog.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { HelperService } from '../../../@core/services/helper.service';
import { CredentialsApiUrl } from '../../../webApiUrls';

@Component({
  selector: 'ngx-credentials',
  templateUrl: './credentials.component.html',
  styleUrls: ['./credentials.component.scss'],
})
export class CredentialsComponent implements OnInit {
  credentialsArr: Credentils[] = [];
  page: Page = {};
  deleteId: string;
  filterOrderBy: string;
  isDeleted = false;
  itemsPerPage: ItemsPerPage[] = [];

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
    this.getAllCredentials(top, skip, orderBy);
  }

  getAllCredentials(top: number, skip: number, orderBy?: string): void {
    let url: string;
    if (orderBy)
      url = `${CredentialsApiUrl.credentials}?$orderby=${orderBy}&$top=${top}&$skip=${skip}`;
    else
      url = `${CredentialsApiUrl.credentials}?$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
    this.httpService.get(url).subscribe((response) => {
      if (response && response.items.length) {
        this.credentialsArr = [...response.items];
        for (const data of this.credentialsArr) {
          if (data.provider === 'AD') {
            data.provider = 'Active Directory';
          } else if (data.provider === 'A') {
            data.provider = 'Application';
          }
        }
        this.page.totalCount = response.totalCount;
      } else this.credentialsArr = [];
    });
  }

  viewCredential(id: string): void {
    this.router.navigate([`/pages/credentials/view/${id}`]);
  }

  pageChanged(event): void {
    this.page.pageNumber = event;
    if (this.filterOrderBy)
      this.pagination(event, this.page.pageSize, `${this.filterOrderBy}`);
    else this.pagination(event, this.page.pageSize);
  }

  deleteCredential(ref): void {
    this.isDeleted = true;
    this.httpService
      .delete(`${CredentialsApiUrl.credentials}/${this.deleteId}`, {
        observe: 'response',
      })
      .subscribe(
        () => {
          ref.close();
          this.httpService.success('Deleted Successfully');
          this.isDeleted = false;
          if (this.credentialsArr.length == 1 && this.page.pageNumber > 1) {
            this.page.pageNumber--;
          }
          if (this.filterOrderBy) {
            this.pagination(
              this.page.pageNumber,
              this.page.pageSize,
              `${this.filterOrderBy}`
            );
          } else this.pagination(this.page.pageNumber, this.page.pageSize);
        },
        () => (this.isDeleted = false)
      );
  }

  openDeleteDialog(ref: TemplateRef<any>, id: string): void {
    this.deleteId = id;
    this.dialogService.openDialog(ref);
  }

  addCredential(): void {
    this.router.navigate([`/pages/credentials/add`]);
  }

  editCredential(id: string): void {
    this.router.navigate([`/pages/credentials/edit/${id}`]);
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

  searchValue(value) {
    if (value.length) {
      this.httpService
        .get(
          `${CredentialsApiUrl.credentials}?$filter=substringof(tolower('${value}'), tolower(Name))`
        )
        .subscribe((response) => {
          this.credentialsArr = response.items;
          this.page.totalCount = response.totalCount;
        });
    } else this.pagination(this.page.pageNumber, this.page.pageSize);
  }
}
