import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AgentsService } from '../agents.service';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TimeDatePipe } from '../../../@core/pipe';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { HelperService } from '../../../@core/services/helper.service';
import { DialogService } from '../../../@core/dialogservices'
import { Page } from '../../../interfaces/paginateInstance';
@Component({
  selector: 'ngx-get-agents-id',
  templateUrl: './get-agents-id.component.html',
  styleUrls: ['./get-agents-id.component.scss']
})
export class GetAgentsIdComponent implements OnInit {
  show_allagents: any = [];
  cred_value: any =[] ;
  addagent: FormGroup;
  isDeleted = false;
  showpage: any = [];
  ParmasAgentId: any;
  sortDir = 1;
  view_dialog: any;
  del_id: any = [];
  showAgentHeartBeat: any = []
  toggle: boolean;
  feild_name: any = [];
  page: Page = {};
  show_perpage_size: boolean = false;
  get_perPage: boolean = false;
  per_page_num: any = [];
  itemsPerPage: ItemsPerPage[] = [];
  showGridHeatbeat: boolean;

  constructor(private acroute: ActivatedRoute, protected router: Router,
    protected agentService: AgentsService, private formBuilder: FormBuilder, private helperService: HelperService,) {
    this.acroute.queryParams.subscribe(params => {
      this.ParmasAgentId = params.id
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
    });
   
  }

 

  getAgentId(id) {
    this.agentService.getAgentbyID(id).subscribe(
      (data: any) => {
        this.show_allagents = data.body;        
        const filterPipe = new TimeDatePipe();
        this.show_allagents.lastReportedOn = filterPipe.transform(this.show_allagents.lastReportedOn, 'lll');
        if (this.show_allagents.isHealthy == true) {
          this.show_allagents.isHealthy = "yes";
        }
        else if (this.show_allagents.isHealthy == false) {
          this.show_allagents.isHealthy = "No";
        }
        this.addagent.patchValue(this.show_allagents);
        this.addagent.disable();
    
      });
      
  }

  getAgentHeartBeatID(id, top, skip) {
    this.get_perPage = false;
    this.agentService.getAgentbyHeartBeatID(id, top, skip).subscribe(
      (data: any) => {
        console.log(data)
        if (data.length == 0) {
          this.showGridHeatbeat = false;
        }
        else if (data.length !== 0) {

          this.showGridHeatbeat = true;
          this.showAgentHeartBeat = data;
          this.page.totalCount = data.totalCount;
          this.get_perPage = true;
        }

      },
      (error) => { }
    );
  }


  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], { queryParams: { PageName: 'OpenBots.Server.Model.AgentModel', id: this.show_allagents.id } })
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
      .getAgentbyHeartBeatIDorder(this.ParmasAgentId, this.page.pageSize, skip, this.feild_name)
      .subscribe((data: any) => {
        this.showAgentHeartBeat = data;
        // this.showAgentHeartBeat = data.items;
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
        this.showAgentHeartBeat = data;
        // this.showAgentHeartBeat = data.items;
        this.page.totalCount = data.totalCount;
      });
  }







  pageChanged(event) {
    this.page.pageNumber = event;
    this.pagination(event, this.page.pageSize);
  }

  pagination(pageNumber, pageSize?) {
    if (this.show_perpage_size == false) {
      const top: number = pageSize;
      const skip = (pageNumber - 1) * pageSize;
      this.getAgentHeartBeatID(this.ParmasAgentId, top, skip);
    } else if (this.show_perpage_size == true) {
      const top: number = this.per_page_num;
      const skip = (pageNumber - 1) * this.per_page_num;
      this.getAgentHeartBeatID(this.ParmasAgentId, top, skip);
    }
  }

  trackByFn(index: number, item: unknown): number | null {
    if (!item) return null;
    return index;
  }


}
