import { Component, OnInit, TemplateRef, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { NbDialogService } from '@nebular/theme';
import { Page } from '../../../interfaces/paginateInstance';
import { JobsService } from '../jobs.service';
import { FormBuilder, FormGroup } from '@angular/forms';
import { HttpResponse } from '@angular/common/http';
import { FileSaverService } from 'ngx-filesaver';

@Component({
  selector: 'ngx-all-jobs',
  templateUrl: './all-jobs.component.html',
  styleUrls: ['./all-jobs.component.scss']
})
export class AllJobsComponent implements OnInit, OnDestroy {
  process_id: any = [];
  agent_id: any = [];
  jobId: any = [];
  showjobs: FormGroup;
  show_alljobs: any = [];
  show_filter_agent: any = [];
  show_filter_process: any = [];
  showpage: any = []
  sortDir = 1;
  view_dialog: any;
  del_id: any = [];
  toggle: boolean;
  feild_name: any = [];
  page: Page = {};
  show_perpage_size: boolean = false;
  per_page_num: any = [];
  abc_filter: string;
  filter: string = "";
  filter_agent_id: string;
  filter_process_id: string;
  filter_jobstatus: string;
  filter_successful: string;


  jobStatus: { id: number; name: string }[] = [
    { id: 0, name: 'Unknown' },
    { id: 1, name: 'New' },
    { id: 2, name: 'Assigned' },
    { id: 3, name: 'InProgress' },
    { id: 4, name: 'Failed' },
    { id: 5, name: 'Completed' },
    { id: 9, name: 'Abandoned' },

  ];
  constructor(protected router: Router, private _FileSaverService: FileSaverService, private dialogService: NbDialogService, private formBuilder: FormBuilder,
    protected jobService: JobsService, private acroute: ActivatedRoute) {


    this.showjobs = this.formBuilder.group({
      processId: [''],
      agentId: ['']
    });
    this.get_filter_agent_process();
  }

  get_filter_agent_process() {
    this.jobService.getAgentName().subscribe(
      (data: any) => {
        this.show_filter_agent = data;
      });
    this.jobService.getProcessName().subscribe(
      (data: any) => {
        this.show_filter_process = data;
      });
  }
  ngOnInit(): void {
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.acroute.queryParams.subscribe((params) => {
      if (params.ProcessID) {
        this.process_id = params.ProcessID;
        this.filter_parmas_process_name(this.process_id)
      }
      if (params.AgentID) {
        this.agent_id = params.AgentID
        this.filter_parmas_agent_name(this.agent_id)
      }
      if (params.JobID) {
        this.jobId = params.JobID
        this.filter_parmas_jobId(this.jobId)
      }

    });

    if (this.agent_id.length == 0 && this.process_id.length == 0 && this.jobId == 0) {
      this.pagination(this.page.pageNumber, this.page.pageSize);
    }


  }

  filter_parmas_agent_name(agent_id) {

    this.showjobs.patchValue({ agentId: agent_id })
    this.common_agent(agent_id)
  }
  filter_parmas_process_name(process_id) {

    this.showjobs.patchValue({ processId: process_id })
    this.comon_process(process_id)
  }


  filter_parmas_jobId(job_id) {
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    this.jobService.JobsFilter(`id+eq+guid'${job_id}'`, this.page.pageSize, skip).subscribe(
      (data: any) => {
        for (let ab of data.items) {
          for (let status of this.jobStatus) {
            if (ab.jobStatus == status.id) {
              ab.jobStatus = status.name
            }
          }
        }
        this.show_alljobs = data.items;
        this.showpage = data
        this.page.totalCount = data.totalCount;

      });
  }







  gotodetail(id) {
    this.router.navigate(['/pages/job/get-jobs-id'], { queryParams: { id: id } })
  }


  gotoprocesslog(id) {
    this.router.navigate(['/pages/processlogs'], { queryParams: { jobId: id } })
  }
  comon_job(val) {
    this.filter_jobstatus = val;
    this.filter_job();
  }

  comon_process(val) {
    this.filter_process_id = val
    this.filter_job();
  }
 
  comon_succesful(val) {
    this.filter_successful = val;
    this.filter_job();
  }

  common_agent(val) {
    this.filter_agent_id = val;
    this.filter_job();
  }


  filter_job() {
    this.abc_filter = "";
    if (this.filter_agent_id != null && this.filter_agent_id != "") {
      this.abc_filter = this.abc_filter + `agentID+eq+guid'${this.filter_agent_id}' and `
    }
    if (this.filter_process_id != null && this.filter_process_id != "") {
      this.abc_filter = this.abc_filter + `ProcessID+eq+guid'${this.filter_process_id}' and `
    }
    if (this.filter_jobstatus != null && this.filter_jobstatus != "") {
      this.abc_filter = this.abc_filter + `jobstatus+eq+'${this.filter_jobstatus}' and `
    }
    if (this.filter_successful != null && this.filter_successful != "") {
      this.abc_filter = this.abc_filter + `isSuccessful+eq+${this.filter_successful} and `
    }
    if (this.abc_filter.endsWith(' and ')) {
      this.abc_filter = this.abc_filter.substring(0, this.abc_filter.length - 5)

    }

    if (this.abc_filter) {
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.jobService.JobsFilter(`${this.abc_filter}`, this.page.pageSize, skip).subscribe(
        (data: any) => {
          for (let ab of data.items) {
            for (let status of this.jobStatus) {
              if (ab.jobStatus == status.id) {
                ab.jobStatus = status.name
              }
            }
          }
          this.show_alljobs = data.items;
          this.showpage = data
          this.page.totalCount = data.totalCount;

        });
    }
    else {
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.get_AllJobs(this.page.pageSize, skip);
    }

  }


  sort(filter_val, vale) {
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    this.feild_name = filter_val + '+' + vale;
    this.jobService.getAllJobsOrder(this.page.pageSize, skip, this.feild_name).subscribe(
      (data: any) => {
        for (let ab of data.items) {
          for (let status of this.jobStatus) {
            if (ab.jobStatus == status.id) {
              ab.jobStatus = status.name
            }
          }
        }
        this.show_alljobs = data.items;
        this.showpage = data
        this.page.totalCount = data.totalCount;
      });
  }

  per_page(val) {

    if (this.abc_filter) {
      this.per_page_num = val
      this.page.pageSize = val;
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.jobService.JobsFilter(`${this.abc_filter}`, this.page.pageSize, skip).subscribe(
        (data: any) => {
          for (let ab of data.items) {
            for (let status of this.jobStatus) {
              if (ab.jobStatus == status.id) {
                ab.jobStatus = status.name
              }
            }
          }
          this.show_alljobs = data.items;
          this.showpage = data
          this.page.totalCount = data.totalCount;
        });
    }
    else if (this.abc_filter == undefined || this.abc_filter == "") {

      this.per_page_num = val
      this.page.pageSize = val;
      this.show_perpage_size = true
      const skip = (this.page.pageNumber - 1) * this.page.pageSize;
      this.jobService.getAllJobs(this.page.pageSize, skip).subscribe(
        (data: any) => {
          for (let ab of data.items) {
            for (let status of this.jobStatus) {
              if (ab.jobStatus == status.id) {
                ab.jobStatus = status.name
              }
            }
          }
          this.show_alljobs = data.items
          this.page.totalCount = data.totalCount;
        });
    }
  }

  get_AllJobs(top, skip) {
    this.feild_name = 'MachineName'
    this.jobService.getAllJobs(top, skip).subscribe(
      (data: any) => {
        for (let ab of data.items) {
          for (let status of this.jobStatus) {
            if (ab.jobStatus == status.id) {
              ab.jobStatus = status.name
            }
          }
        }
        this.show_alljobs = data.items;

        this.showpage = data
        this.page.totalCount = data.totalCount;
      });
  }


  onSortClick(event, filter_val) {
    let target = event.currentTarget,
      classList = target.classList;
    if (classList.contains('fa-chevron-up')) {
      classList.remove('fa-chevron-up');
      classList.add('fa-chevron-down');
      let sort_set = 'desc'
      this.sort(filter_val, sort_set)
      this.sortDir = -1;

    } else {
      classList.add('fa-chevron-up');
      classList.remove('fa-chevron-down');
      let sort_set = 'asc'
      this.sort(filter_val, sort_set)
      this.sortDir = 1;
    }
  }
  pageChanged(event) {
    this.page.pageNumber = event;
    this.pagination(event, this.page.pageSize);
  }

  pagination(pageNumber, pageSize?) {

    if (this.abc_filter) {
      if (this.show_perpage_size == false) {

        const skip = (pageNumber - 1) * pageSize;

        this.jobService.JobsFilter(`${this.abc_filter}`, this.page.pageSize, skip).subscribe(
          (data: any) => {
            for (let ab of data.items) {
              for (let status of this.jobStatus) {
                if (ab.jobStatus == status.id) {
                  ab.jobStatus = status.name
                }
              }
            }
            this.show_alljobs = data.items;
            this.showpage = data
            this.page.totalCount = data.totalCount;

          });
      }
      else if (this.show_perpage_size == true) {
        const top: number = this.per_page_num;
        const skip = (pageNumber - 1) * this.per_page_num;

        this.jobService.JobsFilter(`${this.abc_filter}`, this.page.pageSize, skip).subscribe(
          (data: any) => {
            for (let ab of data.items) {
              for (let status of this.jobStatus) {
                if (ab.jobStatus == status.id) {
                  ab.jobStatus = status.name
                }
              }
            }
            this.show_alljobs = data.items;
            this.showpage = data
            this.page.totalCount = data.totalCount;



          });
      }

    }
    else if (this.abc_filter == undefined || this.abc_filter == "") {
      if (this.show_perpage_size == false) {
        const top: number = pageSize;
        const skip = (pageNumber - 1) * pageSize;
        this.get_AllJobs(top, skip);


      }
      else if (this.show_perpage_size == true) {
        const top: number = this.per_page_num;
        const skip = (pageNumber - 1) * this.per_page_num;
        this.get_AllJobs(top, skip);

      }

    }


  }
  ngOnDestroy() {
    this.process_id = [];
    this.agent_id = [];
  }



  exportFile() {
    if (this.abc_filter) {
      let fileName: string;
      this.jobService
        .getExportFilebyfilter(this.abc_filter)
        .subscribe((data: HttpResponse<Blob>) => {
          fileName = data.headers
            .get('content-disposition')
            .split(';')[1]
            .split('=')[1]
            .replace(/\"/g, '');
          this._FileSaverService.save(data.body, fileName);
        });
    }
    else if (this.abc_filter == "" || this.abc_filter == undefined || this.abc_filter == null) {
      let fileName: string;
      this.jobService
        .getExportFile()
        .subscribe((data: HttpResponse<Blob>) => {
          fileName = data.headers
            .get('content-disposition')
            .split(';')[1]
            .split('=')[1]
            .replace(/\"/g, '');
          this._FileSaverService.save(data.body, fileName);
        });
    }
  }


}
