import { HttpResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { FileSaverService } from 'ngx-filesaver';
import { HelperService } from '../../../@core/services/helper.service';
import { HttpService } from '../../../@core/services/http.service';
import { Agents } from '../../../interfaces/agnets';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { Page } from '../../../interfaces/paginateInstance';
import { Automation } from '../../../interfaces/automations';
import { automationLogsApiUrl } from '../../../webApiUrls';
import { automationsApiUrl } from '../../../webApiUrls/automations';
import { AutomationLogs } from '../../../interfaces/automationLogs';

@Component({
  selector: 'ngx-all-automation-logs',
  templateUrl: './all-automation-logs.component.html',
  styleUrls: ['./all-automation-logs.component.scss'],
})
export class AllAutomationLogsComponent implements OnInit {
  processID: string;
  agentID: string;
  jobID: string;
  processlogFilter: string;
  processjoblogFilter: string;
  showprocessjob: FormGroup;
  agentLookUp: Agents[] = [];
  processLookUp: Automation[] = [];
  show_filter_jobs: any = [];
  page: Page = {};
  allProcessLogs: AutomationLogs[] = [];
  filterOrderBy: string;
  itemsPerPage: ItemsPerPage[] = [];

  constructor(
    private httpService: HttpService,
    private formBuilder: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private helperService: HelperService,
    private filesaver: FileSaverService
  ) {}

  ngOnInit(): void {
    this.getAgentsLookup();
    this.getProcessLookup();
    this.agentID = this.route.snapshot.queryParams['AgentID'];
    this.processID = this.route.snapshot.queryParams['ProcessID'];
    this.jobID = this.route.snapshot.queryParams['jobId'];
    this.showprocessjob = this.formBuilder.group({
      processId: [''],
      agentId: [''],
    });
    this.itemsPerPage = this.helperService.getItemsPerPage();
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    if (this.jobID) {
      this.filterByJobId();
    } else if (this.agentID || this.processID) {
      this.filterAgentProcess();
      this.patchAgentAndProcessValue();
    } else {
      this.pagination(this.page.pageNumber, this.page.pageSize);
    }
  }

  filterByJobId(): void {
    this.showprocessjob.disable();
    let url: string;
    const filterParam = `jobId+eq+guid'${this.jobID}'`;
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    if (this.filterOrderBy) {
      url = `${automationLogsApiUrl.automationLogs}?$filter=${filterParam}&$orderby=${this.filterOrderBy}&$top=${this.page.pageSize}&$skip=${skip}`;
    } else {
      url = `${automationLogsApiUrl.automationLogs}?$filter=${filterParam}&$orderby=createdOn+desc&$top=${this.page.pageSize}&$skip=${skip}`;
    }
    this.getByFilterParam(url);
  }

  /**
   *TODO Patching values to agentId and processId
   *@returns void
   */
  patchAgentAndProcessValue(): void {
    this.showprocessjob.patchValue({
      agentId: this.agentID,
      processId: this.processID,
    });
  }

  /**
   *TODO get Lookups for Processes
   *@returns void
   */
  getProcessLookup(): void {
    this.httpService
      .get(`${automationsApiUrl.getLookUp}`)
      .subscribe((response: Automation[]) => {
        if (response) this.processLookUp = response;
      });
  }

  getAgentsLookup(): void {
    this.httpService.get(`Agents/GetLookup`).subscribe((response: Agents[]) => {
      if (response) this.agentLookUp = response;
    });
  }

  processAndAgentDropdown(name: string, value: string): void {
    if (name == 'process') {
      this.processID = value;
      this.filterAgentProcess();
    } else if (name == 'agent') {
      this.agentID = value;
      this.filterAgentProcess();
    }
  }

  filterAgentProcess(): void {
    let filterQueryParam = '';
    if (this.agentID) {
      filterQueryParam = `agentID+eq+guid'${this.agentID}' and `;
    }
    if (this.processID) {
      filterQueryParam =
        filterQueryParam +
        `${automationsApiUrl.automationId}+eq+guid'${this.processID}' and `;
    }
    if (filterQueryParam.endsWith(' and ')) {
      filterQueryParam = filterQueryParam.substring(
        0,
        filterQueryParam.length - 5
      );
    }

    if (filterQueryParam) {
      let url: string;
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      if (this.filterOrderBy) {
        url = `${automationLogsApiUrl.automationLogs}?$filter=${filterQueryParam}&$orderby=${this.filterOrderBy}&$top=${this.page.pageSize}&$skip=${skip}`;
      } else {
        url = `${automationLogsApiUrl.automationLogs}?$filter=${filterQueryParam}&$orderby=createdOn+desc&$top=${this.page.pageSize}&$skip=${skip}`;
      }
      this.getByFilterParam(url);
    } else {
      this.pagination(this.page.pageNumber, this.page.pageSize);
    }
  }

  getByFilterParam(url: string) {
    this.httpService.get(url).subscribe((data: any) => {
      if (data) {
        this.allProcessLogs = data.items;
        this.page.totalCount = data.totalCount;
      }
    });
  }

  getProcessLogsList(top: number, skip: number, orderBy?: string): void {
    let url: string;
    if (orderBy)
      url = `${automationLogsApiUrl.automationLogs}?$orderby=${orderBy}&$top=${top}&$skip=${skip}`;
    else
      url = `${automationLogsApiUrl.automationLogs}?$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
    this.httpService.get(url).subscribe((response) => {
      if (response) {
        this.page.totalCount = response.totalCount;
        if (response && response.items.length)
          this.allProcessLogs = [...response.items];
      }
    });
  }
  pageChanged(event): void {
    this.page.pageNumber = event;
    if (this.jobID) this.filterByJobId();
    else if (this.processID || this.agentID) {
      this.filterAgentProcess();
    } else {
      this.pagination(event, this.page.pageSize);
    }
  }

  pagination(pageNumber: number, pageSize: number, orderBy?: string): void {
    const top = pageSize;
    this.page.pageSize = pageSize;
    const skip = (pageNumber - 1) * pageSize;
    this.getProcessLogsList(top, skip, orderBy);
  }
  onSortClick(event, param: string): void {
    let target = event.currentTarget,
      classList = target.classList;
    if (classList.contains('fa-chevron-up')) {
      classList.remove('fa-chevron-up');
      classList.add('fa-chevron-down');
      this.filterOrderBy = `${param}+asc`;
      if (this.jobID) this.filterByJobId();
      else if (this.processID || this.agentID) {
        this.filterAgentProcess();
      } else {
        this.pagination(
          this.page.pageNumber,
          this.page.pageSize,
          this.filterOrderBy
        );
      }
    } else {
      classList.remove('fa-chevron-down');
      classList.add('fa-chevron-up');
      this.filterOrderBy = `${param}+desc`;
      if (this.jobID) this.filterByJobId();
      else if (this.processID || this.agentID) {
        this.filterAgentProcess();
      } else {
        this.pagination(
          this.page.pageNumber,
          this.page.pageSize,
          this.filterOrderBy
        );
      }
    }
  }

  navigateToProcessView(id: string): void {
    this.router.navigate([`/pages/automationLogs/view/${id}`]);
  }

  exportFile(): void {
    let fileName: string;
    this.httpService
      .get(`${automationLogsApiUrl.automationLogsExport}zip`, {
        responseType: 'blob',
        observe: 'response',
      })
      .subscribe((response: HttpResponse<Blob>) => {
        fileName = response.headers
          .get('content-disposition')
          .split(';')[1]
          .split('=')[1]
          .replace(/\"/g, '');
        this.filesaver.save(response.body, fileName);
      });
  }

  navigateToJobs(id: string): void {
    this.router.navigate(['/pages/job/list'], {
      queryParams: { JobID: id },
    });
  }
  selectChange(event): void {
    if (event.target.value) {
      this.page.pageNumber = 1;
      this.page.pageSize = +event.target.value;
      if (this.jobID) this.filterByJobId();
      else if (this.processID || this.agentID) {
        this.filterAgentProcess();
      } else {
        this.pagination(this.page.pageNumber, this.page.pageSize);
      }
    }
  }

  trackByFn(index: number, item: unknown): number | null {
    if (!item) return null;
    return index;
  }
}
