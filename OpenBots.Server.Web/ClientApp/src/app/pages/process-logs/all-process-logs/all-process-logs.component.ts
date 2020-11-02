import { HttpResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { FileSaverService } from 'ngx-filesaver';
import { HelperService } from '../../../@core/services/helper.service';
import { HttpService } from '../../../@core/services/http.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { Page } from '../../../interfaces/paginateInstance';
import { ProcessLogs } from '../../../interfaces/processLogs';

@Component({
  selector: 'ngx-all-process-logs',
  templateUrl: './all-process-logs.component.html',
  styleUrls: ['./all-process-logs.component.scss'],
})
export class AllProcessLogsComponent implements OnInit {
  processid: any = [];
  agentid: any = [];
  job_Id: any = [];
  processlogFilter: string;
  processjoblogFilter: string;
  showprocessjob: FormGroup;
  show_filter_agent: any = [];
  show_filter_process: any = [];
  show_filter_jobs: any = []
  page: Page = {};
  allProcessLogs: ProcessLogs[] = [];
  filterOrderBy: string;
  itemsPerPage: ItemsPerPage[] = [];
  filter_agent_id: string;
  filter_job_id: string;
  filter_process_id: string;
  filter_jobstatus: string;
  filter_successful: string;

  constructor(
    private httpService: HttpService,
    private formBuilder: FormBuilder,
    private router: Router, private acroute: ActivatedRoute,
    private helperService: HelperService,
    private filesaver: FileSaverService
  ) {
    this.get_filter_agent_process();
  }

  ngOnInit(): void {
    this.showprocessjob = this.formBuilder.group({
      processId: [''],
      agentId: ['']
    });
    this.page.pageNumber = 1;
    this.page.pageSize = 5;

    this.acroute.queryParams.subscribe((params) => {
      if (params.ProcessID || params.AgentID) {
        this.processid = params.ProcessID;
        this.agentid = params.AgentID;
        this.filter_parmas_process_name(this.processid, this.agentid)
      }
      if (params.jobId) {
        this.job_Id = params.jobId
        this.common_job(params.jobId)
      }
    });

    if (this.agentid.length == 0 && this.processid.length == 0 && this.job_Id.length == 0) {
      this.pagination(this.page.pageNumber, this.page.pageSize);

    }
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }


  filter_parmas_agent_name(agent_id) {
    this.showprocessjob.patchValue({ agentId: agent_id })

  }

  filter_parmas_process_name(process_id, agent_id?) {
    this.showprocessjob.patchValue({ agentId: agent_id })
    this.showprocessjob.patchValue({ processId: process_id })
    this.comon_process(process_id, agent_id, 'other')
  }

  get_filter_agent_process() {

    this.httpService.get(`/Agents/GetLookup`).subscribe(
      (data: any) => {
        this.show_filter_agent = data;
      });
    this.httpService.get(`/Processes/GetLookup`).subscribe(
      (data: any) => {
        this.show_filter_process = data;
      });

  }

  common_job(val) {
    this.filter_job_id = val;
    this.filterJobId();
  }
  filterJobId() {
    this.processjoblogFilter = "";
    if (this.filter_job_id != null && this.filter_job_id != "") {
      this.processjoblogFilter = this.processjoblogFilter + `jobId+eq+guid'${this.filter_job_id}' and `
    }
    if (this.processjoblogFilter.endsWith(' and ')) {
      this.processjoblogFilter = this.processjoblogFilter.substring(0, this.processjoblogFilter.length - 5)
    }

    if (this.processjoblogFilter) {
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.httpService.get(`/processlogs?$filter=${this.processjoblogFilter}&$orderby=createdOn+desc&$top=${this.page.pageSize}&$skip=${skip}`
      ).subscribe(
        (data: any) => {
          this.allProcessLogs = data.items;
          this.page.totalCount = data.totalCount;

        });
    }
    else {
      this.pagination(this.page.pageNumber, this.page.pageSize)
    }

  }

  comon_process(val, val1, val2?) {
    if (val == 'process') {
      this.filter_process_id = val1;
    }
    if (val == 'agent') {
      this.filter_agent_id = val1;
    }
    else if (val2 == 'other') {
      this.filter_process_id = val
      this.filter_agent_id = val1;
    }

    this.filterAgentProcess();
  }





  filterAgentProcess() {
    this.processlogFilter = "";
    if (this.filter_agent_id != null && this.filter_agent_id != "") {
      this.processlogFilter = this.processlogFilter + `agentID+eq+guid'${this.filter_agent_id}' and `
    }
    if (this.filter_process_id != null && this.filter_process_id != "") {
      this.processlogFilter = this.processlogFilter + `ProcessID+eq+guid'${this.filter_process_id}' and `
    }

    if (this.processlogFilter.endsWith(' and ')) {
      this.processlogFilter = this.processlogFilter.substring(0, this.processlogFilter.length - 5)
    }

    if (this.processlogFilter) {

      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.httpService.get(`/processlogs?$filter=${this.processlogFilter}&$orderby=createdOn+desc&$top=${this.page.pageSize}&$skip=${skip}`
      ).subscribe(
        (data: any) => {
          this.allProcessLogs = data.items;
          this.page.totalCount = data.totalCount;

        });
    }
    else {
      this.pagination(this.page.pageNumber, this.page.pageSize)
    }

  }


  getProcessLogsList(top: number, skip: number, orderBy?: string): void {
    let url: string;
    if (orderBy)
      url = `processlogs?$orderby=${orderBy}&$top=${top}&$skip=${skip}`;
    else url = `processlogs?$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
    this.httpService.get(url).subscribe((response) => {
      if (response) {
        this.page.totalCount = response.totalCount;
        if (response && response.items.length !== 0)
          this.allProcessLogs = [...response.items];
      }
    });
  }



    pageChanged(event): void {
    if (this.processlogFilter != undefined || this.processlogFilter != "" ||
        this.processjoblogFilter != undefined || this.processjoblogFilter != "") {
        this.page.pageNumber = event;
        this.pagination(event, this.page.pageSize);

      if (this.processlogFilter) {
        this.page.pageNumber = event;
          this.filterByProceessLogPage(event, this.page.pageSize, this.processlogFilter, this.filterOrderBy);
      }
      else if (this.processjoblogFilter) {
        this.page.pageNumber = event;
          this.filterByProceessLogPage(event, this.page.pageSize, this.processjoblogFilter, this.filterOrderBy);
      }
    }
    else if (this.processlogFilter == null && this.processjoblogFilter == null) {
      this.page.pageNumber = event;
        this.pagination(event, this.page.pageSize, `${this.filterOrderBy}`);
    }
    else if (this.filterOrderBy == "" || this.filterOrderBy == undefined) {
      this.page.pageNumber = event;
        this.pagination(event, this.page.pageSize);
    }

    // if (this.filterOrderBy) {
    //   this.page.pageNumber = event;
    //   this.pagination(event, this.page.pageSize, `${this.filterOrderBy}`);
    // } else if (this.processlogFilter) {
    //   this.page.pageNumber = event;
    //   this.filterByProceessLogPage(event, this.page.pageSize, this.processlogFilter);
    // }
    // else if (this.processjoblogFilter) {
    //   this.page.pageNumber = event;
    //   this.filterByProceessLogPage(event, this.page.pageSize, this.processjoblogFilter);
    // }
    // else {
    //   this.page.pageNumber = event;
    //   this.pagination(event, this.page.pageSize);
    // }
  }


  filterByProceessLogPage(pageNumber: number, pageSize: number, processlogFilter?: string, order?: string) {
    if (order) {
      const top = pageSize;
      this.page.pageSize = pageSize;
      const skip = (pageNumber - 1) * pageSize;

      this.httpService.get(`/processlogs?$filter=${processlogFilter}&$orderby=${order}&$top=${top}&$skip=${skip}`
      ).subscribe(
        (data: any) => {
          this.allProcessLogs = data.items;
          this.page.totalCount = data.totalCount;

        });
    }
    else {
      order = 'createdOn+desc'
      const top = pageSize;
      this.page.pageSize = pageSize;
      const skip = (pageNumber - 1) * pageSize;

      this.httpService.get(`/processlogs?$filter=${processlogFilter}&$orderby=${order}&$top=${top}&$skip=${skip}`
      ).subscribe(
        (data: any) => {
          this.allProcessLogs = data.items;
          this.page.totalCount = data.totalCount;

        });
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

      if (this.processlogFilter) {
        this.filterByProceessLogPage(
          this.page.pageNumber,
          this.page.pageSize,
          `${this.processlogFilter}`, this.filterOrderBy
        );
      }
      else if (this.processjoblogFilter) {
        this.filterByProceessLogPage(
          this.page.pageNumber,
          this.page.pageSize,
          `${this.processjoblogFilter}`, this.filterOrderBy
        );
      }
      else {
        this.pagination(this.page.pageNumber, this.page.pageSize, `${param}+asc`);
      }

    } else {
      classList.remove('fa-chevron-down');
      classList.add('fa-chevron-up');
      this.filterOrderBy = `${param}+desc`;
      if (this.processlogFilter) {
        this.filterByProceessLogPage(
          this.page.pageNumber,
          this.page.pageSize,
          `${this.processlogFilter}`, this.filterOrderBy
        );
      }
      else if (this.processjoblogFilter) {
        this.filterByProceessLogPage(
          this.page.pageNumber,
          this.page.pageSize,
          `${this.processjoblogFilter}`, this.filterOrderBy
        );
      }
      else {
        this.pagination(
          this.page.pageNumber,
          this.page.pageSize,
          `${param}+desc`
        );
      }
    }
  }


  navigateToProcessView(id: string): void {
    this.router.navigate([`/pages/processlogs/view/${id}`]);
  }


  exportFile(): void {
    let fileName: string;
    this.httpService
      .get(`ProcessLogs/export/zip`, {
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
      if (this.filterOrderBy) {
        this.pagination(
          this.page.pageNumber,
          this.page.pageSize,
          `${this.filterOrderBy}`
        );
      }
      if (this.processlogFilter) {
        this.filterByProceessLogPage(
          this.page.pageNumber,
          this.page.pageSize,
          `${this.processlogFilter}`
        );
      }
      else if (this.processjoblogFilter) {
        this.filterByProceessLogPage(
          this.page.pageNumber,
          this.page.pageSize,
          `${this.processjoblogFilter}`
        );
      }
      else {
        this.pagination(this.page.pageNumber, this.page.pageSize);
      }
    }
  }
}
