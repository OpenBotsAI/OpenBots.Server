import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AgentsService } from '../agents.service';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TimeDatePipe } from '../../../@core/pipe';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { HelperService } from '../../../@core/services/helper.service';
import { Page } from '../../../interfaces/paginateInstance';
@Component({
  selector: 'ngx-get-agents-id',
  templateUrl: './get-agents-id.component.html',
  styleUrls: ['./get-agents-id.component.scss'],
})
export class GetAgentsIdComponent implements OnInit {
  showAllAgents: any = [];
  addagent: FormGroup;
  isDeleted = false;
  showpage: any = [];
  ParmasAgentId: any;
  sortDir = 1;
  showAgentHeartBeat: any = [];
  toggle: boolean;
  feild_name: any = [];
  page: Page = {};
  showPerPageSize: boolean = false;
  getPerPage: boolean = false;
  perPageNum: any = [];
  itemsPerPage: ItemsPerPage[] = [];
  showGridHeatbeat: boolean;

  constructor(
    private acroute: ActivatedRoute,
    protected router: Router,
    protected agentService: AgentsService,
    private formBuilder: FormBuilder,
    private helperService: HelperService
  ) {
    this.acroute.queryParams.subscribe((params) => {
      this.ParmasAgentId = params.id;
      this.getAgentId(this.ParmasAgentId);
    });
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.pagination(this.page.pageNumber, this.page.pageSize);
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  ngOnInit(): void {
    this.addagent = this.formBuilder.group({
      name: [''],
      machineName: [''],
      macAddresses: [''],
      ipAddresses: [''],
      credentialId: [''],
      credentialName: [''],
      isEnabled: [''],
      createdBy: [''],
      createdOn: [''],
      deleteOn: [''],
      deletedBy: [''],
      id: [''],
      isDeleted: [''],
      isHealthy: [''],
      lastReportedMessage: [''],
      lastReportedOn: [''],
      lastReportedStatus: [''],
      lastReportedWork: [''],
      timestamp: [''],
      updatedBy: [''],
      updatedOn: [''],
      isEnhancedSecurity: [],
      ipOption: [''],
    });
  }

  getAgentId(id) {
    this.agentService.getAgentbyID(id).subscribe((data: any) => {
      this.showAllAgents = data.body;
      const filterPipe = new TimeDatePipe();
      this.showAllAgents.lastReportedOn = filterPipe.transform(
        this.showAllAgents.lastReportedOn,
        'lll'
      );
      if (this.showAllAgents.isHealthy == true) {
        this.showAllAgents.isHealthy = 'yes';
      } else if (this.showAllAgents.isHealthy == false) {
        this.showAllAgents.isHealthy = 'No';
      }
      this.addagent.patchValue(this.showAllAgents);
      this.addagent.disable();
    });
  }

  getAgentHeartBeatID(id, top, skip) {
    this.getPerPage = false;
    this.agentService.getAgentbyHeartBeatID(id, top, skip).subscribe(
      (data: any) => {
        if (data.items.length == 0) {
          this.showGridHeatbeat = false;
        } else if (data.items.length !== 0) {
          this.showGridHeatbeat = true;
          this.showAgentHeartBeat = data.items;
          this.page.totalCount = data.totalCount;
          this.getPerPage = true;
        }
      },
      (error) => {}
    );
  }

  refreshData() {
    this.getAgentHeartBeatID(this.ParmasAgentId, 5, 0);
  }

  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], {
      queryParams: { PageName: 'Agent', id: this.showAllAgents.id },
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
      .getAgentbyHeartBeatIDorder(
        this.ParmasAgentId,
        this.page.pageSize,
        skip,
        this.feild_name
      )
      .subscribe((data: any) => {
        this.showAgentHeartBeat = data.items;
      });
  }

  perPage(val) {
    this.perPageNum = val;
    this.showPerPageSize = true;
    this.page.pageSize = val;
    const skip = (this.page.pageNumber - 1) * this.perPageNum;
    this.agentService
      .getAgentbyHeartBeatID(this.ParmasAgentId, this.page.pageSize, skip)
      .subscribe((data: any) => {
        this.showAgentHeartBeat = data.items;
        this.page.totalCount = data.totalCount;
      });
  }

  pageChanged(event) {
    this.page.pageNumber = event;
    this.pagination(event, this.page.pageSize);
  }

  pagination(pageNumber, pageSize?) {
    if (this.showPerPageSize == false) {
      const top: number = pageSize;
      const skip = (pageNumber - 1) * pageSize;
      this.getAgentHeartBeatID(this.ParmasAgentId, top, skip);
    } else if (this.showPerPageSize == true) {
      const top: number = this.perPageNum;
      const skip = (pageNumber - 1) * this.perPageNum;
      this.getAgentHeartBeatID(this.ParmasAgentId, top, skip);
    }
  }

  trackByFn(index: number, item: unknown): number {
    if (!item) return null;
    return index;
  }
}
