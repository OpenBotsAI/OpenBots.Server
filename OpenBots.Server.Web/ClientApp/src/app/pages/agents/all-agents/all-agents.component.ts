import { Component, OnInit, TemplateRef } from '@angular/core';
import { Router } from '@angular/router';
import { NbToastrService } from '@nebular/theme';
import { AgentsService } from '../agents.service';
import { Page } from '../../../interfaces/paginateInstance';
import { SignalRService } from '../../../@core/services/signal-r.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { HelperService } from '../../../@core/services/helper.service';
import { DialogService } from '../../../@core/dialogservices';
import { BlockUI, NgBlockUI } from 'ng-block-ui';

@Component({
  selector: 'ngx-all-agents',
  templateUrl: './all-agents.component.html',
  styleUrls: ['./all-agents.component.scss'],
})
export class AllAgentsComponent implements OnInit {
  @BlockUI() blockUI: NgBlockUI;
  isDeleted = false;
  showpage: any = [];
  showAllAgents: any = [];
  sortDir = 1;
  view_dialog: any;
  del_id: any = [];
  toggle: boolean;
  feild_name: any = [];
  page: Page = {};
  show_perpage_size: boolean = false;
  get_perPage: boolean = false;
  per_page_num: any = [];
  itemsPerPage: ItemsPerPage[] = [];
  searchedValue: string;
  filterOrderBy: string;
  constructor(
    public router: Router,
    private dialogService: DialogService,
    protected agentService: AgentsService,
    protected signalRService: SignalRService,
    private helperService: HelperService,
    private toastrService: NbToastrService
  ) {}

  ngOnInit(): void {
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.pagination(this.page.pageNumber, this.page.pageSize);
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }
  gotoadd() {
    this.router.navigate(['/pages/agents/new']);
  }
  gotoedit(id) {
    this.router.navigate([`/pages/agents/edit/${id}`]);
  }
  gotojobs(id) {
    this.router.navigate(['/pages/job/list'], {
      queryParams: { AgentID: id },
    });
  }

  gotodetail(id) {
    this.router.navigate(['/pages/agents/get-agents-id'], {
      queryParams: { id: id },
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

  sort(filter_value, vale) {
    const top = this.page.pageSize;
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    this.filterOrderBy = `${filter_value}+${vale}`;
    if (this.searchedValue) {
      if (this.filterOrderBy) {
        this.agentService
          .getFilterPagination(
            top,
            skip,
            this.filterOrderBy,
            this.searchedValue
          )
          .subscribe((data: any) => {
            this.showpage = data;
            this.showAllAgents = data.items;
            this.page.totalCount = data.totalCount;
          });
      } else {
        this.agentService
          .getFilterPagination(top, skip, 'createdOn+desc', this.searchedValue)
          .subscribe((data: any) => {
            this.showpage = data;
            this.showAllAgents = data.items;
            this.page.totalCount = data.totalCount;
          });
      }
    } else if (this.filterOrderBy) {
      this.agentService
        .getFilterPagination(top, skip, this.filterOrderBy)
        .subscribe((data: any) => {
          this.showpage = data;
          this.showAllAgents = data.items;
          this.page.totalCount = data.totalCount;
        });
    } else {
      this.agentService
        .getFilterPagination(top, skip, 'createdOn+desc')
        .subscribe((data: any) => {
          this.showpage = data;
          this.showAllAgents = data.items;
          this.page.totalCount = data.totalCount;
        });
    }
  }

  patch_Agent(event, id) {
    this.toggle = event.target.checked;
    this.agentService.patchAgent(id, this.toggle).subscribe((data: any) => {
      if (this.toggle == true) {
        this.toastrService.success('Agent is now enabled.', 'Success');
      } else if (this.toggle == false) {
        this.toastrService.success('Agent is now disabled.', 'Success');
      }
    });
  }

  per_page(val) {
    this.per_page_num = val;
    this.show_perpage_size = true;
    this.page.pageSize = val;
    const skip = (this.page.pageNumber - 1) * this.per_page_num;
    if (this.searchedValue) {
      if (this.filterOrderBy) {
        this.agentService
          .getFilterPagination(
            this.page.pageSize,
            skip,
            this.filterOrderBy,
            this.searchedValue
          )
          .subscribe((data: any) => {
            this.showpage = data;
            this.showAllAgents = data.items;
            this.page.totalCount = data.totalCount;
          });
      } else {
        this.agentService
          .getFilterPagination(
            this.page.pageSize,
            skip,
            'createdOn+desc',
            this.searchedValue
          )
          .subscribe((data: any) => {
            this.showpage = data;
            this.showAllAgents = data.items;
            this.page.totalCount = data.totalCount;
          });
      }
    } else if (this.filterOrderBy) {
      this.agentService
        .getFilterPagination(this.page.pageSize, skip, this.filterOrderBy)
        .subscribe((data: any) => {
          this.showpage = data;
          this.showAllAgents = data.items;
          this.page.totalCount = data.totalCount;
        });
    } else {
      this.agentService
        .getFilterPagination(this.page.pageSize, skip, 'createdOn+desc')
        .subscribe((data: any) => {
          this.showpage = data;
          this.showAllAgents = data.items;
          this.page.totalCount = data.totalCount;
        });
    }
  }

  open2(dialog: TemplateRef<any>, id: any) {
    this.del_id = [];
    this.view_dialog = dialog;
    this.dialogService.openDialog(dialog);
    this.del_id = id;
  }

  del_agent(ref) {
    this.isDeleted = true;
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    this.agentService.delAgentbyID(this.del_id).subscribe(
      () => {
        this.isDeleted = false;
        this.toastrService.success('Agent Delete Successfully', 'Success');
        ref.close();
        this.pagination(this.page.pageNumber, this.page.pageSize);
      },
      () => (this.isDeleted = false)
    );
  }

  get_allagent(top, skip) {
    this.get_perPage = false;
    this.agentService
      .getFilterPagination(top, skip, 'createdOn+desc')
      .subscribe(
        (data: any) => {
          this.showpage = data;
          this.showAllAgents = data.items;
          this.page.totalCount = data.totalCount;
          this.get_perPage = true;
        },
        (error) => {}
      );
  }

  pageChanged(event) {
    this.page.pageNumber = event;
    this.pagination(event, this.page.pageSize);
  }

  pagination(pageNumber, pageSize) {
    const top = pageSize;
    this.page.pageSize = pageSize;
    const skip = (pageNumber - 1) * pageSize;
    if (this.searchedValue) {
      if (this.filterOrderBy) {
        this.agentService
          .getFilterPagination(
            top,
            skip,
            this.filterOrderBy,
            this.searchedValue
          )
          .subscribe((data: any) => {
            this.showpage = data;
            this.showAllAgents = data.items;
            this.page.totalCount = data.totalCount;
          });
      } else {
        this.agentService
          .getFilterPagination(top, skip, 'createdOn+desc', this.searchedValue)
          .subscribe((data: any) => {
            this.showpage = data;
            this.showAllAgents = data.items;
            this.page.totalCount = data.totalCount;
          });
      }
    } else if (this.filterOrderBy) {
      this.agentService
        .getFilterPagination(top, skip, this.filterOrderBy)
        .subscribe((data: any) => {
          this.showpage = data;
          this.showAllAgents = data.items;
          this.page.totalCount = data.totalCount;
        });
    } else {
      this.agentService
        .getFilterPagination(top, skip, 'createdOn+desc')
        .subscribe((data: any) => {
          this.showpage = data;
          this.showAllAgents = data.items;
          this.page.totalCount = data.totalCount;
        });
    }
  }
  trackByFn(index: number, item: unknown): number {
    if (!item) return null;
    return index;
  }

  searchValue(event) {
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    if (event.target.value.length >= 2) {
      this.searchedValue = event.target.value;
      if (this.filterOrderBy) {
        this.agentService
          .getFilterPagination(
            this.page.pageSize,
            skip,
            this.filterOrderBy,
            this.searchedValue
          )
          .subscribe((data: any) => {
            this.showpage = data;
            this.showAllAgents = data.items;
            this.page.totalCount = data.totalCount;
          });
      } else {
        this.agentService
          .getFilterPagination(
            this.page.pageSize,
            skip,
            'createdOn+desc',
            this.searchedValue
          )
          .subscribe((data: any) => {
            this.showpage = data;
            this.showAllAgents = data.items;
            this.page.totalCount = data.totalCount;
          });
      }
    } else if (!event.target.value.length) {
      this.searchedValue = null;
      if (this.filterOrderBy) {
        this.agentService
          .getFilterPagination(this.page.pageSize, skip, this.filterOrderBy)
          .subscribe((data: any) => {
            this.showpage = data;
            this.showAllAgents = data.items;
            this.page.totalCount = data.totalCount;
          });
      } else
        this.agentService
          .getFilterPagination(this.page.pageSize, skip, 'createdOn+desc')
          .subscribe((data: any) => {
            this.showpage = data;
            this.showAllAgents = data.items;
            this.page.totalCount = data.totalCount;
          });
    }
  }
}
