import { Component, OnInit, TemplateRef } from '@angular/core';
import { Router } from '@angular/router';
import { DialogService } from '../../../@core/dialogservices';
import { HelperService } from '../../../@core/services/helper.service';
import { HttpService } from '../../../@core/services/http.service';
import { AgentGroup } from '../../../interfaces/agentGroup';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { Page } from '../../../interfaces/paginateInstance';
import { AgentGroupAPiUrl } from '../../../webApiUrls/agentGroup';

@Component({
  selector: 'ngx-all-agent-group-grid',
  templateUrl: './all-agent-group-grid.component.html',
  styleUrls: ['./all-agent-group-grid.component.scss'],
})
export class AllAgentGroupGridComponent implements OnInit {
  page: Page = {};
  searchedValue: string;
  filterOrderBy: string;
  itemsPerPage: ItemsPerPage[] = [];
  allAgentGroupArr: AgentGroup[] = [];
  deleteId: string;

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
    this.getAllAgentGroup(top, skip, orderBy);
  }

  addAgentGroup(): void {
    this.router.navigate(['/pages/agentgroup']);
  }

  getAllAgentGroup(top: number, skip: number, orderBy?: string): void {
    let url: string;
    if (this.searchedValue) {
      if (orderBy)
        url = `${AgentGroupAPiUrl.agentGroups}?$filter=substringof(tolower('${this.searchedValue}'), tolower(Name))&$orderby=${orderBy}&$top=${top}&$skip=${skip}`;
      else
        url = `${AgentGroupAPiUrl.agentGroups}?$filter=substringof(tolower('${this.searchedValue}'), tolower(Name))&$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
    } else if (orderBy)
      url = `${AgentGroupAPiUrl.agentGroups}?$orderby=${orderBy}&$top=${top}&$skip=${skip}`;
    else
      url = `${AgentGroupAPiUrl.agentGroups}?$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
    this.httpService.get(url).subscribe((response) => {
      console.log('res', response);
      if (response && response.items && response.items.length) {
        this.allAgentGroupArr = [...response.items];
      } else this.allAgentGroupArr = [];
      this.page.totalCount = response.totalCount;
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

  selectChange(event): void {
    this.page.pageSize = +event.target.value;
    this.page.pageNumber = 1;
    if (event.target.value && this.filterOrderBy) {
      this.pagination(
        this.page.pageNumber,
        this.page.pageSize,
        this.filterOrderBy
      );
    } else this.pagination(this.page.pageNumber, this.page.pageSize);
  }

  trackByFn(index: number, item: unknown): number {
    if (!item) return null;
    return index;
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
            `${AgentGroupAPiUrl.agentGroups}?$filter=substringof(tolower('${event.target.value}'), tolower(Name))`
          )
          .subscribe((response) => {
            this.allAgentGroupArr = [...response.items];
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

  viewAgentGroup(id: string): void {
    // this.router.navigate([])
  }

  editAgentGroup(id: string): void {
    this.router.navigate([`/pages/agentgroup/edit/${id}`]);
  }

  openDeleteDialog(ref: TemplateRef<any>, id: string): void {
    this.deleteId = id;
    this.dialogService.openDialog(ref);
  }

  deleteAgentGroup() {}

  pageChanged(event): void {
    this.page.pageNumber = event;
    if (this.filterOrderBy)
      this.pagination(event, this.page.pageSize, `${this.filterOrderBy}`);
    else this.pagination(event, this.page.pageSize);
  }
}
