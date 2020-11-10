import { Component, OnInit, TemplateRef } from '@angular/core';
import { Router } from '@angular/router';
import { NbToastrService, NbDialogService } from '@nebular/theme';
import { AgentsService } from '../agents.service';
import { Page } from '../../../interfaces/paginateInstance';
import { SignalRService } from '../../../@core/services/signal-r.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { HelperService } from '../../../@core/services/helper.service';
import { DialogService } from '../../../@core/dialogservices';

@Component({
  selector: 'ngx-all-agents',
  templateUrl: './all-agents.component.html',
  styleUrls: ['./all-agents.component.scss'],
})
export class AllAgentsComponent implements OnInit {
  isDeleted = false;
  showpage: any = [];
  show_allagents: any = [];
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
    this.router.navigate(['/pages/agents/edit'], { queryParams: { id: id } });
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
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    this.feild_name = filter_value + '+' + vale;
    this.agentService
      .getAllAgentOrder(this.page.pageSize, skip, this.feild_name)
      .subscribe((data: any) => {
        this.showpage = data;
        this.show_allagents = data.items;
      });
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
    console.log(val);
    this.per_page_num = val;
    this.show_perpage_size = true;
    this.page.pageSize = val;
    const skip = (this.page.pageNumber - 1) * this.per_page_num;
    this.agentService
      .getAllAgent(this.page.pageSize, skip)
      .subscribe((data: any) => {
        this.showpage = data;
        this.show_allagents = data.items;
        this.page.totalCount = data.totalCount;
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
    this.agentService.delAgentbyID(this.del_id).subscribe(() => {
      this.isDeleted = false;
      this.toastrService.success('Agent Delete Successfully', 'Success');
      ref.close();
      this.get_allagent(this.page.pageSize, skip);
    },
      () => (this.isDeleted = false));
  }

  get_allagent(top, skip) {
    this.get_perPage = false;
    this.agentService.getAllAgent(top, skip).subscribe(
      (data: any) => {
        this.showpage = data;
        this.show_allagents = data.items;
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

  pagination(pageNumber, pageSize?) {
    if (this.show_perpage_size == false) {
      const top: number = pageSize;
      const skip = (pageNumber - 1) * pageSize;
      this.get_allagent(top, skip);
    } else if (this.show_perpage_size == true) {
      const top: number = this.per_page_num;
      const skip = (pageNumber - 1) * this.per_page_num;
      this.get_allagent(top, skip);
    }
  }
}
